// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Richasy.WebDavKernel;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;
using System.Collections.ObjectModel;

namespace WinUISample.ViewModels;

/// <summary>
/// WebDav 视频页面视图模型
/// </summary>
public sealed partial class WebDavVideoPageViewModel(
    ILogger<WebDavVideoPageViewModel> logger,
    WebDavService service,
    DispatcherQueue dispatcherQueue,
    IFileToolkit fileToolkit) : ViewModelBase
{
    internal WebDavConfig? _config;

    [ObservableProperty]
    public partial bool IsNoConfig { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsItemsEmpty { get; set; }

    [ObservableProperty]
    public partial List<WebDavStorageItemViewModel>? Items { get; set; }

    /// <summary>
    /// 路径段.
    /// </summary>
    public ObservableCollection<WebDavPathSegment> PathSegments { get; } = [];

    /// <summary>
    /// 初始化.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        if (_config is not null)
        {
            return;
        }

        var config = await fileToolkit.ReadLocalDataAsync("WebDavConfig.json", JsonGenContext.Default.WebDavConfig);
        if (config is null || string.IsNullOrEmpty(config.Host))
        {
            IsNoConfig = true;
            return;
        }

        LoadConfig(config);
    }

    private void LoadConfig(WebDavConfig config)
    {
        _config = config;
        IsNoConfig = false;
        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            PathSegments.Clear();
            service.Initialize(config);

            if (PathSegments.Count == 0)
            {
                PathSegments.Add(new WebDavPathSegment { Name = "Root", Path = "/" + config.Path.Trim('/') });
            }

            LoadPathCommand.Execute(PathSegments.Last().Path);
        });
    }

    [RelayCommand]
    private async Task LoadPathAsync(string path)
    {
        var lastPath = PathSegments.LastOrDefault();
        if (lastPath?.Path != path)
        {
            var lastEqualIndex = -1;
            for (var i = 0; i < PathSegments.Count; i++)
            {
                if (PathSegments[i].Path == path)
                {
                    lastEqualIndex = i;
                    break;
                }
            }

            if (lastEqualIndex != -1)
            {
                for (var i = PathSegments.Count - 1; i > lastEqualIndex; i--)
                {
                    PathSegments.RemoveAt(i);
                }
            }
            else
            {
                var directoryName = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                PathSegments.Add(new WebDavPathSegment { Name = Uri.UnescapeDataString(directoryName), Path = path });
            }
        }

        try
        {
            IsLoading = true;
            Items = default;
            var items = await service.GetItemsAsync(path);
            Items = [.. items.Select(p => new WebDavStorageItemViewModel(p))];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "加载 WebDav 路径失败.");
        }
        finally
        {
            IsLoading = false;
            IsItemsEmpty = Items is null || Items.Count == 0;
        }
    }

    [RelayCommand]
    private async Task AddConfigAsync(WebDavConfig webDavConfig)
    {
        await fileToolkit.WriteLocalDataAsync("WebDavConfig.json", webDavConfig, JsonGenContext.Default.WebDavConfig);
        LoadConfig(webDavConfig);
    }

    [RelayCommand]
    private async Task UpdateConfigAsync(WebDavConfig webDavConfig)
    {
        if (_config is null)
        {
            return;
        }

        await fileToolkit.WriteLocalDataAsync("WebDavConfig.json", _config, JsonGenContext.Default.WebDavConfig);
        LoadConfig(_config);
    }
}

/// <summary>
/// WebDav 路径片段.
/// </summary>
public sealed class WebDavPathSegment
{
    /// <summary>
    /// 显示名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 路径.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 是否为根目录.
    /// </summary>
    public bool IsRoot => Path == "/";

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WebDavPathSegment segment && Path == segment.Path;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Path);
}