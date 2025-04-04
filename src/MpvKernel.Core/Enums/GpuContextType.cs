// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// GPU 上下文类型.
/// </summary>
public enum GpuContextType
{
    /// <summary>
    /// 自动选择（默认）。请注意，此上下文必须单独使用，并且不参与优先级列表.
    /// </summary>
    Auto,

    /// <summary>
    /// Win32/WGL.
    /// </summary>
    Windows,

    /// <summary>
    /// VK_KHR_win32_surface.
    /// </summary>
    WindowsVulkan,

    /// <summary>
    /// Direct3D11 通过 OpenGL ES 转换层 ANGLE。这几乎支持 win 后端所做的一切（如果 ANGLE 构建足够新）.
    /// </summary>
    Angle,

    /// <summary>
    /// Win32，使用 WGL 进行渲染，使用 Direct3D 9Ex 进行演示。适用于 NVIDIA 和 AMD。带有最新驱动程序的新英特尔芯片也可以工作.
    /// </summary>
    DxInterop,

    /// <summary>
    /// Win32，原生 Direct3D 11 渲染.
    /// </summary>
    D3D11,
}
