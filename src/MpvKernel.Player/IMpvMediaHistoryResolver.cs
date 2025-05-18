// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Player;

/// <summary>
/// 媒体历史记录解析器接口.
/// </summary>
public interface IMpvMediaHistoryResolver
{
    /// <summary>
    /// 获取起始位置.
    /// </summary>
    /// <returns>起始位置（秒）.</returns>
    public Task<double> GetStartPositionAsync();

    /// <summary>
    /// 保存历史记录.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public Task SaveHistoryAsync(double position, double duration);
}
