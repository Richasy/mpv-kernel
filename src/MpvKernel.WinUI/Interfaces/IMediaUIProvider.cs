// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// 媒体 UI 提供器.
/// </summary>
public interface IMediaUIProvider
{
    /// <summary>
    /// 提供用于显示的UI元素.
    /// </summary>
    /// <returns>元素.</returns>
    public UIElement GetUIElement();

    /// <summary>
    /// 获取背景元素.
    /// </summary>
    /// <returns>元素.</returns>
    public UIElement? GetBackgroundElement();
}
