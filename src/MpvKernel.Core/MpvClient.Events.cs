// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Richasy.MpvKernel.Core.Enums;
using System.Runtime.InteropServices;

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

    private void HandleEvent(MpvEvent @event)
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
                SendNotify(Enums.MpvClientEventId.StateChanged, MpvPlayerState.Idle);
                break;
            case MpvEventId.Seek:
                SendNotify(Enums.MpvClientEventId.StateChanged, MpvPlayerState.Seeking);
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

        if (eventProp.Name == "pause")
        {
            var state = await GetPlayerStateAsync();
            SendNotify(MpvClientEventId.StateChanged, state);
        }
        else if (eventProp.Name == "volume")
        {
            var volume = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.VolumeChanged, volume);
        }
        else if (eventProp.Name == "duration")
        {
            var duration = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.DurationChanged, duration);
        }
        else if (eventProp.Name == "time-pos")
        {
            var position = Marshal.PtrToStructure<double>(eventProp.DataPtr);
            SendNotify(MpvClientEventId.PositionChanged, position);
        }
    }
}
