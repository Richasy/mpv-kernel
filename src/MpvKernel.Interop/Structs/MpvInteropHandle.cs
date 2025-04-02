// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel;

/// <summary>
/// Represents a handle to an Mpv instance.
/// </summary>
public struct MpvInteropHandle
{
    /// <summary>
    /// Pointer to this struct. This is used as a unique identifier.
    /// </summary>
    public IntPtr Handle { get; set; }

    /// <summary>
    /// Creates a new MpvHandle instance with a handle set to IntPtr.Zero, representing a null or non-existent handle.
    /// </summary>
    public static MpvInteropHandle None => new MpvInteropHandle { Handle = IntPtr.Zero };

    /// <summary>
    /// Converts an instance of MpvHandle to a boolean value. The result indicates whether the handle is valid based on
    /// its internal pointer.
    /// </summary>
    /// <param name="handle">The parameter represents an instance that is checked for validity by examining its internal pointer.</param>
    public static implicit operator bool(MpvInteropHandle handle) => handle.Handle != IntPtr.Zero;

    /// <summary>
    /// Converts an instance of MpvHandle to a boolean value. The result indicates whether the handle is valid based on
    /// its internal pointer.
    /// </summary>
    /// <param name="handle">The parameter represents an instance that is checked for validity by examining its internal pointer.</param>
    /// <returns>True if the handle is valid, otherwise false.</returns>
    public static bool ToBoolean(MpvInteropHandle handle) => handle.Handle != IntPtr.Zero;
}
