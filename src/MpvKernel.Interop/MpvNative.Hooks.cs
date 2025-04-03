// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// A hook is like a synchronous event that blocks the player.
    /// <para>You register a hook handler with this function. You will get an event, which you need to handle, and once things are ready, you can let the player continue with mpv_hook_continue().</para>
    /// <para>Currently, hooks can't be removed explicitly. But they will be implicitly removed if the mpv_handle it was registered with is destroyed. This also continues the hook if it was being handled by the destroyed mpv_handle (but this should be avoided, as it might mess up order of hook execution).</para>
    /// <para>Hook handlers are ordered globally by priority and order of registration. Handlers for the same hook with same priority are invoked in order of registration (the handler registered first is run first). Handlers with lower priority are run first (which seems backward).</para>
    /// <para>See the "Hooks" section in the manpage to see which hooks are currently defined.</para>
    /// <para>Some hooks might be reentrant (so you get multiple MPV_EVENT_HOOK for the same hook). If this can happen for a specific hook type, it will be explicitly documented in the manpage.</para>
    /// <para>Only the mpv_handle on which this was called will receive the hook events, or can "continue" them.</para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">This will be used for the <c>mpv_event.reply_userdata</c> field for the received MPV_EVENT_HOOK events. If you have no use for this, pass 0.</param>
    /// <param name="name">The hook name. This should be one of the documented names. But if the name is unknown, the hook event will simply be never raised.</param>
    /// <param name="priority">See remarks above. Use 0 as a neutral default.</param>
    /// <returns>error code (usually fails only on OOM)</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_hook_add", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError HookAdd(MpvInteropHandle handle, ulong replyUserData, string name, int priority);

    /// <summary>
    /// Respond to a MPV_EVENT_HOOK event. You must call this after you have handled
    /// the event. There is no way to "cancel" or "stop" the hook.
    /// <para>
    /// Calling this will typically unblock the player for whatever the hook
    /// is responsible for (e.g. for the "on_load" hook it lets it continue
    /// playback).
    /// </para>
    /// <para>
    /// It is explicitly undefined behavior to call this more than once for each
    /// MPV_EVENT_HOOK, to pass an incorrect ID, or to call this on a <c>mpv_handle</c>
    /// different from the one that registered the handler and received the event.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="id">
    /// This must be the value of the mpv_event_hook.id field for the
    /// corresponding MPV_EVENT_HOOK.
    /// </param>
    /// <returns>
    /// Error code.
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_hook_continue")]
    public static partial MpvError HookContinue(MpvInteropHandle handle, ulong id);
}
