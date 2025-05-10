// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using WinUISample.Controls;
using WinUISample.ViewModels;

namespace WinUISample.Forms;

/// <summary>
/// MPV 播放器窗口.
/// </summary>
public sealed partial class MpvPlayerWindow : IAsyncDisposable
{
    private readonly AppWindow _rootWindow;
    private readonly DesktopWindowXamlSource _xamlSource;
    private readonly CursorGrid _rootGrid;

    /// <summary>
    /// MPV 播放器窗口.
    /// </summary>
    public MpvPlayerWindow(PlayerViewModel vm)
    {
        _rootWindow = AppWindow.Create();
        _rootWindow.AssociateWithDispatcherQueue(DispatcherQueue.GetForCurrentThread());
        _rootWindow.Changed += OnWindowChanged;

        _rootWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        _rootWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        _rootWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        _rootGrid = new CursorGrid();
        var overlay = new PlayerOverlay();
        overlay.ViewModel = vm;
        _rootGrid.Children.Add(overlay);
        _xamlSource = new DesktopWindowXamlSource();
        _xamlSource.Initialize(_rootWindow.Id);
        _xamlSource.Content = _rootGrid;
    }

    /// <summary>
    /// 对象是否已经被释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 窗口句柄.
    /// </summary>
    public IntPtr Handle => Win32Interop.GetWindowFromWindowId(_rootWindow.Id);

    /// <summary>
    /// XAML 根元素.
    /// </summary>
    public XamlRoot? XamlRoot => _xamlSource?.Content?.XamlRoot;

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (IsDisposed)
        {
            return ValueTask.CompletedTask;
        }

        IsDisposed = true;

        if (_rootWindow != null)
        {
            _rootWindow.Changed -= OnWindowChanged;
            _rootWindow.Destroy();
        }

        _xamlSource?.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 显示窗口.
    /// </summary>
    public void Show()
    {
        _rootWindow.Show();
        Windows.Win32.PInvoke.SetForegroundWindow(new(Handle));
    }

    /// <summary>
    /// 关闭窗口.
    /// </summary>
    public void Close() => _rootWindow.Destroy();

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
    /// 设置主题.
    /// </summary>
    /// <param name="theme">元素主题.</param>
    public void SetTheme(ElementTheme theme)
        => _rootGrid.RequestedTheme = theme;

    /// <summary>
    /// 获取窗口对象.
    /// </summary>
    /// <returns><see cref="AppWindow"/>.</returns>
    public AppWindow GetWindow()
        => _rootWindow;

    private void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange || args.DidVisibilityChange || args.DidPresenterChange)
        {
            UpdateXamlSourcePosition();
        }
    }

    private void UpdateXamlSourcePosition()
    {
        var size = _rootWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen
            ? _rootWindow.Size
            : _rootWindow.ClientSize;
        _xamlSource?.SiteBridge?.MoveAndResize(new Windows.Graphics.RectInt32(
            0,
            0,
            size.Width,
            size.Height));
    }
}
