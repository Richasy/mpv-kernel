// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Models;

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// 媒体源解析器接口.
/// </summary>
public interface IMediaSourceResolver
{
    /// <summary>
    /// 获取媒体源的 ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 初始化.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public Task InitializeAsync();

    /// <summary>
    /// 获取播放源.
    /// </summary>
    /// <returns>用于视频播放的源数据.</returns>
    public (string url, MpvPlayOptions options) GetSource();

    /// <summary>
    /// 获取媒体标题.
    /// </summary>
    /// <returns>标题.</returns>
    public string GetTitle();
}
