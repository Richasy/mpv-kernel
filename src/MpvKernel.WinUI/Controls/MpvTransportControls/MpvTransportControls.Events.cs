// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.WinUI.Controls;

public partial class MpvTransportControls
{
    /// <summary>
    /// 上一个按钮点击事件.
    /// </summary>
    public event EventHandler PrevButtonClick;

    /// <summary>
    /// 下一个按钮点击事件.
    /// </summary>
    public event EventHandler NextButtonClick;

    /// <summary>
    /// 前跳按钮点击事件.
    /// </summary>
    public event EventHandler ForwardSkipButtonClick;

    /// <summary>
    /// 后退按钮点击事件.
    /// </summary>
    public event EventHandler BackwardSkipButtonClick;

    /// <summary>
    /// 播放/暂停按钮点击事件.
    /// </summary>
    public event EventHandler PlayPauseButtonClick;
}
