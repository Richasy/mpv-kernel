// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CliSample;
using Microsoft.Extensions.Logging;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Player;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
var client = await MpvClient.CreateAsync(@"C:\Users\zrich\Desktop\libmpv-2.dll", logger: logger);
client.ReachFileLoading += async(s, _) =>
{
    Console.WriteLine("[MPV] Reach file loading.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
};
client.ReachFileEnd += async (s, _) =>
{
    Console.WriteLine("[MPV] Reach file end.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
    await client.DisposeAsync();
};
client.ReachFileLoaded += async (s, _) =>
{
    Console.WriteLine("[MPV] Reach file loaded.");
    var state = await client.GetPlayerStateAsync();
    Console.WriteLine($"State: {state}");
    var duration = await client.GetDurationAsync();
    Console.WriteLine($"Duration: {Math.Round(duration.Value,2)}s");
};

await client.SetLogLevelAsync(MpvLogLevel.Warn);
await client.UseIdleAsync(default);
await client.UseKeepOpenAsync(true);
var player = new MpvPlayer(client, new WebSourceResolver());
await player.InitializeAsync();
await Task.Delay(4000);
Console.WriteLine($"Position: {Math.Round(player.Position, 2)}s");
Console.WriteLine($"State: {player.PlaybackState}");
Console.ReadKey();
