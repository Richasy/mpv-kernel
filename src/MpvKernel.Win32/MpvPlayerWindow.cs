// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;
using System.ComponentModel;
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

    private readonly MpvClient _client;
    private readonly string _className;
    private static uint _classCounter;

    private readonly WNDPROC _wndProc;

    private HWND _windowHandle;
    private float _currentDpiScale = 1.0f;

    /// <summary>
    /// Initializes a new instance of the player window with a specified client for media control.
    /// </summary>
    /// <param name="client">The parameter provides the necessary interface for interacting with the media player.</param>
    public MpvPlayerWindow(MpvClient client)
    {
        _client = client;
        _client.Shutdown += OnClientShutdown;
        _className = $"MpvPlayerWindowClass_{++_classCounter}";
        _wndProc = WindowProc;

        RegisterWindowClass();
        CreateWindow();
        UpdateDpiScale(); // 初始化DPI缩放比例
    }

    /// <summary>
    /// 窗口句柄.
    /// </summary>
    public IntPtr Handle => _windowHandle;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.RemoveWindowSubclass(_windowHandle, DpiChangedSubclassProc, 0);
            PInvoke.DestroyWindow(_windowHandle);
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
        var width = ScaleToDpi(800);
        var height = ScaleToDpi(600);
        unsafe
        {
            _windowHandle = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_APPWINDOW,
                _className,
                _windowName,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                0,
                0,
                width,
                height,
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
    }
}
