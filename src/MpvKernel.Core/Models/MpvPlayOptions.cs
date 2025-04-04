// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// MPV 播放选项.
/// </summary>
public sealed class MpvPlayOptions
{
    /// <summary>
    /// 播放窗口句柄.
    /// </summary>
    public IntPtr? WindowHandle { get; set; }

    /// <summary>
    /// 起始位置.
    /// </summary>
    public double? StartPosition { get; set; }
}
