// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using FluentResults;
using static Richasy.MpvKernel.Core.Enums.MpvClientProperties;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 获取全屏状态.
    /// </summary>
    /// <returns>是否全屏.</returns>
    public async Task<Result<bool>> GetFullScreenStateAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, FullScreen, MpvFormat.Flag, out node));
        var stateResult = WrapAsResult(errorCode, "Mpv | get fullscreen failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        return node.Flag != 0;
    }

    /// <summary>
    /// 设置全屏模式.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetFullScreenStateAsync(bool isFullScreen)
    {
        var errorCode = MpvError.Success;
        var onTopNode = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, CompactOverlay, MpvFormat.Flag, ref onTopNode));
        var stateResult = WrapAsResult(errorCode, "Mpv | set FullScreen/ontop failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var node = new MpvNode(isFullScreen);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, FullScreen, MpvFormat.Flag, ref node));
        return WrapAsResult(errorCode, "Mpv | set FullScreen/fullscreen failed");
    }

    /// <summary>
    /// 获取小窗状态.
    /// </summary>
    /// <returns>是否小窗.</returns>
    public async Task<Result<bool>> GetCompactOverlayStateAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, CompactOverlay, MpvFormat.Flag, out node));
        var stateResult = WrapAsResult(errorCode, "Mpv | get ontop failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        return node.Flag != 0;
    }

    /// <summary>
    /// 设置小窗置顶模式.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetCompactOverlayStateAsync(bool isCompactOverlay)
    {
        var errorCode = MpvError.Success;
        var fsNode = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "fullscreen", MpvFormat.Flag, ref fsNode));
        var stateResult = WrapAsResult(errorCode, "Mpv | Set CompactOverlay/fullscreen failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var onTopNode = new MpvNode(isCompactOverlay);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "ontop", MpvFormat.Flag, ref onTopNode));
        return WrapAsResult(errorCode, "Mpv | Set CompactOverlay/ontop failed");
    }
}
