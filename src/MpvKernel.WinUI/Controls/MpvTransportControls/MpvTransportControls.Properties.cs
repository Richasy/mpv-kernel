// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace Richasy.MpvKernel.WinUI.Controls;

public partial class MpvTransportControls
{
    /// <summary>
    /// <see cref="InfoElement"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty InfoElementProperty =
        DependencyProperty.Register(nameof(InfoElement), typeof(object), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsNextButtonEnabled"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsNextButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsNextButtonEnabled), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsPreviousButtonEnabled"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsPreviousButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsPreviousButtonEnabled), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsVideoNavigationButtonsVisible"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty NextButtonToolTipProperty =
        DependencyProperty.Register(nameof(NextButtonToolTip), typeof(string), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="PreviousButtonToolTip"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty PreviousButtonToolTipProperty =
        DependencyProperty.Register(nameof(PreviousButtonToolTip), typeof(string), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsVideoNavigationButtonsVisible"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsVideoNavigationButtonsVisibleProperty =
        DependencyProperty.Register(nameof(IsVideoNavigationButtonsVisible), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsSkipButtonsVisible"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsSkipButtonsVisibleProperty =
        DependencyProperty.Register(nameof(IsSkipButtonsVisible), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsBuffering"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsBufferingProperty =
        DependencyProperty.Register(nameof(IsBuffering), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsPlayPauseButtonEnabled"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsPlayPauseButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsPlayPauseButtonEnabled), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="PlayPauseSymbol"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty PlayPauseSymbolProperty =
        DependencyProperty.Register(nameof(PlayPauseSymbol), typeof(FluentIcons.Common.Symbol), typeof(MpvTransportControls), new PropertyMetadata(FluentIcons.Common.Symbol.Play));

    /// <summary>
    /// <see cref="Volume"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty VolumeProperty =
        DependencyProperty.Register(nameof(Volume), typeof(double), typeof(MpvTransportControls), new PropertyMetadata(100d));

    /// <summary>
    /// <see cref="Speed"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(MpvTransportControls), new PropertyMetadata(1d));

    /// <summary>
    /// <see cref="IsProgressVisible"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsProgressVisibleProperty =
        DependencyProperty.Register(nameof(IsProgressVisible), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(true));

    /// <summary>
    /// <see cref="Duration"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(double), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="Progress"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="IsPreviewProgressVisible"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsPreviewProgressVisibleProperty =
        DependencyProperty.Register(nameof(IsPreviewProgressVisible), typeof(bool), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// <see cref="PreviewProgress"/> 的依赖属性.
    /// </summary>
    public static readonly DependencyProperty PreviewProgressProperty =
        DependencyProperty.Register(nameof(PreviewProgress), typeof(double), typeof(MpvTransportControls), new PropertyMetadata(default));

    /// <summary>
    /// 信息元素（位于传输控件左侧区域），可以用来展示视频信息.
    /// </summary>
    public object InfoElement
    {
        get => (object)GetValue(InfoElementProperty);
        set => SetValue(InfoElementProperty, value);
    }

    /// <summary>
    /// 下一个按钮是否可用.
    /// </summary>
    public bool IsNextButtonEnabled
    {
        get => (bool)GetValue(IsNextButtonEnabledProperty);
        set => SetValue(IsNextButtonEnabledProperty, value);
    }

    /// <summary>
    /// 上一个按钮是否可用.
    /// </summary>
    public bool IsPreviousButtonEnabled
    {
        get => (bool)GetValue(IsPreviousButtonEnabledProperty);
        set => SetValue(IsPreviousButtonEnabledProperty, value);
    }

    /// <summary>
    /// 视频导航按钮组（上一个/下一个）是否可见.
    /// </summary>
    public bool IsVideoNavigationButtonsVisible
    {
        get => (bool)GetValue(IsVideoNavigationButtonsVisibleProperty);
        set => SetValue(IsVideoNavigationButtonsVisibleProperty, value);
    }

    /// <summary>
    /// 显示在下一个按钮上的提示信息.
    /// </summary>
    public string NextButtonToolTip
    {
        get => (string)GetValue(NextButtonToolTipProperty);
        set => SetValue(NextButtonToolTipProperty, value);
    }

    /// <summary>
    /// 显示在上一个按钮上的提示信息.
    /// </summary>
    public string PreviousButtonToolTip
    {
        get => (string)GetValue(PreviousButtonToolTipProperty);
        set => SetValue(PreviousButtonToolTipProperty, value);
    }

    /// <summary>
    /// 是否显示跳过按钮（跳过前后 10 秒那种）.
    /// </summary>
    public bool IsSkipButtonsVisible
    {
        get => (bool)GetValue(IsSkipButtonsVisibleProperty);
        set => SetValue(IsSkipButtonsVisibleProperty, value);
    }

    /// <summary>
    /// 播放暂停按钮的图标.
    /// </summary>
    public FluentIcons.Common.Symbol PlayPauseSymbol
    {
        get => (FluentIcons.Common.Symbol)GetValue(PlayPauseSymbolProperty);
        set => SetValue(PlayPauseSymbolProperty, value);
    }

    /// <summary>
    /// 播放暂停按钮是否可用.
    /// </summary>
    public bool IsPlayPauseButtonEnabled
    {
        get => (bool)GetValue(IsPlayPauseButtonEnabledProperty);
        set => SetValue(IsPlayPauseButtonEnabledProperty, value);
    }

    /// <summary>
    /// 是否正在缓冲.
    /// </summary>
    public bool IsBuffering
    {
        get => (bool)GetValue(IsBufferingProperty);
        set => SetValue(IsBufferingProperty, value);
    }

    /// <summary>
    /// 音量.
    /// </summary>
    public double Volume
    {
        get => (double)GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }

    /// <summary>
    /// 播放速度.
    /// </summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>
    /// 底部进度条是否可见.
    /// </summary>
    public bool IsProgressVisible
    {
        get => (bool)GetValue(IsProgressVisibleProperty);
        set => SetValue(IsProgressVisibleProperty, value);
    }

    /// <summary>
    /// 时长.
    /// </summary>
    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// 播放进度.
    /// </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// 预览进度条是否可见.
    /// </summary>
    public bool IsPreviewProgressVisible
    {
        get => (bool)GetValue(IsPreviewProgressVisibleProperty);
        set => SetValue(IsPreviewProgressVisibleProperty, value);
    }

    /// <summary>
    /// 预览进度.
    /// </summary>
    public double PreviewProgress
    {
        get => (double)GetValue(PreviewProgressProperty);
        set => SetValue(PreviewProgressProperty, value);
    }
}
