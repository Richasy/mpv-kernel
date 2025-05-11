// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Richasy.BiliKernel;
using Richasy.ReaderKernel;
using Richasy.WinUIKernel.Share;
using Richasy.WinUIKernel.Share.Toolkits;
using RichasyKernel;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using Windows.Storage;
using WinUISample.Extensions;
using WinUISample.ViewModels;

namespace WinUISample;

internal static class GlobalDependencies
{
    /// <summary>
    /// 获取服务提供程序.
    /// </summary>
    public static Kernel Kernel { get; private set; }

    public static void Initialize()
    {
        if (Kernel is not null)
        {
            return;
        }

        Kernel = Kernel.CreateBuilder()
            .AddSerilog()
            .AddDispatcherQueue()
            .AddShareToolkits()
            .AddXamlRootProvider()

            .AddBiliClient()
            .AddBiliAuthenticator()
            .AddWinUICookiesResolver()
            .AddWinUITokenResolver()
            .AddWinUIQRCodeResolver(RenderBiliQRCodeAsync)
            .AddTVAuthenticationService()
            .AddMyProfileService()
            .AddUserService()
            .AddPlayerService()

            .AddWebDavConnector()

            .AddSingleton<AppViewModel>()
            .AddSingleton<LocalVideoPageViewModel>()
            .AddSingleton<BiliVideoPageViewModel>()
            .AddSingleton<WebDavVideoPageViewModel>()
            .AddTransient<PlayerViewModel>()
            .Build();

        WinUIKernelShareExtensions.InitializeShareKernel(Kernel);
    }

    public static IKernelBuilder AddDispatcherQueue(this IKernelBuilder builder)
    {
        var queue = DispatcherQueue.GetForCurrentThread();
        builder.Services.AddSingleton(queue);
        return builder;
    }

    public static IKernelBuilder AddShareToolkits(this IKernelBuilder builder)
    {
        builder.Services.AddSingleton<IAppToolkit, SharedAppToolkit>()
            .AddSingleton<ISettingsToolkit, SharedSettingsToolkit>()
            .AddSingleton<IFileToolkit, SharedFileToolkit>()
            .AddSingleton<IFontToolkit, SharedFontToolkit>();
        return builder;
    }

    public static IKernelBuilder AddXamlRootProvider(this IKernelBuilder builder)
    {
        builder.Services.AddSingleton<IXamlRootProvider, XamlRootProvider>();
        return builder;
    }

    public static IKernelBuilder AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IKernelBuilder kernelBuilder)
        where T : class
    {
        kernelBuilder.Services.AddSingleton<T>();
        return kernelBuilder;
    }

    public static IKernelBuilder AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IKernelBuilder kernelBuilder)
        where T : class
    {
        kernelBuilder.Services.AddTransient<T>();
        return kernelBuilder;
    }

    public static RichasyKernel.IKernelBuilder AddSerilog(this RichasyKernel.IKernelBuilder builder)
    {
        var loggerPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logger");
        if (!Directory.Exists(loggerPath))
        {
            Directory.CreateDirectory(loggerPath);
        }

        // Create a logger with current date.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(loggerPath, $"log-{DateTimeOffset.Now:yyyy-MM-dd}.txt"))
            .CreateLogger();

        builder.Services.AddLogging(b => b.AddSerilog(dispose: true));
        return builder;
    }

    public static T Get<T>(this object ele)
        where T : class
        => Kernel.GetRequiredService<T>();

    private static Task RenderBiliQRCodeAsync(byte[] imageData)
    {
        var vm = Kernel.GetRequiredService<BiliVideoPageViewModel>();
        vm.RenderQRCodeCommand.Execute(imageData);
        return Task.CompletedTask;
    }
}
