// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core;

public sealed partial class MpvClient
{
    /// <summary>
    /// 播放指定路径的文件.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task PlayAsync(string filePath)
    {
        List<string> args = ["loadfile", $"\"{filePath}\""];
        MpvError errorCode = MpvError.Success;
        await Task.Run(() => errorCode = MpvNative.SetCommandString(_handle, string.Join(' ', args)));
        ThrowIfFailed(errorCode, $"{ClientName} | loadfile failed");
    }
}
