// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

/// <summary>(see mpv_node)</summary>
[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct MpvNodeList
{
    /// <summary>Number of entries. Negative values are not allowed.</summary>
    [FieldOffset(0)]
    public int Num;

    [FieldOffset(8)]
    internal IntPtr _nodesPtr;

    /// <summary>
    /// <para>Mpv_FORMAT_NODE_ARRAY:</para>
    /// <para>unused (typically NULL), access is not allowed</para>
    /// </summary>
    /// <remarks>
    /// <para>Mpv_FORMAT_NODE_MAP:</para>
    /// <para>keys[N] refers to key of the Nth key/value pair. If num &gt; 0, keys[0] to</para>
    /// <para>keys[num-1] (inclusive) are valid. Otherwise, this can be NULL.</para>
    /// <para>The keys are in random order. The only guarantee is that keys[N] belongs</para>
    /// <para>to the value values[N]. NULL keys are not allowed.</para>
    /// </remarks>
    [FieldOffset(16)]
    internal IntPtr _keysPtr;

    /// <summary>
    /// Convert to an array of <see cref="MpvNode"/>.
    /// </summary>
    /// <param name="n"></param>
    public static explicit operator MpvNode[](MpvNodeList n)
    {
        var result = new MpvNode[n.Num];
        for (var i = 0; i < n.Num; i++)
        {
            result[i] = Marshal.PtrToStructure<MpvNode>(n._nodesPtr + i * 16);
        }

        return result;
    }

    /// <summary>
    /// Convert to a dictionary of <see cref="string"/> keys and <see cref="MpvNode"/> values.
    /// </summary>
    public static explicit operator Dictionary<string, MpvNode>?(MpvNodeList n)
    {
        if (n._keysPtr == IntPtr.Zero)
        {
            return null;
        }
        var arr = (MpvNode[])n;
        var result = new Dictionary<string, MpvNode>();
        for (var i = 0; i < n.Num; i++)
        {
            var key = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(n._keysPtr + i * nint.Size));
            result[key!] = arr[i];
        }

        return result;
    }

    /// <summary>
    /// Convert to an array of <see cref="MpvNode"/>.
    /// </summary>
    public static MpvNode[]? ToMpvNodeArray(MpvNodeList n) => (MpvNode[]?)n;

    /// <summary>
    /// Convert to an array of <see cref="MpvNode"/>.
    /// </summary>
    public static Dictionary<string, MpvNode>? ToDictionary(MpvNodeList n) => (Dictionary<string, MpvNode>?)n;
}
