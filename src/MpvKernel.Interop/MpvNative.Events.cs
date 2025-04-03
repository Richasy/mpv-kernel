// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Enable or disable the given event.
    /// <para>Some events are enabled by default. Some events can't be disabled.</para>
    /// <para>Informational note: currently, all events are enabled by default, except <c>MPV_EVENT_TICK</c>.</para>
    /// <para>Safe to be called from mpv render API threads.</para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="eventId">See enum mpv_event_id.</param>
    /// <param name="enabled"><c>true</c> to enable receiving this event, <c>false</c> to disable it.</param>
    /// <returns>Error code.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_request_event")]
    public static partial MpvError RequestEvent(MpvInteropHandle handle, MpvEventId eventId, int enabled);

    /// <summary>
    /// Wait for the next event, or until the timeout expires, or if another thread
    /// makes a call to <c>mpv_wakeup()</c>. Passing 0 as timeout will never wait, and
    /// is suitable for polling.
    /// <para>
    /// The internal event queue has a limited size (per client handle). If you
    /// don't empty the event queue quickly enough with <c>mpv_wait_event()</c>, it will
    /// overflow and silently discard further events. If this happens, making
    /// asynchronous requests will fail as well (with <c>MPV_ERROR_EVENT_QUEUE_FULL</c>).
    /// </para>
    /// <para>
    /// Only one thread is allowed to call this on the same <c>mpv_handle</c> at a time.
    /// The API won't complain if more than one thread calls this, but it will cause
    /// race conditions in the client when accessing the shared <c>mpv_event</c> struct.
    /// Note that most other API functions are not restricted by this, and no API
    /// function internally calls <c>mpv_wait_event()</c>. Additionally, concurrent calls
    /// to different <c>mpv_handles</c> are always safe.
    /// </para>
    /// <para>
    /// As long as the timeout is 0, this is safe to be called from <c>mpv</c> render API
    /// threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="timeout">
    /// Timeout in seconds, after which the function returns even if
    /// no event was received. A <c>MPV_EVENT_NONE</c> is returned on
    /// timeout. A value of 0 will disable waiting. Negative values
    /// will wait with an infinite timeout.
    /// </param>
    /// <returns>
    /// A struct containing the event ID and other data. The pointer (and
    /// fields in the struct) stay valid until the next <c>mpv_wait_event()</c>
    /// call, or until the <c>mpv_handle</c> is destroyed. You must not write to
    /// the struct, and all memory referenced by it will be automatically
    /// released by the API on the next <c>mpv_wait_event()</c> call, or when the
    /// context is destroyed. The return value is never <c>NULL</c>.
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_wait_event")]
    public static partial IntPtr WaitEvent(MpvInteropHandle handle, double timeout);

    /// <summary>
    /// Interrupt the current <c>mpv_wait_event()</c> call. This will wake up the thread
    /// currently waiting in <c>mpv_wait_event()</c>. If no thread is waiting, the next
    /// <c>mpv_wait_event()</c> call will return immediately. <para>This is to avoid lost
    /// wakeups.</para>
    /// </summary>
    /// <remarks>
    /// <para><c>mpv_wait_event()</c> will receive a <c>MPV_EVENT_NONE</c> if it's woken up due to 
    /// this call. However, note that this dummy event might be skipped if there are
    /// already other events queued. All that matters is that the waiting thread
    /// is woken up.</para>
    /// <para>It is safe to be called from mpv render API threads.</para>
    /// </remarks>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_wakeup")]
    public static partial void Wakeup(MpvInteropHandle handle);

    /// <summary>
    /// Set a custom function that should be called when there are new events. Use
    /// this if blocking in <c>mpv_wait_event()</c> to wait for new events is not feasible.
    /// <para>
    /// Keep in mind that the callback will be called from foreign threads. You
    /// must not make any assumptions of the environment, and you must return as
    /// soon as possible (i.e. no long blocking waits). Exiting the callback through
    /// any other means than a normal return is forbidden (no throwing exceptions,
    /// no <c>longjmp()</c> calls). You must not change any local thread state (such as
    /// the C floating point environment).
    /// </para>
    /// <para>
    /// You are not allowed to call any client API functions inside of the callback.
    /// In particular, you should not do any processing in the callback, but wake up
    /// another thread that does all the work. The callback is meant strictly for
    /// notification only, and is called from arbitrary core parts of the player,
    /// that make no considerations for reentrant API use or allowing the callee to
    /// spend a lot of time doing other things. Keep in mind that it's also possible
    /// that the callback is called from a thread while a <c>mpv API</c> function is called
    /// (i.e. it can be reentrant).
    /// </para>
    /// <para>
    /// In general, the client API expects you to call <c>mpv_wait_event()</c> to receive
    /// notifications, and the wakeup callback is merely a helper utility to make
    /// this easier in certain situations. Note that it's possible that there's
    /// only one wakeup callback invocation for multiple events. You should call
    /// <c>mpv_wait_event()</c> with no timeout until <c>MPV_EVENT_NONE</c> is reached, at which
    /// point the event queue is empty.
    /// </para>
    /// <para>
    /// If you actually want to do processing in a callback, spawn a thread that
    /// does nothing but call <c>mpv_wait_event()</c> in a loop and dispatches the result
    /// to a callback.
    /// </para>
    /// <para>
    /// Only one wakeup callback can be set.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="callback">Function that should be called if a wakeup is required</param>
    /// <param name="userData">Arbitrary userdata passed to <paramref name="callback"/></param>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_wakeup_callback")]
    public static partial void SetWakeupCallBack(MpvInteropHandle handle, MpvWakeupCallback<IntPtr> callback, IntPtr userData);

    /// <summary>
    /// Block until all asynchronous requests are done. This affects functions like
    /// <see cref="SetCommandAsync(MpvInteropHandle, ulong, string[])"/>, which return immediately and return their result as
    /// events.
    /// <para>
    /// This is a helper, and somewhat equivalent to calling <see cref="WaitEvent(MpvInteropHandle, double)"/> in a
    /// loop until all known asynchronous requests have sent their reply as event,
    /// except that the event queue is not emptied.
    /// </para>
    /// </summary>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_wait_async_requests")]
    public static partial void WaitAsyncRequests(MpvInteropHandle handle);
}

/// <summary>
/// Defines a delegate that represents a method to be called when a wakeup event occurs.
/// </summary>
/// <typeparam name="T">Specifies the type of data that will be passed to the callback method.</typeparam>
/// <param name="data">Contains the information that will be provided to the callback when the event is triggered.</param>
public delegate void MpvWakeupCallback<in T>(T data);