// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// Audio channel layout type enumeration.
/// </summary>
public enum AudioChannelLayoutType
{
    /// <summary>
    /// Send the audio device whatever it accepts, preferring the audio's original channel layout.
    /// Can cause issues with HDMI (see the warning below).
    /// </summary>
    Auto,

    /// <summary>
    /// Force a downmix to stereo. These are special-cases of the previous item.
    /// </summary>
    Stereo,

    /// <summary>
    ///Force a downmix to mono. These are special-cases of the previous item.
    /// </summary>
    Mono,

    /// <summary>
    /// List of ,-separated channel layouts which should be allowed.
    /// Technically, this only adjusts the filter chain output to the best matching layout in the list,
    /// and passes the result to the audio API.
    /// It's possible that the audio API will select a different channel layout.
    /// </summary>
    Custom,
}
