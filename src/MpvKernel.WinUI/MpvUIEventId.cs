// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// MPV UI 事件 ID.
/// </summary>
public enum MpvUIEventId
{
    /// <summary>
    /// 预览播放进度更新.
    /// </summary>
    PreviewPositionChanged,

    /// <summary>
    /// 音量改变.
    /// </summary>
    VolumeChanged,

    /// <summary>
    /// 状态检查结果.
    /// </summary>
    StateChecked,

    /// <summary>
    /// 光标移动.
    /// </summary>
    PointerMoved,

    /// <summary>
    /// 单击.
    /// </summary>
    Tapped,
}
