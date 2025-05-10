// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Player.Models;

/// <summary>
/// MPV 字幕信息.
/// </summary>
public sealed class MpvSubtitle(double position, string? content)
{
    /// <summary>
    /// 时间戳（秒）.
    /// </summary>
    public double Position { get; } = position;

    /// <summary>
    /// 字幕文本.
    /// </summary>
    public string? Content { get; } = content;
}
