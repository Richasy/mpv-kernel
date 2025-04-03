// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Load a config file.
    /// <para>This loads and parses the file, and sets every entry in the config file's default section
    /// as if <c>mpv_set_option_string()</c> is called.</para>
    /// <para>The filename should be an absolute path. If it isn't, the actual path used is unspecified.
    /// (Note: an absolute path starts with '/' on UNIX.) If the file wasn't found,
    /// <c>MPV_ERROR_INVALID_PARAMETER</c> is returned.</para>
    /// <para>If a fatal error happens when parsing a config file, <c>MPV_ERROR_OPTION_ERROR</c> is returned.
    /// Errors when setting options as well as other types of errors are ignored (even if options do not exist).
    /// You can still try to capture the resulting error messages with <c>mpv_request_log_messages()</c>.
    /// Note that it's possible that some options were successfully set even if any of these errors happen.</para>
    /// </summary>
    /// <param name="handle">The client handle.</param>
    /// <param name="filename">Absolute path to the config file on the local filesystem.</param>
    /// <returns>Error code.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_load_config_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError LoadConfigFile(MpvInteropHandle handle, string filename);
}
