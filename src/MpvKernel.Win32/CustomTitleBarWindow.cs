// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Richasy.MpvKernel.Win32;

/// <summary>
/// 自定义标题栏窗口.
/// </summary>
internal sealed class CustomTitleBarWindow : IDisposable
{
    private static uint _classCounter;
    private readonly string _glassClassName;
    private readonly HWND _mainWindow;
    private HWND _glassWindow;
    private IMpvTitleBarElement? _element;
    private bool _isActive;
    private RECT _dragRegion;
    private bool _enabledDrag = true;

    // 常量定义
    private const int topBorderVisibleHeight = 1;

    /// <summary>
    /// Initialize a new instance of <see cref="CustomTitleBarWindow"/>.
    /// </summary>
    /// <param name="mainWindow"></param>
    public CustomTitleBarWindow(IntPtr mainWindow)
    {
        _mainWindow = new(mainWindow);
        _glassClassName = $"GlassWindowClass{++_classCounter}";
        Initialize();
    }

    /// <summary>
    /// 设置是否激活.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active)
    {
        if (_isActive == active) return;

        _isActive = active;

        if (active)
        {
            // 显示窗口并更新位置
            UpdateWindowPositions();
            PInvoke.ShowWindow(_glassWindow, SHOW_WINDOW_CMD.SW_SHOW);
        }
        else
        {
            // 隐藏窗口
            PInvoke.ShowWindow(_glassWindow, SHOW_WINDOW_CMD.SW_HIDE);
        }
    }

    public void SetElement(IMpvTitleBarElement element)
    {
        _element = element;
        UpdateWindowPositions();
    }

    /// <summary>
    /// 更新窗口位置.
    /// </summary>
    public void UpdateWindowPositions()
    {
        if (!_isActive) return;

        PInvoke.GetClientRect(_mainWindow, out var clientRect);
        var point = new Point(clientRect.left, clientRect.top);
        PInvoke.ClientToScreen(_mainWindow, ref point);
        clientRect.left = point.X;
        clientRect.top = point.Y;

        // 更新玻璃窗口位置和大小
        PInvoke.SetWindowPos(
            _glassWindow,
            new HWND(-1),
            clientRect.left,
            clientRect.top,
            clientRect.right - clientRect.left,
            _dragRegion.Height,
            SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);

        // 将 element hwnd 设为置顶，并使其大小填满整个标题栏区域.
        if (_element != null)
        {
            _element.SetPosition(0, 0, _dragRegion.Width, _dragRegion.Height);
            var windowRegion = PInvoke.CreateRectRgn(0, 0, _dragRegion.Width, _dragRegion.Height);
            foreach (var area in _element.GetInteractiveAreas())
            {
                var rgn = PInvoke.CreateRectRgn(
                    Util.Pt2Pix(area.X),
                    Util.Pt2Pix(area.Y),
                    Util.Pt2Pix(area.Right),
                    Util.Pt2Pix(area.Bottom));
                PInvoke.CombineRgn(windowRegion, windowRegion, rgn, RGN_COMBINE_MODE.RGN_DIFF);
            }

            _ = PInvoke.SetWindowRgn(_glassWindow, windowRegion, false);
        }
    }

    /// <summary>
    /// 设置可拖拽区域.
    /// </summary>
    public void SetDragRegion(Rectangle region)
    {
        _dragRegion = region;
        UpdateWindowPositions();
    }

    /// <summary>
    /// 设置是否允许拖拽.
    /// </summary>
    /// <param name="enabled"></param>
    public void SetDragEnabled(bool enabled) => _enabledDrag = enabled;

    /// <inheritdoc/>
#pragma warning disable CA1063 // 正确实现 IDisposable
    public void Dispose()
#pragma warning restore CA1063 // 正确实现 IDisposable
    {
        PInvoke.DestroyWindow(_glassWindow);
        PInvoke.UnregisterClass(_glassClassName, default);
        GC.SuppressFinalize(this);
    }

    internal bool HandleMessage(uint uMsg, WPARAM wParam, LPARAM lParam, out LRESULT result)
    {
        result = default;

        if (!_isActive)
        {
            return false;
        }

        switch (uMsg)
        {
            case PInvoke.WM_NCCALCSIZE:
                // 处理非客户区计算，扩展客户区到标题栏区域
                if (wParam != 0) // wParam为TRUE表示计算有效客户区
                {
                    var ncCalcSizeParams = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);
                    var originalRect = ncCalcSizeParams.rgrc._0;

                    // 保存原始窗口矩形
                    ncCalcSizeParams.rgrc._2 = originalRect;

                    // 扩展客户区到覆盖标题栏
                    ncCalcSizeParams.rgrc._0.top = originalRect.top + GetTopBorderHeight();

                    Marshal.StructureToPtr(ncCalcSizeParams, lParam, false);

                    result = new LRESULT((int)PInvoke.WVR_REDRAW);
                    return true;
                }
                break;

            case PInvoke.WM_NCHITTEST:
                // 处理命中测试，确保拖动和按钮正常工作
                var hitTestResult = HandleNcHitTest(lParam);
                if (hitTestResult != PInvoke.HTCLIENT)
                {
                    result = new LRESULT((int)hitTestResult);
                    return true;
                }

                break;

            case PInvoke.WM_DWMCOMPOSITIONCHANGED:
                // DWM组合状态变化时重新计算标题栏
                UpdateWindowPositions();
                result = new LRESULT(0);
                return true;
        }

        return false;
    }

    private void Initialize()
    {
        // 1. 隐藏系统标题栏
        ExtendClientArea();

        // 2. 创建透明玻璃窗口
        CreateGlassWindow();

        SetActive(true);
    }

    private void ExtendClientArea()
    {
        // 发送 WM_NCCALCSIZE 消息来扩展客户区
        PInvoke.GetWindowRect(_mainWindow, out var windowRect);
        var ncCalcSizeParams = new NCCALCSIZE_PARAMS
        {
            rgrc = new RECT[3]
            {
                windowRect, // 新客户区
                windowRect, // 旧客户区
                windowRect  // 旧窗口区
            }
        };

        var lparam = Marshal.AllocHGlobal(Marshal.SizeOf(ncCalcSizeParams));
        Marshal.StructureToPtr(ncCalcSizeParams, lparam, false);
        PInvoke.SendMessage(_mainWindow, PInvoke.WM_NCCALCSIZE, (WPARAM)1, lparam);
        Marshal.FreeHGlobal(lparam);

        // 更新窗口位置以应用更改
        PInvoke.SetWindowPos(
            _mainWindow,
            HWND.Null,
            windowRect.left,
            windowRect.top,
            windowRect.right - windowRect.left,
            windowRect.bottom - windowRect.top,
            SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }

    private void CreateGlassWindow()
    {
        unsafe
        {
            fixed (char* className = _glassClassName)
            {
                // 注册窗口类
                var wndClass = new WNDCLASSEXW
                {
                    cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                    style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                    lpfnWndProc = GlassWindowProc,
                    hInstance = default,
                    hCursor = PInvoke.LoadCursor(default, PInvoke.IDC_ARROW),
                    hbrBackground = new(PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH).Value),
                    lpszClassName = className
                };

                PInvoke.RegisterClassEx(wndClass);
            }

            // 创建窗口
            _glassWindow = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TRANSPARENT | WINDOW_EX_STYLE.WS_EX_NOACTIVATE,
                _glassClassName,
                "Glass Window",
                WINDOW_STYLE.WS_POPUP,
                0, 0, 0, 0,
                _mainWindow,
                null,
                PInvoke.GetModuleHandle(null),
                null);

            // 设置窗口为透明
            PInvoke.SetLayeredWindowAttributes(
                _glassWindow,
                default,
                0,
                LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);
        }
    }

    private LRESULT GlassWindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        if (HandleMessage(uMsg, wParam, lParam, out var result))
        {
            return result;
        }

        return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
    }

    private LRESULT HandleNcHitTest(LPARAM lParam)
    {
        var point = new Point(GET_X_LPARAM(lParam.Value), GET_Y_LPARAM(lParam));
        PInvoke.ScreenToClient(_mainWindow, ref point);
        // 1. 在拖动区域检查
        if (_enabledDrag && IsPointInDragRegion(point))
            return new((int)PInvoke.HTCAPTION);

        // 2. 检查窗口边框
        if (!PInvoke.IsZoomed(_mainWindow))
        {
            PInvoke.GetWindowRect(_mainWindow, out var windowRect);
            var borderSize = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSIZEFRAME);
            if (point.X < borderSize)
            {
                return point.Y < borderSize ? new((int)PInvoke.HTTOPLEFT) :
                       point.Y > windowRect.Height - borderSize ? new((int)PInvoke.HTBOTTOMLEFT) :
                       new((int)PInvoke.HTLEFT);
            }

            if (point.X > windowRect.Width - borderSize)
            {
                return point.Y < borderSize ? new((int)PInvoke.HTTOPRIGHT) :
                       point.Y > windowRect.Height - borderSize ? new((int)PInvoke.HTBOTTOMRIGHT) :
                       new((int)PInvoke.HTRIGHT);
            }

            if (point.Y < borderSize)
            {
                return new((int)PInvoke.HTTOP);
            }

            if (point.Y > windowRect.Height - borderSize)
            {
                return new((int)PInvoke.HTBOTTOM);
            }
        }

        return new((int)PInvoke.HTCLIENT);
    }

    private bool IsPointInDragRegion(Point point)
    {
        return point.X >= _dragRegion.left && point.X <= _dragRegion.right &&
               point.Y >= _dragRegion.top && point.Y <= _dragRegion.bottom;
    }

    private void ShowSystemMenu(HWND hWnd, LPARAM lParam)
    {
        var hMenu = PInvoke.GetSystemMenu(_mainWindow, false);

        // 更新菜单项状态
        var style = PInvoke.GetWindowLong(_mainWindow, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        PInvoke.EnableMenuItem(hMenu, PInvoke.SC_RESTORE,
            (style & (int)WINDOW_STYLE.WS_MAXIMIZE) != 0 ? MENU_ITEM_FLAGS.MF_ENABLED : MENU_ITEM_FLAGS.MF_GRAYED);
        PInvoke.EnableMenuItem(hMenu, PInvoke.SC_MOVE,
            (style & (int)WINDOW_STYLE.WS_MAXIMIZE) == 0 ? MENU_ITEM_FLAGS.MF_ENABLED : MENU_ITEM_FLAGS.MF_GRAYED);
        PInvoke.EnableMenuItem(hMenu, PInvoke.SC_SIZE,
            (style & (int)(WINDOW_STYLE.WS_MAXIMIZE | WINDOW_STYLE.WS_MINIMIZE)) == 0 ?
            MENU_ITEM_FLAGS.MF_ENABLED : MENU_ITEM_FLAGS.MF_GRAYED);
        PInvoke.EnableMenuItem(hMenu, PInvoke.SC_MINIMIZE,
            (style & (int)WINDOW_STYLE.WS_MINIMIZE) == 0 ? MENU_ITEM_FLAGS.MF_ENABLED : MENU_ITEM_FLAGS.MF_GRAYED);
        PInvoke.EnableMenuItem(hMenu, PInvoke.SC_MAXIMIZE,
            (style & (int)WINDOW_STYLE.WS_MAXIMIZE) == 0 ? MENU_ITEM_FLAGS.MF_ENABLED : MENU_ITEM_FLAGS.MF_GRAYED);

        unsafe
        {
            var cmd = PInvoke.TrackPopupMenuEx(
            hMenu,
            Convert.ToUInt32(TRACK_POPUP_MENU_FLAGS.TPM_RETURNCMD),
            GET_X_LPARAM(lParam),
            GET_Y_LPARAM(lParam),
            _mainWindow,
            null);

            if (cmd != 0)
            {
                var wparam = Marshal.AllocHGlobal(Marshal.SizeOf<WPARAM>());
                Marshal.WriteInt32(wparam, (int)cmd);
                PInvoke.SendMessage(_mainWindow, PInvoke.WM_SYSCOMMAND, new((nuint)wparam), 0);
                Marshal.FreeHGlobal(wparam);
            }
        }
    }

    private int GetTopBorderHeight()
    {
        if (!IsTitlebarVisible() || PInvoke.IsZoomed(_mainWindow))
            return 0;

        return topBorderVisibleHeight;
    }

    private bool IsTitlebarVisible() => _isActive;

    private static int GET_X_LPARAM(nint lParam) => (short)(((uint)lParam.ToInt64()) & 0xFFFF);

    private static int GET_Y_LPARAM(nint lParam) => (short)(((int)lParam >> 16) & 0xFFFF);
}
