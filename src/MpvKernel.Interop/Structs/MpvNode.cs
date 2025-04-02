// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

/// <summary>Generic data storage.</summary>
/// <remarks>
/// <para>If mpv writes this struct (e.g. via mpv_get_property()), you must not change</para>
/// <para>the data. In some cases (mpv_get_property()), you have to free it with</para>
/// <para>mpv_free_node_contents(). If you fill this struct yourself, you're also</para>
/// <para>responsible for freeing it, and you must not call mpv_free_node_contents().</para>
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct MpvNode
{
    /// <summary>valid if format==Mpv_FORMAT_STRING</summary>
    public string? StringValue => Marshal.PtrToStringUTF8(_structuredValue);

    [FieldOffset(0)]
    internal IntPtr _structuredValue;

    /// <summary>valid if format==Mpv_FORMAT_FLAG</summary>
    /// <remarks>0 = no; 1 = yes</remarks>
    [FieldOffset(0)]
    public readonly int Flag;

    /// <summary>valid if format==Mpv_FORMAT_INT64</summary>
    [FieldOffset(0)]
    public readonly long IntegerValue;

    /// <summary>valid if format==Mpv_FORMAT_DOUBLE</summary>
    [FieldOffset(0)]
    public readonly double DoubleValue;

    /// <summary>valid if format==Mpv_FORMAT_NODE_ARRAY</summary>
    /// <summary>or if format==Mpv_FORMAT_NODE_MAP</summary>
    public MpvNodeList RemoteNodeListValue => Marshal.PtrToStructure<MpvNodeList>(_structuredValue);

    /// <summary>valid if format==Mpv_FORMAT_BYTE_ARRAY</summary>
    public MpvByteArray ByteArrayValue => Marshal.PtrToStructure<MpvByteArray>(_structuredValue);

    /// <summary>
    /// <para>Type of the data stored in this struct. This value rules what members in</para>
    /// <para>the given union can be accessed. The following formats are currently</para>
    /// <para>defined to be allowed in mpv_node:</para>
    /// </summary>
    /// <remarks>
    /// <para>Mpv_FORMAT_STRING       (u.string)</para>
    /// <para>Mpv_FORMAT_FLAG         (u.flag)</para>
    /// <para>Mpv_FORMAT_INT64        (u.int64)</para>
    /// <para>Mpv_FORMAT_DOUBLE       (u.double_)</para>
    /// <para>Mpv_FORMAT_NODE_ARRAY   (u.list)</para>
    /// <para>Mpv_FORMAT_NODE_MAP     (u.list)</para>
    /// <para>Mpv_FORMAT_BYTE_ARRAY   (u.ba)</para>
    /// <para>Mpv_FORMAT_NONE         (no member)</para>
    /// <para>If you encounter a value you don't know, you must not make any</para>
    /// <para>assumptions about the contents of union u.</para>
    /// </remarks>
    [FieldOffset(8)]
    public MpvFormat Format;

    /// <summary>
    /// Initializes a new instance of the MpvNode class with a string value. It sets the format to String and converts
    /// the input to UTF-8.
    /// </summary>
    /// <param name="value">The input string that will be converted to a UTF-8 encoded format for internal representation.</param>
    public MpvNode(string? value)
    {
        Format = MpvFormat.String;
        _structuredValue = Marshal.StringToCoTaskMemUTF8(value);
    }

    /// <summary>
    /// Initializes a new instance of the MpvNode class with a specified boolean value. The format is set to Flag, and
    /// the flag is assigned based on the boolean input.
    /// </summary>
    /// <param name="value">A boolean input that determines whether the flag is set to 1 for true or 0 for false.</param>
    public MpvNode(bool value)
    {
        Format = MpvFormat.Flag;
        Flag = value ? 1 : 0;
    }

    /// <summary>
    /// Initializes a new instance of MpvNode with a 64-bit integer value.
    /// </summary>
    /// <param name="value">The long integer to be stored in the node.</param>
    public MpvNode(long value)
    {
        Format = MpvFormat.Int64;
        IntegerValue = value;
    }

    /// <summary>
    /// Initializes an instance of MpvNode with a double value and sets its format to Double.
    /// </summary>
    /// <param name="value">The numeric value to be stored in the node.</param>
    public MpvNode(double value)
    {
        Format = MpvFormat.Double;
        DoubleValue = value;
    }

    /// <summary>
    /// Initializes a new instance of the MpvNode class using a provided list of nodes. It allocates memory for the
    /// structured value.
    /// </summary>
    /// <param name="value">The list of nodes to be converted into a structured format for further processing.</param>
    public MpvNode(MpvNodeList value)
    {
        Format = value._keysPtr != IntPtr.Zero ? MpvFormat.NodeMap : MpvFormat.NodeArray;
        _structuredValue = Marshal.AllocCoTaskMem(Marshal.SizeOf<MpvNodeList>());
        Marshal.StructureToPtr(value, _structuredValue, false);
    }

    /// <summary>
    /// Initializes a new instance of the MpvNode class with a byte array value.
    /// </summary>
    /// <param name="value">The byte array is used to set the structured value of the node.</param>
    public MpvNode(MpvByteArray value)
    {
        Format = MpvFormat.ByteArray;
        _structuredValue = Marshal.AllocCoTaskMem(Marshal.SizeOf<MpvByteArray>());
        Marshal.StructureToPtr(value, _structuredValue, false);
    }
}
