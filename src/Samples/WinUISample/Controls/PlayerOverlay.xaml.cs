// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.WinUIKernel.Share.Base;
using WinUISample.ViewModels;

namespace WinUISample.Controls;

/// <summary>
/// 播放器控制基类.
/// </summary>
public abstract class PlayerControlBase : LayoutUserControlBase<PlayerViewModel>;

/// <summary>
/// 播放器叠加层.
/// </summary>
public sealed partial class PlayerOverlay : PlayerControlBase
{
    /// <summary>
    /// 初始化 <see cref="PlayerOverlay"/> 类的新实例.
    /// </summary>
    public PlayerOverlay() => InitializeComponent();

    private async void OnProgressSliderValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        => await ViewModel.ChangePositionAsync(e.NewValue);
}
