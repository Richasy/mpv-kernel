// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 获取全屏状态.
    /// </summary>
    /// <returns>是否全屏.</returns>
    public async Task<bool> GetFullScreenStateAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "fullscreen", MpvFormat.Flag, out node));
        ThrowIfFailed(errorCode, "Mpv | get fullscreen failed");
        return node.Flag != 0;
    }

    /// <summary>
    /// 设置全屏模式.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetFullScreenState(bool isFullScreen)
    {
        var errorCode = MpvError.Success;

        var onTopNode = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "ontop", MpvFormat.Flag, ref onTopNode));
        ThrowIfFailed(errorCode, "Mpv | Set fullscreen/ontop failed");

        var node = new MpvNode(isFullScreen);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "fullscreen", MpvFormat.Flag, ref node));
        ThrowIfFailed(errorCode, "Mpv | set fullscreen failed");
    }

    /// <summary>
    /// 获取小窗状态.
    /// </summary>
    /// <returns>是否小窗.</returns>
    public async Task<bool> GetCompactOverlayStateAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "ontop", MpvFormat.Flag, out node));
        ThrowIfFailed(errorCode, "Mpv | get ontop failed");
        return node.Flag != 0;
    }

    /// <summary>
    /// 设置小窗置顶模式.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetCompactOverlayState(bool isCompactOverlay)
    {
        var errorCode = MpvError.Success;
        var fsNode = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "fullscreen", MpvFormat.Flag, ref fsNode));
        ThrowIfFailed(errorCode, "Mpv | Set compactOverlay/fullscreen failed");

        var onTopNode = new MpvNode(isCompactOverlay);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "ontop", MpvFormat.Flag, ref onTopNode));
        ThrowIfFailed(errorCode, "Mpv | Set ontop failed");
    }
}
