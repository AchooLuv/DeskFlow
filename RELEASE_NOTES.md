# DeskFlow v1.0.27

限制窗口最小尺寸。

## 本版调整

- `MinimumSize` 从 `940 × 620` 调整为 `1000 × 660`。
- 这是之前版本的默认窗口大小；现在窗口不能再缩小到比它更小。
- 最大化、还原、放大不受影响。
- 不涉及壁纸挂载、VLC、任务栏或开机启动逻辑。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `4E9BC572FF777B679060C733EDA982B38F911B4FF62284EF5C219DAAF22C3DFB` |
| `DeskFlow-1.0.27-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `7A09280E8527835DBBDF25CF36C353F4F017CC0ED8EC875872BD03E797DA6B86` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
