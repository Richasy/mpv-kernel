// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using System.Timers;

namespace Richasy.MpvKernel.Player;

public sealed partial class MpvPlayer
{
    private async void OnStatusTimerElapsedAsync(object? sender, ElapsedEventArgs e)
        => await RefreshStatusAsync();

    private async void OnHistoryTimerElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        if (_historyResolver != null && !IsLoading && Duration > 0 && !_positionRecorded)
        {
            try
            {
                await _historyResolver.SaveHistoryAsync(Position);
                _positionRecorded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save history.");
            }
        }
    }

    private async void OnClientShutdown(object? sender, EventArgs e)
    {
        Client.Shutdown -= OnClientShutdown;
        Client.ReachFileLoaded -= OnFileLoaded;
        Client.ReachFileLoading -= OnFileLoading;
        Client.ReachFileEnd -= OnFileEnd;
        Client.DataNotify -= OnDataNotify;
        var position = Position;
        _uiContext.Post(_ =>
        {
            PlaybackState = MpvPlayerState.Idle;
            Position = 0;
            Duration = 0;
            Volume = 0;
            PlaybackRate = 0;
            IsLoading = false;
            IsFullScreen = false;
            IsCompactOverlay = false;
        }, default);

        if (_historyResolver != null)
        {
            await _historyResolver.SaveHistoryAsync(Position);
        }
    }

    private void OnFileEnd(object? sender, EventArgs e)
    {
        _uiContext.Post(_ =>
        {
            IsLoading = false;
            PlaybackState = MpvPlayerState.End;
        }, default);
    }

    private void OnDataNotify(object? sender, MpvClientNotifyEventArgs e)
    {
        switch (e.Id)
        {
            case MpvClientEventId.StateChanged:
                _uiContext.Post(_ => PlaybackState = (MpvPlayerState)e.Data, default);
                break;
            case MpvClientEventId.VolumeChanged:
                _uiContext.Post(_ => Volume = (double)e.Data, default);
                break;
            case MpvClientEventId.SpeedChanged:
                _uiContext.Post(_ => PlaybackRate = (double)e.Data, default);
                break;
            case MpvClientEventId.DurationChanged:
                _uiContext.Post(_ => Duration = (double)e.Data, default);
                break;
            case MpvClientEventId.PositionChanged:
                _positionRecorded = false;
                _uiContext.Post(_ => Position = (double)e.Data, default);
                break;
            case MpvClientEventId.FullScreenChanged:
                _uiContext.Post(_ => IsFullScreen = (bool)e.Data, default);
                break;
            case MpvClientEventId.CompactOverlayChanged:
                _uiContext.Post(_ => IsCompactOverlay = (bool)e.Data, default);
                break;
            default:
                break;
        }
    }

    private async void OnFileLoaded(object? sender, EventArgs e)
    {
        _uiContext.Post(_ => IsLoading = false, default);
        await RefreshStatusAsync();
    }

    private void OnFileLoading(object? sender, EventArgs e)
        => _uiContext.Post(_ => IsLoading = true, default);
}
