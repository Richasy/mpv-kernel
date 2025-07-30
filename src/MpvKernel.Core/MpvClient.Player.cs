// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using FluentResults;
using Richasy.MpvKernel.Core.Enums;
using Richasy.MpvKernel.Core.Models;
using System.Globalization;
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
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        List<string> commandArgs = [];
        List<string> commandOptions = [];
        if (extension.StartsWith(".iso"))
        {
            var isBd = filePath.Contains("BD", StringComparison.Ordinal);
            var preferLoader = options?.InitExtraLoader;
            if (!string.IsNullOrEmpty(preferLoader))
            {
                isBd = string.Equals(preferLoader, "bd", StringComparison.OrdinalIgnoreCase);
            }

            commandArgs = ["loadfile", isBd ? "bd://" : "dvd://", "replace", "0"];
            await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, isBd ? "bluray-device" : "dvd-device", filePath));
            ThrowIfFailed(errorCode, "Mpv | set bluray/dvd device failed");
        }
        else
        {
            commandArgs = ["loadfile", $"\"{filePath}\"", "replace", "0"];
        }

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

            //if (!string.IsNullOrEmpty(options.MediaName))
            //{
            //    commandOptions.Add($"force-media-title=\"{options.MediaName}\"");
            //}

            if (options.Subtitles?.Count > 0)
            {
                if (options.Subtitles.Count == 1)
                {
                    commandOptions.Add($"sub-file=\"{options.Subtitles[0]}\"");
                }
                else
                {
                    foreach (var item in options.Subtitles)
                    {
                        commandOptions.Add($"sub-files-append={item}");
                    }
                }
            }

            if (options.AudioTracks?.Count > 0)
            {
                if (options.AudioTracks.Count == 1)
                {
                    commandOptions.Add($"audio-file=\"{options.AudioTracks[0]}\"");
                }
                else
                {
                    foreach (var item in options.AudioTracks)
                    {
                        commandOptions.Add($"audio-files-append=\"{item}\"");
                    }
                }
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
        if (volume is < 0)
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

    /// <summary>
    /// 获取当前的缓存速度(byte/s).
    /// </summary>
    /// <returns>结果.</returns>
    public async Task<Result<long>> GetCacheSpeedAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, CacheSpeed, MpvFormat.Int64, out result));
        var cacheSpeedResult = WrapAsResult(errorCode, "Mpv | get cache speed failed");
        if (cacheSpeedResult.IsFailed)
        {
            return cacheSpeedResult;
        }

        return result.IntegerValue;
    }

    /// <summary>
    /// 设置字幕轨道.
    /// </summary>
    /// <param name="trackId">字幕ID.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetSubtitleTrackAsync(int? trackId)
    {
        var errorCode = MpvError.Success;
        var node = trackId.HasValue ? new MpvNode(trackId.Value) : new MpvNode("no");
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "sid", MpvFormat.Node, ref node));
        return WrapAsResult(errorCode, "Mpv | set subtitle track failed");
    }

    /// <summary>
    /// 设置外挂字幕轨道.
    /// </summary>
    /// <param name="externalUrl">字幕路径.</param>
    /// <param name="flag">行为标记.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetExternalSubtitleTrackAsync(string externalUrl, string flag = "select")
    {
        var errorCode = MpvError.Success;
        try
        {
            var waitTask = Task.Delay(TimeSpan.FromSeconds(8));
            var subTask = Task.Run(() => errorCode = MpvNative.SetCommand(_handle, ["sub-add", externalUrl, flag]));
            await Task.WhenAny(waitTask, subTask);
        }
        catch (Exception)
        {
            return Result.Fail("Mpv | set external subtitle track failed: operation timed out");
        }

        return WrapAsResult(errorCode, "Mpv | set external subtitle track failed");
    }

    /// <summary>
    /// 设置音频轨道.
    /// </summary>
    /// <param name="trackId">音频ID.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetAudioTrackAsync(int? trackId)
    {
        var errorCode = MpvError.Success;
        var node = trackId.HasValue ? new MpvNode(trackId.Value) : new MpvNode("no");
        await Task.Run(() => errorCode = MpvNative.SetProperty(_handle, "aid", MpvFormat.Node, ref node));
        return WrapAsResult(errorCode, "Mpv | set audio track failed");
    }

    /// <summary>
    /// 设置外挂音频轨道.
    /// </summary>
    /// <param name="externalUrl">音频路径.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task<Result> SetExternalAudioTrackAsync(string externalUrl)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, $"audio-add {externalUrl} cached"));
        return WrapAsResult(errorCode, "Mpv | set external audio track failed");
    }

    /// <summary>
    /// 获取文件的轨道列表.
    /// </summary>
    /// <returns>轨道信息.</returns>
    public async Task<Result<List<MpvTrackInfo>>> GetTracksAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "track-list", MpvFormat.Node, out result));
        if (errorCode != MpvError.Success)
        {
            return Result.Fail($"Mpv | get subtitle tracks failed: {errorCode}");
        }

        if (MpvNodeList.ToMpvNodeArray(result.RemoteNodeListValue) is not MpvNode[] trackList)
        {
            return Result.Fail("Mpv | get subtitle tracks failed: invalid node format");
        }

        var resultList = new List<MpvTrackInfo>();
        foreach (var item in trackList)
        {
            var trackMeta = MpvNodeList.ToDictionary(item.RemoteNodeListValue);
            var track = new MpvTrackInfo();
            var typeStr = trackMeta.TryGetValue("type", out var typeNode) ? typeNode.StringValue : null;
            track.Type = typeStr switch
            {
                "audio" => MpvTrackType.Audio,
                "video" => MpvTrackType.Video,
                "sub" => MpvTrackType.Subtitle,
                _ => MpvTrackType.Unknown,
            };
            track.Id = trackMeta.TryGetValue("id", out var idNode) ? Convert.ToInt32(idNode.IntegerValue) : -1;
            var title = trackMeta.TryGetValue("title", out var titleNode) ? titleNode.StringValue : null;
            var lang = trackMeta.TryGetValue("lang", out var langNode) ? langNode.StringValue : null;
            var codecDesc = trackMeta.TryGetValue("codec-desc", out var codecDescNode) ? codecDescNode.StringValue : null;
            var demux = trackMeta.TryGetValue("demux-samplerate", out var demuxNode) ? demuxNode.IntegerValue.ToString() : null;
            var decoder = trackMeta.TryGetValue("decoder-desc", out var decoderNode) ? decoderNode.StringValue : null;
            var metadata = trackMeta.TryGetValue("metadata", out var metadataNode) ? MpvNodeList.ToDictionary(metadataNode.RemoteNodeListValue) : null;
            long? audioChannels = trackMeta.TryGetValue("audio-channels", out var audioChannelsNode) ? audioChannelsNode.IntegerValue : null;
            var langString = string.IsNullOrEmpty(lang) ? default : new CultureInfo(lang).DisplayName;
            track.Title = track.Type switch
            {
                MpvTrackType.Audio => string.IsNullOrEmpty(lang) ? title ?? codecDesc : $"{title} {codecDesc}".Trim(),
                MpvTrackType.Subtitle => string.IsNullOrEmpty(lang) ? title ?? decoder ?? codecDesc : $"{title} {decoder} {codecDesc}".Trim(),
                _ => title,
            };

            if (track.Type == MpvTrackType.Audio && audioChannels.HasValue)
            {
                track.Title = $"{track.Title} ({audioChannels} ch)".Trim();
            }

            track.Language = lang;
            track.Current = trackMeta.TryGetValue("selected", out var currentNode) && currentNode.Flag != 0;
            resultList.Add(track);
        }

        return Result.Ok(resultList);
    }

    /// <summary>
    /// 获取当前的音频轨道信息.
    /// </summary>
    /// <returns><see cref="MpvTrackInfo"/>.</returns>
    public async Task<Result<MpvTrackInfo>> GetCurrentAudioTrackAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "current-tracks/audio", MpvFormat.Node, out result));
        if (errorCode != MpvError.Success)
        {
            return Result.Fail($"Mpv | get current audio track failed: {errorCode}");
        }

        var trackMeta = MpvNodeList.ToDictionary(result.RemoteNodeListValue);
        if (trackMeta == null || trackMeta.Count == 0)
        {
            return Result.Fail("Mpv | get current audio track failed: invalid node format");
        }

        var title = trackMeta.TryGetValue("title", out var titleNode) ? titleNode.StringValue : null;
        var codecDesc = trackMeta.TryGetValue("codecDesc", out var codecDescNode) ? codecDescNode.StringValue : null;
        var codec = trackMeta.TryGetValue("codec", out var codecNode) ? codecDescNode.StringValue : null;
        var lang = trackMeta.TryGetValue("lang", out var langNode) ? langNode.StringValue : null;
        var track = new MpvTrackInfo
        {
            Type = MpvTrackType.Audio,
            Id = trackMeta.TryGetValue("id", out var idNode) ? Convert.ToInt32(idNode.IntegerValue) : -1,
            Title = string.IsNullOrEmpty(codecDesc) ? title : $"{title} ({codecDesc})".Trim(),
            Codec = codec,
            Language = lang,
            Current = true,
        };

        return Result.Ok(track);
    }

    /// <summary>
    /// 获取当前的字幕轨道信息.
    /// </summary>
    /// <returns><see cref="MpvTrackInfo"/>.</returns>
    public async Task<Result<MpvTrackInfo>> GetCurrentSubtitleTrackAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "current-tracks/sub", MpvFormat.Node, out result));
        if (errorCode != MpvError.Success)
        {
            return Result.Fail($"Mpv | get current subtitle track failed: {errorCode}");
        }
        var trackMeta = MpvNodeList.ToDictionary(result.RemoteNodeListValue);
        if (trackMeta == null || trackMeta.Count == 0)
        {
            return Result.Fail("Mpv | get current subtitle track failed: invalid node format");
        }

        var title = trackMeta.TryGetValue("title", out var titleNode) ? titleNode.StringValue : null;
        var lang = trackMeta.TryGetValue("lang", out var langNode) ? langNode.StringValue : null;
        var codec = trackMeta.TryGetValue("codec", out var codecNode) ? codecNode.StringValue : null;
        var track = new MpvTrackInfo
        {
            Type = MpvTrackType.Subtitle,
            Id = trackMeta.TryGetValue("id", out var idNode) ? Convert.ToInt32(idNode.IntegerValue) : -1,
            Title = string.IsNullOrEmpty(lang) ? title : $"{title} ({lang})".Trim(),
            Codec = codec,
            Current = true,
        };
        return Result.Ok(track);
    }

    /// <summary>
    /// 设置音频通道布局.
    /// </summary>
    public async Task<Result> SetAudioChannelLayoutAsync(AudioChannelLayoutType layout, string[]? customLayouts = default)
    {
        var errorCode = MpvError.Success;
        var layoutStr = layout switch
        {
            AudioChannelLayoutType.Stereo => "stereo",
            AudioChannelLayoutType.Auto => "auto",
            AudioChannelLayoutType.Mono => "mono",
            AudioChannelLayoutType.Custom when customLayouts != null => string.Join(',', customLayouts),
            _ => throw new ArgumentOutOfRangeException(nameof(layout), "Unsupported audio channel layout type."),
        };

        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "audio-channels", layoutStr));
        return WrapAsResult(errorCode, "Mpv | set audio channel layout failed");
    }

    /// <summary>
    /// 获取章节列表.
    /// </summary>
    /// <returns>章节列表</returns>
    public async Task<Result<List<MpvChapterInfo>>> GetChaptersAsync()
    {
        var errorCode = MpvError.Success;
        var result = new MpvNode();
        await Task.Run(() => errorCode = MpvNative.GetProperty(_handle, "chapter-list", MpvFormat.Node, out result));
        if (errorCode != MpvError.Success)
        {
            return Result.Fail($"Mpv | get chapter list failed: {errorCode}");
        }

        if (MpvNodeList.ToMpvNodeArray(result.RemoteNodeListValue) is not MpvNode[] chapterList)
        {
            return Result.Fail("Mpv | get chapter list failed: invalid node format");
        }

        var resultList = new List<MpvChapterInfo>();
        foreach (var item in chapterList)
        {
            var chapterMeta = MpvNodeList.ToDictionary(item.RemoteNodeListValue);
            if (chapterMeta == null || chapterMeta.Count == 0)
            {
                continue;
            }
            var title = chapterMeta.TryGetValue("title", out var titleNode) ? titleNode.StringValue : null;
            var time = chapterMeta.TryGetValue("time", out var timeNode) ? timeNode.DoubleValue : 0;
            resultList.Add(new MpvChapterInfo
            {
                Title = title ?? string.Empty,
                Time = time,
            });
        }

        return Result.Ok(resultList);
    }

    /// <summary>
    /// 设置字幕位置.
    /// </summary>
    /// <param name="percentage">垂直方向百分比.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task SetSubtitlePositionAsync(int percentage)
    {
        if (percentage is < 0 or > 150)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Subtitle position must be between 0 and 150.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(percentage);
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "sub-pos", MpvFormat.Int64, ref node));
        ThrowIfFailed(errorCode, "Mpv | set subtitle position failed");
    }

    /// <summary>
    /// 设置次要字幕位置.
    /// </summary>
    /// <param name="percentage">垂直方向百分比.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task SetSecondarySubtitlePositionAsync(int percentage)
    {
        if (percentage is < 0 or > 150)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Secondary subtitle position must be between 0 and 150.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(percentage);
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "secondary-sub-pos", MpvFormat.Int64, ref node));
        ThrowIfFailed(errorCode, "Mpv | set secondary subtitle position failed");
    }

    /// <summary>
    /// 设置文本字幕字号
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task SetSubtitleFontSizeAsync(int fontSize)
    {
        if (fontSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Subtitle font size must greater than 0.");
        }

        var errorCode = MpvError.Success;
        var node = new MpvNode(fontSize);
        await Task.Run(() => errorCode = MpvNative.SetOption(_handle, "sub-font-size", MpvFormat.Int64, ref node));
        ThrowIfFailed(errorCode, "Mpv | set subtitle font size failed");
    }

    /// <summary>
    /// 设置字幕字体.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task SetSubtitleFontFamilyAsync(string fontFamily)
    {
        if (string.IsNullOrEmpty(fontFamily))
        {
            throw new ArgumentNullException(nameof(fontFamily), "Subtitle font family cannot be null or empty.");
        }

        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetOptionString(_handle, "sub-font", fontFamily));
        ThrowIfFailed(errorCode, "Mpv | set subtitle font family failed");
    }

    /// <summary>
    /// 移除字幕轨道.
    /// </summary>
    /// <param name="subtitleId">字幕 ID.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task RemoveSubtitleAsync(string? subtitleId)
    {
        var errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, $"sub-remove {subtitleId ?? string.Empty}".Trim()));
        ThrowIfFailed(errorCode, "Mpv | remove subtitle failed");
    }
}
