// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.Player;
using Richasy.MpvKernel.Player.Models;

namespace CliSample;

internal sealed class WebSourceResolver : IMpvMediaSourceResolver
{
    public Task<MpvMediaSource> GetSourceAsync()
    {
        const string url = "https://www.tootootool.com/wp-content/uploads/2020/11/big_buck_bunny_720p_1mb.mp4";
        var options = new MpvPlayOptions
        {
            StartPosition = 4,
        };

        return Task.FromResult(new MpvMediaSource(url, options: options));
    }
}
