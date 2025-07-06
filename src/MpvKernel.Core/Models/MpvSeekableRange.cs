// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// 表示可跳转范围的模型类，通常用于视频或音频播放器中，表示可以跳转的时间范围。
/// </summary>
public sealed class MpvSeekableRange
{
    /// <summary>
    /// 起始位置
    /// </summary>
    public double Start { get; set; }

    /// <summary>
    /// 结束位置
    /// </summary>
    public double End { get; set; }
}
