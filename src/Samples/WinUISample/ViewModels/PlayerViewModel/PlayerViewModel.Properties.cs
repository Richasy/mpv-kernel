// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Player;
using WinUISample.Forms;

namespace WinUISample.ViewModels;

/// <summary>
/// 播放器视图模型.
/// </summary>
public sealed partial class PlayerViewModel
{
    private MpvClient? _client;
    private MpvPlayerWindow? _playerWindow;

    [ObservableProperty]
    public partial MpvPlayer Player { get; set; }

    [ObservableProperty]
    public partial string? PositionText { get; set; }

    [ObservableProperty]
    public partial string? DurationText { get; set; }
}
