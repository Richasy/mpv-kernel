// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Player;
using Richasy.MpvKernel.Player.Models;

namespace WinUISample.Models;

internal abstract class MediaSourceResolverBase : IMpvMediaSourceResolver
{
    public IntPtr WindowHandle { get; set; }

    public abstract Task<MpvMediaSource> GetSourceAsync();
}
