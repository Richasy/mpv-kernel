// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// MPV 客户端事件 ID.
/// </summary>
public enum MpvClientEventId
{
    /// <summary>
    /// 播放状态发生变化.
    /// </summary>
    StateChanged,

    /// <summary>
    /// 音量发生变化.
    /// </summary>
    VolumeChanged,

    /// <summary>
    /// 播放速度发生变化.
    /// </summary>
    SpeedChanged,

    /// <summary>
    /// 视频时长发生变化.
    /// </summary>
    DurationChanged,

    /// <summary>
    /// 播放位置发生变化.
    /// </summary>
    PositionChanged,

    /// <summary>
    /// 全屏状态发生变化.
    /// </summary>
    FullScreenChanged,

    /// <summary>
    /// 小窗状态发生变化.
    /// </summary>
    CompactOverlayChanged,
}
