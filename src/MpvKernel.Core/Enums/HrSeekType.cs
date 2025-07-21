// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// Select when to use precise seeks that are not limited to keyframes.
/// </summary>
public enum HrSeekType
{
    /// <summary>
    /// Never use precise seeks.
    /// </summary>
    No,

    /// <summary>
    /// Use precise seeks if the seek is to an absolute position in the file,
    /// such as a chapter seek, but not for relative seeks like the default behavior of arrow keys.
    /// </summary>
    Absolute,

    /// <summary>
    /// Like absolute, but enable hr-seeks in audio-only cases.
    /// The exact behavior is implementation specific and may change with new releases (default).
    /// </summary>
    Default,

    /// <summary>
    /// Use precise seeks whenever possible.
    /// </summary>
    Yes,
}
