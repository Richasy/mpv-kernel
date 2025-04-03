// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Send a command to the player. Commands are the same as those used in
    /// input.conf, except that this function takes parameters in a pre-split
    /// form.
    /// <para>
    /// The commands and their parameters are documented in input.rst.
    /// </para>
    /// <para>
    /// Does not use OSD and string expansion by default (unlike <see cref="SetCommandString(MpvInteropHandle, string)"/>
    /// and input.conf).
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="args">
    /// <para>
    /// NULL-terminated list of strings. Usually, the first item
    /// is the command, and the following items are arguments.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// Error code.
    /// </para>
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetCommand(MpvInteropHandle handle, string[] args);

    /// <summary>
    /// Same as mpv_command, but uses input.conf parsing for splitting arguments.
    /// This is slightly simpler, but also more error prone, since arguments may
    /// need quoting/escaping.
    /// <para>
    /// This also has OSD and string expansion enabled by default.
    /// </para>
    /// </summary>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetCommandString(MpvInteropHandle handle, string args);

    /// <summary>
    /// Same as mpv_command(), but allows passing structured data in any format.
    /// <para>In particular, calling mpv_command() is exactly like calling
    /// mpv_command_node() with the format set to MPV_FORMAT_NODE_ARRAY, and
    /// every arg passed in order as MPV_FORMAT_STRING.</para>
    /// <para>Does not use OSD and string expansion by default.</para>
    /// <para>The args argument can have one of the following formats:</para>
    /// <para>MPV_FORMAT_NODE_ARRAY:</para>
    /// <para>Positional arguments. Each entry is an argument using an arbitrary
    /// format (the format must be compatible to the used command). Usually,
    /// the first item is the command name (as <c>MPV_FORMAT_STRING</c>). The order
    /// of arguments is as documented in each command description.</para>
    /// <para>MPV_FORMAT_NODE_MAP:</para>
    /// <para>Named arguments. This requires at least an entry with the key "name"
    /// to be present, which must be a string, and contains the command name.
    /// The special entry "_flags" is optional, and if present, must be an
    /// array of strings, each being a command prefix to apply. All other
    /// entries are interpreted as arguments. They must use the argument names
    /// as documented in each command description. Some commands do not
    /// support named arguments at all, and must use <c>MPV_FORMAT_NODE_ARRAY</c>.</para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="command">mpv_node with format set to one of the values documented
    /// above (see there for details)</param>
    /// <param name="result">Optional, pass null if unused. If not null, and if the
    /// function succeeds, this is set to command-specific return
    /// data. You must call mpv_free_node_contents() to free it
    /// (again, only if the command actually succeeds).
    /// Not many commands actually use this at all.</param>
    /// <returns>error code (the result parameter is not set on error)</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command_node")]
    public static partial MpvError SetCommandNode(MpvInteropHandle handle, ref MpvNode command, out MpvNode result);

    /// <summary>
    /// This is essentially identical to mpv_command() but it also returns a result.
    /// <para>
    /// Does not use OSD and string expansion by default.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="args">
    /// NULL-terminated list of strings. Usually, the first item
    /// is the command, and the following items are arguments.
    /// </param>
    /// <param name="result">
    /// Optional, pass NULL if unused. If not NULL, and if the
    /// function succeeds, this is set to command-specific return
    /// data. You must call mpv_free_node_contents() to free it
    /// (again, only if the command actually succeeds).
    /// <para>
    /// Not many commands actually use this at all.
    /// </para>
    /// </param>
    /// <returns>
    /// Error code (the result parameter is not set on error)
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command_ret", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetCommandRet(MpvInteropHandle handle, string[] args, out MpvNode result);

    /// <summary>
    /// Same as mpv_command, but run the command asynchronously.
    /// <para>
    /// Commands are executed asynchronously. You will receive a 
    /// MPV_EVENT_COMMAND_REPLY event. This event will also have an 
    /// error code set if running the command failed. For commands that 
    /// return data, the data is put into mpv_event_command.result.
    /// </para>
    /// <para>
    /// The only case when you do not receive an event is when the function call 
    /// itself fails. This happens only if parsing the command itself (or otherwise 
    /// validating it) fails, i.e. the return code of the API call is not 0 or 
    /// positive.
    /// </para>
    /// <para>
    /// Safe to be called from mpv render API threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">
    /// the value mpv_event.reply_userdata of the reply will
    /// be set to (see section about asynchronous calls)
    /// </param>
    /// <param name="args">
    /// NULL-terminated list of strings (see mpv_command())
    /// </param>
    /// <returns>
    /// error code (if parsing or queuing the command fails)
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command_async", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetCommandAsync(MpvInteropHandle handle, ulong replyUserData, string[] args);

    /// <summary>
    /// Same as <see cref="SetCommandNode(MpvInteropHandle, ref MpvNode, out MpvNode)"/>, but run it asynchronously.
    /// Basically, this function is to <see cref="SetCommandNode(MpvInteropHandle, ref MpvNode, out MpvNode)"/> what <see cref="SetCommandAsync(MpvInteropHandle, ulong, string[])"/> is to
    /// <see cref="SetCommand(MpvInteropHandle, string[])"/>.
    ///
    /// <para>
    /// See <see cref="SetCommandAsync(MpvInteropHandle, ulong, string[])"/> for details.
    /// </para>
    ///
    /// <para>
    /// Safe to be called from mpv render API threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">
    /// The value <see cref="MpvEvent.UserData"/> of the reply will
    /// be set to (see section about asynchronous calls).
    /// </param>
    /// <param name="args">
    /// As in <see cref="MpvNode"/>
    /// </param>
    /// <returns>
    /// Error code (if parsing or queuing the command fails)
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_command_node_async")]
    public static partial MpvError SetCommandNodeAsync(MpvInteropHandle handle, ulong replyUserData, ref MpvNode args);

    /// <summary>
    /// Signal to all async requests with the matching ID to abort. This affects
    /// the following API calls:
    /// <para>
    ///      mpv_command_async
    ///      mpv_command_node_async
    /// </para>
    /// All of these functions take a reply_userdata parameter. This API function
    /// tells all requests with the matching reply_userdata value to try to return
    /// as soon as possible. If there are multiple requests with matching ID, it
    /// aborts all of them.
    /// <para>
    /// This API function is mostly asynchronous itself. It will not wait until the
    /// command is aborted. Instead, the command will terminate as usual, but with
    /// some work not done. How this is signaled depends on the specific command (for
    /// example, the "subprocess" command will indicate it by <c>true</c> in the result). 
    /// How long it takes also depends on the situation. The aborting process is completely 
    /// asynchronous.
    /// </para>
    /// <para>
    /// Not all commands may support this functionality. In this case, this function
    /// will have no effect. The same is true if the request using the passed
    /// reply_userdata has already terminated, has not been started yet, or was
    /// never in use at all.</para>
    /// <para>
    /// You have to be careful of race conditions: the time during which the abort
    /// request will be effective is _after_ e.g. <c>mpv_command_async()</c> has returned,
    /// and before the command has signaled completion with <c>MPV_EVENT_COMMAND_REPLY</c>.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">ID of the request to be aborted (see above)</param>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_abort_async_command")]
    public static partial void AbortAsyncCommand(MpvInteropHandle handle, ulong replyUserData);
}
