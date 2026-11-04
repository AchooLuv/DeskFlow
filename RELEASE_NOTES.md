# DeskFlow v1.0.25

修复最大化/拖动窗口边缘时，三个页面的内容不跟随窗口宽度重排的问题。

## 原因

三个页面都继承自 `PageBase`，但 `PageBase` 只在切换页面或刷新状态时调用 `LayoutPage()`，没有在控件尺寸变化时重排。因此右侧内容一直保持首次布局的固定宽度。

## 修复

在 `PageBase.OnResize` 中调用 `LayoutPage()`：

- 图片轮播页
- 动态壁纸页
- 设置页

现在最大化、还原、拖动边缘改变窗口大小时，预览区、侧栏卡片、文件夹卡片、操作区和设置卡片都会跟随宽度重新布局。

本版不涉及壁纸挂载、VLC、任务栏或开机启动逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `8E70196E7A32919A50421CEF1E7312F074D4105D1BBAC9598805E06A2EDF59F1` |
| `DeskFlow-1.0.25-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `F106EB49DEEA511611072306285AA0EAB23A7825B73C56B0122DF96674BB2017` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
