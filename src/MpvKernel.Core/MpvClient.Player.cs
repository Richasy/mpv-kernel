// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using FluentResults;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using static Richasy.MpvKernel.Core.Enums.MpvClientProperties;

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
                commandOptions.Add($"speed={Math.Round(options.InitialSpeed.Value, 2)}");
            }

            if (!string.IsNullOrEmpty(options.ExtraAudioUrl))
            {
                commandOptions.Add($"audio-file=\"{options.ExtraAudioUrl}\"");
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
    public async Task<Result> PauseAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(true);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, Pause, MpvFormat.Flag, ref node));
        return WrapAsResult(errorCode, "Mpv | set pause failed");
    }

    /// <summary>
    /// 使播放器恢复播放.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> ResumeAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(false);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, Pause, MpvFormat.Flag, ref node));
        return WrapAsResult(errorCode, "Mpv | set pause failed");
    }

    /// <summary>
    /// 停止播放.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> StopAsync()
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(true);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, Stop, MpvFormat.Flag, ref node));
        return WrapAsResult(errorCode, "Mpv | set stop failed");
    }

    /// <summary>
    /// 获取当前是否为暂停状态.
    /// </summary>
    /// <returns>是否暂停.</returns>
    public async Task<Result<MpvPlayerState>> GetPlayerStateAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, CoreIdle, MpvFormat.Flag, out result));
        var stateResult = WrapAsResult(errorCode, "Mpv | get core-idle failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var isCoreIdle = result.Flag != 0;
        if (!isCoreIdle)
        {
            return MpvPlayerState.Playing;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, PausedForCache, MpvFormat.Flag, out result));
        stateResult = WrapAsResult(errorCode, "Mpv | get paused-for-cache failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var isBuffering = result.Flag != 0;
        if (isBuffering)
        {
            return MpvPlayerState.Buffering;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, Seeking, MpvFormat.Flag, out result));
        stateResult = WrapAsResult(errorCode, "Mpv | get seeking failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var isSeeking = result.Flag != 0;
        if (isSeeking)
        {
            return MpvPlayerState.Seeking;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, EofReached, MpvFormat.Flag, out result));
        stateResult = WrapAsResult(errorCode, "Mpv | get eof-reached failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

        var isEnd = result.Flag != 0;
        if (isEnd)
        {
            return MpvPlayerState.End;
        }

        result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, IdleActive, MpvFormat.Flag, out result));
        stateResult = WrapAsResult(errorCode, "Mpv | get idle-active failed");
        if (stateResult.IsFailed)
        {
            return stateResult;
        }

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
    public async Task<Result<double>> GetCurrentPositionAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, TimePosition, MpvFormat.Double, out result));
        var positionResult = WrapAsResult(errorCode, "Mpv | get time-pos failed");
        if (positionResult.IsFailed)
        {
            return positionResult;
        }

        return result.DoubleValue;
    }

    /// <summary>
    /// 设置当前播放位置.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetCurrentPositionAsync(double position)
    {
        var errorCode = MpvError.Success;
        var node = new MpvNode(position);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, TimePosition, MpvFormat.Double, ref node));
        return WrapAsResult(errorCode, "Mpv | set time-pos failed");
    }

    /// <summary>
    /// 获取当前播放文件的时长.
    /// </summary>
    /// <returns>时长（秒）</returns>
    public async Task<Result<double>> GetDurationAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, Duration, MpvFormat.Double, out result));
        var durationResult = WrapAsResult(errorCode, "Mpv | get duration failed");
        if (durationResult.IsFailed)
        {
            return durationResult;
        }

        return result.DoubleValue;
    }

    /// <summary>
    /// 获取当前播放文件的音量.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<double>> GetVolumeAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, Volume, MpvFormat.Double, out result));
        var volumeResult = WrapAsResult(errorCode, "Mpv | get volume failed");
        if (volumeResult.IsFailed)
        {
            return volumeResult;
        }

        return result.DoubleValue;
    }

    /// <summary>
    /// 设置音量.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the volume value is less than 0 or greater than 100.</exception>
    public async Task<Result> SetVolumeAsync(double volume)
    {
        if (volume is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(volume);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, Volume, MpvFormat.Double, ref node));
        return WrapAsResult(errorCode, "Mpv | set volume failed");
    }

    /// <summary>
    /// 获取当前播放速度.
    /// </summary>
    /// <returns>播放速度.</returns>
    public async Task<Result<double>> GetSpeedAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, Speed, MpvFormat.Double, out result));
        var speedResult = WrapAsResult(errorCode, "Mpv | get speed failed");
        if (speedResult.IsFailed)
        {
            return speedResult;
        }

        return result.DoubleValue;
    }

    /// <summary>
    /// 设置播放速度.
    /// </summary>
    /// <param name="speed">播放速度.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<Result> SetSpeedAsync(double speed)
    {
        if (speed is < 0.01 or > 100)
        {
            return Result.Fail("Speed must be between 0.01 and 100.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(speed);
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, Speed, MpvFormat.Double, ref node));
        return WrapAsResult(errorCode, "Mpv | set speed failed");
    }
}
