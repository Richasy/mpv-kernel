// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Dispatching;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;
using Richasy.WinUIKernel.Share.ViewModels;

namespace Richasy.MpvKernel.WinUI.Core;

public abstract partial class MpvPlayerViewModelBase : ViewModelBase
{
    /// <summary>
    /// 创建 <see cref="MpvPlayerViewModelBase"/> 类的新实例.
    /// </summary>
    protected MpvPlayerViewModelBase(
        IMediaSourceResolver sourceResolver,
        IMediaBackgroundProvider uiProvider,
        ILogger? logger = default)
    {
        Logger = logger ?? NullLogger.Instance;
        SourceResolver = sourceResolver;
        BackgroundUIProvider = uiProvider;
        Queue = DispatcherQueue.GetForCurrentThread();
    }

    /// <summary>
    /// 进行内部初始化.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    protected virtual async Task CoreInitializeAsync(
        MpvInitializeOptions? options,
        MpvLogLevel logLevel = MpvLogLevel.Warn,
        bool useIdle = true,
        bool keepOpen = true)
    {
        if (Client is not null)
        {
            return;
        }

        Client = await MpvClient.CreateAsync(options, Logger);
        Client.DataNotify += OnClientDataNotify;
        Client.ReachFileLoading += OnFileLoading;
        Client.ReachFileLoaded += OnFileLoaded;

        await Client.SetLogLevelAsync(logLevel);
        await Client.UseIdleAsync(useIdle);
        await Client.UseKeepOpenAsync(keepOpen);
    }
}
