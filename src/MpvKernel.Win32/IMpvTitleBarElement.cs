// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Drawing;

namespace Richasy.MpvKernel.Win32;

/// <summary>
/// MPV 标题栏元素接口.
/// </summary>
public interface IMpvTitleBarElement : IMpvUIElement
{
    /// <summary>
    /// 获取当前元素的可交互区域.
    /// </summary>
    /// <returns>可交互区域列表.</returns>
    public List<Rectangle> GetInteractiveAreas();

    /// <summary>
    /// 设置位置.
    /// </summary>
    public void SetPosition(int x, int y, int width, int height);

    /// <summary>
    /// 可交互区域发生变化时触发.
    /// </summary>
    public event EventHandler InteractiveAreaChanged;
}
