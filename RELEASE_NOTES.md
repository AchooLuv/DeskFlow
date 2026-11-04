# DeskFlow v1.0.30

把“打开日志 / 开源许可 / 恢复默认设置”三个按钮放回关于卡片右下角，同时保留 v1.0.29 的紧凑信息排版。

## 本版调整

- 三个按钮恢复底部右对齐。
- 关于文字区宽度收窄到按钮左侧，最后一行不会再和按钮重叠。
- 关于信息保持：
  - 版本
  - 图片轮播 · 动态壁纸 · 任务栏透明
  - 系统平台
  - GitHub 地址
  - GPL v3 / © AchooLuv
- 不涉及壁纸挂载、VLC、任务栏、开机启动或窗口尺寸逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `33BF14080E5DA20704BD310224C7ACE5CE657C63C42302C1F65EBFC6B1ED4A9F` |
| `DeskFlow-1.0.30-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `8C4BBEDC5DD6494E6EC457FAA262F9A4D3E2BBA93F72E31D3BAD0F1D15777B8B` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
