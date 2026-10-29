# DeskFlow v1.0.23

紧急修复 v1.0.22 动态壁纸完全无法播放的问题。

## 回归原因

v1.0.22 为了延迟显示视频，使用了：

```csharp
SynchronizationContext sync = _sync;
```

但 `WallpaperController` 是在 `Application.Run()` 之前构造的，此时 `SynchronizationContext.Current` 为 `null`，延迟回调直接返回，`ShowVideo()` 永远不会执行。所以视频一直在后台播放，窗口却始终不显示。

## 修复

- 延迟显示视频改为直接投递到壁纸窗口句柄：
  - `WallpaperWindow.BeginInvoke(...)`
  - 不再依赖 `SynchronizationContext`
- 视频首帧前继续显示系统壁纸/旧画面，约 400ms 后显示视频。
- 停止视频时先隐藏视频、显示过渡背景，再停止 VLC。
- 退出时先 `PrepareExit()`，再恢复系统壁纸、销毁窗口。
- 保留 v1.0.22 移除 `0x052C` 桌面重排、WorkerW 目标修正、系统壁纸恢复。

## 验证

- 本地启动后确认 VLC video main/output 窗口为 `vis=True`，动态壁纸可显示。
- 启动录制不再出现持续黑帧。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `2FE9104E299DDDA7D19BC04470AD99687AE8E75098A927E657BC6FA56EC298E8` |
| `DeskFlow-1.0.23-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `290B4A8E841904CFB5ABEFA0A66A10A399A12F0B555D9BCE5B369CE082AB01CF` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
