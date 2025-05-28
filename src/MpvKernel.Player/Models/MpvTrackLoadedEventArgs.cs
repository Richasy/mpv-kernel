// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Models;

namespace Richasy.MpvKernel.Player.Models;

/// <summary>
/// MPV 轨道加载事件参数.
/// </summary>
public sealed class MpvTrackLoadedEventArgs(List<MpvTrackInfo> tracks) : EventArgs
{
    /// <summary>
    /// 轨道列表.
    /// </summary>
    public List<MpvTrackInfo> Tracks { get; } = tracks;
}
