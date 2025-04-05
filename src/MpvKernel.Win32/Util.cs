// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Windows.Win32.Foundation;

namespace Richasy.MpvKernel.Win32;
internal static class Util
{
    private static float _currentDpiScale = 1.0f;

    public static void UpdateDpiScale(HWND handle)
    {
        // 获取当前窗口DPI
        var dpi = Windows.Win32.PInvoke.GetDpiForWindow(handle);
        _currentDpiScale = dpi / 96.0f; // 96是100%缩放的标准DPI
    }

    /// <summary>
    /// 将逻辑像素转换为物理像素（考虑DPI缩放）
    /// </summary>
    public static int Pt2Pix(int value) => (int)(value * _currentDpiScale);

    /// <summary>
    /// 将物理像素转换为逻辑像素（考虑DPI缩放）
    /// </summary>
    public static int Pix2Pt(int value) => (int)(value / _currentDpiScale);
}
