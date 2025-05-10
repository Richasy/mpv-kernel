// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.WinUI;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;
using Windows.Storage;
using WinUISample.Models.Constants;

namespace WinUISample.ViewModels;

/// <summary>
/// 应用程序视图模型.
/// </summary>
public sealed partial class AppViewModel : ViewModelBase
{
    /// <summary>
    /// Initialize a new instance of the <see cref="AppViewModel"/> class.
    /// </summary>
    public AppViewModel(
        ISettingsToolkit settingsToolkit,
        IFileToolkit fileToolkit)
    {
        _settingsToolkit = settingsToolkit;
        _fileToolkit = fileToolkit;
        CurrentSectionType = settingsToolkit.ReadLocalSetting("SectionType", SectionType.Local);
        LibMpvPath = settingsToolkit.ReadLocalSetting("LibMpvPath", string.Empty);
        if (!string.IsNullOrEmpty(LibMpvPath))
        {
            MpvNative.Initialize(LibMpvPath);
        }
    }

    /// <summary>
    /// 打开视频.
    /// </summary>
    public async Task OpenVideoAsync(string videoPath, MpvPlayOptions? options = null)
    {
        var existWindow = PlayerWindows.Find(p => p.Id == videoPath);
        if (existWindow != null)
        {
            existWindow.Window!.Show();
            return;
        }

        var playerVM = this.Get<PlayerViewModel>();
        PlayerWindows.Add(playerVM);
        await playerVM.LoadAsync(videoPath, options);
    }

    [RelayCommand]
    private async Task PickLibMpvAsync()
    {
        var file = await _fileToolkit.PickFileAsync(".dll", MainWindow);
        if (file != null)
        {
            LibMpvPath = file.Path;
            _settingsToolkit.WriteLocalSetting("LibMpvPath", file.Path);
        }
    }

    [RelayCommand]
    private static async Task OpenLoggerFolderAsync()
    {
        var folder = App.LoggerFolder;
        var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);
        await Windows.System.Launcher.LaunchFolderAsync(storageFolder);
    }

    private async Task OpenPlayerWindowAsync()
    {
        var mpvPath = _settingsToolkit.ReadLocalSetting("LibMpvPath", string.Empty);
        var client = await MpvClient.CreateAsync(mpvPath);
        await client.SetLogLevelAsync(MpvLogLevel.Info);
        await client.UseIdleAsync(true);
        var playerWindow = new MpvPlayerWindow(client, this.Get<DispatcherQueue>());
        var wnd = playerWindow.GetWindow();

    }

    partial void OnCurrentSectionTypeChanged(SectionType value)
    {
        _settingsToolkit.WriteLocalSetting("SectionType", value);
    }
}
