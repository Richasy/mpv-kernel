using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

/// <summary>
/// Mpv event property.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MpvEventProperty
{
    /// <summary>
    /// Stores the name as a string. It represents the identity of an object or entity.
    /// </summary>
    public string Name;

    /// <summary>
    /// Format.
    /// </summary>
    public MpvFormat Format;

    /// <summary>
    /// Data.
    /// </summary>
    public IntPtr DataPtr; //Expand to all formats
}