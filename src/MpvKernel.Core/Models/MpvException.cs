// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// Represents an exception specific to the Mpv library with an error code and custom message. It can also include an
/// inner exception.
/// </summary>
public sealed class MpvException : Exception
{
    /// <summary>
    /// 错误代码.
    /// </summary>
    public MpvError Code { get; set; }

    /// <summary>
    /// Represents an exception specific to the Mpv library, initialized with a custom error message.
    /// </summary>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    /// <param name="error">Holds the error code associated with the exception.</param>
    public MpvException(string message, MpvError error) : base(message) => Code = error;

    /// <summary>
    /// Initializes a new instance of the exception class with a specified error message and a reference to the inner
    /// exception.
    /// </summary>
    /// <param name="message">Provides a description of the error that occurred.</param>
    /// <param name="error">Holds the error code associated with the exception.</param>
    /// <param name="innerException">Holds the exception that is the cause of the current exception.</param>
    public MpvException(string message, MpvError error, Exception innerException) : base(message, innerException) => Code = error;
}
