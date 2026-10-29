# DeskFlow v1.0.22

修复动态壁纸模式下启动、切换、退出的短暂黑屏。

## 本版修复

- **启动不再触发 Explorer 重绘**：移除了每次挂载都向 Progman 发送未公开 `0x052C` 的逻辑；该消息会让 Explorer 重排桌面并产生可见黑闪。
- **视频首帧前不显示黑色交换链**：libVLC 的 `Started` 事件早于 Direct3D 首帧，v1.0.22 在 `Started` 后保留过渡背景约 400ms，再显示视频宿主。
- **切换模式**：
  - 视频 → 图片：先显示旧图片/系统壁纸，再停止 VLC，避免 D3D 销毁黑帧。
  - 图片 → 视频：旧图片/系统壁纸保持显示，视频首帧延迟揭示。
- **退出**：先隐藏视频、显示过渡背景，再恢复系统壁纸、销毁窗口，避免退出瞬间黑闪。
- **系统壁纸过渡背景**：继续使用当前系统壁纸作为启动/切换/退出时的过渡内容。
- Windows 10 只挂 WorkerW，不退回 Progman。

## 验证

- 用 30fps 桌面录制启动过程，修复前约 0.4s 全黑，修复后未检测到黑帧。
- 动态壁纸启动/切换/退出仍需在真实桌面上确认。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `260571119EB997459B511E7E212B86071D23A633905FB8024421DBB4FAD7D946` |
| `DeskFlow-1.0.22-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `83AE1D0726ED9A91F0A0CC377E914AF8BF47CC701347D176CE0A258218284463` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
