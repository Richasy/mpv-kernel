// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Create a new mpv instance and an associated client API handle to control
    /// the mpv instance. This instance is in a pre-initialized state,
    /// and needs to be initialized to be actually used with most other API
    /// functions.
    /// <para>
    /// Some API functions will return MPV_ERROR_UNINITIALIZED in the uninitialized
    /// state. You can call mpv_set_property() (or mpv_set_property_string() and
    /// other variants, and before mpv 0.21.0 mpv_set_option() etc.) to set initial
    /// options. After this, call mpv_initialize() to start the player, and then use
    /// e.g. mpv_command() to start playback of a file.
    /// </para>
    /// <para>
    /// The point of separating handle creation and actual initialization is that
    /// you can configure things which can't be changed during runtime.
    /// </para>
    /// <para>
    /// Unlike the command line player, this will have initial settings suitable
    /// for embedding in applications. The following settings are different:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>stdin/stdout/stderr and the terminal will never be accessed. This is
    ///         equivalent to setting the <c>--no-terminal</c> option.
    ///         (Technically, this also suppresses C signal handling.)</description>
    ///     </item>
    ///     <item>
    ///         <description>No config files will be loaded. This is roughly equivalent to using
    ///         <c>--config=no</c>. Since libmpv 1.15, you can actually re-enable this option,
    ///         which will make libmpv load config files during mpv_initialize(). If you
    ///         do this, you are strongly encouraged to set the "config-dir" option too.
    ///         (Otherwise it will load the mpv command line player's config.)
    ///         For example:
    ///         <code>
    ///         mpv_set_option_string(mpv, "config-dir", "/my/path"); // set config root
    ///         mpv_set_option_string(mpv, "config", "yes"); // enable config loading
    ///         (call mpv_initialize() <c>_after_</c> this)
    ///         </code>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Idle mode is enabled, which means the playback core will enter idle mode
    ///         if there are no more files to play on the internal playlist, instead of
    ///         exiting. This is equivalent to the <c>--idle</c> option.</description>
    ///     </item>
    ///     <item>
    ///         <description>Disable parts of input handling.</description>
    ///     </item>
    ///     <item>
    ///         <description>Most of the different settings can be viewed with the command line player
    ///         by running <c>"mpv --show-profile=libmpv"</c>.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    /// All this assumes that API users want a mpv instance that is strictly
    /// isolated from the command line player's configuration, user settings, and
    /// so on. You can re-enable disabled features by setting the appropriate
    /// options.
    /// </para>
    /// <para>
    /// The mpv command line parser is not available through this API, but you can
    /// set individual options with mpv_set_property(). Files for playback must be
    /// loaded with mpv_command() or others.
    /// </para>
    /// <para>
    /// Note that you should avoid doing concurrent accesses on the uninitialized
    /// client handle. (Whether concurrent access is definitely allowed or not has
    /// yet to be decided.)
    /// </para>
    /// </summary>
    /// <returns>a new mpv client API handle. Returns <c>NULL</c> on error. Currently, this
    /// can happen in the following situations:
    /// <list type="bullet">
    ///     <item>
    ///         <description>out of memory</description>
    ///     </item>
    ///     <item>
    ///         <description><c>LC_NUMERIC</c> is not set to <c>"C"</c> (see general remarks)</description>
    ///     </item>
    /// </list>
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_create")]
    public static partial MpvInteropHandle Create();

    /// <summary>
    /// Initialize an uninitialized mpv instance. If the mpv instance is already
    /// running, an error is returned.
    /// <para>
    /// This function needs to be called to make full use of the client API if the
    /// client API handle was created with mpv_create().
    /// </para>
    /// <para>
    /// Only the following options are required to be set <c>before</c> mpv_initialize():
    /// </para>
    /// <para>
    /// - Options which are only read at initialization time:
    ///   - config
    ///   - config-dir
    ///   - input-conf
    ///   - load-scripts
    ///   - script
    ///   - player-operation-mode
    ///   - input-app-events (macOS)
    /// </para>
    /// <para>
    /// - All encoding mode options
    /// </para>
    /// </summary>
    /// <returns><see cref="MpvError"/> error code</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_initialize")]
    public static partial MpvError Initialize(MpvInteropHandle handle);

    /// <summary>
    /// Create a new client handle connected to the same player core as <paramref name="handle"/>.
    /// <para>
    /// This context has its own event queue, its own <c>mpv_request_event()</c> state, its own
    /// <c>mpv_request_log_messages()</c> state, its own set of observed properties, and
    /// its own state for asynchronous operations. Otherwise, everything is shared.
    /// </para>
    /// <para>
    /// This handle should be destroyed with <c>mpv_destroy()</c> if no longer
    /// needed. The core will live as long as there is at least one handle referencing
    /// it. Any handle can make the core quit, which will result in every handle
    /// receiving <c>MPV_EVENT_SHUTDOWN</c>.
    /// </para>
    /// <para>
    /// This function cannot be called before the main handle was initialized with
    /// <c>mpv_initialize()</c>. The new handle is always initialized, unless <paramref name="handle"/> = <c>null</c> was
    /// passed.
    /// </para>
    /// </summary>
    /// <param name="handle">
    /// Used to get the reference to the mpv core; handle-specific
    /// settings and parameters are not used.
    /// If <c>null</c>, this function behaves like <c>mpv_create()</c> (ignores <paramref name="name"/>).
    /// </param>
    /// <param name="name">
    /// The client name. This will be returned by <c>mpv_client_name()</c>. If
    /// the name is already in use, or contains non-alphanumeric
    /// characters (other than '_'), the name is modified to fit.
    /// If <c>null</c>, an arbitrary name is automatically chosen.
    /// </param>
    /// <returns>A new handle, or <c>null</c> on error.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_create_client", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvInteropHandle CreateClient(MpvInteropHandle handle, string name);

    /// <summary>
    /// This is the same as mpv_create_client(), but the created mpv_handle is
    /// treated as a weak reference. If all mpv_handles referencing a core are
    /// weak references, the core is automatically destroyed.
    /// <para>
    /// This still goes through normal uninit of course. Effectively, if the last non-weak mpv_handle
    /// is destroyed, then the weak mpv_handles receive MPV_EVENT_SHUTDOWN and are
    /// asked to terminate as well.
    /// </para>
    /// <para>
    /// Note if you want to use this like refcounting: you have to be aware that
    /// mpv_terminate_destroy() <c>and</c> mpv_destroy() for the last non-weak
    /// mpv_handle will block until all weak mpv_handles are destroyed.
    /// </para>
    /// </summary>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_create_weak_client", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvInteropHandle CreateWeakClient(MpvInteropHandle handle, string name);

    /// <summary>
    /// Disconnect and destroy the mpv_handle. ctx will be deallocated with this API call.
    /// <para>
    /// If the last mpv_handle is detached, the core player is destroyed. In addition,
    /// if there are only weak mpv_handles (such as created by mpv_create_weak_client() or
    /// internal scripts), these mpv_handles will be sent MPV_EVENT_SHUTDOWN.
    /// This function may block until these clients have responded to the shutdown event,
    /// and the core is finally destroyed.
    /// </para>
    /// </summary>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_destroy")]
    public static partial void Destroy(MpvInteropHandle handle);

    /// <summary>
    /// Similar to mpv_destroy(), but brings the player and all clients down
    /// as well, and waits until all of them are destroyed. This function blocks. <para />
    /// The advantage over mpv_destroy() is that while mpv_destroy() merely
    /// detaches the client handle from the player, this function quits the player, <para />
    /// waits until all other clients are destroyed (i.e. all mpv_handles are
    /// detached), and also waits for the final termination of the player.
    /// </summary>
    /// <remarks>
    /// Since mpv_destroy() is called somewhere on the way, it's not safe to
    /// call other functions concurrently on the same context. <para />
    /// Since mpv client API version 1.29: <para />
    /// The first call on any mpv_handle will block until the core is destroyed. <para /> <c>true</c>
    ///  This means it will wait until other mpv_handle have been destroyed. If you
    ///  want asynchronous destruction, just run the "quit" command, and then react
    ///  to the MPV_EVENT_SHUTDOWN event. <para />
    ///  If another mpv_handle already called mpv_terminate_destroy(), this call will
    ///  not actually block. It will destroy the mpv_handle, and exit immediately, <para />
    ///  while other mpv_handles might still be uninitializing.
    /// Before mpv client API version 1.29: <para />
    ///  If this is called on a mpv_handle that was not created with mpv_create(), <para />
    ///  this function will merely send a quit command and then call
    ///  mpv_destroy(), without waiting for the actual shutdown.
    /// </remarks>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_terminate_destroy")]
    public static partial void TerminateDestroy(MpvInteropHandle handle);

    /// <summary>
    /// Return the name of this client handle. Every client has its own unique
    /// name, which is mostly used for user interface purposes.
    /// <para/>
    /// </summary>
    /// <returns>The client name. The string is read-only and is valid until the
    /// mpv_handle is destroyed.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_client_name", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string GetClientName(MpvInteropHandle handle);

    /// <summary>
    /// Return the ID of this client handle. Every client has its own unique ID.
    /// This ID is never reused by the core, even if the mpv_handle at hand gets destroyed
    /// and new handles get allocated.
    /// <para>
    /// IDs are never 0 or negative.
    /// </para>
    /// <para>
    /// Some mpv APIs (not necessarily all) accept a name in the form "@<c>&lt;id&gt;</c>" in
    /// addition of the proper mpv_client_name(), where "<c>&lt;id&gt;</c>" is the ID in decimal
    /// form (e.g. "@123"). For example, the "script-message-to" command takes the
    /// client name as first argument, but also accepts the client ID formatted in
    /// this manner.
    /// </para>
    /// </summary>
    /// <returns>The client ID.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_client_id")]
    public static partial long GetClientId(MpvInteropHandle handle);
}
