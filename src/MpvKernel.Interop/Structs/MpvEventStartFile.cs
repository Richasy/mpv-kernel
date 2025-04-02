using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

/// <summary>Since API version 1.108.</summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct MpvEventStartFile
{
    /// <summary>Playlist entry ID of the file being loaded now.</summary>
    public long PlaylistEntryId;
}