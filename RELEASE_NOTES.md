# DeskFlow v1.0.4

精简播放引擎，并修复普通权限启动与视频窗口嵌入问题。

## 本版更新

- **安装包从约 60 MB 降到约 19.5 MB**：只打包本地视频播放需要的 VLC 插件，移除了 VLC GUI、编码器、串流、Lua、可视化和网络访问插件。
- **修复普通权限无法启动**：构建目录/程序文件曾继承 Windows Low 完整性标签，双击时进程被降为低权限，无法写入 `%LOCALAPPDATA%`。v1.0.4 的发布文件为 Medium 完整性，直接双击即可启动。
- **修复视频另开 VLC 窗口**：补充 `libdrawable` 视频输出插件，视频会正确嵌入桌面 WorkerW，不再生成独立的 `VLC (Direct3D11 output)` 窗口。
- **数据目录回退更稳**：`%LOCALAPPDATA%\DeskFlow` 不可写时，优先使用 `%LOCALAPPDATA%\DeskFlow-<用户SID>`，最后才使用 `%TEMP%\DeskFlow`。
- **释放失败可诊断**：完整异常会写入 `%TEMP%\DeskFlow-error.log` 或 `%LOCALAPPDATA%\DeskFlow-error.log`。
- 保留 v1.0.3 的视频封面、截图、页面排版、模式切换、默认静音等全部改进。

## 精简版范围

已测试以下容器与编码，播放和封面截图均正常：

- 容器：MKV、MP4、AVI、MOV、WMV、WEBM
- 编码：H.264、HEVC、AV1、VP8、VP9、MPEG-2

图片轮播、WebP 解码、任务栏透明、开机自启等功能不受影响。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `B8F0EF47DEE8C2AA70CB27DD41E12D1757E9941F71112F323935134E28F4E5A8` |
| `DeskFlow-1.0.4-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `877485AFCB923A8B5EDE86944B5FDF20260EDC88817A5AF3E7B254E6D5E1BA4B` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`，之后启动无需等待。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景。DeskFlow 会持续尝试完全透明样式；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 任务栏透明使用未公开 API，与部分第三方任务栏工具或自定义主题共存时可能失效。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果以后需要播放网络流、蓝光或字幕，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
