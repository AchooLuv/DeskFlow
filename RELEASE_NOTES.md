# DeskFlow v1.0.29

优化设置页“关于 DeskFlow”的信息和按钮布局。

## 本版调整

- 关于信息更贴合软件本身：
  - DeskFlow 版本
  - 图片轮播 · 动态壁纸 · 任务栏透明
  - 系统平台
  - GitHub 地址
  - GPL v3 / © 2026 AchooLuv
- “打开日志 / 开源许可 / 恢复默认设置”移到关于卡片右上角，避免和最后一行文字重叠。
- 关于卡片的文本宽度跟随窗口调整。
- 最小窗口和稍小窗口下不再发生按钮与文字重叠。
- 不涉及壁纸挂载、VLC、任务栏、开机启动或窗口尺寸逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `1849D1A77796447B7F42851213C5AE1468A350F098CBA3F50F37344EA3CBAD88` |
| `DeskFlow-1.0.29-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `7CAD4FC26F6F652BDAD43AA891544EAA9F7AF646B8120B8DCF5A8B051E77F34B` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
