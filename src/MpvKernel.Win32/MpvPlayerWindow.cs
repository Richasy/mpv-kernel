// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Richasy.MpvKernel.Win32;

/// <summary>
/// MPV 播放器窗口.
/// </summary>
public partial class MpvPlayerWindow : IAsyncDisposable
{
    private const string _windowName = "MpvPlayerWindow";

    private static uint _classCounter;
    private readonly MpvClient _client;
    private readonly string _className;
    private readonly Rectangle _initRect;

    private readonly WNDPROC _wndProc;

    private HWND _windowHandle;
    private CustomTitleBarWindow _customTitleBar;

    /// <summary>
    /// Initializes a new instance of the player window with a specified client for media control.
    /// </summary>
    public MpvPlayerWindow(MpvClient client, Rectangle initRect)
    {
        _client = client;
        _initRect = initRect;
        _client.Shutdown += OnClientShutdown;
        _className = $"MpvPlayerWindowClass_{++_classCounter}";
        _wndProc = WindowProc;

        RegisterWindowClass();
        CreateWindow();
        Util.UpdateDpiScale(_windowHandle); // 初始化DPI缩放比例
    }

    /// <summary>
    /// 窗口句柄.
    /// </summary>
    public IntPtr Handle => _windowHandle;

    /// <summary>
    /// 标题栏元素.
    /// </summary>
    public IMpvTitleBarElement? TitleBarElement
    {
        get => field;
        set
        {
            field = value;
            _customTitleBar?.SetElement(TitleBarElement);
        }
    }

    /// <summary>
    /// 覆盖客户区的UI元素.
    /// </summary>
    public IMpvUIElement? ClientElement { get; set; }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.RemoveWindowSubclass(_windowHandle, DpiChangedSubclassProc, 0);
            _customTitleBar?.Dispose();
            PInvoke.DestroyWindow(_windowHandle);
            TitleBarElement?.Dispose();
            _windowHandle = default;
        }

        if (_client != null)
        {
            _client.Shutdown -= OnClientShutdown;
            await _client.DisposeAsync();
        }

        PInvoke.UnregisterClass(_className, default);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 初始化.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task InitializeAsync(string filePath)
    {
        var options = new MpvPlayOptions
        {
            WindowHandle = _windowHandle,
        };

        await _client.PlayAsync(filePath, options);
        PInvoke.SetWindowPos(_windowHandle, default, _initRect.X, _initRect.Y, Util.Pt2Pix(_initRect.Width), Util.Pt2Pix(_initRect.Height), SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
        Activate();
    }

    /// <summary>
    /// Activates a window if its handle is valid. It shows the window normally and brings it to the foreground.
    /// </summary>
    public void Activate()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.ShowWindow(_windowHandle, SHOW_WINDOW_CMD.SW_SHOWNORMAL);
            PInvoke.SetForegroundWindow(_windowHandle);
        }
    }

    /// <summary>
    /// Hide the window if its handle is valid. It hides the window from the screen.
    /// </summary>
    public void Hide()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.ShowWindow(_windowHandle, SHOW_WINDOW_CMD.SW_HIDE);
        }
    }

    /// <summary>
    /// Closes the window associated with the given handle if it is valid. Sends a close message to the window.
    /// </summary>
    public void Close()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.SendMessage(_windowHandle, PInvoke.WM_CLOSE, default, default);
        }
    }

    private void RegisterWindowClass()
    {
        unsafe
        {
            fixed (char* className = _className)
            {
                var wndClassEx = new WNDCLASSEXW
                {
                    cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                    style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                    lpfnWndProc = _wndProc,
                    hCursor = PInvoke.LoadCursor(default, PInvoke.IDC_ARROW),
                    lpszClassName = new PCWSTR(className),
                    hbrBackground = new Windows.Win32.Graphics.Gdi.HBRUSH(PInvoke.GetStockObject(Windows.Win32.Graphics.Gdi.GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH).Value),
                };

                if (PInvoke.RegisterClassEx(wndClassEx) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }
    }

    private void CreateWindow()
    {
        unsafe
        {
            _windowHandle = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_APPWINDOW,
                _className,
                _windowName,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                _initRect.X,
                _initRect.Y,
                Util.Pt2Pix(_initRect.Width),
                Util.Pt2Pix(_initRect.Height),
                default,
                default,
                default,
                default);
        }

        if (_windowHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // 监听DPI变化
        PInvoke.SetWindowSubclass(_windowHandle, DpiChangedSubclassProc, 0, 0);
        _customTitleBar = new CustomTitleBarWindow(_windowHandle);
        UpdateTitleBarRegion();
    }

    private void UpdateTitleBarRegion()
    {
        PInvoke.GetClientRect(_windowHandle, out var clientRect);
        var dragRegion = new RECT
        {
            left = 0,
            top = 0,
            right = clientRect.right - clientRect.left,
            bottom = Util.Pt2Pix(32) // 32像素高的标题栏区域
        };
        _customTitleBar.SetDragRegion(dragRegion);
    }
}
