# 架构设计

![架构设计](./assets/arch.png)

## 本机层

从 [zhongfly/mpv-winbuild](https://github.com/zhongfly/mpv-winbuild) 获取构建的 libmpv2.dll

## 互操作

为方便 C# 调用，需将 mpv 暴露的 client.h 中公开的数据结构、方法转换成 C# 可用的互操作代码，基于 P/Invoke 实现。

## 核心层

核心对象为 `MpvClient`，建立在互操作层的基础上，可以视作对 [mpv.io](https://mpv.io/manual/stable/) 的无头实现。

它公开一系列现代化 API，并简化常用播放功能的调用。

初始化一个 MpvClient 视作创建一个播放器实例，其本身不存储任何状态。

在理想情况下，调用方不需要关心互操作层，只需要在初始化 MpvClient 时注入 libmpv2.dll 的路径。

但考虑到应尽可能将控制权限移交给开发者，`MpvClient` 仍会暴露 `MpvInteropHandle` 给调用方，调用方可以直接通过 `MpvNative` 访问底层API。

## 状态层

该层将是 UI 绑定的数据源，但本身在设计时不应与 WinUI 捆绑，它是平台无关的视图模型。

状态层被封装成一个 `MpvPlayer` 类型，其本身不负责创建 MpvClient 实例，MpvClient 应由调用方创建并初始化，然后注入到 MpvPlayer 之中，以此来尽可能地解耦。

MpvPlayer 负责维护播放器内部状态，主要对接外部模块（比如字幕、播放列表等），播放器内部操作，比如暂停、播放等将通过 MpvClient 进行。

