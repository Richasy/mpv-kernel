// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;
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

    /// <summary>
    /// 处理数据通知.
    /// </summary>
    /// <param name="id">事件 ID.</param>
    /// <param name="data">数据.</param>
    public void HandleDataNotify(MpvClientEventId id, object data);

    /// <summary>
    /// 因内部的一些操作需要重新加载播放数据时触发.
    /// </summary>
    public event EventHandler RequestReload;

    /// <summary>
    /// 因内部的一些操作需要清除播放数据时触发.
    /// </summary>
    public event EventHandler RequestClear;
}
