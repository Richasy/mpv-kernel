// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// 媒体播放器背景 UI 提供器.
/// </summary>
public interface IMediaBackgroundProvider
{
    /// <summary>
    /// 获取背景元素.
    /// </summary>
    /// <returns>元素.</returns>
    public UIElement? GetBackgroundElement();
}
