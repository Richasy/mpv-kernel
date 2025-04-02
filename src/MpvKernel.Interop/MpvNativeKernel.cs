// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using static Richasy.MpvKernel.Constants;

namespace Richasy.MpvKernel;

/// <summary>
/// Interop class for the MPV library.
/// </summary>
public sealed partial class MpvNativeKernel
{
    /// <summary>
    /// Initializes an instance of the MpvNativeKernel class. It sets up the necessary environment for the MPV library.
    /// </summary>
    /// <param name="dllPath">Specifies the path to the dynamic link library required for MPV functionality.</param>
    public MpvNativeKernel(string dllPath)
    {
        MpvImportResolver.Initialize(dllPath);
    }

    #region Command
    
    #endregion
}
