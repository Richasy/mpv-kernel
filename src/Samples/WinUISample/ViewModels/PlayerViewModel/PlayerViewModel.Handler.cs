// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Windowing;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.WinUI;
using Richasy.MpvKernel;
using Microsoft.UI.Dispatching;

namespace WinUISample.ViewModels;

public sealed partial class PlayerViewModel
{
    private void OnFileLoaded(object? sender, EventArgs e) => _queue.TryEnqueue(() => IsFileLoading = false);

    private void OnFileLoading(object? sender, EventArgs e) => _queue.TryEnqueue(() => IsFileLoading = true);

    private void OnClientDataNotify(object? sender, MpvClientNotifyEventArgs e)
    {
        _queue.TryEnqueue(() =>
        {
            switch (e.Id)
            {
                case MpvClientEventId.StateChanged:
                    HandleStateChanged((MpvPlayerState)e.Data);
                    break;
                case MpvClientEventId.VolumeChanged:
                    var volume = (double)e.Data;
                    if (Math.Abs(volume - Volume) >= 1)
                    {
                        Volume = volume;
                    }

                    break;
                case MpvClientEventId.SpeedChanged:
                    var speed = (double)e.Data;
                    if (Math.Abs(speed - Speed) >= 0.1)
                    {
                        Speed = speed;
                        // _sourceResolver.HandleSpeedChanged(speed);
                    }

                    break;
                case MpvClientEventId.DurationChanged:
                    Duration = (double)e.Data;
                    // _sourceResolver.HandleProgressChanged(CurrentPosition, Duration);
                    break;
                case MpvClientEventId.PositionChanged:
                    CurrentPosition = (double)e.Data;
                    IsProgressChanging = false;
                    // _sourceResolver.HandleProgressChanged(CurrentPosition, Duration);
                    break;
                case MpvClientEventId.FullScreenChanged:
                    IsFullScreen = (bool)e.Data;
                    if (IsFullScreen)
                    {
                        Window?.GetWindow().SetPresenter(AppWindowPresenterKind.FullScreen);
                    }
                    else
                    {
                        Window?.GetWindow().SetPresenter(AppWindowPresenterKind.Default);
                    }
                    break;
                case MpvClientEventId.CompactOverlayChanged:
                    IsCompactOverlay = (bool)e.Data;
                    if (IsCompactOverlay)
                    {
                        Window?.GetWindow().SetPresenter(AppWindowPresenterKind.CompactOverlay);
                    }
                    else
                    {
                        Window?.GetWindow().SetPresenter(AppWindowPresenterKind.Default);
                    }
                    break;
                default:
                    break;
            }
        });
    }

    private void OnUINotify(object? sender, MpvUINotifyEventArgs e)
    {
        _queue.TryEnqueue(() =>
        {
            switch (e.Id)
            {
                case MpvUIEventId.PreviewPositionChanged:
                    IsProgressChanging = true;
                    PreviewPosition = (double)e.Data;
                    break;
                case MpvUIEventId.VolumeChanged:
                    IsVolumeChanging = true;
                    _tipTimer?.Stop();
                    _tipTimer?.Start();
                    break;
                case MpvUIEventId.PointerMoved:
                    break;
                case MpvUIEventId.Tapped:
                    IsControlVisible = !IsControlVisible;
                    break;
                default:
                    break;
            }
        });
    }

    private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
        => CloseWindow();

    private void OnTipTimerTick(DispatcherQueueTimer sender, object args)
        => IsVolumeChanging = false;

    private async void OnRequestReload(object? sender, EventArgs e)
        => await LoadMediaAsync();

    private void OnRequestClear(object? sender, EventArgs e)
    {
        MpvNative.SetCommandString(Client.Handle, "stop");
    }

    private void HandleStateChanged(MpvPlayerState state)
    {
        LastState = state;
        IsPlaying = LastState == MpvPlayerState.Playing;
        IsIdle = LastState == MpvPlayerState.Idle;
        IsRestartVisible = LastState == MpvPlayerState.End;
        CheckBackdropVisible();
    }
}
