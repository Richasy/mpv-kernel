// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.BiliKernel.Bili.Authorization;
using Richasy.BiliKernel.Bili.Media;
using Richasy.BiliKernel.Bili.User;
using Richasy.BiliKernel.Models.Media;
using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.Player.Models;
using System.Text.RegularExpressions;

namespace WinUISample.Models;

internal sealed class BiliMediaSourceResolver(string link) : MediaSourceResolverBase
{
    private const string VideoReferer = "https://www.bilibili.com";
    private const string VideoUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69";

    public override async Task<MpvMediaSource> GetSourceAsync()
    {
        var videoId = GetVideoId(link);
        if (string.IsNullOrEmpty(videoId))
        {
            throw new Exception("无效的视频链接.");
        }

        var playerService = this.Get<IPlayerService>();
        var videoInfo = await playerService.GetVideoPageDetailAsync(new MediaIdentifier(videoId, default, default))
            ?? throw new Exception("获取视频信息失败.");

        var userService = this.Get<IMyProfileService>();
        var userInfo = await userService.GetMyProfileAsync();
        var isVip = userInfo.IsVip;

        var firstPart = videoInfo.Parts.FirstOrDefault();
        var playInfo = await playerService.GetVideoPlayDetailAsync(new MediaIdentifier(videoInfo.Information.Identifier.Id, default, default), Convert.ToInt64(firstPart.Identifier.Id));
        var availableFormats = playInfo.Formats.Where(p => !p.NeedVip || (p.NeedVip && (isVip ?? false))).ToList();

        var title = videoInfo.Information.Identifier.Title;
        var headers = new Dictionary<string, string>();
        var cookies = this.Get<IBiliCookiesResolver>().GetCookieString();
        var referer = VideoReferer;
        var userAgent = VideoUserAgent;
        headers.Add("Cookie", cookies);
        headers.Add("Referer", referer);
        var maxQuality = availableFormats.Max(p => p.Quality);
        var selectedFormat = availableFormats.Find(p => p.Quality == maxQuality);
        var vSeg = playInfo.Videos?.FirstOrDefault(p => p.Id == selectedFormat.Quality.ToString());
        var maxAudioQuality = playInfo.Audios?.Max(p => Convert.ToInt32(p.Id));
        var aSeg = playInfo.Audios?.FirstOrDefault(p => p.Id == maxAudioQuality.ToString());
        var videoUrl = vSeg?.BaseUrl;
        var audioUrl = aSeg?.BaseUrl;
        var options = new MpvPlayOptions
        {
            WindowHandle = WindowHandle,
            HttpHeaders = headers,
            UserAgent = userAgent,
            EnableCookies = true,
        };

        var fileUrl = string.Empty;
        if (!string.IsNullOrEmpty(videoUrl))
        {
            fileUrl = videoUrl;
            if (!string.IsNullOrEmpty(audioUrl))
            {
                options.ExtraAudioUrl = audioUrl;
            }
        }
        else if (!string.IsNullOrEmpty(audioUrl))
        {
            fileUrl = audioUrl;
        }

        return new MpvMediaSource(fileUrl, videoUrl, title, options);
    }

    private static string GetVideoId(string url)
    {
        var bvMatch = Regex.Match(url, @"[Bb][Vv]([0-9A-Za-z]+)");
        if (bvMatch.Success)
        {
            return "BV" + bvMatch.Groups[1].Value;
        }

        // 匹配 AV 号（av 后跟数字）
        var avMatch = Regex.Match(url, @"[Aa][Vv](\d+)");
        if (avMatch.Success)
        {
            return avMatch.Groups[1].Value;
        }

        return string.Empty;
    }
}
