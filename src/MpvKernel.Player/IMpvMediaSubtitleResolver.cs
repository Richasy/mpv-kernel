// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Player;

/// <summary>
/// 媒体字幕解析器接口.
/// </summary>
public interface IMpvMediaSubtitleResolver
{
    /// <summary>
    /// 显示字幕.
    /// </summary>
    /// <param name="position">当前位置.</param>
    public void ShowSubtitle(int position);
}
