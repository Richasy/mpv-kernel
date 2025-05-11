// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;

namespace WinUISample.ViewModels;

/// <summary>
/// 哔哩哔哩视频页面视图模型.
/// </summary>
public sealed partial class BiliVideoPageViewModel : ViewModelBase, IDisposable
{
    private readonly ISettingsToolkit _settingsToolkit;
    private readonly ILogger<BiliVideoPageViewModel> _logger;
    private readonly Richasy.BiliKernel.Bili.Authorization.IAuthenticationService _authService;
    private readonly Richasy.BiliKernel.Bili.User.IMyProfileService _profileService;
    private readonly Richasy.BiliKernel.Bili.Media.IPlayerService _playerService;
    private readonly DispatcherQueue _dispatcherQueue;

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initialize a new instance of the <see cref="BiliVideoPageViewModel"/> class.
    /// </summary>
    public BiliVideoPageViewModel(
        ISettingsToolkit settingsToolkit,
        DispatcherQueue dispatcherQueue,
        ILogger<BiliVideoPageViewModel> logger,
        Richasy.BiliKernel.Bili.Authorization.IAuthenticationService authService,
        Richasy.BiliKernel.Bili.User.IMyProfileService profileService,
        Richasy.BiliKernel.Bili.Media.IPlayerService playerService)
    {
        _settingsToolkit = settingsToolkit;
        _dispatcherQueue = dispatcherQueue;
        _logger = logger;
        _authService = authService;
        _profileService = profileService;
        _playerService = playerService;
        VideoLink = settingsToolkit.ReadLocalSetting("LastBiliVideoLink", string.Empty);
    }

    /// <summary>
    /// 二维码图片控件.
    /// </summary>
    public Image? QRCodeImage { get; private set; }

    [ObservableProperty]
    public partial bool IsQRCodeLoading { get; set; }

    [ObservableProperty]
    public partial bool IsSignedIn { get; set; }

    [ObservableProperty]
    public partial BitmapImage? UserAvatar { get; set; }

    [ObservableProperty]
    public partial string? UserName { get; set; }

    [ObservableProperty]
    public partial string? VideoLink { get; set; }

    /// <summary>
    /// 初始化视图模型.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task InitializeAsync(Image qrcodeImageControl)
    {
        QRCodeImage = qrcodeImageControl;
        IsSignedIn = await CheckAuthorizeStatusAsync();
        if (!IsSignedIn)
        {
            await ReloadQRCodeAsync();
        }
        else
        {
            await InitializeUserInfoAsync();
        }
    }

    [RelayCommand]
    private async Task ReloadQRCodeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            IsQRCodeLoading = true;
            await _authService.SignInAsync(cancellationToken: _cancellationTokenSource.Token);
            IsSignedIn = await CheckAuthorizeStatusAsync();
            if (IsSignedIn)
            {
                await InitializeUserInfoAsync();
            }
            else
            {
                _logger.LogWarning("未能成功获取授权信息，扫码可能出现了异常.");
            }
        }
        catch (Exception ex)
        {
            IsQRCodeLoading = false;
            _logger.LogInformation(ex, "登录过程中出现异常.");
        }
    }

    [RelayCommand]
    private void RenderQRCode(byte[] imageData)
    {
        if (QRCodeImage is null)
        {
            throw new InvalidOperationException("二维码图片控件尚未就绪，请初始化模块.");
        }

        _dispatcherQueue.TryEnqueue(async () =>
        {
            await using var stream = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream.AsRandomAccessStream()).AsTask();
            QRCodeImage.Source = bitmap;
            IsQRCodeLoading = false;
        });
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (string.IsNullOrEmpty(VideoLink))
        {
            _logger.LogError("无效的视频链接.");
            return;
        }

        _settingsToolkit.WriteLocalSetting("LastBiliVideoLink", VideoLink);
        await this.Get<AppViewModel>().OpenVideoAsync(VideoLink);
    }

    private async Task<bool> CheckAuthorizeStatusAsync()
    {
        try
        {
            await _authService.EnsureTokenAsync(_cancellationTokenSource?.Token ?? default).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "检查授权状态时出现异常.");
        }

        return false;
    }

    private async Task InitializeUserInfoAsync()
    {
        var userInfo = await _profileService.GetMyProfileAsync();
        if (userInfo != null)
        {
            UserName = userInfo.User.Name;
            UserAvatar = new(userInfo.User.Avatar.Uri);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => _cancellationTokenSource?.Dispose();
}
