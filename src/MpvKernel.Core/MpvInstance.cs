// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.MpvKernel.Core.Models;

namespace Richasy.MpvKernel.Core;

/// <summary>
/// MPV 实例.
/// </summary>
public sealed class MpvInstance : IAsyncDisposable
{
    private readonly MpvInteropHandle _handle;
    private readonly ILogger _logger;
    private readonly Dictionary<string, MpvClient> _clients = [];

    /// <summary>
    /// 是否已初始化.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 是否已经释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private MpvInstance(MpvInteropHandle handle, ILogger? logger)
    {
        _handle = handle;
        _logger = logger ?? new NullLogger<MpvInstance>();
    }

    /// <summary>
    /// 创建一个新的 MPV 实例.
    /// </summary>
    /// <param name="options">初始化选项.</param>
    /// <param name="logger">日志记录.</param>
    /// <returns><see cref="MpvInstance"/>.</returns>
    public static async Task<MpvInstance> CreateAsync(MpvInitializeOptions? options = null, ILogger? logger = null)
    {
        var instanceHandle = MpvNative.Create();
        var instance = new MpvInstance(instanceHandle, logger);
        await instance.InitializeAsync(options).ConfigureAwait(false);
        return instance;
    }

    /// <summary>
    /// 创建一个客户端.
    /// </summary>
    /// <param name="name">客户端名称.</param>
    /// <returns><see cref="MpvClient"/>.</returns>
    public Task<MpvClient> CreateClientAsync(string name = "")
    {
        ObjectDisposedException.ThrowIf(IsDisposed, nameof(MpvInstance));
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Mpv not initialized");
        }

        if (string.IsNullOrEmpty(name))
        {
            name = Guid.NewGuid().ToString("N");
        }

        if (_clients.ContainsKey(name))
        {
            throw new InvalidOperationException("Already have a same name client");
        }

        return Task.Run(() =>
        {
            var clientHandle = MpvNative.CreateWeakClient(_handle, name);
            var c = new MpvClient(name, clientHandle, _logger);
            _clients.Add(name, c);
            return c;
        });
    }

    /// <summary>
    /// 获取已创建的客户端.
    /// </summary>
    /// <param name="name">客户端名称.</param>
    /// <returns><see cref="MpvClient"/>.</returns>
    public MpvClient? GetClient(string name)
    {
        if (_clients.TryGetValue(name, out var client))
        {
            return client;
        }

        return default;
    }

    /// <summary>
    /// 移除一个客户端.
    /// </summary>
    /// <param name="name">客户端名称.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task RemoveClientAsync(string name)
    {
        if (_clients.TryGetValue(name, out var client))
        {
            _clients.Remove(name);
            await client.DisposeAsync();
        }
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
        ObjectDisposedException.ThrowIf(IsDisposed, nameof(MpvInstance));
        IsDisposed = true;

        _logger.LogInformation("Instance disposing...");
        await Task.Run(() => MpvNative.Destroy(_handle));
        _logger.LogInformation("Instance disposed...");
    }
}
