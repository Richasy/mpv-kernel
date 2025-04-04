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
                UpdateDpiScale();
                return new(0);

            default:
                return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }

    /// <summary>
    /// 当窗口关闭时发生.
    /// </summary>
    protected virtual async Task OnClosed()
    {
        await DisposeAsync();
    }
}
