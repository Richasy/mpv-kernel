// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

/// <summary>
/// Interop class for the MPV library.
/// </summary>
public static partial class MpvNative
{
    /// <summary>
    /// Initializes the MPV import resolver for the application.
    /// </summary>
    /// <param name="dllPath">Specifies the path to the dynamic link library required for initialization.</param>
    public static void Initialize(string dllPath)
        => MpvImportResolver.Initialize(dllPath);

    /// <summary>
    /// Return a string describing the error. For unknown errors, the string
    /// <para>"unknown error"</para> is returned.
    /// </summary>
    /// <param name="error">Error number, see enum mpv_error</param>
    /// <returns>
    /// A static string describing the error. The string is completely
    /// <para>static, i.e. doesn't need to be deallocated, and is valid forever.</para>
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_error_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string GetErrorString(MpvError error);

    /// <summary>
    /// General function to deallocate memory returned by some of the API functions.
    /// <para>Call this only if it's explicitly documented as allowed. Calling this on</para>
    /// <para>mpv memory not owned by the caller will lead to undefined behavior.</para>
    /// </summary>
    /// <param name="data">A valid pointer returned by the API, or <c>null</c>.</param>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_free")]
    public static partial void Free(IntPtr data);

    /// <summary>
    /// Retrieves the version of the client API for the MPV library.
    /// </summary>
    /// <returns>Returns the API version as an unsigned long integer.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_client_api_version")]
    public static partial ulong GetClientApiVersion();
}
