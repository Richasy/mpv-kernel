// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Win32;

/// <summary>
/// MPV UI 控件接口.
/// </summary>
public interface IMpvUIElement : IDisposable
{
    /// <summary>
    /// 获取控件句柄.
    /// </summary>
    /// <returns>句柄.</returns>
    public nint GetHandle();
}
