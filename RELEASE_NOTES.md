# DeskFlow v1.0.28

关于页面去掉“播放引擎 VLC 3.0.21 · WebP libwebp 1.6.0”这一行。

## 本版调整

- 删除“关于 DeskFlow”中的播放引擎版本行。
- 后续的 GitHub 链接和开源说明上移，避免留下空档。
- 不涉及壁纸挂载、VLC、任务栏、开机启动或窗口尺寸逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `DC7102CAFF14CBA34E178D4B0C2D6775ADCE8B788071728DF0F0782759DB83DB` |
| `DeskFlow-1.0.28-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `F86A18C717F913EF5CAF49A1A9D55F6C8DB83CB5243BE16DB045EA2980CFD901` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
