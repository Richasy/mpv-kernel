// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using Richasy.MpvKernel;
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
        DecodeType = settingsToolkit.ReadLocalSetting("DecodeType", DecodeType.Auto);
        if (!string.IsNullOrEmpty(LibMpvPath))
        {
            MpvNative.Initialize(LibMpvPath);
        }
    }

    /// <summary>
    /// 打开视频.
    /// </summary>
    public async Task OpenVideoAsync(string videoPath)
    {
        var playerVM = this.Get<PlayerViewModel>();
        await playerVM.InitializeAsync(videoPath, CurrentSectionType);
        Players.Add(playerVM);
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

    partial void OnCurrentSectionTypeChanged(SectionType value)
    {
        _settingsToolkit.WriteLocalSetting("SectionType", value);
    }

    partial void OnDecodeTypeChanged(DecodeType value)
        => _settingsToolkit.WriteLocalSetting("DecodeType", value);
}
