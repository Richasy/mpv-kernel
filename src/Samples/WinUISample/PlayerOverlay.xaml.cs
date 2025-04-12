// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls.Primitives;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.WinUI;
using Richasy.WinUIKernel.Share.Base;

namespace WinUISample;

/// <summary>
/// 播放器覆盖层.
/// </summary>
public sealed partial class PlayerOverlay : LayoutUserControlBase, IMpvUIElement
{
    private readonly MpvClient _client;
    private readonly Action<bool> _toggleFullScreenAction;
    private readonly Action<bool> _toggleCompactOverlayAction;
    private double _lastPosition;
    private double _lastDuration;

    /// <summary>
    /// 初始化一个 <see cref="PlayerOverlay"/> 类的新实例.
    /// </summary>
    public PlayerOverlay(MpvClient client, Action<bool> toggleFullScreenAction, Action<bool> toggleCompactOverlayAction)
    {
        InitializeComponent();
        _client = client;
        _toggleFullScreenAction = toggleFullScreenAction;
        _toggleCompactOverlayAction = toggleCompactOverlayAction;
    }

    /// <inheritdoc/>
    public void Disconnect()
    {
        _client.ReachFileLoading -= OnFileLoading;
        _client.ReachFileLoaded -= OnFileLoaded;
        _client.ReachFileEnd -= OnFileEnd;
        _client.DataNotify -= OnDataNotify;
    }

    /// <inheritdoc/>
    public void HandleUINotify(MpvUIEventId id, object data)
    {
        if (id == MpvUIEventId.PreviewPositionChanged)
        {
            ProgressSlider.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ProgressSliderFake.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            ProgressSliderFake.Value = (double)(double)data;
        }
        else if (id == MpvUIEventId.Tapped)
        {
            PlayerControlPanel.Visibility = PlayerControlPanel.Visibility == Microsoft.UI.Xaml.Visibility.Visible ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    /// <inheritdoc/>
    protected override void OnControlLoaded()
    {
        _client.ReachFileLoading += OnFileLoading;
        _client.ReachFileLoaded += OnFileLoaded;
        _client.ReachFileEnd += OnFileEnd;
        _client.DataNotify += OnDataNotify;
    }

    /// <inheritdoc/>
    protected override void OnControlUnloaded()
        => Disconnect();

    private void OnDataNotify(object? sender, MpvClientNotifyEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Id)
            {
                case MpvClientEventId.StateChanged:
                    HandlePlayerStateChanged((MpvPlayerState)e.Data);
                    break;
                case MpvClientEventId.VolumeChanged:
                    {
                        var volume = (double)e.Data;
                        if (Math.Abs(volume - VolumeSlider.Value) > 1)
                        {
                            VolumeSlider.Value = volume;
                        }
                    }
                    break;
                case MpvClientEventId.DurationChanged:
                    {
                        var duration = (double)e.Data;
                        _lastDuration = duration;
                        DurationBlock.Text = TimeSpan.FromSeconds(duration).ToString(@"hh\:mm\:ss");
                        ProgressSlider.Maximum = duration;
                        ProgressSliderFake.Maximum = duration;
                        PlayProgress.Maximum = duration;
                    }
                    break;
                case MpvClientEventId.PositionChanged:
                    {
                        var position = (double)e.Data;
                        _lastPosition = position;
                        ProgressSlider.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        ProgressSliderFake.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        PositionBlock.Text = TimeSpan.FromSeconds(position).ToString(@"hh\:mm\:ss");
                        PlayProgress.Value = position;
                        if (Math.Abs(position - ProgressSlider.Value) > 1)
                        {
                            ProgressSlider.Value = position;
                        }
                    }
                    break;
                case MpvClientEventId.FullScreenChanged:
                    FullScreenIcon.Symbol = (bool)e.Data ? FluentIcons.Common.Symbol.FullScreenMinimize : FluentIcons.Common.Symbol.FullScreenMaximize;
                    _toggleFullScreenAction?.Invoke((bool)e.Data);
                    break;
                case MpvClientEventId.CompactOverlayChanged:
                    CompactOverlayIcon.Symbol = (bool)e.Data ? FluentIcons.Common.Symbol.ContractDownLeft : FluentIcons.Common.Symbol.ContractUpRight;
                    _toggleCompactOverlayAction?.Invoke((bool)e.Data);
                    break;
                default:
                    break;
            }
        });
    }
    private void OnFileEnd(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            FileLoadingWidget.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            OverlayRect.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PlayerControlPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ReplayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        });
    }

    private void OnFileLoaded(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            FileLoadingWidget.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            OverlayRect.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PlayerControlPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            var volumeResult = await _client.GetVolumeAsync();
            if (volumeResult.IsSuccess)
            {
                VolumeSlider.Value = volumeResult.Value;
            }
        });
    }

    private void OnFileLoading(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            FileLoadingWidget.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            OverlayRect.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PlayerControlPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        });
    }

    private void HandlePlayerStateChanged(MpvPlayerState state)
    {
        if (state == MpvPlayerState.Idle)
        {
            FileLoadingWidget.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            OverlayRect.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PlayerControlPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            OverlayRect.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PlayerControlPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            FileLoadingWidget.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ReplayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            if (state == MpvPlayerState.Playing)
            {
                PlayPauseIcon.Symbol = FluentIcons.Common.Symbol.Pause;
                PlayPauseButton.IsEnabled = true;
                PlayPauseRing.IsActive = false;
            }
            else if (state is MpvPlayerState.Paused or MpvPlayerState.End)
            {
                PlayPauseIcon.Symbol = FluentIcons.Common.Symbol.Play;
                PlayPauseButton.IsEnabled = true;
                PlayPauseRing.IsActive = false;
                if (state == MpvPlayerState.End)
                {
                    ReplayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }
            else if (state is MpvPlayerState.Buffering or MpvPlayerState.Seeking)
            {
                PlayPauseButton.IsEnabled = false;
                PlayPauseRing.IsActive = true;
            }
        }
    }

    private void OnVolumeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var newValue = e.NewValue;
        DispatcherQueue.TryEnqueue(async () =>
        {
            var currentVolumeResult = await _client.GetVolumeAsync();
            if (currentVolumeResult.IsFailed || Math.Abs(newValue - currentVolumeResult.Value) < 1)
            {
                return;
            }

            await _client.SetVolumeAsync(newValue);
        });
    }

    private async void OnPlayPauseButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var stateResult = await _client.GetPlayerStateAsync();
        if (stateResult.IsFailed)
        {
            return;
        }

        var state = stateResult.Value;
        var isValidState = state is MpvPlayerState.Playing or MpvPlayerState.Paused;
        if (isValidState)
        {
            if (state == MpvPlayerState.Playing)
            {
                await _client.PauseAsync();
            }
            else
            {
                await _client.ResumeAsync();
            }
        }
    }

    private async void OnForwardButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var newPos = Math.Min(_lastPosition + 30, _lastDuration - 0.1);
        await _client.SetCurrentPositionAsync(newPos);
    }

    private async void OnBackwardButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var newPos = Math.Max(_lastPosition - 10, 0);
        await _client.SetCurrentPositionAsync(newPos);
    }

    private async void OnReplayButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => await _client.ReplayAsync();

    private async void OnProgressValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var v = e.NewValue;
        if (Math.Abs(v - _lastPosition) > 1.5)
        {
            await _client.SetCurrentPositionAsync(v);
        }
    }

    private async void OnFullScreenButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var isFullScreenResult = await _client.GetFullScreenStateAsync();
        if (isFullScreenResult.IsFailed)
        {
            return;
        }

        await _client.SetFullScreenStateAsync(!isFullScreenResult.Value);
    }

    private async void OnCompactOverlayButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var isCompactOverlayResult = await _client.GetCompactOverlayStateAsync();
        if (isCompactOverlayResult.IsFailed)
        {
            return;
        }

        await _client.SetCompactOverlayStateAsync(!isCompactOverlayResult.Value);
    }
}
