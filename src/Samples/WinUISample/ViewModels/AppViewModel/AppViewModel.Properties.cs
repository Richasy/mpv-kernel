// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Richasy.WinUIKernel.Share.Toolkits;
using WinUISample.Models.Constants;

namespace WinUISample.ViewModels;

public sealed partial class AppViewModel
{
    private readonly ISettingsToolkit _settingsToolkit;
    private readonly IFileToolkit _fileToolkit;

    /// <summary>
    /// 激活的XAML ROOT.
    /// </summary>
    public XamlRoot? ActivateXamlRoot { get; set; }

    /// <summary>
    /// 当前选中区块.
    /// </summary>
    [ObservableProperty]
    public partial SectionType CurrentSectionType { get; set; }

    /// <summary>
    /// LibMpv DLL 路径.
    /// </summary>
    [ObservableProperty]
    public partial string LibMpvPath { get; set; }

    /// <summary>
    /// 播放器窗口列表.
    /// </summary>
    public List<PlayerViewModel> PlayerWindows { get; set; } = [];

    /// <summary>
    /// 主窗口.
    /// </summary>
    public MainWindow MainWindow { get; set; }
}
