// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace MpvKernel.WinUI;

/// <summary>
/// MPV UI 组件接口.
/// </summary>
public interface IMpvUIElement
{
    /// <summary>
    /// 需要从视觉树上移除时触发.
    /// </summary>
    public void Disconnect();

    /// <summary>
    /// 处理 UI 交互通知.
    /// </summary>
    public void HandleUINotify(MpvUIEventId id, object data);
}
