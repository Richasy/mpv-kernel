// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 设置视频输出类型.
    /// </summary>
    /// <param name="type">类型.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="NotImplementedException">使用了未受支持的视频输出类型.</exception>
    public async Task SetVideoOutput(VideoOutputType type)
    {
        var errorCode = MpvError.Success;
        var output = type switch
        {
            VideoOutputType.Null => "null",
            VideoOutputType.Gpu => "gpu",
            VideoOutputType.GpuNext => "gpu-next",
            VideoOutputType.Direct3D => "direct3d",
            VideoOutputType.SDL => "sdl",
            _ => throw new NotImplementedException(),
        };
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "vo", output));
        ThrowIfFailed(errorCode, "Mpv | set video output failed");
    }

    /// <summary>
    /// 设置 GPU API.
    /// </summary>
    /// <param name="type">API 类型.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetGpuApiAsync(GpuApiType type)
    {
        var errorCode = MpvError.Success;
        var api = type switch
        {
            GpuApiType.Auto => "auto",
            GpuApiType.OpenGL => "opengl",
            GpuApiType.Vulkan => "vulkan",
            GpuApiType.D3D11 => "d3d11",
            _ => throw new NotImplementedException(),
        };
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "gpu-api", api));
        ThrowIfFailed(errorCode, "Mpv | set gpu-api failed");
    }

    /// <summary>
    /// 设置 GPU 后端类型.
    /// </summary>
    /// <param name="type">类型.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetGpuContextAsync(GpuContextType type)
    {
        var errorCode = MpvError.Success;
        var context = type switch
        {
            GpuContextType.Auto => "auto",
            GpuContextType.Windows => "win",
            GpuContextType.WindowsVulkan => "winvk",
            GpuContextType.Angle => "angle",
            GpuContextType.DxInterop => "dxinterop",
            GpuContextType.D3D11 => "d3d11",
            _ => throw new NotImplementedException(),
        };
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "gpu-context", context));
        ThrowIfFailed(errorCode, "Mpv | set gpu-context failed");
    }
}
