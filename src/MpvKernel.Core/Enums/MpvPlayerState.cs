// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// MPV 播放器状态.
/// </summary>
public enum MpvPlayerState
{
    /// <summary>
    /// 空闲状态（没有播放内容）.
    /// </summary>
    Idle,

    /// <summary>
    /// 正在播放.
    /// </summary>
    Playing,

    /// <summary>
    /// 已暂停.
    /// </summary>
    Paused,

    /// <summary>
    /// 正在缓冲.
    /// </summary>
    Buffering,

    /// <summary>
    /// 因播放位置变动正在重新加载.
    /// </summary>
    Seeking,

    /// <summary>
    /// 播放结束（只有在启用 keep-open）才会出现.
    /// </summary>
    End,
}
