// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.WinUI.Core;

public abstract partial class MpvPlayerViewModelBase
{
    /// <summary>
    /// 窗口最小宽度.
    /// </summary>
    protected virtual int WindowMinWidth => 640;

    /// <summary>
    /// 窗口最小高度.
    /// </summary>
    protected virtual int WindowMinHeight => 480;

    /// <summary>
    /// 日志记录器.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// UI 线程调度器.
    /// </summary>
    protected DispatcherQueue Queue { get; }

    /// <summary>
    /// 媒体源解析器.
    /// </summary>
    protected IMediaSourceResolver SourceResolver { get; }

    /// <summary>
    /// 背景 UI 提供者.
    /// </summary>
    protected IMediaBackgroundProvider BackgroundUIProvider { get; }

    /// <summary>
    /// 播放客户端.
    /// </summary>
    protected MpvClient? Client { get; set; }

    /// <summary>
    /// 播放窗口.
    /// </summary>
    protected MpvPlayerWindow? Window { get; set; }

    /// <summary>
    /// 正在加载播放源.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSourceLoading { get; set; }

    /// <summary>
    /// 正在播放.
    /// </summary>
    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    /// <summary>
    /// 播放器是否处于空闲状态.
    /// </summary>
    [ObservableProperty]
    public partial bool IsIdle { get; set; }

    /// <summary>
    /// 当前播放器状态.
    /// </summary>
    [ObservableProperty]
    public partial MpvPlayerState CurrentState { get; set; }

    [ObservableProperty]
    public partial double Duration { get; set; }

    [ObservableProperty]
    public partial double Position { get; set; }

    [ObservableProperty]
    public partial double Volume { get; set; }

    [ObservableProperty]
    public partial double Speed { get; set; }

    [ObservableProperty]
    public partial bool IsFullScreen { get; set; }

    [ObservableProperty]
    public partial bool IsCompactOverlay { get; set; }

    [ObservableProperty]
    public partial bool IsProgressChanging { get; set; }

    [ObservableProperty]
    public partial double PreviewPosition { get; set; }

    [ObservableProperty]
    public partial bool IsVolumeChanging { get; set; }

    [ObservableProperty]
    public partial bool IsControlVisible { get; set; }
}
