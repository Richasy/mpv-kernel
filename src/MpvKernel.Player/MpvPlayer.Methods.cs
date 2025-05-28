// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Player.Models;

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
        }, default);
    }

    private void TryShowSubtitle()
        => _subtitleResolver?.ShowSubtitle(Convert.ToInt32(Position));

    private async Task InitializeTracksAsync()
    {
        var tracks = await Client.GetTracksAsync();
        if (tracks.IsSuccess)
        {
            _uiContext.Post(_ =>
            {
                var args = new MpvTrackLoadedEventArgs(tracks.Value);
                TrackLoaded?.Invoke(this, args);
            }, default);
        }
    }
}
