// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core;

internal static class InternalUtils
{
    /// <summary>
    /// Converts a log level enumeration to its corresponding string representation.
    /// </summary>
    /// <param name="level">The log level is used to determine the appropriate string output.</param>
    /// <returns>A string that represents the specified log level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided log level does not match any defined values.</exception>
    public static string ToMpvLogLevelString(this MpvLogLevel level)
    {
        return level switch
        {
            MpvLogLevel.None => "no",
            MpvLogLevel.Fatal => "fatal",
            MpvLogLevel.Error => "error",
            MpvLogLevel.Warn => "warn",
            MpvLogLevel.Info => "info",
            MpvLogLevel.V => "v",
            MpvLogLevel.Debug => "debug",
            MpvLogLevel.Trace => "trace",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }

    /// <summary>
    /// Converts a string representation of a log level to its corresponding MpvLogLevel enumeration value.
    /// </summary>
    /// <param name="level">Accepts a string that represents a log level.</param>
    /// <returns>Returns the corresponding MpvLogLevel based on the input string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the input string does not match any valid log level.</exception>
    public static MpvLogLevel ToMpvLogLevel(this string level)
    {
        return level switch
        {
            "no" => MpvLogLevel.None,
            "fatal" => MpvLogLevel.Fatal,
            "error" => MpvLogLevel.Error,
            "warn" => MpvLogLevel.Warn,
            "info" => MpvLogLevel.Info,
            "v" => MpvLogLevel.V,
            "debug" => MpvLogLevel.Debug,
            "trace" => MpvLogLevel.Trace,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }
}
