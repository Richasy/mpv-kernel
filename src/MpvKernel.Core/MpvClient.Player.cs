// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 播放指定路径的文件.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task PlayAsync(string filePath)
    {
        List<string> args = ["loadfile", $"\"{filePath}\""];
        MpvError errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, string.Join(' ', args)));
        ThrowIfFailed(errorCode, $"{ClientName} | loadfile failed");
    }

    /// <summary>
    /// 使播放器暂停.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task PauseAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(true);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "pause", MpvFormat.Flag, ref node));
        ThrowIfFailed(errorCode, $"{ClientName} | set pause failed");
    }

    /// <summary>
    /// 使播放器恢复播放.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task ResumeAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "pause", MpvFormat.Flag, ref node));
        ThrowIfFailed(errorCode, $"{ClientName} | set pause failed");
    }

    /// <summary>
    /// 获取当前是否为暂停状态.
    /// </summary>
    /// <returns>是否暂停.</returns>
    public async Task<MpvPlayerState> GetPlayerStateAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "core-idle", MpvFormat.Flag, out result));
        ThrowIfFailed(errorCode, $"{ClientName} | get pause failed");
        var isCoreIdle = result.Flag != 0;
        if (!isCoreIdle)
        {
            return MpvPlayerState.Playing;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "paused-for-cache", MpvFormat.Flag, out result));
        var isBuffering = result.Flag != 0;
        if (isBuffering)
        {
            return MpvPlayerState.Buffering;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "seeking", MpvFormat.Flag, out result));
        var isSeeking = result.Flag != 0;
        if (isSeeking)
        {
            return MpvPlayerState.Seeking;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "eof-reached", MpvFormat.Flag, out result));
        var isEnd = result.Flag != 0;
        if (isEnd)
        {
            return MpvPlayerState.End;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "idle-active", MpvFormat.Flag, out result));
        var isIdle = result.Flag != 0;
        if (isIdle)
        {
            return MpvPlayerState.Idle;
        }

        return MpvPlayerState.Paused;
    }

    /// <summary>
    /// 获取当前播放位置.
    /// </summary>
    /// <returns>播放位置（秒）.</returns>
    public async Task<double> GetCurrentPositionAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "time-pos", MpvFormat.Double, out result));
        ThrowIfFailed(errorCode, $"{ClientName} | get time-pos failed");
        return result.DoubleValue;
    }

    /// <summary>
    /// 获取当前播放文件的时长.
    /// </summary>
    /// <returns>时长（秒）</returns>
    public async Task<double> GetDurationAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "duration", MpvFormat.Double, out result));
        ThrowIfFailed(errorCode, $"{ClientName} | get duration failed");
        return result.DoubleValue;
    }
}
