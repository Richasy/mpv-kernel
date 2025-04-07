// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// MPV 播放选项.
/// </summary>
public sealed class MpvPlayOptions
{
    /// <summary>
    /// 播放窗口句柄.
    /// </summary>
    public IntPtr? WindowHandle { get; set; }

    /// <summary>
    /// 起始位置.
    /// </summary>
    public double? StartPosition { get; set; }

    /// <summary>
    /// 初始音量.
    /// </summary>
    public double? InitialVolume { get; set; }

    /// <summary>
    /// 初始播放速度.
    /// </summary>
    public double? InitialSpeed { get; set; }

    /// <summary>
    /// 请求头，通常用于鉴权.
    /// </summary>
    public Dictionary<string, string>? HttpHeaders { get; set; }

    /// <summary>
    /// User agent，最好单独设置而不是合并在 HttpHeaders 属性中.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 启用 Cookies.
    /// </summary>
    public bool? EnableCookies { get; set; }

    /// <summary>
    /// 启用 ytdl.
    /// </summary>
    public bool? EnableYtdl { get; set; }

    /// <summary>
    /// 附加音轨 URL.
    /// </summary>
    public string? ExtraAudioUrl { get; set; }
}
