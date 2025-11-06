// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Core.Enums.MpvClientProperties;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 播放器关闭事件，应在此事件中释放客户端.
    /// </summary>
    public event EventHandler Shutdown;

    /// <summary>
    /// 文件播放结束事件.
    /// </summary>
    public event EventHandler ReachFileEnd;

    /// <summary>
    /// 文件开始加载事件.
    /// </summary>
    public event EventHandler ReachFileLoading;

    /// <summary>
    /// 文件加载完成事件.（此时开始尝试播放，如果是网络文件，此时开始缓冲）
    /// </summary>
    public event EventHandler ReachFileLoaded;

    /// <summary>
    /// 发生错误.
    /// </summary>
    public event EventHandler<MpvError> ErrorOccurred;

    /// <summary>
    /// 缓冲状态发生变化事件.
    /// </summary>
    public event EventHandler<MpvCacheStateEventArgs> CacheStateChanged;

    private async void HandleEvent(MpvEvent @event)
    {
        switch (@event.EventId)
        {
            case MpvEventId.StartFile:
                ReachFileLoading?.Invoke(this, EventArgs.Empty);
                break;
            case MpvEventId.EndFile:
                ReachFileEnd?.Invoke(this, EventArgs.Empty);
                break;
            case MpvEventId.LogMessage:
                var logMessage = Marshal.PtrToStructure<MpvEventLogMessage>(@event.DataPtr);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[MPV] Log message: {logMessage.Level} - {logMessage.Text}");
#endif
                _logger.LogInformation($"[MPV] Log message: {logMessage.Level} - {logMessage.Text}");
                if (logMessage.LogLevel == MpvLogLevel.Warn)
                {
                    if (logMessage.Text.Contains("HTTP error", StringComparison.OrdinalIgnoreCase))
                    {
                        if (logMessage.Text.Contains("404", StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorOccurred?.Invoke(this, MpvError.Http404);
                        }
                        else if (logMessage.Text.Contains("403", StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorOccurred?.Invoke(this, MpvError.Http403);
                        }
                        else if (logMessage.Text.Contains("500", StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorOccurred?.Invoke(this, MpvError.Http500);
                        }

                        ErrorOccurred?.Invoke(this, MpvError.VoInitFailed);
                    }
                    if (logMessage.Text.Contains("error reading packet", StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.LoadingFailed);
                    }
                    else if (logMessage.Text.Contains("Failed to seek when reading header element", StringComparison.Ordinal))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.TlsError);
                    }
                }
                else if (logMessage.LogLevel == MpvLogLevel.Error)
                {
                    if (logMessage.Text.Contains("Subprocess failed", StringComparison.OrdinalIgnoreCase))
                    {
                        // 意味着播放失败，需要抛出该异常.
                        ErrorOccurred?.Invoke(this, MpvError.VoInitFailed);
                    }
                    else if (logMessage.Text.Contains("Couldn't open Blu-ray device", StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.BluRayInitFailed);
                    }
                    else if (logMessage.Text.Contains("Couldn't open DVD device"))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.DvdInitFailed);
                    }
                    else if (logMessage.Text.Contains("Failed to open http"))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.VoInitFailed);
                    }
                    else if (logMessage.Text.Contains("tls: IO error") || logMessage.Text.Contains("Seek failed"))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.TlsError);
                    }
                    else if (logMessage.Text.Contains("Passthrough format unsupported"))
                    {
                        ErrorOccurred?.Invoke(this, MpvError.PassthroughFormatUnsupported);
                    }
                }

                break;
            case MpvEventId.FileLoaded:
                ReachFileLoaded?.Invoke(this, EventArgs.Empty);
                break;
            case MpvEventId.Idle:
            case MpvEventId.Seek:
                {
                    var stateResult = await GetPlayerStateAsync();
                    if (stateResult.IsFailed)
                    {
                        return;
                    }

                    SendNotify(MpvClientEventId.StateChanged, stateResult.Value);
                }

                break;
            case MpvEventId.PropertyChange:
                var eventProp = Marshal.PtrToStructure<MpvEventProperty>(@event.DataPtr);
                HandleObservePropertyChanged(eventProp);
                break;
            case MpvEventId.PlaybackRestart:
                SendNotify(MpvClientEventId.PlaybackRestart, default);
                break;
            default:
                _logger.LogInformation($"[MPV] Event received: {@event.EventId}");
                break;
        }

        if (@event.Error != MpvError.Success)
        {
            ErrorOccurred?.Invoke(this, @event.Error);
        }
    }

    private async void HandleObservePropertyChanged(MpvEventProperty eventProp)
    {
        if (eventProp.DataPtr == IntPtr.Zero)
        {
            return;
        }

        if (eventProp.Name == Pause || eventProp.Name == CoreIdle || eventProp.Name == Seeking)
        {
            var stateResult = await GetPlayerStateAsync();
            if (stateResult.IsFailed)
            {
                return;
            }

            SendNotify(MpvClientEventId.StateChanged, stateResult.Value);
        }
        else if (eventProp.Name == Volume)
        {
            var volume = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.VolumeChanged, volume);
        }
        else if (eventProp.Name == Duration)
        {
            var duration = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.DurationChanged, duration);
        }
        else if (eventProp.Name == TimePosition)
        {
            var position = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.PositionChanged, position);
        }
        else if (eventProp.Name == FullScreen)
        {
            var isFullScreen = Marshal.PtrToStructure<MpvNode>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.FullScreenChanged, isFullScreen.Flag != 0);
        }
        else if (eventProp.Name == CompactOverlay)
        {
            var isOnTop = Marshal.PtrToStructure<MpvNode>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.CompactOverlayChanged, isOnTop.Flag != 0);
        }
        else if (eventProp.Name == Speed)
        {
            var speed = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.SpeedChanged, speed);
        }
        else if (eventProp.Name == CacheSpeed)
        {
            var cacheSpeed = Marshal.PtrToStructure<long>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.CacheSpeedChanged, cacheSpeed);
        }
        else if (eventProp.Name == Metadata)
        {
            var metadata = Marshal.PtrToStructure<MpvNode>(eventProp.DataPtr);
            if (MpvNodeList.ToDictionary(metadata.RemoteNodeListValue) is Dictionary<string, MpvNode> nodeDict)
            {
                SendNotify(MpvClientEventId.MetadataLoaded, nodeDict.Select(p => (p.Key, p.Value.StringValue)).ToDictionary());
            }
        }
        else if (eventProp.Name == TrackCount)
        {
            SendNotify(MpvClientEventId.TrackCountChanged, default);
        }
        else if (eventProp.Name == DemuxerCacheState)
        {
            var cacheState = Marshal.PtrToStructure<MpvNode>(eventProp.DataPtr);
            if (MpvNodeList.ToDictionary(cacheState.RemoteNodeListValue) is Dictionary<string, MpvNode> nodeDict
                && nodeDict.ContainsKey("seekable-ranges"))
            {
                var rangeNodes = MpvNodeList.ToMpvNodeArray(nodeDict.First(p => p.Key == "seekable-ranges").Value.RemoteNodeListValue);
                var ranges = new List<MpvSeekableRange>();
                foreach (var item in rangeNodes ?? [])
                {
                    var map = MpvNodeList.ToDictionary(item.RemoteNodeListValue);
                    if (map != null && map.TryGetValue("start", out var startNode) && map.TryGetValue("end", out var endNode))
                    {
                        ranges.Add(new MpvSeekableRange
                        {
                            Start = startNode.DoubleValue,
                            End = endNode.DoubleValue
                        });
                    }
                }

                var bofCached = nodeDict.TryGetValue("bof-cached", out var bofCachedNode) && bofCachedNode.Flag != 0;
                var eofCached = nodeDict.TryGetValue("eof-cached", out var eofCachedNode) && eofCachedNode.Flag != 0;
                var fwBytes = nodeDict.TryGetValue("fw-bytes", out var fwBytesNode) ? fwBytesNode.IntegerValue : 0;
                var fileBytes = nodeDict.TryGetValue("file-cache-bytes", out var bwBytesNode) ? bwBytesNode.IntegerValue : 0;
                var cacheStateArgs = new MpvCacheStateEventArgs(ranges)
                {
                    BofCached = bofCached,
                    EofCached = eofCached,
                    FwBytes = fwBytes,
                    FileCacheBytes = fileBytes
                };

                CacheStateChanged?.Invoke(this, cacheStateArgs);
            }
        }
    }
}
