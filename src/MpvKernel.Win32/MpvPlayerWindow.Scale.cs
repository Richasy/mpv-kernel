// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Richasy.MpvKernel.Win32;

public partial class MpvPlayerWindow
{
    /// <summary>
    /// 设置窗口大小（使用逻辑像素，会自动转换为物理像素）
    /// </summary>
    public void SetSize(int width, int height)
    {
        if (_windowHandle != IntPtr.Zero)
        {
            PInvoke.SetWindowPos(
                _windowHandle,
                default,
                0, 0,
                Util.Pt2Pix(width),
                Util.Pt2Pix(height),
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER
            );
        }
    }

    /// <summary>
    /// 获取当前窗口大小（返回逻辑像素）
    /// </summary>
    public (int Width, int Height) GetSize()
    {
        if (_windowHandle == IntPtr.Zero)
            return (0, 0);

        PInvoke.GetClientRect(_windowHandle, out var rect);
        return (Util.Pix2Pt(rect.right - rect.left), Util.Pix2Pt(rect.bottom - rect.top));
    }

    private LRESULT DpiChangedSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == PInvoke.WM_DPICHANGED)
        {
            Util.UpdateDpiScale(_windowHandle);

            // 建议的新窗口大小和位置
            var rect = Marshal.PtrToStructure<RECT>((IntPtr)lParam);
            PInvoke.SetWindowPos(
                hWnd,
                default,
                rect.left,
                rect.top,
                rect.right - rect.left,
                rect.bottom - rect.top,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
            );

            return new(0);
        }

        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
