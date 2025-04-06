// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 播放指定路径的文件.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task PlayAsync(string filePath, MpvPlayOptions? options = null)
    {
        if (!Uri.IsWellFormedUriString(filePath, UriKind.Absolute))
        {
            filePath = filePath.Replace("\\", "/");
        }

        _cachedDuration = default;
        _cachedSnapshot = MpvPlayerSnapshot.Create(filePath, options);
        var errorCode = MpvError.Success;
        List<string> commandArgs = ["loadfile", $"\"{filePath}\""];

        if (options != null)
        {
            if (options.WindowHandle != null)
            {
                var node = new MpvNode(options.WindowHandle.Value.ToInt64());
                await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "wid", MpvFormat.Int64, ref node));
                ThrowIfFailed(errorCode, "Mpv | set wid failed");
            }
        }

        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, string.Join(' ', commandArgs)));
        ThrowIfFailed(errorCode, "Mpv | loadfile failed");
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
        ThrowIfFailed(errorCode, "Mpv | set pause failed");
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
        ThrowIfFailed(errorCode, "Mpv | set pause failed");
    }

    /// <summary>
    /// 重新播放当前文件.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task ReplayAsync()
    {
        if (_cachedSnapshot == null)
        {
            throw new InvalidOperationException("Replay failed, please play a file first.");
        }

        if (_cachedSnapshot.Type == MpvFileType.LocalFile)
        {
            if (_cachedSnapshot.Options?.StartPosition != null)
            {
                _cachedSnapshot.Options.StartPosition = null;
            }

            await PlayAsync(_cachedSnapshot.FilePath!, _cachedSnapshot.Options);
        }
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
        ThrowIfFailed(errorCode, "Mpv | get pause failed");
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
        ThrowIfFailed(errorCode, "Mpv | get time-pos failed");
        return result.DoubleValue;
    }

    /// <summary>
    /// 设置当前播放位置.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetCurrentPositionAsync(double position)
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(position);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "time-pos", MpvFormat.Double, ref node));
        ThrowIfFailed(errorCode, "Mpv | set time-pos failed");
    }

    /// <summary>
    /// 获取当前播放文件的时长.
    /// </summary>
    /// <returns>时长（秒）</returns>
    public async Task<double> GetDurationAsync()
    {
        if (_cachedDuration == null)
        {
            var errorCode = MpvError.Success;
            var result = new MpvNode();
            await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "duration", MpvFormat.Double, out result));
            ThrowIfFailed(errorCode, "Mpv | get duration failed");
            _cachedDuration = result.DoubleValue;
        }

        return _cachedDuration.Value;
    }

    /// <summary>
    /// 获取当前播放文件的音量.
    /// </summary>
    /// <returns></returns>
    public async Task<double> GetVolumeAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "volume", MpvFormat.Double, out result));
        ThrowIfFailed(errorCode, "Mpv | get volume failed");
        return result.DoubleValue;
    }

    /// <summary>
    /// 设置音量.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the volume value is less than 0 or greater than 100.</exception>
    public async Task SetVolumeAsync(double volume)
    {
        if (volume < 0 || volume > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(volume);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "volume", MpvFormat.Double, ref node));
        ThrowIfFailed(errorCode, "Mpv | set volume failed");
    }
}
