// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Player;

/// <summary>
/// MPV 播放器.
/// </summary>
public sealed partial class MpvPlayer : ObservableObject, IAsyncDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MpvPlayer"/> class.
    /// </summary>
    public MpvPlayer(
        MpvClient client,
        IMpvMediaSourceResolver sourceResolver,
        IMpvMediaSubtitleResolver? subtitleResolver = null,
        ILogger? logger = null)
    {
        Client = client;
        _sourceResolver = sourceResolver;
        _subtitleResolver = subtitleResolver;
        _logger = logger ?? NullLogger.Instance;
        _uiContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must be created on UI thread.");

        // 内部维护一个定时器用于刷新播放器状态.
        _statusTimer = new System.Timers.Timer(5000);
        _statusTimer.Elapsed += OnStatusTimerElapsedAsync;
        PlaybackState = MpvPlayerState.Idle;
        Client.DataNotify += OnDataNotify;
        Client.ReachFileLoading += OnFileLoading;
        Client.ReachFileLoaded += OnFileLoaded;
        Client.ReachFileEnd += OnFileEnd;
        Client.Shutdown += OnClientShutdown;
    }

    /// <summary>
    /// 初始化播放器.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task InitializeAsync(bool alsoPlay = true)
    {
        if (_sourceResolver is null)
        {
            throw new InvalidOperationException("Source resolver is not set.");
        }

        try
        {
            _cachedSource = await _sourceResolver.GetSourceAsync();
            Title = _cachedSource.Title;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get media source.");
            throw;
        }

        _statusTimer.Start();
        if (alsoPlay)
        {
            CheckStateProperties();
            await Client.PlayAsync(_cachedSource.Url, _cachedSource.Options);
        }
    }

    /// <summary>
    /// 刷新播放器状态.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task RefreshStatusAsync()
    {
        if (!Client.IsInitialized || Client.IsDisposed)
        {
            return;
        }

        var state = await Client.GetPlayerStateAsync();
        if (state.IsSuccess)
        {
            _uiContext.Post(_ => PlaybackState = state.Value, null);
        }

        var duration = await Client.GetDurationAsync();
        if (duration.IsSuccess)
        {
            _uiContext.Post(_ => Duration = duration.Value, null);
        }

        var position = await Client.GetCurrentPositionAsync();
        if (position.IsSuccess)
        {
            _uiContext.Post(_ => Position = position.Value, null);
        }

        var volume = await Client.GetVolumeAsync();
        if (volume.IsSuccess)
        {
            _uiContext.Post(_ => Volume = volume.Value, null);
        }

        var rate = await Client.GetSpeedAsync();
        if (rate.IsSuccess)
        {
            _uiContext.Post(_ => PlaybackRate = rate.Value, null);
        }

        var isFullScreen = await Client.GetFullScreenStateAsync();
        if (isFullScreen.IsSuccess)
        {
            _uiContext.Post(_ => IsFullScreen = isFullScreen.Value, null);
        }

        var isCompactOverlay = await Client.GetCompactOverlayStateAsync();
        if (isCompactOverlay.IsSuccess)
        {
            _uiContext.Post(_ => IsCompactOverlay = isCompactOverlay.Value, null);
        }
    }

    /// <summary>
    /// 重新播放当前媒体.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task ReplayAsync()
    {
        try
        {
            _cachedSource = await _sourceResolver.GetSourceAsync();
            Title = _cachedSource.Title;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get media source.");
            throw;
        }

        await Client.PlayAsync(_cachedSource.Url, _cachedSource.Options);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _statusTimer.Elapsed -= OnStatusTimerElapsedAsync;
        _statusTimer.Stop();
        _statusTimer.Dispose();
        await Client.DisposeAsync();
    }

    partial void OnPlaybackStateChanged(MpvPlayerState value)
        => CheckStateProperties();

    partial void OnPositionChanged(double value)
        => TryShowSubtitle();
}
