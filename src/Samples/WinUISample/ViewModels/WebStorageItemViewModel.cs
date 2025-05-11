// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richasy.WebDavKernel;
using Richasy.WinUIKernel.Share.ViewModels;

namespace WinUISample.ViewModels;

/// <summary>
/// WebDAV 存储项视图模型.
/// </summary>
public sealed partial class WebDavStorageItemViewModel : ViewModelBase<WebDavStorageItem>
{
    private readonly string[] _supportedExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".rmvb", ".wmv" };

    [ObservableProperty]
    public partial bool IsFolder { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavStorageItemViewModel"/> class.
    /// </summary>
    public WebDavStorageItemViewModel(
        WebDavStorageItem data)
        : base(data)
    {
        IsFolder = data.IsFolder;
        IsEnabled = true;
        if (IsFolder)
        {
            Icon = "📁";
        }
        else if (_supportedExtensions.Contains(data.Extension))
        {
            Icon = "🎞️";
        }
        else
        {
            Icon = "❔";
            IsEnabled = false;
        }
    }

    /// <summary>
    /// 是否可用.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// 是否可选.
    /// </summary>
    public bool IsSelectable => !IsFolder && IsEnabled;

    [RelayCommand]
    private async Task ActivateAsync()
    {
        var pageVM = this.Get<WebDavVideoPageViewModel>();
        if (IsFolder)
        {
            pageVM.LoadPathCommand.Execute(Data.RelativePath);
        }
        else
        {
            await this.Get<AppViewModel>().OpenVideoAsync(Data.FullPath);
        }
    }
}
