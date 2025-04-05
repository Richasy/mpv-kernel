// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Richasy.MpvKernel.Win32;

public partial class MpvPlayerWindow
{
    private LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case PInvoke.WM_CLOSE:
                // 处理关闭消息
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _ = OnClosed();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                return new(0);

            case PInvoke.WM_DPICHANGED:
                Util.UpdateDpiScale(_windowHandle);
                UpdateTitleBarRegion();
                return new(0);

            case PInvoke.WM_SIZE:
                UpdateTitleBarRegion();
                return new(0);

            case PInvoke.WM_NCCALCSIZE:
                // 让自定义标题栏处理非客户区计算
                if (_customTitleBar != null && _customTitleBar.HandleMessage(uMsg, wParam, lParam, out var result))
                {
                    return result;
                }

                break;

            case PInvoke.WM_NCHITTEST:
                // 让自定义标题栏处理命中测试
                if (_customTitleBar != null && _customTitleBar.HandleMessage(uMsg, wParam, lParam, out var hitTestResult))
                {
                    return hitTestResult;
                }

                break;

            default:
                return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }

        return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
    }

    /// <summary>
    /// 当窗口关闭时发生.
    /// </summary>
    protected virtual async Task OnClosed()
    {
        await DisposeAsync();
    }
}
