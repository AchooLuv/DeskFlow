# DeskFlow v1.0.20

修复启动、退出和切换壁纸时的黑屏。

## 根因

- Windows 10 壁纸应挂到 **WorkerW**。旧代码在 WorkerW 未就绪时退到 **Progman**，隐藏壁纸窗口后会露出 Progman 的黑色背景，导致启动、切换、退出黑屏。
- 切换模式时旧代码先清空/隐藏壁纸窗口，再加载新内容，中间必然出现黑屏。
- 挂载失败时旧版会显示全屏普通黑窗，开机 Explorer 未就绪时会直接盖住桌面。

## 修复

- Windows 10 只接受 **WorkerW**；找不到时保持隐藏并重试，不再退回 Progman。
- 切换视频/图片时保留上一张图片或视频帧，直到新内容就绪，不再先隐藏窗口。
- 挂载成功后窗口保持隐藏，只有图片或视频首帧就绪才显示。
- 退出时先隐藏窗口、解除父子关系并重绘 Progman/WorkerW，避免残留黑窗。
- 保留 v1.0.19 的 fallback 隐藏逻辑。

## 说明

- 开机自动启动必须先替换 `C:\Program Files\DeskFlow.exe` 为 v1.0.20，否则开机仍运行旧版。
- 事件日志中没有发现 NVIDIA 显示驱动 TDR/超时记录，GPU 状态正常；本次黑屏更符合桌面窗口挂载/隐藏路径问题。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `D181F5B060EA52F96B25548EA16D67EA55952BB0A01FFEAAB0F68830294FB533` |
| `DeskFlow-1.0.20-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `DCB5B0794A5CE1BA10552F1E803C31B7407ED4CBD8464BFD10CCA43C8A7FCCD2` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
