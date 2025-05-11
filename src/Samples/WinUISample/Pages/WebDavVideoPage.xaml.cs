// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Richasy.WinUIKernel.Share.Base;
using WinUISample.Controls;
using WinUISample.ViewModels;

namespace WinUISample.Pages;

/// <summary>
/// WebDav视频页面.
/// </summary>
public sealed partial class WebDavVideoPage : WebDavVideoPageBase
{
    /// <summary>
    /// 初始化 <see cref="WebDavVideoPage"/> 类的新实例.
    /// </summary>
    public WebDavVideoPage() => InitializeComponent();

    /// <inheritdoc/>
    protected override async void OnPageLoaded()
        => await ViewModel.InitializeAsync();

    private async void OnAddConfigButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new WebDavConfigDialog();
        await dialog.ShowAsync();
    }

    private async void OnEditConfigButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new WebDavConfigDialog(ViewModel._config);
        await dialog.ShowAsync();
    }

    private void OnPathSegmentClick(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        var seg = args.Item as WebDavPathSegment;
        ViewModel.LoadPathCommand.Execute(seg.Path);
    }
}

/// <summary>
/// WebDav视频页面基类.
/// </summary>
public abstract class WebDavVideoPageBase : LayoutPageBase<WebDavVideoPageViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavVideoPageBase"/> class.
    /// </summary>
    protected WebDavVideoPageBase() => ViewModel = this.Get<WebDavVideoPageViewModel>();
}