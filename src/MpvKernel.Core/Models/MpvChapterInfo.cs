// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// MPV 章节信息.
/// </summary>
public sealed class MpvChapterInfo
{
    /// <summary>
    /// 章节标题.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 章节开始时间.
    /// </summary>
    public double Time { get; set; }
}
