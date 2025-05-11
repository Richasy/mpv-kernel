// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.WinUIKernel.Share.Base;
using WinUISample.ViewModels;

namespace WinUISample.Pages;

/// <summary>
/// 哔哩哔哩视频页面.
/// </summary>
public sealed partial class BiliVideoPage : BiliVideoPageBase
{
    /// <summary>
    /// 初始化 <see cref="BiliVideoPage"/> 类的新实例.
    /// </summary>
    public BiliVideoPage() => InitializeComponent();

    /// <inheritdoc/>
    protected override async void OnPageLoaded()
        => await ViewModel.InitializeAsync(QRCodeImage);
}

/// <summary>
/// 哔哩哔哩视频页面基类.
/// </summary>
public abstract class BiliVideoPageBase : LayoutPageBase<BiliVideoPageViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BiliVideoPageBase"/> class.
    /// </summary>
    protected BiliVideoPageBase() => ViewModel = this.Get<BiliVideoPageViewModel>();
}