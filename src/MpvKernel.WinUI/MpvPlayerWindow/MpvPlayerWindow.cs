// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.WinUI.Controls;

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// MPV 独立播放窗口.
/// </summary>
public sealed partial class MpvPlayerWindow : IAsyncDisposable
{
    private readonly MpvClient _client;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly DispatcherQueueTimer _autoCheckTimer;

    private readonly AppWindow _topWindow;
    private readonly DesktopWindowXamlSource _xamlSource;
    private readonly CursorGrid _rootGrid;

    /// <summary>
    /// Initializes a new instance of the player window.
    /// </summary>
    public MpvPlayerWindow(MpvClient client, DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
        _autoCheckTimer = dispatcherQueue.CreateTimer();
        _autoCheckTimer.Interval = TimeSpan.FromMilliseconds(1000);
        _autoCheckTimer.IsRepeating = true;
        _autoCheckTimer.Tick += OnAutoCheckTimerTick;

        _client = client;
        _topWindow = AppWindow.Create();
        _topWindow.AssociateWithDispatcherQueue(dispatcherQueue);
        _topWindow.Destroying += OnWindowDestroying;
        _topWindow.Changed += OnWindowChanged;

        _rootGrid = new CursorGrid();
        _rootGrid.Children.Add(new PlayerInteractivePanel(client, HandleInteractiveNotify));

        _xamlSource = new DesktopWindowXamlSource();
        _xamlSource.Initialize(_topWindow.Id);
        _xamlSource.Content = _rootGrid;

        Handle = Win32Interop.GetWindowFromWindowId(_topWindow.Id);
    }

    /// <summary>
    /// 显示窗口.
    /// </summary>
    public void Show() => _topWindow.Show();

    /// <summary>
    /// 隐藏窗口.
    /// </summary>
    public void Hide() => _topWindow.Hide();

    /// <summary>
    /// 关闭窗口.
    /// </summary>
    public void Close() => _topWindow.Destroy();

    /// <summary>
    /// 隱藏光标.
    /// </summary>
    public void HideCursor()
        => _rootGrid?.HideCursor();

    /// <summary>
    /// 显示光标.
    /// </summary>
    public void ShowCursor()
        => _rootGrid?.ShowCursor();

    /// <summary>
    /// 设置底部 UI 元素.
    /// </summary>
    /// <param name="element"></param>
    public void SetBackgroundElement(UIElement element)
    {
        if (_rootGrid.Children.Any(p => p is Grid { Name: "bkg" }))
        {
            var oldElement = _rootGrid.Children[0];
            _rootGrid.Children.RemoveAt(0);
        }

        if (element is not null)
        {
            var grid = new Grid()
            {
                Name = "bkg",
            };
            grid.Children.Add(element);
            _rootGrid.Children.Insert(0, grid);
        }
    }

    /// <summary>
    /// 设置要显示的 UI 元素.
    /// </summary>
    /// <param name="element">UI 元素.</param>
    public void SetUIElement(UIElement element)
    {
        if (_rootGrid.Children.Any(p => p is Grid { Name: "frg" }))
        {
            var oldElement = (_rootGrid.Children.Last() as Grid)!.Children[0] as IMpvUIElement;
            oldElement?.Disconnect();
            _rootGrid.Children.RemoveAt(_rootGrid.Children.Count - 1);
        }

        if (element is not null)
        {
            var grid = new Grid()
            {
                Name = "frg",
            };
            grid.Children.Add(element);
            _rootGrid.Children.Add(grid);
        }
    }

    /// <summary>
    /// 获取窗口对象.
    /// </summary>
    /// <returns><see cref="AppWindow"/>.</returns>
    public AppWindow GetWindow()
        => _topWindow;

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (IsDisposed)
        {
            return ValueTask.CompletedTask;
        }

        IsDisposed = true;
        if (_autoCheckTimer != null)
        {
            _autoCheckTimer.Tick -= OnAutoCheckTimerTick;
            _autoCheckTimer.Stop();
        }

        if (_topWindow != null)
        {
            if (_rootGrid.Children.Any(p => p is Grid { Name: "frg" }))
            {
                var oldElement = (_rootGrid.Children.Last() as Grid)!.Children[0] as IMpvUIElement;
                oldElement?.Disconnect();
            }

            _topWindow.Destroying -= OnWindowDestroying;
            _topWindow.Changed -= OnWindowChanged;
            _topWindow.Destroy();
        }

        _xamlSource?.Dispose();
        return _client.DisposeAsync();
    }

    private void UpdateXamlSourcePosition()
    {
        var size = _topWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen
            ? _topWindow.Size
            : _topWindow.ClientSize;
        _xamlSource.SiteBridge.MoveAndResize(new Windows.Graphics.RectInt32(
            0,
            0,
            size.Width,
            size.Height));
    }

    private void HandleInteractiveNotify(MpvUIEventId id, object data)
    {
        UINotify?.Invoke(this, new(id, data));
        if (_rootGrid.Children.Any(p => p is Grid { Name: "frg" }))
        {
            var element = (_rootGrid.Children.Last() as Grid)!.Children[0] as IMpvUIElement;
            element?.HandleUINotify(id, data);
        }
    }
}
