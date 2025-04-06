// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Richasy.MpvKernel.Core;

namespace MpvKernel.WinUI;

/// <summary>
/// MPV 独立播放窗口.
/// </summary>
public sealed class MpvPlayerWindow : IAsyncDisposable
{
    private readonly MpvClient _client;
    private readonly DispatcherQueue _dispatcherQueue;

    private readonly AppWindow _topWindow;
    private readonly DesktopWindowXamlSource _xamlSource;
    private readonly Grid _rootGrid;

    /// <summary>
    /// Initializes a new instance of the player window.
    /// </summary>
    public MpvPlayerWindow(MpvClient client, DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;

        _client = client;
        _topWindow = AppWindow.Create();
        _topWindow.AssociateWithDispatcherQueue(dispatcherQueue);
        _topWindow.Destroying += OnWindowDestroying;
        _topWindow.Changed += OnWindowChanged;

        _rootGrid = new Grid();
        _rootGrid.Children.Add(new PlayerInteractiveControl(client, HandleInteractiveNotify));

        _xamlSource = new DesktopWindowXamlSource();
        _xamlSource.Initialize(_topWindow.Id);
        _xamlSource.Content = _rootGrid;

        Handle = Win32Interop.GetWindowFromWindowId(_topWindow.Id);
    }

    /// <summary>
    /// 对象是否已经被释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 窗口句柄.
    /// </summary>
    public IntPtr Handle { get; }

    /// <summary>
    /// 显示窗口.
    /// </summary>
    public void Show() => _topWindow.Show();

    /// <summary>
    /// 设置要显示的 UI 元素.
    /// </summary>
    /// <param name="element">UI 元素.</param>
    public void SetUIElement(UIElement? element)
    {
        if (_rootGrid.Children.Count > 1)
        {
            var oldElement = _rootGrid.Children[1] as IMpvUIElement;
            oldElement?.Disconnect();
            _rootGrid.Children.RemoveAt(1);
        }

        if (element is not null)
        {
            _rootGrid.Children.Add(element);
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
        if (_topWindow != null)
        {
            if (_rootGrid?.Children.Count > 1)
            {
                var oldElement = _rootGrid.Children[1] as IMpvUIElement;
                oldElement?.Disconnect();
            }

            _topWindow.Destroying -= OnWindowDestroying;
            _topWindow.Changed -= OnWindowChanged;
            _topWindow.Destroy();
        }

        _xamlSource?.Dispose();
        return _client.DisposeAsync();
    }

    private void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange || args.DidVisibilityChange)
        {
            UpdateXamlSourcePosition();
        }
    }

    private async void OnWindowDestroying(AppWindow sender, object args) => await DisposeAsync();

    private void UpdateXamlSourcePosition()
    {
        _xamlSource.SiteBridge.MoveAndResize(new Windows.Graphics.RectInt32(
            0,
            0,
            _topWindow.ClientSize.Width,
            _topWindow.ClientSize.Height));
    }

    private void HandleInteractiveNotify(MpvUIEventId id, object data)
    {
        if (_rootGrid?.Children.Count > 1)
        {
            var element = _rootGrid.Children[1] as IMpvUIElement;
            element?.HandleUINotify(id, data);
        }
    }
}
