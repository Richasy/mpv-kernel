// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Core.Models;

internal sealed class MpvPlayerSnapshot
{
    public MpvFileType Type { get; private set; }

    public string? FilePath { get; private set; }

    public MpvPlayOptions? Options { get; private set; }

    public static MpvPlayerSnapshot Create(string? filePath, MpvPlayOptions? options)
    {
        return new MpvPlayerSnapshot
        {
            Type = MpvFileType.LocalFile,
            FilePath = filePath,
            Options = options,
        };
    }
}
