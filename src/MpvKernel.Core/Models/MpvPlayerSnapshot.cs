// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Models;

internal sealed class MpvPlayerSnapshot(string? filePath, MpvPlayOptions? options)
{
    public string? FilePath { get; } = filePath;

    public MpvPlayOptions? Options { get; internal set; } = options;
}
