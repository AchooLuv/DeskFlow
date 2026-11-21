# DeskFlow v1.0.31

修复任务栏完全透明在开始菜单、搜索浮层和 DeskFlow 自身窗口切换后失效的问题。

## 实现方式

- 用 `SetWinEventHook` 监听 shell 浮层窗口和前台窗口变化，事件驱动，不做常驻轮询。
- 仅在这些状态变化时刷新：
  - 开始菜单/搜索浮层打开、关闭
  - DeskFlow 自己的窗口/托盘菜单进入、离开前台
- 每次只做：
  - 1 次完整重应用（写一次 `TaskbarAcrylicOpacity=0`、广播一次、设置 accent）
  - 9 次轻量 accent 重设
- 普通 Alt-Tab/切换其他应用不触发任务栏重设，避免之前的性能问题。
- 设置关闭时仍恢复任务栏原样式。

## 验证

实际安装版、实际设置下：

- 打开/关闭搜索浮层后任务栏保持透明
- 关闭 DeskFlow 设置窗口到托盘后任务栏保持透明
- 普通前台切换不触发任务栏刷新

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `4DF631A9A2C788E76C266ED010AF0030DA3FDFBE69EE8244D6276F39BF0E3AF2` |
| `DeskFlow-1.0.31-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `D09289EA5038E51AD29B3AC130D3E092C9F28778271CE857816503F48E777E13` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
