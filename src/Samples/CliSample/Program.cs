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
client.ReachFileLoading += async(s, _) =>
{
    Console.WriteLine($"[{client.ClientName}] Reach file loading.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
};
client.ReachFileEnd += async (s, _) =>
{
    Console.WriteLine($"[{client.ClientName}] Reach file end.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
    await instance.DisposeAsync();
};
client.ReachFileLoaded += async (s, _) =>
{
    Console.WriteLine($"[{client.ClientName}] Reach file loaded.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
    var duration = await client.GetDurationAsync();
    Console.WriteLine($"Duration: {Math.Round(duration,2)}s");
};

await client.SetLogLevelAsync(MpvLogLevel.Warn);
await client.UseIdleAsync(true);
var state = await client.GetPlayerStateAsync();
Console.WriteLine($"State: {state}");
await client.PlayAsync("https://www.tootootool.com/wp-content/uploads/2020/11/big_buck_bunny_720p_1mb.mp4");
await Task.Delay(5000);
var pos = await client.GetCurrentPositionAsync();
Console.WriteLine($"Position: {Math.Round(pos, 2)}s");
state = await client.GetPlayerStateAsync();
Console.WriteLine($"State: {state}");
Console.ReadKey();
