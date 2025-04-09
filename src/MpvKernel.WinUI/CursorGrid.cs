// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace Richasy.MpvKernel.WinUI;

/// <summary>
/// 光标网格.
/// </summary>
internal sealed partial class CursorGrid : Grid
{
    /// <summary>
    /// 隐藏光标.
    /// </summary>
    public void HideCursor()
        => ProtectedCursor?.Dispose();

    /// <summary>
    /// 显示光标.
    /// </summary>
    public void ShowCursor()
        => ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
}
