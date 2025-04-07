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
    public async Task SetVideoOutputAsync(VideoOutputType type)
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

    /// <summary>
    /// 设置硬件解码类型.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetHardwareDecodeAsync(HardwareDecodeType type)
    {
        var errorCode = MpvError.Success;
        var decode = type switch
        {
            HardwareDecodeType.Auto => "auto",
            HardwareDecodeType.None => "no",
            HardwareDecodeType.AutoUnsafe => "auto-unsafe",
            HardwareDecodeType.D3D11va => "d3d11va",
            HardwareDecodeType.D3D11vaCopy => "d3d11va-copy",
            HardwareDecodeType.Nvdec => "nvdec",
            HardwareDecodeType.NvdecCopy => "nvdec-copy",
            HardwareDecodeType.Vulkan => "vulkan",
            HardwareDecodeType.VulkanCopy => "vulkan-copy",
            HardwareDecodeType.Dxva2 => "dxva2",
            HardwareDecodeType.Dxva2Copy => "dxva2-copy",
            _ => throw new NotImplementedException(),
        };
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "hwdec", decode));
        ThrowIfFailed(errorCode, "Mpv | set hwdec failed");
    }

    /// <summary>
    /// 设置HTTP请求头.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetHttpHeadersAsync(Dictionary<string, string> headers)
    {
        var errorCode = MpvError.Success;
        var headerStr = string.Join('\n', headers.Select(p => $"{p.Key}: {p.Value}"));
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "http-header-fields", headerStr));
        ThrowIfFailed(errorCode, "Mpv | set http-header-fields failed");
    }

    /// <summary>
    /// 设置截图输出目录.
    /// </summary>
    /// <param name="directory">目录路径.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetScreenshotDirectoryAsync(string directory)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "screenshot-dir", directory));
        ThrowIfFailed(errorCode, "Mpv | set screenshot-dir failed");
    }

    /// <summary>
    /// 设置截图命名模板.
    /// </summary>
    /// <param name="template">命名模板</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetScreenshotTemplateAsync(string template = "mpv-shot%n")
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "screenshot-template", template));
        ThrowIfFailed(errorCode, "Mpv | set screenshot-template failed");
    }

    /// <summary>
    /// 设置截图格式.
    /// </summary>
    /// <param name="format">文件格式.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetScreenshotFormatAsync(ScreenshotFormat format)
    {
        var errorCode = MpvError.Success;
        var formatStr = format switch
        {
            ScreenshotFormat.Png => "png",
            ScreenshotFormat.Webp => "webp",
            ScreenshotFormat.Jxl => "jxl",
            ScreenshotFormat.Avif => "avif",
            _ => "jpg",
        };

        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "screenshot-format", formatStr));
        ThrowIfFailed(errorCode, "Mpv | set screenshot-format failed");
    }
}
