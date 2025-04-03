// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// Defines operation modes for the MPV player.
/// </summary>
public enum MpvPlayerOperationMode
{
    /// <summary>
    /// Default mode.
    /// </summary>
    CPlayer,

    /// <summary>
    /// Pseudo GUI mode.
    /// </summary>
    /// <remarks>
    /// https://mpv.io/manual/stable/#pseudo-gui-mode
    /// </remarks>
    PseudoGui,
}
