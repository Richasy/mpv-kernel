// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;

namespace WinUISample.ViewModels;

/// <summary>
/// 本地视频页面视图模型.
/// </summary>
public sealed partial class LocalVideoPageViewModel : ViewModelBase
{
    private readonly ISettingsToolkit _settingsToolkit;
    private readonly IFileToolkit _fileToolkit;

    [ObservableProperty]
    public partial string? LastVideoPath { get; set; }

    /// <summary>
    /// Initialize a new instance of the <see cref="LocalVideoPageViewModel"/> class.
    /// </summary>
    public LocalVideoPageViewModel(ISettingsToolkit settingsToolkit, IFileToolkit fileToolkit)
    {
        _settingsToolkit = settingsToolkit;
        _fileToolkit = fileToolkit;
        LastVideoPath = settingsToolkit.ReadLocalSetting("LastLocalVideoPath", string.Empty);
    }

    [RelayCommand]
    private async Task OpenLocalFileAsync()
    {
        var file = await _fileToolkit.PickFileAsync(".mp4,.mkv,.avi,.mov,.rmvb,.wmv", this.Get<AppViewModel>().MainWindow);
        if (file != null)
        {
            LastVideoPath = file.Path;
            _settingsToolkit.WriteLocalSetting("LastLocalVideoPath", file.Path);
            await this.Get<AppViewModel>().OpenVideoAsync(file.Path);
        }
    }

    [RelayCommand]
    private async Task ReopenLastFileAsync()
    {
        if (string.IsNullOrEmpty(LastVideoPath) || !File.Exists(LastVideoPath))
        {
            var dialog = new ContentDialog()
            {
                Title = "提示",
                Content = "没有找到上次播放的视频文件，是否重新选择？",
                PrimaryButtonText = "重新选择",
                CloseButtonText = "取消",
                XamlRoot = this.Get<AppViewModel>().ActivateXamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await OpenLocalFileAsync();
            }

            return;
        }

        await this.Get<AppViewModel>().OpenVideoAsync(LastVideoPath);
    }
}
