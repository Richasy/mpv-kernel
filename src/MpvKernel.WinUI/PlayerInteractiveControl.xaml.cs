// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Richasy.MpvKernel.Core;
using Windows.Foundation;

namespace MpvKernel.WinUI;

/// <summary>
/// 播放器交互面板.
/// </summary>
public sealed partial class PlayerInteractiveControl : UserControl
{
    private readonly MpvClient _client;
    private readonly GestureRecognizer _gestureRecognizer;
    private readonly Action<MpvUIEventId, object>? _notifyAction;

    private Point _startPoint;
    private InteractiveArea _interactiveArea;
    private double _totalDeltaX; // 用于进度控制.

    /// <summary>
    /// 初始化一个新的 <see cref="PlayerInteractiveControl"/> 实例.
    /// </summary>
    public PlayerInteractiveControl(MpvClient client, Action<MpvUIEventId, object>? notifyAction = null)
    {
        InitializeComponent();
        _notifyAction = notifyAction;
        _client = client;
        _gestureRecognizer = new GestureRecognizer
        {
            GestureSettings = GetDefaultSettings()
        };
        _gestureRecognizer.Tapped += OnRecognizerTapped;
        _gestureRecognizer.ManipulationStarted += OnRecognizerManipulationStarted;
        _gestureRecognizer.ManipulationCompleted += OnRecognizerManipulationCompleted;
        _gestureRecognizer.ManipulationUpdated += OnRecognizerManipulationUpdated;

        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);
        _startPoint = point.Position;
        var width = ActualWidth;
        var sideWidth = Math.Max(80, width / 5d);

        if (point.Position.X < sideWidth)
        {
            _interactiveArea = InteractiveArea.Left;
        }
        else if (point.Position.X > width - sideWidth)
        {
            _interactiveArea = InteractiveArea.Right;
        }
        else
        {
            _interactiveArea = InteractiveArea.Middle;
        }

        CapturePointer(e.Pointer);
        _gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(this));
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        base.OnPointerMoved(e);
        _notifyAction?.Invoke(MpvUIEventId.PointerMoved, e);
        if (PointerCaptures?.Any(p => p.PointerId == e.Pointer.PointerId) != true || e.GetIntermediatePoints(this) is null)
        {
            return;
        }

        _gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this));
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!PointerCaptures.Any(p => p.PointerId == e.Pointer.PointerId))
        {
            return;
        }

        _gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(this));
        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerCanceled(PointerRoutedEventArgs e)
    {
        base.OnPointerCanceled(e);
        _gestureRecognizer.CompleteGesture();
        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private static GestureSettings GetDefaultSettings()
    {
        return GestureSettings.ManipulationTranslateX |
            GestureSettings.ManipulationTranslateY |
            GestureSettings.Tap |
            GestureSettings.DoubleTap |
            GestureSettings.Hold |
            GestureSettings.HoldWithMouse;
    }

    private async void OnRecognizerTapped(GestureRecognizer sender, TappedEventArgs args)
    {
        if (args.TapCount == 2)
        {
            var state = await _client.GetPlayerStateAsync();
            if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Playing)
            {
                await _client.PauseAsync();
            }
            else if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Paused)
            {
                await _client.ResumeAsync();
            }

            var newState = await _client.GetPlayerStateAsync();
        }
    }

    private void OnRecognizerManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        => _startPoint = args.Position;

    private async void OnRecognizerManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
    {
        var deltaX = args.Position.X - _startPoint.X;
        var deltaY = args.Position.Y - _startPoint.Y;
        _totalDeltaX += deltaX;
        switch (_interactiveArea)
        {
            case InteractiveArea.Left:
                {
                    // 左侧调整亮度之类的.
                }
                break;
            case InteractiveArea.Middle:
                {
                    // 中间区域调整播放进度.
                    if (Math.Abs(deltaX) > 2 && Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        var newPos = await GetNewPositionAsync();
                        if (newPos != null)
                        {
                            _notifyAction?.Invoke(MpvUIEventId.PreviewPositionChanged, newPos.Value);
                        }
                    }
                }

                break;
            case InteractiveArea.Right:
                {
                    // 只处理纵向滑动.
                    if (Math.Abs(deltaY) > 5 && Math.Abs(deltaY) > Math.Abs(deltaX))
                    {
                        // 调整音量（每10像素移动改变1音量）
                        var volumeChange = -deltaY / 10;
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            var currentVolume = await _client.GetVolumeAsync();
                            var newVolume = Math.Max(0, Math.Min(100, currentVolume + volumeChange));
                            _notifyAction?.Invoke(MpvUIEventId.VolumeChanged, newVolume);
                            await _client.SetVolumeAsync(newVolume);
                        });
                    }
                }

                break;
            default:
                break;
        }

        _startPoint = args.Position;
    }

    private async void OnRecognizerManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
    {
        if (_interactiveArea == InteractiveArea.Middle && Math.Abs(_totalDeltaX) > 10)
        {
            // 处理进度变化.
            var newPos = await GetNewPositionAsync();
            if (newPos.HasValue)
            {
                await _client.SetCurrentPositionAsync(newPos.Value);
            }
        }

        _startPoint = new(0, 0);
        _totalDeltaX = 0;
        _interactiveArea = InteractiveArea.None;
    }

    private async Task<double?> GetNewPositionAsync()
    {
        var state = await _client.GetPlayerStateAsync();
        var isValidState = state is Richasy.MpvKernel.Core.Enums.MpvPlayerState.Playing or Richasy.MpvKernel.Core.Enums.MpvPlayerState.Paused;
        if (isValidState)
        {
            var duration = await _client.GetDurationAsync();
            var currentPosition = await _client.GetCurrentPositionAsync();
            var newPosition = currentPosition + (_totalDeltaX / ActualWidth / 2 * duration);
            if (newPosition < 0)
            {
                newPosition = 0;
            }
            else if (newPosition > duration)
            {
                newPosition = duration - 0.1;
            }

            return newPosition;
        }

        return default;
    }
}
