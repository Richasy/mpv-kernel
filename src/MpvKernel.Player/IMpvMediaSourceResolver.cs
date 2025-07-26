// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Player.Models;

namespace Richasy.MpvKernel.Player;

/// <summary>
/// 媒体源解析器接口.
/// </summary>
public interface IMpvMediaSourceResolver
{
    /// <summary>
    /// 获取媒体源.
    /// </summary>
    /// <returns>播放链接及配置.</returns>
    public Task<MpvMediaSource> GetSourceAsync();

    /// <summary>
    /// 克隆一份实例，保留当前状态.
    /// </summary>
    /// <returns></returns>
    public IMpvMediaSourceResolver Clone();
}
