// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// GPU API 类型.
/// </summary>
public enum GpuApiType
{
    /// <summary>
    /// 使用任何可用的 API（默认）。请注意，用于此值的默认 GPU API 可能会发生更改，因此不得依赖。如果需要使用某个 GPU API，则必须明确指定.
    /// </summary>
    Auto,

    /// <summary>
    /// 仅允许 OpenGL（需要 OpenGL 2.1+或 GLES 2.0+）.
    /// </summary>
    OpenGL,

    /// <summary>
    /// 仅允许 Vulkan（需要有效/有效的 --spirv-compiler ）.
    /// </summary>
    Vulkan,

    /// <summary>
    /// 只允许 --gpu-context=d3d11.
    /// </summary>
    D3D11,
}
