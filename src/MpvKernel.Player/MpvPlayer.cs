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

        // 内部维护一个定时器用于刷新播放器状态.
        _statusTimer = new Timer(OnStatusTimerCallbackAsync, default, Timeout.Infinite, 5000);

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get media source.");
            throw;
        }

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
            PlaybackState = state.Value;
        }

        var duration = await Client.GetDurationAsync();
        if (duration.IsSuccess)
        {
            Duration = duration.Value;
        }

        var position = await Client.GetCurrentPositionAsync();
        if (position.IsSuccess)
        {
            Position = position.Value;
        }

        var volume = await Client.GetVolumeAsync();
        if (volume.IsSuccess)
        {
            Volume = volume.Value;
        }

        var rate = await Client.GetSpeedAsync();
        if (rate.IsSuccess)
        {
            PlaybackRate = rate.Value;
        }

        var isFullScreen = await Client.GetFullScreenStateAsync();
        if (isFullScreen.IsSuccess)
        {
            IsFullScreen = isFullScreen.Value;
        }

        var isCompactOverlay = await Client.GetCompactOverlayStateAsync();
        if (isCompactOverlay.IsSuccess)
        {
            IsCompactOverlay = isCompactOverlay.Value;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _statusTimer.DisposeAsync();
        await Client.DisposeAsync();
    }

    partial void OnPlaybackStateChanged(MpvPlayerState value)
        => CheckStateProperties();

    partial void OnPositionChanged(double value)
        => TryShowSubtitle();
}
