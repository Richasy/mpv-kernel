// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
MpvNative.Initialize(@"C:\Users\zrich\Desktop\libmpv-2.dll");
var instance = await MpvInstance.CreateAsync(logger: logger);
var client = await instance.CreateClientAsync("TestClient");
client.Initialize();
client.Shutdown += async (s, _) => await instance.RemoveClientAsync(((MpvClient)s!).ClientName);
await client.SetLogLevelAsync(MpvLogLevel.Warn);
await client.PlayAsync("https://sf1-cdn-tos.huoshanstatic.com/obj/media-fe/xgplayer_doc_video/mp4/xgplayer-demo-360p.mp4");
await Task.Delay(5000);
await instance.DisposeAsync();
Console.ReadKey();
