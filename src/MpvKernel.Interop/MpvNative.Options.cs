// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Set an option. Note that you can't normally set options during runtime. It
    /// works in uninitialized state (see mpv_create()), and in some cases at
    /// runtime.
    /// <para>
    /// Using a format other than <c>MPV_FORMAT_NODE</c> is equivalent to constructing a
    /// <c>mpv_node</c> with the given format and data, and passing the <c>mpv_node</c> to this
    /// function.
    /// </para>
    /// <para>
    /// Note: this is semi-deprecated. For most purposes, this is not needed anymore.
    /// Starting with mpv version 0.21.0 (version 1.23) most options can be set
    /// with <c>mpv_set_property()</c> (and related functions), and even before
    /// <c>mpv_initialize()</c>. In some obscure corner cases, using this function
    /// to set options might still be required (see
    /// "Inconsistencies between options and properties" in the manpage). Once
    /// these are resolved, the option setting functions might be fully
    /// deprecated.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="name">Option name. This is the same as on the mpv command line, but
    /// without the leading "--".</param>
    /// <param name="format">See enum mpv_format.</param>
    /// <param name="data">Option value (according to the format).</param>
    /// <returns>Error code.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_option", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetOption(MpvInteropHandle handle, string name, MpvFormat format, ref MpvNode data);

    /// <summary>
    /// Convenience function to set an option to a string value.
    /// <para>This is like calling <c>mpv_set_option()</c> with <c>MPV_FORMAT_STRING</c>.</para>
    /// </summary>
    /// <returns>Error code</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_option_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetOptionString(MpvInteropHandle handle, string name, string value);
}
