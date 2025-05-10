// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;

namespace Richasy.MpvKernel.Player;

public sealed partial class MpvPlayer
{
    private async void OnStatusTimerCallbackAsync(object? state)
        => await RefreshStatusAsync();

    private void OnClientShutdown(object? sender, EventArgs e)
    {
        Client.Shutdown -= OnClientShutdown;
        Client.ReachFileLoaded -= OnFileLoaded;
        Client.ReachFileLoading -= OnFileLoading;
        Client.ReachFileEnd -= OnFileEnd;
        Client.DataNotify -= OnDataNotify;
        PlaybackState = MpvPlayerState.Idle;
        Position = 0;
        Duration = 0;
        Volume = 0;
        PlaybackRate = 0;
        IsLoading = false;
        IsFullScreen = false;
        IsCompactOverlay = false;
    }

    private void OnFileEnd(object? sender, EventArgs e) => PlaybackState = MpvPlayerState.End;

    private void OnDataNotify(object? sender, MpvClientNotifyEventArgs e)
    {
        switch (e.Id)
        {
            case MpvClientEventId.StateChanged:
                PlaybackState = (MpvPlayerState)e.Data;
                break;
            case MpvClientEventId.VolumeChanged:
                Volume = (double)e.Data;
                break;
            case MpvClientEventId.SpeedChanged:
                PlaybackRate = (double)e.Data;
                break;
            case MpvClientEventId.DurationChanged:
                Duration = (double)e.Data;
                break;
            case MpvClientEventId.PositionChanged:
                Position = (double)e.Data;
                break;
            case MpvClientEventId.FullScreenChanged:
                IsFullScreen = (bool)e.Data;
                break;
            case MpvClientEventId.CompactOverlayChanged:
                IsCompactOverlay = (bool)e.Data;
                break;
            default:
                break;
        }
    }

    private void OnFileLoaded(object? sender, EventArgs e)
        => IsLoading = false;

    private void OnFileLoading(object? sender, EventArgs e)
        => IsLoading = true;
}
