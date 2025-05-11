// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.WebDavKernel;
using System.Text.Json.Serialization;

namespace WinUISample;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WebDavConfig))]
internal sealed partial class JsonGenContext : JsonSerializerContext
{
}
