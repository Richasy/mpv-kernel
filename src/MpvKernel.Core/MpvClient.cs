// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using System.Runtime.InteropServices;

namespace Richasy.MpvKernel.Core;

/// <summary>
/// MPV 客户端.
/// </summary>
public sealed partial class MpvClient : IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly MpvInteropHandle _handle;
    private Task? _eventLoopTask;
    private CancellationTokenSource? _eventCts;
    private double? _cachedDuration;
    private MpvPlayerSnapshot? _cachedSnapshot;

    /// <summary>
    /// Initialize a new instance of the <see cref="MpvClient"/> class.
    /// </summary>
    internal MpvClient(MpvInteropHandle handle, ILogger? logger = null)
    {
        _handle = handle;
        _logger = logger ?? new NullLogger<MpvClient>();
    }

    /// <summary>
    /// 数据通知事件.
    /// </summary>
    public event EventHandler<MpvClientNotifyEventArgs> DataNotify;

    /// <summary>
    /// 是否已初始化.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 是否已经被释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// MPV 句柄.
    /// </summary>
    public MpvInteropHandle Handle => _handle;

    /// <summary>
    /// 创建一个新的 MPV 实例.
    /// </summary>
    /// <param name="options">初始化选项.</param>
    /// <param name="logger">日志记录.</param>
    /// <returns><see cref="MpvClient"/>.</returns>
    public static async Task<MpvClient> CreateAsync(MpvInitializeOptions? options = null, ILogger? logger = null)
    {
        var instanceHandle = MpvNative.Create();
        var instance = new MpvClient(instanceHandle, logger);
        await instance.InitializeAsync(options).ConfigureAwait(false);
        MpvNative.ObserveProperty(instanceHandle, 0, "duration", MpvFormat.Double);
        MpvNative.ObserveProperty(instanceHandle, 0, "time-pos", MpvFormat.Double);
        MpvNative.ObserveProperty(instanceHandle, 0, "volume", MpvFormat.Double);
        MpvNative.ObserveProperty(instanceHandle, 0, "speed", MpvFormat.Double);
        MpvNative.ObserveProperty(instanceHandle, 0, "pause", MpvFormat.Flag);
        MpvNative.ObserveProperty(instanceHandle, 0, "core-idle", MpvFormat.Flag);
        MpvNative.ObserveProperty(instanceHandle, 0, "fullscreen", MpvFormat.Flag);
        MpvNative.ObserveProperty(instanceHandle, 0, "ontop", MpvFormat.Flag);
        return instance;
    }

    /// <summary>
    /// 设置日志等级.
    /// </summary>
    /// <param name="level">等级.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetLogLevelAsync(MpvLogLevel level)
    {
        var errorCode = MpvError.Success;
        _logger.LogInformation($"Set Mpv log level to {level}.");
        await Task.Run(() => errorCode = MpvNative.RequestLogMessages(_handle, level.ToMpvLogLevelString()));
        ThrowIfFailed(errorCode, "Mpv | set log level failed");
    }

    /// <summary>
    /// 设置配置文件.
    /// </summary>
    /// <param name="filePath">配置文件地址.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetConfigFileAsync(string filePath)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.LoadConfigFile(_handle, filePath));
        ThrowIfFailed(errorCode, "Mpv | load config file failed");
    }

    /// <summary>
    /// 是否开启闲置状态.
    /// </summary>
    /// <param name="idleEnable"><c>null</c> 对应 once, <c>true</c> 对应 yes, <c>false</c> 对应 no</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task UseIdleAsync(bool? idleEnable)
    {
        var state = idleEnable == null ? "once" : idleEnable == true ? "yes" : "no";
        var errorCode = MpvError.Success;
        _logger.LogInformation($"Set Mpv idle to {state}.");
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "idle", state));
        ThrowIfFailed(errorCode, "Mpv | set idle failed");
    }

    /// <summary>
    /// 是否允许保持开启状态（播放完成后保持最后一帧显示）
    /// </summary>
    /// <param name="isKeepOpen">是否开启</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task UseKeepOpenAsync(bool isKeepOpen)
    {
        var errorCode = MpvError.Success;
        var state = isKeepOpen ? "yes" : "no";
        _logger.LogInformation($"Set Mpv keep open to {state}.");
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "keep-open", state));
        ThrowIfFailed(errorCode, "Mpv | set keep open failed");
    }

    /// <summary>
    /// 初始化（启动事件轮询）.
    /// </summary>
    private void Run()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, typeof(MpvClient));

        if (_eventCts != null)
        {
            return;
        }

        _eventCts = new CancellationTokenSource();
        _eventLoopTask = Task.Run(() =>
        {
            while (_eventCts != null && !_eventCts.Token.IsCancellationRequested)
            {
                var eventPtr = MpvNative.WaitEvent(_handle, -1);
                var eventData = Marshal.PtrToStructure<MpvEvent>(eventPtr);
                HandleEvent(eventData);

                if (eventData.EventId == MpvEventId.Shutdown)
                {
                    _logger.LogInformation("Mpv | Shutdown event received.");
                    Shutdown?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }, _eventCts.Token);
    }

    /// <summary>
    /// 初始化 MPV 实例.
    /// </summary>
    /// <param name="options">初始化选项.</param>
    /// <returns><see cref="Task"/>.</returns>
    private async Task InitializeAsync(MpvInitializeOptions? options = null)
    {
        if (IsInitialized)
        {
            return;
        }

        var errorCode = MpvError.Success;
        await Task.Run(() =>
        {
            if (options != null)
            {
                if (options.UseConfig != null)
                {
                    errorCode = MpvNative.SetOptionString(_handle, "config", options.UseConfig.Value ? "yes" : "no");
                }

                ThrowIfFailed(errorCode, "Instance | set --config failed");

                if (!string.IsNullOrEmpty(options.ConfigDirectory))
                {
                    errorCode = MpvNative.SetOptionString(_handle, "config-dir", options.ConfigDirectory);
                }

                ThrowIfFailed(errorCode, "Instance | set --config-dir failed");

                if (!string.IsNullOrEmpty(options.InputConfigPath))
                {
                    errorCode = MpvNative.SetOptionString(_handle, "input-conf", options.InputConfigPath);
                }

                ThrowIfFailed(errorCode, "Instance | set --input-conf failed");

                if (options.LoadScripts != null)
                {
                    errorCode = MpvNative.SetOptionString(_handle, "load-scripts", options.LoadScripts.Value ? "yes" : "no");
                }

                ThrowIfFailed(errorCode, "Instance | set --load-scripts failed");

                if (!string.IsNullOrEmpty(options.ScriptPath))
                {
                    errorCode = MpvNative.SetOptionString(_handle, "script", options.ScriptPath);
                }

                ThrowIfFailed(errorCode, "Instance | set --script failed");

                if (options.PlayerOperationMode != null)
                {
                    var mode = options.PlayerOperationMode switch
                    {
                        Enums.MpvPlayerOperationMode.PseudoGui => "pseudo-gui",
                        _ => "cplayer",
                    };
                    errorCode = MpvNative.SetOptionString(_handle, "player-operation-mode", mode);
                }
            }

            errorCode = MpvNative.Initialize(_handle);
        });

        ThrowIfFailed(errorCode, "Instance | initialize failed");
        Run();
        IsInitialized = true;
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
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (_eventCts != null)
        {
            await _eventCts.CancelAsync();
            _eventCts.Dispose();
            _eventCts = null;
        }

        await Task.Run(() => MpvNative.SetCommandString(_handle, "stop"));
        await Task.Run(() => MpvNative.Destroy(_handle));
    }

    private void SendNotify(MpvClientEventId id, object? data) => DataNotify?.Invoke(this, new(id, data));
}
