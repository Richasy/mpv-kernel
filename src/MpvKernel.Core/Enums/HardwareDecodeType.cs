// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// 硬件解码类型.
/// </summary>
public enum HardwareDecodeType
{
    /// <summary>
    /// 强制软解.
    /// </summary>
    None,

    /// <summary>
    /// 启用任何白名单硬件解码器.
    /// </summary>
    Auto,

    /// <summary>
    /// 强制启用找到的任何硬件解码器.
    /// </summary>
    AutoUnsafe,

    /// <summary>
    /// 需要 --vo=gpu 和 --gpu-context=d3d11 或 --gpu-context=angle （仅限 Windows 8+）
    /// </summary>
    D3D11va,

    /// <summary>
    /// 将视频复制回系统 RAM（仅限 Windows 8+）
    /// </summary>
    D3D11vaCopy,

    /// <summary>
    /// 需要 --vo=gpu （任何平台 CUDA 都可用）
    /// </summary>
    Nvdec,

    /// <summary>
    /// 将视频复制回系统 RAM（任何平台 CUDA 都可用）
    /// </summary>
    NvdecCopy,

    /// <summary>
    /// 需要 --vo=gpu-next （任何带有 Vulkan 视频解码的平台）
    /// </summary>
    Vulkan,

    /// <summary>
    /// 将视频复制回系统 RAM（任何带有 Vulkan 视频解码的平台）
    /// </summary>
    VulkanCopy,

    /// <summary>
    /// 需要 --vo=gpu 和 --gpu-context=d3d11 ， --gpu-context=angle 或 --gpu-context=dxinterop （仅限 Windows）
    /// </summary>
    Dxva2,

    /// <summary>
    /// 将视频复制回系统 RAM（仅限 Windows）
    /// </summary>
    Dxva2Copy,
}
