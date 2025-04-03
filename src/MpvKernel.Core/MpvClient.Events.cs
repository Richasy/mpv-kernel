// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 播放器关闭事件，应在此事件中释放客户端.
    /// </summary>
    public event EventHandler Shutdown;

    private void HandleEvent(MpvEvent @event)
    {
        switch (@event.EventId)
        {
            case MpvEventId.StartFile:
                _logger.LogInformation($"[{ClientName}] Start file event received.");
                break;
            case MpvEventId.EndFile:
                _logger.LogInformation($"[{ClientName}] End file event received.");
                break;
            case MpvEventId.LogMessage:
                var logMessage = Marshal.PtrToStructure<MpvEventLogMessage>(@event.DataPtr);
                _logger.LogInformation($"[{ClientName}] Log message: {logMessage.Level} - {logMessage.Text}");
                break;
            case MpvEventId.FileLoaded:
                _logger.LogInformation($"[{ClientName}] File loaded event received.");
                break;
            default:
                _logger.LogInformation($"[{ClientName}] Event received: {@event.EventId}");
                break;
        }
    }
}
