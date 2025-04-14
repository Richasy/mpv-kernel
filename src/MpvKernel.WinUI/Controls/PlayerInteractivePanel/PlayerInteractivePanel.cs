// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Richasy.MpvKernel.Core;
using Windows.Foundation;

namespace Richasy.MpvKernel.WinUI.Controls;

/// <summary>
/// 播放器交互面板.
/// </summary>
public sealed partial class PlayerInteractivePanel : Control
{
    private readonly MpvClient _client;
    private readonly Action<MpvUIEventId, object>? _notifyAction;
    private readonly DispatcherTimer _tapTimer;

    private Point _startPoint;
    private InteractiveArea _interactiveArea;
    private double _totalDeltaX; // 用于进度控制.
    private int _tapCount;
    private bool _isManipulating;

    /// <summary>
    /// 初始化一个新的 <see cref="PlayerInteractivePanel"/> 实例.
    /// </summary>
    public PlayerInteractivePanel(MpvClient client, Action<MpvUIEventId, object>? notifyAction = null)
    {
        DefaultStyleKey = typeof(PlayerInteractivePanel);
        _notifyAction = notifyAction;
        _client = client;

        _tapTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300),
        };
        _tapTimer.Tick += OnTapTimerTick;

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _startPoint = point.Position;
        var width = ActualWidth;
        var sideWidth = Math.Max(80, width / 5d);

        if (point.Position.X > width - sideWidth)
        {
            _interactiveArea = InteractiveArea.Aside;
        }
        else
        {
            _interactiveArea = InteractiveArea.Main;
        }

        CapturePointer(e.Pointer);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        _notifyAction?.Invoke(MpvUIEventId.PointerMoved, e);
        if (PointerCaptures?.Any(p => p.PointerId == e.Pointer.PointerId) != true)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        var deltaX = currentPoint.Position.X - _startPoint.X;
        var deltaY = currentPoint.Position.Y - _startPoint.Y;

        if (!_isManipulating && (Math.Abs(deltaX) > 5 || Math.Abs(deltaY) > 5))
        {
            _isManipulating = true;
            _tapTimer.Stop();
            _tapCount = 0;
        }

        if (_isManipulating)
        {
            _totalDeltaX += deltaX;
            HandleManipulationUpdate(deltaX, deltaY);
            _startPoint = currentPoint.Position;
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        if (PointerCaptures?.Any(p => p.PointerId == e.Pointer.PointerId) != true)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        var deltaX = currentPoint.Position.X - _startPoint.X;
        var deltaY = currentPoint.Position.Y - _startPoint.Y;

        if (!_isManipulating)
        {
            // 处理点击/双击
            _tapCount++;
            if (_tapCount == 1)
            {
                _tapTimer.Start();
            }
        }
        else
        {
            // 处理操作完成
            HandleManipulationCompleted();
        }

        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerCanceled(PointerRoutedEventArgs e)
    {
        base.OnPointerCanceled(e);
        _tapTimer.Stop();
        _tapCount = 0;
        _isManipulating = false;
        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private async void OnTapTimerTick(object? sender, object? e)
    {
        _tapTimer.Stop();
        if (_tapCount == 2)
        {
            // 处理双击
            var stateResult = await _client.GetPlayerStateAsync();
            if(stateResult.IsSuccess)
            {
                var state = stateResult.Value;
                if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Playing)
                {
                    await _client.PauseAsync();
                }
                else if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Paused)
                {
                    await _client.ResumeAsync();
                }
            }
        }
        else if (_tapCount == 1)
        {
            _notifyAction?.Invoke(MpvUIEventId.Tapped, default);
        }

        _tapCount = 0;
    }

    private async void HandleManipulationUpdate(double deltaX, double deltaY)
    {
        switch (_interactiveArea)
        {
            case InteractiveArea.Main:
                {
                    // 中间区域调整播放进度.
                    if (Math.Abs(deltaX) > 2 && Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        await _client.PauseAsync();
                        var newPos = await GetNewPositionAsync();
                        if (newPos != null)
                        {
                            _notifyAction?.Invoke(MpvUIEventId.PreviewPositionChanged, newPos.Value);
                        }
                    }
                }

                break;
            case InteractiveArea.Aside:
                {
                    // 只处理纵向滑动.
                    if (Math.Abs(deltaY) > 5 && Math.Abs(deltaY) > Math.Abs(deltaX))
                    {
                        // 调整音量（每10像素移动改变1音量）
                        var volumeChange = -deltaY / 10;
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            var currentVolumeResult = await _client.GetVolumeAsync();
                            if(currentVolumeResult.IsSuccess)
                            {
                                var newVolume = Math.Max(0, Math.Min(100, currentVolumeResult.Value + volumeChange));
                                _notifyAction?.Invoke(MpvUIEventId.VolumeChanged, newVolume);
                                await _client.SetVolumeAsync(newVolume);
                            }
                        });
                    }
                }

                break;
            default:
                break;
        }
    }

    private async void HandleManipulationCompleted()
    {
        if (_interactiveArea == InteractiveArea.Main && Math.Abs(_totalDeltaX) > 10)
        {
            // 处理进度变化.
            var newPos = await GetNewPositionAsync();
            if (newPos.HasValue)
            {
                await _client.SetCurrentPositionAsync(newPos.Value);
                await _client.ResumeAsync();
            }
        }

        _startPoint = new(0, 0);
        _totalDeltaX = 0;
        _interactiveArea = InteractiveArea.None;
        _isManipulating = false;
    }

    private async Task<double?> GetNewPositionAsync()
    {
        var stateResult = await _client.GetPlayerStateAsync();
        if(stateResult.IsFailed)
        {
            return null;
        }

        var state = stateResult.Value;
        var isValidState = state is Richasy.MpvKernel.Core.Enums.MpvPlayerState.Playing or Richasy.MpvKernel.Core.Enums.MpvPlayerState.Paused;
        if (isValidState)
        {
            var durationResult = await _client.GetDurationAsync();
            var currentPositionResult = await _client.GetCurrentPositionAsync();
            if(durationResult.IsFailed || currentPositionResult.IsFailed)
            {
                return default;
            }

            var duration = durationResult.Value;
            var currentPosition = currentPositionResult.Value;
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
