// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Core.Enums;

internal enum MpvFileType
{
    /// <summary>
    /// 本地文件.
    /// </summary>
    LocalFile,

    /// <summary>
    /// 网络文件（特定于需要设置请求头的文件）.
    /// </summary>
    NetworkFile,

    /// <summary>
    /// 视频或音频切片.
    /// </summary>
    Segments,
}
