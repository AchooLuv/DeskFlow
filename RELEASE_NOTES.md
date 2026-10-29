# DeskFlow v1.0.21

进一步修复启动/切换的短暂黑屏和退出后桌面全黑。

## 本版修复

- **启动不再黑屏**：启动时把系统当前壁纸作为过渡背景，先显示它，再替换为图片或视频，不再露出黑色父窗口。
- **切换模式不再黑屏**：保留上一张图片/视频帧；没有旧内容时显示系统壁纸，直到新内容就绪。
- **退出恢复系统壁纸**：退出前先通过 `SystemParametersInfo(SPI_SETDESKWALLPAPER)` 恢复当前系统壁纸，再隐藏/销毁壁纸窗口，并重绘 Progman/WorkerW。
- **Windows 10 只挂 WorkerW**：找不到 WorkerW 时保持隐藏重试，不再退回 Progman 的黑色背景。
- **不再显示全屏 fallback 黑窗**。
- 保留 v1.0.20 的 WorkerW 目标修正、切换保留旧内容、退出重绘。
- 保留之前 VLC/WebP/异步预览/性能清理。

## 说明

- 开机自动启动必须先替换 `C:\Program Files\DeskFlow.exe` 为 v1.0.21，否则仍然运行旧版。
- 系统日志未发现 NVIDIA 显示驱动 TDR/超时，GPU 状态正常；本次修复针对桌面窗口和系统壁纸恢复路径。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `7FB9C99209286C99AA0D8DBA4D38BE404BE1432717EE380A62E5A066FD2CFEB8` |
| `DeskFlow-1.0.21-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `0A641536C58CEA78BDBABC058EE6DF83B8BBC8247958811F4D79E51999CEAE04` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
