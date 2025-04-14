// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Richasy.WinUIKernel.Share.Base;
using WinUIEx;
using WinUISample.ViewModels;

namespace WinUISample;

/// <summary>
/// Main window.
/// </summary>
public sealed partial class MainWindow : WindowBase
{
    /// <summary>
    /// Initializes a new instance of the MainWindow class. It sets up the user interface components.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Width = 800;
        Height = 600;
        this.CenterOnScreen();
        AppVM = this.Get<AppViewModel>();
        AppVM.MainWindow = this;
        Activated += OnActivated;
    }

    /// <summary>
    /// 应用视图模型.
    /// </summary>
    public AppViewModel AppVM { get; }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated)
        {
            AppVM.ActivateXamlRoot = Content.XamlRoot;
        }
    }
}
