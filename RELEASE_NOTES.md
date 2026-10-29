# DeskFlow v1.0.24

紧急修复 v1.0.23 重启后“壁纸未挂载到桌面”的问题。

## 回归原因

v1.0.22 为了消除黑闪，完全移除了向 Progman 发送 `0x052C` 的逻辑。但该消息正是 **WorkerW 不存在时创建桌面挂载点** 所必需的。重启后 Explorer 尚未创建 WorkerW，程序就永远找不到挂载目标，于是提示未挂载。

## 修复

- **只在 WorkerW 不存在时发送一次 `0x052C`** 创建挂载点。
- 已有 WorkerW 时绝不发送，避免每次挂载都让 Explorer 重排桌面造成黑闪。
- 发送后重新扫描 WorkerW；找不到就保持窗口隐藏并重试。
- 保留 v1.0.23 的视频显示修复（`BeginInvoke`，不依赖空的 `SynchronizationContext`）。
- 保留系统壁纸过渡背景、视频首帧延迟揭示、退出恢复系统壁纸。

## 说明

- 开机自动启动请先把 `C:\Program Files\DeskFlow.exe` 替换为 v1.0.24。
- 这次修复的是“完全用不了”的挂载回归；黑闪问题后续再继续收敛。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `927BD86914F4FE18E2FEAE303437A9EFA9399DBEB852077E91FD4CFCCFA55439` |
| `DeskFlow-1.0.24-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `118D34FFC487A425044A66CB9FC163A8798B40E172E27031E6F12954EB1B02D1` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
