// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Richasy.MpvKernel.Player.Models;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Player;

public sealed partial class MpvPlayer
{
    private readonly IMpvMediaSourceResolver? _sourceResolver;
    private readonly IMpvMediaHistoryResolver? _historyResolver;
    private readonly IMpvMediaSubtitleResolver? _subtitleResolver;
    private readonly ILogger _logger;
    private readonly SynchronizationContext _uiContext;
    private readonly System.Timers.Timer _statusTimer;
    private readonly System.Timers.Timer _historyTimer;

    private MpvMediaSource? _cachedSource;
    private bool _positionRecorded;

    /// <summary>
    /// MPV 播放器的客户端实例.
    /// </summary>
    public MpvClient Client { get; }

    /// <summary>
    /// 播放器 ID.
    /// </summary>
    public string? Id => _cachedSource?.Id;

    /// <summary>
    /// 播放状态.
    /// </summary>
    [ObservableProperty]
    public partial MpvPlayerState PlaybackState { get; private set; }

    /// <summary>
    /// 是否正在播放.
    /// </summary>
    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    /// <summary>
    /// 是否正在初始加载媒体文件.
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>
    /// 是否正在缓冲.
    /// </summary>
    [ObservableProperty]
    public partial bool IsBuffering { get; private set; }

    /// <summary>
    /// 媒体播放是否已经停止.
    /// </summary>
    [ObservableProperty]
    public partial bool IsStopped { get; private set; }

    /// <summary>
    /// 播放音量.
    /// </summary>
    [ObservableProperty]
    public partial double Volume { get; private set; }

    /// <summary>
    /// 播放速率.
    /// </summary>
    [ObservableProperty]
    public partial double PlaybackRate { get; private set; }

    /// <summary>
    /// 当前播放时间.
    /// </summary>
    [ObservableProperty]
    public partial double Position { get; private set; }

    /// <summary>
    /// 媒体播放时长.
    /// </summary>
    [ObservableProperty]
    public partial double Duration { get; private set; }

    /// <summary>
    /// 是否为全屏.
    /// </summary>
    [ObservableProperty]
    public partial bool IsFullScreen { get; private set; }

    /// <summary>
    /// 是否为小窗置顶.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCompactOverlay { get; private set; }

    /// <summary>
    /// 媒体标题.
    /// </summary>
    [ObservableProperty]
    public partial string? Title { get; set; }
}
