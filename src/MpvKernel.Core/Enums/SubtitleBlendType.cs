// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// Blend subtitles directly onto upscaled video frames, before interpolation and/or color management.
/// </summary>
public enum SubtitleBlendType
{
    /// <summary>
    /// 启用.
    /// </summary>
    Yes,

    /// <summary>
    /// 将字幕直接混合到上采样的视频帧上，插值和/或颜色管理之前。
    /// </summary>
    Video,

    /// <summary>
    /// 禁用.
    /// </summary>
    No
}
