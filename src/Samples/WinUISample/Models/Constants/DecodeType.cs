// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace WinUISample.Models.Constants;

/// <summary>
/// 偏好的解码模式.
/// </summary>
public enum DecodeType
{
    /// <summary>
    /// 自动.
    /// </summary>
    Auto,

    /// <summary>
    /// D3D11硬解.
    /// </summary>
    D3D11,

    /// <summary>
    /// NVDEC硬解.
    /// </summary>
    NVDEC,

    /// <summary>
    /// Vulkan硬解.
    /// </summary>
    Vulkan,
}
