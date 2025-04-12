// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Richasy.MpvKernel.Core.Enums;
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
                break;
            case MpvEventId.FileLoaded:
                ReachFileLoaded?.Invoke(this, EventArgs.Empty);
                break;
            case MpvEventId.Idle:
            case MpvEventId.Seek:
                {
                    var stateResult = await GetPlayerStateAsync();
                    if(stateResult.IsFailed)
                    {
                        _logger.LogError($"[MPV] Failed to get player state: {stateResult.Errors}");
                        return;
                    }

                    SendNotify(MpvClientEventId.StateChanged, stateResult.Value);
                }

                break;
            case MpvEventId.PropertyChange:
                var eventProp = Marshal.PtrToStructure<MpvEventProperty>(@event.DataPtr);
                HandleObservePropertyChanged(eventProp);
                break;
            default:
                _logger.LogInformation($"[MPV] Event received: {@event.EventId}");
                break;
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
                _logger.LogError($"[MPV] Failed to get player state: {stateResult.Errors}");
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
    }
}
