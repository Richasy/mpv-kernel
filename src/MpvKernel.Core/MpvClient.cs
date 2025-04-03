// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.MpvKernel.Core.Models;
using System.Runtime.InteropServices;

namespace Richasy.MpvKernel.Core;

/// <summary>
/// MPV 客户端.
/// </summary>
public sealed partial class MpvClient : IAsyncDisposable
{
    private readonly MpvInteropHandle _handle;
    private readonly ILogger _logger;
    private Task? _eventLoopTask;
    private CancellationTokenSource? _eventCts;

    /// <summary>
    /// Initialize a new instance of the <see cref="MpvClient"/> class.
    /// </summary>
    internal MpvClient(string name, MpvInteropHandle handle, ILogger? logger = null)
    {
        ClientName = name;
        _handle = handle;
        _logger = logger ?? new NullLogger<MpvClient>();
    }

    /// <summary>
    /// 客户端ID.
    /// </summary>
    public string ClientName { get; }

    /// <summary>
    /// 是否已经被释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 设置日志等级.
    /// </summary>
    /// <param name="level">等级.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetLogLevelAsync(MpvLogLevel level)
    {
        var errorCode = MpvError.Success;
        _logger.LogInformation($"Set {ClientName} log level to {level}.");
        await Task.Run(() => errorCode = MpvNative.RequestLogMessages(_handle, level.ToMpvLogLevelString()));
        ThrowIfFailed(errorCode, $"{ClientName} | set log level failed");
    }

    /// <summary>
    /// 初始化（启动事件轮询）.
    /// </summary>
    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, typeof(MpvClient));

        if (_eventCts != null)
        {
            return;
        }

        _eventCts = new CancellationTokenSource();
        _eventLoopTask = Task.Run(() =>
        {
            while (!_eventCts.Token.IsCancellationRequested)
            {
                var eventPtr = MpvNative.WaitEvent(_handle, -1);
                var eventData = Marshal.PtrToStructure<MpvEvent>(eventPtr);
                HandleEvent(eventData);

                if (eventData.EventId == MpvEventId.Shutdown)
                {
                    _logger.LogInformation($"[{ClientName}] Shutdown event received.");
                    Shutdown?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }, _eventCts.Token);
    }

    private static void ThrowIfFailed(MpvError errorCode, string message)
    {
        if (errorCode != MpvError.Success)
        {
            throw new MpvException(message, errorCode);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        IsDisposed = true;
        if (_eventCts != null)
        {
            await _eventCts.CancelAsync();
            _eventCts.Dispose();
            _eventCts = null;
        }

        await Task.Run(() => MpvNative.Destroy(_handle));
    }
}
