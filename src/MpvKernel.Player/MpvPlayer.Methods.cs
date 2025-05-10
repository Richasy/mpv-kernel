// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Player;

public sealed partial class MpvPlayer
{
    private void CheckStateProperties()
    {
        _uiContext.Post(_ =>
        {
            IsPlaying = PlaybackState == Core.Enums.MpvPlayerState.Playing;
            IsBuffering = PlaybackState is Core.Enums.MpvPlayerState.Buffering or Core.Enums.MpvPlayerState.Seeking;
            IsStopped = PlaybackState is Core.Enums.MpvPlayerState.Idle or Core.Enums.MpvPlayerState.End;
            IsLoading = !IsPlaying && !IsStopped && !IsBuffering;
        }, default);
    }

    private void TryShowSubtitle()
        => _subtitleResolver?.ShowSubtitle(Convert.ToInt32(Position));
}
