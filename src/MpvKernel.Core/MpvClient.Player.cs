// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using System.Threading.Tasks;

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
        _cachedSnapshot = new(filePath, options);
        var errorCode = MpvError.Success;
        List<string> commandArgs = ["loadfile", $"\"{filePath}\"", "replace", "0"];
        List<string> commandOptions = [];

        if (options != null)
        {
            if (options.WindowHandle != null)
            {
                var node = new MpvNode(options.WindowHandle.Value.ToInt64());
                await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "wid", MpvFormat.Int64, ref node));
                ThrowIfFailed(errorCode, "Mpv | set wid failed");
            }

            if (options.EnableYtdl != null)
            {
                await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "ytdl", options.EnableYtdl.Value ? "yes" : "no"));
                ThrowIfFailed(errorCode, "Mpv | set ytdl failed");
            }

            if (options.EnableCookies != null)
            {
                await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "cookies", options.EnableCookies.Value ? "yes" : "no"));
                ThrowIfFailed(errorCode, "Mpv | set cookies failed");
            }

            if (!string.IsNullOrEmpty(options.UserAgent))
            {
                await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "user-agent", options.UserAgent));
                ThrowIfFailed(errorCode, "Mpv | set user-agent failed");
            }

            if (options.HttpHeaders != null)
            {
                var headers = options.HttpHeaders.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToArray();
                var headerStr = string.Join("\n", headers);
                await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "http-header-fields", headerStr));
            }

            if (options.StartPosition != null)
            {
                commandOptions.Add($"start={Math.Round(options.StartPosition.Value)}");
            }

            if (options.InitialVolume != null)
            {
                commandOptions.Add($"volume={Math.Round(options.InitialVolume.Value)}");
            }

            if (options.InitialSpeed != null)
            {
                commandOptions.Add($"speed={Math.Round(options.InitialSpeed.Value)}");
            }
        }

        if (commandOptions.Count > 0)
        {
            var optionStr = string.Join(',', commandOptions);
            commandArgs.Add(optionStr);
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
    public async Task ReplayAsync(double startPos = 0d)
    {
        if (_cachedSnapshot == null)
        {
            throw new InvalidOperationException("Replay failed, please play a file first.");
        }

        if (startPos > 0)
        {
            _cachedSnapshot.Options ??= new MpvPlayOptions();
            _cachedSnapshot.Options.StartPosition = startPos;
        }

        await PlayAsync(_cachedSnapshot.FilePath!, _cachedSnapshot.Options);
    }

    /// <summary>
    /// 停止播放.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task StopAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(true);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "stop", MpvFormat.Flag, ref node));
        ThrowIfFailed(errorCode, "Mpv | set stop failed");
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
        if (volume is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(volume);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "volume", MpvFormat.Double, ref node));
        ThrowIfFailed(errorCode, "Mpv | set volume failed");
    }

    /// <summary>
    /// 获取当前播放速度.
    /// </summary>
    /// <returns>播放速度.</returns>
    public async Task<double> GetSpeedAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "speed", MpvFormat.Double, out result));
        ThrowIfFailed(errorCode, "Mpv | get speed failed");
        return result.DoubleValue;
    }

    /// <summary>
    /// 设置播放速度.
    /// </summary>
    /// <param name="speed">播放速度.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task SetSpeedAsync(double speed)
    {
        if (speed is < 0.01 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between 0.01 and 100.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(speed);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "speed", MpvFormat.Double, ref node));
        ThrowIfFailed(errorCode, "Mpv | set speed failed");
    }
}
