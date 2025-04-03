// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

public static partial class MpvNative
{
    /// <summary>
    /// Set a property to a given value. Properties are essentially variables which
    /// can be queried or set at runtime. For example, writing to the pause property
    /// will actually pause or unpause playback.
    /// <para>
    /// If the format doesn't match with the internal format of the property, access
    /// usually will fail with <c>MPV_ERROR_PROPERTY_FORMAT</c>. In some cases, the data
    /// is automatically converted and access succeeds. For example, <c>MPV_FORMAT_INT64</c>
    /// is always converted to <c>MPV_FORMAT_DOUBLE</c>, and access using <c>MPV_FORMAT_STRING</c>
    /// usually invokes a string parser. The same happens when calling this function
    /// with <c>MPV_FORMAT_NODE</c>: the underlying format may be converted to another
    /// type if possible.
    /// </para>
    /// <para>
    /// Using a format other than <c>MPV_FORMAT_NODE</c> is equivalent to constructing a
    /// <c>mpv_node</c> with the given format and data, and passing the <c>mpv_node</c> to this
    /// function. (Before API version 1.21, this was different.)
    /// </para>
    /// <para>
    /// Note: starting with mpv 0.21.0 (client API version 1.23), this can be used to
    /// set options in general. It even can be used before <c>mpv_initialize()</c>
    /// has been called. If called before <c>mpv_initialize()</c>, setting properties
    /// not backed by options will result in <c>MPV_ERROR_PROPERTY_UNAVAILABLE</c>.
    /// In some cases, properties and options still conflict. In these cases,
    /// <c>mpv_set_property()</c> accesses the options before <c>mpv_initialize()</c>, and
    /// the properties after <c>mpv_initialize()</c>. These conflicts will be removed
    /// in mpv 0.23.0. See <c>mpv_set_option()</c> for further remarks.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="name">The property name. See input.rst for a list of properties.</param>
    /// <param name="format">See enum mpv_format.</param>
    /// <param name="data">Option value.</param>
    /// <returns>Error code</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_property", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetProperty(MpvInteropHandle handle, string name, MpvFormat format, ref MpvNode data);

    /// <summary>
    /// Convenience function to set a property to a string value.
    /// <para>
    /// This is like calling <see cref="SetProperty(MpvInteropHandle, string, MpvFormat, ref MpvNode)"/> with <c>MPV_FORMAT_STRING</c>.
    /// </para>
    /// </summary>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_property_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetPropertyString(MpvInteropHandle handle, string name, string data);

    /// <summary>
    /// Set a property asynchronously. You will receive the result of the operation
    /// as MPV_EVENT_SET_PROPERTY_REPLY event. The <see cref="MpvError"/> field will contain
    /// the result status of the operation. Otherwise, this function is similar to 
    /// <see cref="SetProperty(MpvInteropHandle, string, MpvFormat, ref MpvNode)"/>.
    /// <para>
    /// Safe to be called from MPV render API threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">See section about asynchronous calls</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">See enum <see cref="MpvFormat"/>.</param>
    /// <param name="data">
    /// Option value. The value will be copied by the function. It
    /// will never be modified by the client API.
    /// </param>
    /// <returns>Error code if sending the request failed</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_set_property_async", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError SetPropertyAsync(MpvInteropHandle handle, ulong replyUserData, string name, MpvFormat format, ref MpvNode data);

    /// <summary>
    /// Read the value of the given property.
    /// </summary>
    /// <para>
    /// If the format doesn't match with the internal format of the property, access
    /// usually will fail with <c>MPV_ERROR_PROPERTY_FORMAT</c>. In some cases, the data
    /// is automatically converted and access succeeds. For example, <c>MPV_FORMAT_INT64</c>
    /// is always converted to <c>MPV_FORMAT_DOUBLE</c>, and access using <c>MPV_FORMAT_STRING</c>
    /// usually invokes a string formatter.
    /// </para>
    /// <param name="handle">Client handle.</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">see enum mpv_format.</param>
    /// <param name="data">
    /// Pointer to the variable holding the option value. On
    /// success, the variable will be set to a copy of the option
    /// value. For formats that require dynamic memory allocation,
    /// you can free the value with <c>mpv_free()</c> (strings) or
    /// <c>mpv_free_node_contents()</c> (<c>MPV_FORMAT_NODE</c>).
    /// </param>
    /// <returns>error code</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_get_property", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError GetProperty(MpvInteropHandle handle, string name, MpvFormat format, out MpvNode data);

    /// <summary>
    /// Return the value of the property with the given name as string. This is
    /// equivalent to mpv_get_property() with MPV_FORMAT_STRING.
    /// <para>
    /// See MPV_FORMAT_STRING for character encoding issues.
    /// </para>
    /// <para>
    /// On error, <c>null</c> is returned. Use mpv_get_property() if you want fine-grained
    /// error reporting.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="name">The property name.</param>
    /// <returns>
    /// Property value, or <c>null</c> if the property can't be retrieved. Free
    /// the string with mpv_free().
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_get_property_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string GetPropertyString(MpvInteropHandle handle, string name);

    /// <summary>
    /// Return the property as "OSD" formatted string. This is the same as
    /// <c>mpv_get_property_string</c>, but using <c>MPV_FORMAT_OSD_STRING</c>.
    /// </summary>
    /// <returns>
    /// Property value, or <c>null</c> if the property can't be retrieved.
    /// <para>Free the string with <c>mpv_free()</c>.</para>
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_get_property_osd_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string GetPropertyOsdString(MpvInteropHandle handle, string name);

    /// <summary>
    /// Get a property asynchronously. You will receive the result of the operation
    /// as well as the property data with the MPV_EVENT_GET_PROPERTY_REPLY event.
    /// <para>
    /// You should check the mpv_event.error field on the reply event.
    /// </para>
    /// <para>
    /// Safe to be called from mpv render API threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">See section about asynchronous calls.</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">See enum mpv_format.</param>
    /// <returns>Error code if sending the request failed.</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_get_property_async", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError GetPropertyAsync(MpvInteropHandle handle, ulong replyUserData, string name, MpvFormat format);

    /// <summary>
    /// Convenience function to delete a property.
    /// <para>
    /// This is equivalent to running the command <c>del [name]</c>.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="name">
    /// The property name. See input.rst for a list of properties.
    /// </param>
    /// <returns>
    /// Error code.
    /// </returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_del_property", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError DeleteProperty(MpvInteropHandle handle, string name);

    /// <summary>
    /// Get a notification whenever the given property changes. You will receive
    /// updates as MPV_EVENT_PROPERTY_CHANGE. Note that this is not very precise:
    /// for some properties, it may not send updates even if the property changed.
    /// This depends on the property, and it's a valid feature request to ask for
    /// better update handling of a specific property. (For some properties, like
    /// <c>clock</c>, which shows the wall clock, this mechanism doesn't make too
    /// much sense anyway.)
    /// <para></para>
    /// Property changes are coalesced: the change events are returned only once the
    /// event queue becomes empty (e.g. mpv_wait_event() would block or return
    /// MPV_EVENT_NONE), and then only one event per changed property is returned.
    /// <para></para>
    /// You always get an initial change notification. This is meant to initialize
    /// the user's state to the current value of the property.
    /// <para></para>
    /// Normally, change events are sent only if the property value changes according
    /// to the requested format. mpv_event_property will contain the property value
    /// as data member.
    /// <para></para>
    /// <warning>
    /// If a property is unavailable or retrieving it caused an error,
    /// MPV_FORMAT_NONE will be set in mpv_event_property, even if the
    /// format parameter was set to a different value. In this case, the
    /// mpv_event_property.data field is invalid.
    /// </warning>
    /// <para></para>
    /// If the property is observed with the format parameter set to MPV_FORMAT_NONE,
    /// you get low-level notifications whether the property <c>may</c> have changed, and
    /// the data member in mpv_event_property will be unset. With this mode, you
    /// will have to determine yourself whether the property really changed. On the
    /// other hand, this mechanism can be faster and uses less resources.
    /// <para></para>
    /// Observing a property that doesn't exist is allowed. (Although it may still
    /// cause some sporadic change events.)
    /// <para></para>
    /// Keep in mind that you will get change notifications even if you change a
    /// property yourself. Try to avoid endless feedback loops, which could happen
    /// if you react to the change notifications triggered by your own change.
    /// <para></para>
    /// Only the mpv_handle on which this was called will receive the property
    /// change events, or can unobserve them.
    /// <para></para>
    /// Safe to be called from MPV render API threads.
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="replyUserData">This will be used for the mpv_event.reply_userdata
    /// field for the received MPV_EVENT_PROPERTY_CHANGE events. (Also see section about
    /// asynchronous calls, although this function is somewhat different from
    /// actual asynchronous calls.)
    /// If you have no use for this, pass 0.
    /// Also see mpv_unobserve_property().
    /// </param>
    /// <param name="name">The property name.</param>
    /// <param name="format">See enum mpv_format. Can be MPV_FORMAT_NONE to omit values
    /// from the change events.</param>
    /// <returns>Error code (usually fails only on OOM or unsupported format).</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_observe_property", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MpvError ObserveProperty(MpvInteropHandle handle, ulong replyUserData, string name, MpvFormat format);

    /// <summary>
    /// Undo mpv_observe_property. This will remove all observed properties for
    /// which the given number was passed as reply_userdata to mpv_observe_property.
    /// <para>
    /// Safe to be called from mpv render API threads.
    /// </para>
    /// </summary>
    /// <param name="handle">Client handle.</param>
    /// <param name="registeredReplyUserData">ID that was passed to mpv_observe_property</param>
    /// <returns>Negative value is an error code, <c>>=0</c> is number of removed properties
    /// on success (includes the case when <c>0</c> were removed)</returns>
    [LibraryImport(MpvLibraryName, EntryPoint = "mpv_unobserve_property")]
    public static partial MpvError UnobservedProperty(MpvInteropHandle handle, ulong registeredReplyUserData);
}
