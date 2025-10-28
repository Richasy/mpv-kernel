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
            HardwareDecodeType.D3D12va => "d3d12va",
            HardwareDecodeType.D3D12vaCopy => "d3d12va-copy",
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

    /// <summary>
    /// 设置是否验证TLS证书.
    /// </summary>
    /// <param name="enabled">是否验证</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetTlsVerifyAsync(bool enabled)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "tls-verify", enabled ? "yes" : "no"));
        ThrowIfFailed(errorCode, "Mpv | set tls-verify failed");
    }

    /// <summary>
    /// 设置是否自动加载配置文件.
    /// </summary>
    public async Task SetLoadAutoProfilesAsync(bool? enabled)
    {
        var errorCode = MpvError.Success;
        var value = enabled switch
        {
            true => "yes",
            false => "no",
            _ => "auto",
        };
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "load-auto-profiles", value));
        ThrowIfFailed(errorCode, "Mpv | set load-auto-profiles failed");
    }

    /// <summary>
    /// 设置自动创建播放列表的行为.
    /// </summary>
    /// <param name="kind">行为模式.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetAutoCreatePlaylistAsync(AutoCreatePlaylistKind kind)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "autocreate-playlist", kind.ToString().ToLowerInvariant()));
        ThrowIfFailed(errorCode, "Mpv | set autocreate-playlist failed");
    }

    /// <summary>
    /// 设置HTTP代理.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetHttpProxyAsync(string? proxy)
    {
        var errorCode = MpvError.Success;
        if (string.IsNullOrEmpty(proxy))
        {
            await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "http-proxy", "none"));
        }
        else
        {
            await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "http-proxy", proxy));
        }

        ThrowIfFailed(errorCode, "Mpv | set http-proxy failed");
    }

    /// <summary>
    /// 设置 demuxer 最大字节数（视作缓冲容量）.
    /// </summary>
    /// <param name="byteSize">接受 KiB, MiB, GiB 作为单位后缀.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetDemuxerMaxBytesAsync(string byteSize)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "demuxer-max-bytes", byteSize));
        ThrowIfFailed(errorCode, "Mpv | set demuxer-max-bytes failed");
    }

    /// <summary>
    /// 设置 demuxer 读取头部的秒数（视作缓冲秒数）.
    /// </summary>
    /// <param name="seconds">秒数</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetDemuxerReadheadSecondsAsync(int seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be greater than 0.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(seconds);
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "demuxer-readahead-secs", MpvFormat.Int64, ref node));
        ThrowIfFailed(errorCode, "Mpv | set demuxer-readahead-secs failed");
    }

    /// <summary>
    /// 设置是否将图像字幕缩放到屏幕大小.
    /// </summary>
    public async Task SetStretchImageSubtitleToScreenAsync(bool enabled)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "stretch-image-subs-to-screen", enabled ? "yes" : "no"));
        ThrowIfFailed(errorCode, "Mpv | set stretch-image-subtitle-to-screen failed");
    }

    /// <summary>
    /// 设置目标颜色空间.
    /// </summary>
    public async Task SetTargetColorspaceHintAsync(bool? enabled)
    {
        var errorCode = MpvError.Success;
        var opt = enabled == null ? "auto" : enabled == true ? "yes" : "no";
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "target-colorspace-hint", opt));
        ThrowIfFailed(errorCode, "Mpv | set target-colorspace-hint failed");
    }

    /// <summary>
    /// 设置内置配置文件.
    /// </summary>
    public async Task SetBuiltInProfileAsync(string? profileName)
    {
        var errorCode = MpvError.Success;
        if (string.IsNullOrEmpty(profileName))
        {
            await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "profile", "high-quality"));
        }
        else
        {
            await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "profile", profileName));
        }

        ThrowIfFailed(errorCode, "Mpv | set built-in profile failed");
    }

    /// <summary>
    /// 切换统计信息覆盖层.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task ToggleStatsOverlayAsync()
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, "script-binding stats/display-stats-toggle"));
        ThrowIfFailed(errorCode, "Mpv | toggle stats overlay failed");
    }

    /// <summary>
    /// 发送按键事件到播放器.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task SendKeyPressAsync(string key)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, $"keypress {key}"));
        ThrowIfFailed(errorCode, "Mpv | send key press failed");
    }

    /// <summary>
    /// 设置字幕延迟秒数.
    /// </summary>
    public async Task SetSubtitleDelaySecondsAsync(double seconds)
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(Math.Round(seconds, 2));
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "sub-delay", MpvFormat.Double, ref node));
        ThrowIfFailed(errorCode, "Mpv | Set subtitle delay seconds failed");
    }

    /// <summary>
    /// 设置精确跳转类型.
    /// </summary>
    public async Task SetHrSeekAsync(HrSeekType type)
    {
        var errorCode = MpvError.Success;
        var seekType = type.ToString().ToLowerInvariant();
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "hr-seek", seekType));
        ThrowIfFailed(errorCode, "Mpv | set hr-seek failed");
    }

    /// <summary>
    /// 设置最大音量.
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task SetMaxVolumeAsync(int volume)
    {
        if (volume is < 100 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 100 and 1000.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(volume);
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "volume-max", MpvFormat.Int64, ref node));
        ThrowIfFailed(errorCode, "Mpv | set max volume failed");
    }

    /// <summary>
    /// 设置音频独占模式.
    /// </summary>
    /// <param name="enabled">是否启用.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetAudioExclusiveAsync(bool enabled)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "audio-exclusive", enabled ? "yes" : "no"));
        ThrowIfFailed(errorCode, "Mpv | set audio-exclusive failed");
    }

    /// <summary>
    /// 加载脚本.
    /// </summary>
    public async Task LoadScriptAsync(string scriptPath)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, $"load-script {scriptPath}"));
        ThrowIfFailed(errorCode, "Mpv | set script-dir failed");
    }

    /// <summary>
    /// 设置是否将缓存写入硬盘.
    /// </summary>
    public async Task SetCacheOnDiskAsync(bool enabled)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "cache-on-disk", enabled ? "yes" : "no"));
        ThrowIfFailed(errorCode, "Mpv | set cache-on-disk failed");
    }

    /// <summary>
    /// 设置缓存目录.
    /// </summary>
    public async Task SetCacheDirAsync(string directory)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "demuxer-cache-dir", directory));
        ThrowIfFailed(errorCode, "Mpv | set demuxer-cache-dir failed");
    }

    /// <summary>
    /// 设置静音.
    /// </summary>
    public async Task SetMuteAsync(bool isMute)
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(isMute ? "yes" : "no");
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "mute", MpvFormat.String, ref node));
        ThrowIfFailed(errorCode, "Mpv | set mute failed");
    }

    /// <summary>
    /// 设置字幕混合类型.
    /// </summary>
    public async Task SetBlendSubtitleAsync(SubtitleBlendType blendType)
    {
        var errorCode = MpvError.Success;
        var blendStr = blendType switch
        {
            SubtitleBlendType.Yes => "yes",
            SubtitleBlendType.Video => "video",
            SubtitleBlendType.No => "no",
            _ => "yes",
        };

        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "blend-subtitles", blendStr));
        ThrowIfFailed(errorCode, "Mpv | set blend-subtitles failed");
    }

    /// <summary>
    /// 设置着色器列表.
    /// </summary>
    /// <param name="shaders"></param>
    /// <returns></returns>
    public async Task SetShadersAsync(string[] shaders)
    {
        var errorCode = MpvError.Success;
        var shadersStr = string.Join(';', shaders);
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "glsl-shaders", shadersStr));
        ThrowIfFailed(errorCode, "Mpv | set shaders failed");
    }

    /// <summary>
    /// 设置 NVIDIA VSR (Video Super Resolution) 开关.
    /// 需要 GPU 上下文为 d3d11 (--gpu-context=d3d11).
    /// 推荐使用硬解码 (hwdec=d3d11va).
    /// </summary>
    /// <param name="enabled">是否启用 NVIDIA VSR.</param>
    /// <param name="scale">缩放倍数，默认为 2.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetNvidiaVsrAsync(bool enabled, int scale = 2)
    {
        var errorCode = MpvError.Success;

        // 先移除已存在的 NVIDIA VSR 滤镜，避免重复添加
        await Task.Run(() => MpvNative.SetCommandString(_handle, "vf remove @NVvsr"));

        if (enabled)
        {
            var filterStr = $"@NVvsr:d3d11vpp=format=nv12:scale={scale}:scaling-mode=nvidia";
            await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, $"vf append {filterStr}"));
            ThrowIfFailed(errorCode, "Mpv | enable NVIDIA VSR failed");
        }
    }
}
