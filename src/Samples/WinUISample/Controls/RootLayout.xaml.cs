// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Richasy.WinUIKernel.Share.Base;
using System.ComponentModel;
using WinUISample.Models.Constants;
using WinUISample.Pages;
using WinUISample.ViewModels;

namespace WinUISample.Controls;

/// <summary>
/// 根布局.
/// </summary>
public sealed partial class RootLayout : RootLayoutBase
{
    /// <summary>
    /// 初始化 <see cref="RootLayout"/> 类的新实例.
    /// </summary>
    public RootLayout()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnControlLoaded()
    {
        Section.SelectedIndex = (int)ViewModel.CurrentSectionType;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        CheckPage();
    }

    /// <inheritdoc/>
    protected override void OnControlUnloaded()
        => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentSectionType))
        {
            CheckPage();
        }
    }

    private void OnSectionTypeChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        var section = (SectionType)Section.SelectedIndex;
        if (section != ViewModel.CurrentSectionType)
        {
            ViewModel.CurrentSectionType = section;
        }
    }

    private void CheckPage()
    {
        var pageType = ViewModel.CurrentSectionType switch
        {
            SectionType.Local => typeof(LocalVideoPage),
            _ => typeof(Page),
        };

        if (MainFrame.Content?.GetType() != pageType)
        {
            MainFrame.Navigate(pageType);
        }
    }
}

/// <summary>
/// 根布局基类.
/// </summary>
public abstract class RootLayoutBase : LayoutUserControlBase<AppViewModel>
{
    /// <summary>
    /// 初始化 <see cref="RootLayoutBase"/> 类的新实例.
    /// </summary>
    protected RootLayoutBase() => ViewModel = this.Get<AppViewModel>();
}
