// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

namespace Richasy.MpvKernel.WinUI.Controls;

/// <summary>
/// MPV 传输控件.
/// </summary>
public partial class MpvTransportControls : Control
{
    /// <summary>
    /// 创建 <see cref="MpvTransportControls"/> 类的新实例.
    /// </summary>
    public MpvTransportControls()
    {
        DefaultStyleKey = typeof(MpvTransportControls);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        InitializePrevButton();
        InitializeNextButton();
        InitializeBackwardButton();
        InitializeForwardButton();
        InitializePlayPauseButton();
    }

    private void InitializePrevButton()
    {
        var prevButton = GetTemplateChild("PrevButton") as Button;
        if (prevButton is not null)
        {
            prevButton.Click += OnPrevButtonClick;
        }
    }

    private void InitializeNextButton()
    {
        var nextButton = GetTemplateChild("NextButton") as Button;
        if (nextButton is not null)
        {
            nextButton.Click += OnNextButtonClick;
        }
    }

    private void InitializeBackwardButton()
    {
        var backwardButton = GetTemplateChild("BackwardButton") as Button;
        if (backwardButton is not null)
        {
            backwardButton.Click += OnBackwardButtonClick;
        }
    }

    private void InitializeForwardButton()
    {
        var forwardButton = GetTemplateChild("ForwardButton") as Button;
        if (forwardButton is not null)
        {
            forwardButton.Click += OnForwardButtonClick;
        }
    }

    private void InitializePlayPauseButton()
    {
        var playPauseButton = GetTemplateChild("PlayPauseButton") as Button;
        if (playPauseButton is not null)
        {
            playPauseButton.Click += OnPlayPauseButtonClick;
        }
    }

    private void OnPlayPauseButtonClick(object sender, RoutedEventArgs e)
        => PlayPauseButtonClick?.Invoke(sender, EventArgs.Empty);

    private void OnPrevButtonClick(object sender, RoutedEventArgs e)
        => PrevButtonClick?.Invoke(sender, EventArgs.Empty);

    private void OnNextButtonClick(object sender, RoutedEventArgs e)
        => NextButtonClick?.Invoke(sender, EventArgs.Empty);

    private void OnBackwardButtonClick(object sender, RoutedEventArgs e)
        => BackwardSkipButtonClick?.Invoke(sender, EventArgs.Empty);

    private void OnForwardButtonClick(object sender, RoutedEventArgs e)
        => ForwardSkipButtonClick?.Invoke(sender, EventArgs.Empty);
}
