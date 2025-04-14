using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.WinUI;
using Richasy.WinUIKernel.Share.Toolkits;

namespace WinUISample.ViewModels;

/// <summary>
/// 播放器视图模型.
/// </summary>
public sealed partial class PlayerViewModel
{
    private const int WindowMinWidth = 640;
    private const int WindowMinHeight = 480;

    private readonly ILogger<PlayerViewModel> _logger;
    private readonly DispatcherQueue _queue;
    private readonly ISettingsToolkit _settingsToolkit;
    private DispatcherQueueTimer? _tipTimer;

    private string _lastMediaPath = string.Empty;
    private MpvPlayOptions? _lastPlayOptions;

    /// <summary>
    /// 播放客户端.
    /// </summary>
    internal MpvClient? Client { get; private set; }

    /// <summary>
    /// 播放窗口.
    /// </summary>
    internal MpvPlayerWindow? Window { get; private set; }

    internal string Id { get; private set; }

    /// <summary>
    /// 正在加载媒体源信息.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSourceLoading { get; set; }

    [ObservableProperty]
    public partial bool IsFileLoading { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    /// <summary>
    /// 该状态表示没有任何媒体加载.
    /// </summary>
    [ObservableProperty]
    public partial bool IsIdle { get; set; }

    [ObservableProperty]
    public partial MpvPlayerState LastState { get; set; }

    [ObservableProperty]
    public partial double Duration { get; set; }

    [ObservableProperty]
    public partial double CurrentPosition { get; set; }

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

    [ObservableProperty]
    public partial bool IsBackdropVisible { get; set; }

    [ObservableProperty]
    public partial bool IsRestartVisible { get; set; }
}
