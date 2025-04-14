// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.WinUIKernel.Share.Base;
using WinUISample.ViewModels;

namespace WinUISample.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalVideoPage : LocalVideoPageBase
{
    /// <summary>
    /// 初始化 <see cref="LocalVideoPage"/> 类的新实例.
    /// </summary>
    public LocalVideoPage()
    {
        InitializeComponent();
    }
}

/// <summary>
/// 本地视频页面基类.
/// </summary>
public abstract class LocalVideoPageBase : LayoutPageBase<LocalVideoPageViewModel>
{
    /// <summary>
    /// 初始化 <see cref="LocalVideoPageBase"/> 类的新实例.
    /// </summary>
    protected LocalVideoPageBase() => ViewModel = this.Get<LocalVideoPageViewModel>();
}