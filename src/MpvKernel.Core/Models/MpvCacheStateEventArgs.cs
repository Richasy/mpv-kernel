// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// Mpv 缓冲状态事件参数类，表示播放器的缓存状态信息.
/// </summary>
/// <param name="ranges"></param>
public sealed class MpvCacheStateEventArgs(List<MpvSeekableRange> ranges) : EventArgs
{
    /// <summary>
    /// 已缓冲范围.
    /// </summary>
    public List<MpvSeekableRange> SeekableRanges { get; } = ranges;

    /// <summary>
    /// bof-cached 指示具有最低时间戳的寻址范围是否指向流的开始（BOF）.
    /// </summary>
    /// <remarks>
    /// 当 bof-cached 和 eof-cached 都为 true 时，表示整个流都已缓冲.
    /// </remarks>
    public bool BofCached { get; set; }

    /// <summary>
    /// eof-cached 指示具有最高时间戳的寻址范围是否指向流的结束（EOF）.
    /// </summary>
    /// <remarks>
    /// 当 bof-cached 和 eof-cached 都为 true 时，表示整个流都已缓冲.
    /// </remarks>
    public bool EofCached { get; set; }

    /// <summary>
    /// fw-bytes 是从当前解码位置开始，缓冲区中数据包的字节数.
    /// </summary>
    public long FwBytes { get; set; }

    /// <summary>
    /// file-cache-bytes 是文件缓存中存储的字节数.
    /// </summary>
    public long FileCacheBytes { get; set; }
}
