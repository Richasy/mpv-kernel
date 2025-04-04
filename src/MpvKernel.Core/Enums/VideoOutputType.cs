// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

/// <summary>
/// 视频输出选项（仅支持 Windows）.
/// </summary>
public enum VideoOutputType
{
    /// <summary>
    /// 不产生视频输出。用于基准测试.
    /// 通常，最好使用 --video=no 禁用视频.
    /// </summary>
    Null,

    /// <summary>
    /// 通用、可定制、GPU 加速视频输出驱动程序。它支持扩展的缩放方法，抖动，颜色管理，自定义着色器，HDR 等.
    /// </summary>
    Gpu,

    /// <summary>
    /// 基于 libplacebo 的实验视频渲染器。这支持与 --vo=gpu 几乎相同的功能集.
    /// 通常应该更快，质量更高，但某些功能可能仍然缺失或行为不当.
    /// </summary>
    GpuNext,

    /// <summary>
    /// 使用 Direct3D 接口的视频输出驱动程序.
    /// </summary>
    Direct3D,

    /// <summary>
    /// SDL 2.0+渲染视频输出驱动程序，取决于系统是否具有硬件加速.
    /// </summary>
    SDL,
}
