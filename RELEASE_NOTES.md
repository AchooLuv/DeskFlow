# DeskFlow v1.0.26

修复图片轮播/动态壁纸页最大化后，预览区高度不跟随放大的问题。

## 本版调整

- 第一行预览区（以及右侧同高卡片）现在会吃掉窗口增加的高度。
- 底部操作卡保持稳定的最小高度，不再独占最大化后的全部剩余空间。
- 普通窗口高度下仍保持原来的 236px 预览高度。
- 设置页布局不变。
- 不涉及壁纸挂载、VLC、任务栏或开机启动逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `532A583B37F4D6B9EE964CB311E8996C297CBE939E5CD67138D210644BB9A6F2` |
| `DeskFlow-1.0.26-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `CC6355DDCB92F7098550DA07C6D988DF5B81AF35301FF2D9752ED87F3572DD99` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
