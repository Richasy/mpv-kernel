// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Enable or disable receiving of log messages. These are the messages the
    /// command line player prints to the terminal. This call sets the minimum
    /// required log level for a message to be received with MPV_EVENT_LOG_MESSAGE.
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="minLevel">
    /// Minimal log level as string. Valid log levels:
    /// <para>No</para>
    /// <para>fatal</para>
    /// <para>error</para>
    /// <para>warn</para>
    /// <para>info</para>
    /// <para>v</para>
    /// <para>debug</para>
    /// <para>trace</para>
    /// The value <c>no</c> disables all messages. This is the default.
    /// An exception is the value <c>terminal-default</c>, which uses the
    /// log level as set by the <c>--msg-level</c> option. This works
    /// even if the terminal is disabled. (Since API version 1.19.)
    /// Also see mpv_log_level.
    /// </param>
    /// <returns>Error code</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_request_log_messages", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError RequestLogMessages(MpvInteropHandle handle, string minLevel);
}
