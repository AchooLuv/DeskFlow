# DeskFlow v1.0.19

修复启动/退出黑屏。

## 根因

- 壁纸窗口在 `WorkerW/Progman` 尚未就绪时会退化成**全屏普通黑窗**，开机自动启动时 Explorer 还没准备好，就会覆盖整个桌面，表现为纯黑、无法操作。
- 即使挂载成功，旧代码也会在图片/视频首帧准备好之前先显示黑色窗口；视频模式下 `StartVideo` 提前 `ShowVideo()`，加重了这个问题。
- 退出时 fallback 窗口只清空图片、不隐藏，桌面可能继续被黑窗覆盖。

## 修复

- 挂载失败时**绝不再显示全屏 fallback 窗口**，保持隐藏，10 秒维护定时器重试。
- 壁纸窗口挂载后默认隐藏，只有：
  - 图片已经加载完成，或
  - 视频收到首帧/Started 事件
  才显示。
- `ShowFallback`、`SetImage(null)`、`HideVideo`（无图片时）统一隐藏窗口，露出系统壁纸。
- 桌面挂载只改变窗口位置，不再顺带 `SWP_SHOWWINDOW`，避免重试时把黑窗重新显示。
- 退出/Detach 时先隐藏再解除父子关系，避免退出后残留黑窗。

## 说明

如果你用开机自动启动，请先把 `C:\Program Files\DeskFlow.exe` 替换成 v1.0.19，否则开机启动的仍是旧版。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `AE35B3482CD33CED94516FC9BE5759B6AAFF3028B5A7C6281FDA6F52FD80D1E0` |
| `DeskFlow-1.0.19-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `5F170584CAFE1ECA4F2B89923861448AE01D2498B6B59291C67EDC55EA616BEA` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
