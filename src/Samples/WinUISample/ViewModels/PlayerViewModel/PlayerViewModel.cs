using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.WinUI;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;
using Windows.Graphics;
using WinUISample.Models.Constants;
using Windows.Win32;

namespace WinUISample.ViewModels;

/// <summary>
/// 播放器视图模型.
/// </summary>
public sealed partial class PlayerViewModel : ViewModelBase
{
    /// <summary>
    /// 创建播放器视图模型的实例.
    /// </summary>
    public PlayerViewModel(
        ILogger<PlayerViewModel> logger,
        DispatcherQueue dispatcherQueue,
        ISettingsToolkit settingsToolkit)
    {
        _logger = logger;
        _queue = dispatcherQueue;
        _settingsToolkit = settingsToolkit;
        _tipTimer = _queue.CreateTimer();
        _tipTimer.Interval = TimeSpan.FromSeconds(1);
        _tipTimer.Tick += OnTipTimerTick;
    }

    /// <summary>
    /// 加载视频文件.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task LoadAsync(string fileUrl, MpvPlayOptions? options)
    {
        if (Client is null)
        {
            Client = await MpvClient.CreateAsync();
            Client.DataNotify += OnClientDataNotify;
            Client.ReachFileLoading += OnFileLoading;
            Client.ReachFileLoaded += OnFileLoaded;

            await Client.SetLogLevelAsync(MpvLogLevel.Warn);
            await Client.UseIdleAsync(true);
            await Client.UseKeepOpenAsync(true);

            await InitializeDecodeAsync();

            InitializeWindow();
        }

        Id = _lastMediaPath;
        _lastMediaPath = fileUrl;
        _lastPlayOptions = options ?? new MpvPlayOptions();
        await LoadMediaAsync();
    }

    private async Task InitializeDecodeAsync()
    {
        var decodeType = PreferDecodeType.Auto;
        try
        {
            decodeType = _settingsToolkit.ReadLocalSetting("PreferDecode", PreferDecodeType.Auto);
        }
        catch (Exception)
        {
            _settingsToolkit.WriteLocalSetting("PreferDecode", PreferDecodeType.Auto);
        }

        if (decodeType == PreferDecodeType.Auto)
        {
            await Client.SetVideoOutputAsync(VideoOutputType.Gpu);
            await Client.SetGpuContextAsync(GpuContextType.Auto);
            await Client.SetHardwareDecodeAsync(HardwareDecodeType.Auto);
        }
        else if (decodeType == PreferDecodeType.D3D11)
        {
            await Client.SetVideoOutputAsync(VideoOutputType.Gpu);
            await Client.SetGpuContextAsync(GpuContextType.D3D11);
            await Client.SetHardwareDecodeAsync(HardwareDecodeType.D3D11va);
        }
        else if (decodeType == PreferDecodeType.NVDEC)
        {
            await Client.SetVideoOutputAsync(VideoOutputType.Gpu);
            await Client.SetGpuContextAsync(GpuContextType.Auto);
            await Client.SetHardwareDecodeAsync(HardwareDecodeType.Nvdec);
        }
        else if (decodeType == PreferDecodeType.Vulkan)
        {
            await Client.SetVideoOutputAsync(VideoOutputType.GpuNext);
            await Client.SetGpuContextAsync(GpuContextType.WindowsVulkan);
            await Client.SetHardwareDecodeAsync(HardwareDecodeType.Vulkan);
        }
        else if (decodeType == PreferDecodeType.DXVA2)
        {
            await Client.SetVideoOutputAsync(VideoOutputType.Gpu);
            await Client.SetGpuContextAsync(GpuContextType.D3D11);
            await Client.SetHardwareDecodeAsync(HardwareDecodeType.Dxva2);
        }
    }

    private void MoveAndResize()
    {
        var lastPoint = GetSavedWindowPosition();
        var displayArea = DisplayArea.GetFromPoint(lastPoint, DisplayAreaFallback.Primary)
            ?? DisplayArea.Primary;
        var rect = GetRenderRect(displayArea.WorkArea);
        Window.GetWindow().MoveAndResize(rect);
    }

    private void InitializeWindow()
    {
        Window = new MpvPlayerWindow(Client, this.Get<DispatcherQueue>());
        Window.UINotify += OnUINotify;
        MoveAndResize();
        var wnd = Window.GetWindow();
        wnd.TitleBar.ExtendsContentIntoTitleBar = true;
        wnd.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        wnd.TitleBar.ButtonForegroundColor = Colors.Transparent;
        wnd.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        wnd.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        if (wnd.Presenter is OverlappedPresenter presenter)
        {
            var scaleFactor = PInvoke.GetDpiForWindow(new(Win32Interop.GetWindowFromWindowId(wnd.Id))) / 96d;
            presenter.PreferredMinimumWidth = Convert.ToInt32(WindowMinWidth * scaleFactor);
            presenter.PreferredMinimumHeight = Convert.ToInt32(WindowMinHeight * scaleFactor);
        }

        var isMaximized = _settingsToolkit.ReadLocalSetting("IsPlayerWindowMaximized", false);
        if (isMaximized)
        {
            (wnd.Presenter as OverlappedPresenter).Maximize();
        }

        wnd.Closing += OnWindowClosing;

        //if (_uiProvider != null)
        //{
        //    var element = _uiProvider.GetUIElement();
        //    IsControlVisible = true;
        //    Window.SetUIElement(element);

        //    var background = _uiProvider.GetBackgroundElement();
        //    if (background is not null)
        //    {
        //        Window.SetBackgroundElement(background);
        //    }
        //}

        Window.Show();
    }

    private async Task LoadMediaAsync()
    {
        if (Client is null)
        {
            return;
        }

        _lastPlayOptions.WindowHandle = Window.Handle;
        _lastPlayOptions.InitialVolume = _settingsToolkit.ReadLocalSetting("PlayerVolume", 100d);
        _lastPlayOptions.InitialSpeed = _settingsToolkit.ReadLocalSetting("PlayerSpeed", 1d);
        Window.GetWindow().Title = Path.GetFileName(_lastMediaPath);

        await Client.PlayAsync(_lastMediaPath, _lastPlayOptions);
    }

    private RectInt32 GetRenderRect(RectInt32 workArea)
    {
        var scaleFactor = PInvoke.GetDpiForWindow(new(Win32Interop.GetWindowFromWindowId(Window.GetWindow().Id))) / 96d;
        var previousWidth = _settingsToolkit.ReadLocalSetting("PlayerWindowWidth", 1120d);
        var previousHeight = _settingsToolkit.ReadLocalSetting("PlayerWindowHeight", 740d);
        var width = Convert.ToInt32(previousWidth * scaleFactor);
        var height = Convert.ToInt32(previousHeight * scaleFactor);

        // Ensure the window is not larger than the work area.
        if (height > workArea.Height - 20)
        {
            height = workArea.Height - 20;
        }

        var lastPoint = GetSavedWindowPosition();
        var isZeroPoint = lastPoint.X == 0 && lastPoint.Y == 0;
        var isValidPosition = lastPoint.X >= workArea.X && lastPoint.Y >= workArea.Y;
        var left = isZeroPoint || !isValidPosition
            ? (workArea.Width - width) / 2d
            : lastPoint.X;
        var top = isZeroPoint || !isValidPosition
            ? (workArea.Height - height) / 2d
            : lastPoint.Y;
        return new RectInt32(Convert.ToInt32(left), Convert.ToInt32(top), width, height);
    }

    private void SaveCurrentWindowStats()
    {
        if (IsFullScreen || IsCompactOverlay)
        {
            return;
        }

        var wnd = Window.GetWindow();
        var scaleFactor = PInvoke.GetDpiForWindow(new(Win32Interop.GetWindowFromWindowId(wnd.Id))) / 96d;
        var left = wnd.Position.X;
        var top = wnd.Position.Y;
        var isMaximized = PInvoke.IsZoomed(new(Win32Interop.GetWindowFromWindowId(Window.GetWindow().Id)));
        _settingsToolkit.WriteLocalSetting("IsPlayerWindowMaximized", (bool)isMaximized);

        if (!isMaximized)
        {
            _settingsToolkit.WriteLocalSetting("PlayerWindowPositionLeft", left);
            _settingsToolkit.WriteLocalSetting("PlayerWindowPositionTop", top);

            if (wnd.Size.Height >= WindowMinHeight && wnd.Size.Width >= WindowMinWidth)
            {
                _settingsToolkit.WriteLocalSetting("PlayerWindowHeight", (wnd.Size.Height / scaleFactor) * 1d);
                _settingsToolkit.WriteLocalSetting("PlayerWindowWidth", (wnd.Size.Width / scaleFactor) * 1d);
            }
        }
    }

    private PointInt32 GetSavedWindowPosition()
    {
        var left = _settingsToolkit.ReadLocalSetting("PlayerWindowPositionLeft", 0);
        var top = _settingsToolkit.ReadLocalSetting("PlayerWindowPositionTop", 0);
        return new PointInt32(left, top);
    }

    private void CheckBackdropVisible()
        => IsBackdropVisible = IsFileLoading || (LastState is MpvPlayerState.Seeking or MpvPlayerState.Buffering && CurrentPosition <= 0.5) || IsIdle || IsSourceLoading;

    partial void OnIsFileLoadingChanged(bool value) => CheckBackdropVisible();

    partial void OnIsIdleChanged(bool value) => CheckBackdropVisible();

    partial void OnIsSourceLoadingChanged(bool value) => CheckBackdropVisible();

    partial void OnIsProgressChangingChanged(bool value)
    {
        if (!IsControlVisible)
        {
            IsControlVisible = true;
        }
    }

    partial void OnSpeedChanged(double value)
    {
        _settingsToolkit.WriteLocalSetting("PlayerSpeed", value);
    }

    partial void OnVolumeChanged(double value)
    {
        if (value is < 0 or > 100)
        {
            return;
        }

        _settingsToolkit.WriteLocalSetting("PlayerVolume", value);
    }
}
