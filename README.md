# DeskFlow

DeskFlow 是一个轻量的 Windows 动态壁纸引擎：把图片文件夹变成轮播壁纸，或把喜欢的视频放上桌面。它常驻托盘、占用克制，并在全屏程序、锁屏或电池模式下自动暂停。

![DeskFlow 界面](docs/screenshot.png)

## 功能

### 图片轮播

- 支持 WEBP、JPG/JPEG、PNG、BMP、GIF（静态首帧）。
- 可设置轮播间隔（3 秒 – 1 小时），支持随机顺序。
- 直接选择一个文件夹作为轮播源，文件夹内的图片会自动识别；支持刷新列表。
- 图片按“填满屏幕”方式缩放，保留原始比例并居中裁切。
- 支持 JPEG EXIF 方向自动纠正；WebP 在系统没有解码器时由内置 `dwebp` 解码。

### 动态壁纸

- 支持 MKV、MP4、AVI、MOV、WMV、WEBM、FLV、M4V、TS、MPG 等常见视频格式。
- 播放倍率 0.25× – 2.00×。
- 音量 0 – 100%，默认静音。
- 循环播放默认开启，可关闭。
- 使用 VLC 3.0.21 播放引擎，优先启用硬件解码；视频嵌入桌面 WorkerW，位于桌面图标之下。

### 桌面与系统

- **任务栏默认完全透明**，不使用毛玻璃 / 亚克力效果，并会在 Explorer 重建后自动恢复。
- 全屏游戏或全屏视频运行时自动暂停壁纸。
- 使用电池时可自动暂停，延长续航。
- 锁屏时暂停，解锁后恢复。
- 关闭窗口默认最小化到托盘；托盘菜单可暂停、下一张、切换任务栏透明或退出。
- 可选开机自动启动。
- 单实例运行；重复启动会唤醒已经运行的 DeskFlow。

## 系统要求

- **Windows 10（1809 及以上）/ Windows 11，64 位。**
- 不需要单独安装 VLC 或 .NET：程序使用 Windows 自带的 .NET Framework 4.8，首次启动会把内置的 VLC 播放组件释放到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21`。

### 为什么不支持 Windows 7 / 8.1

技术上可以勉强运行，但需要额外处理旧版 WorkerW、任务栏接口、.NET 运行时和 VLC 兼容性，而 Windows 7/8.1 已停止安全更新，实际收益很低。因此本项目只针对 Windows 10/11 64 位维护和测试。

## 使用

1. 从 [Releases](https://github.com/AchooLuv/DeskFlow/releases) 下载 `DeskFlow.exe`（单文件，内置播放引擎）。
2. 双击运行。首次启动会显示“正在准备播放引擎”，约数秒后进入主界面。
3. **图片轮播**：选择图片文件夹，设置间隔；壁纸会立即开始轮播。
4. **动态壁纸**：选择视频，调整倍率、音量或循环；壁纸会立即播放。
5. 已经配置过两种壁纸后，直接点击左侧“图片轮播”或“动态壁纸”即可切换，不需要重新选择文件。
6. 关闭窗口后程序仍在托盘运行。右键托盘图标可以暂停、切歌或退出。

设置保存在 `%APPDATA%\DeskFlow\settings.json`，日志在 `%LOCALAPPDATA%\DeskFlow\DeskFlow.log`。

> `DeskFlow.exe` 没有购买代码签名证书，Windows SmartScreen 可能提示“未知发布者”。请核对 Releases 中的 SHA-256 后运行。

## 性能设计

DeskFlow 刻意保持简单，避免为了“动态壁纸”付出不必要的开销：

- 视频由 VLC 硬件解码优先播放，不经过逐帧 GDI+ 绘制。
- 图片只在切换时解码一次，并缩放到屏幕尺寸后缓存；绘制时直接贴图。
- 轮播使用单个定时器，不轮询文件系统。
- 全屏检测每 2 秒执行一次轻量系统查询；空闲时不进行高频重绘。
- 锁屏、全屏程序、电池模式会暂停视频/轮播。
- 任务栏透明只在启动、系统事件和低频率维护时重新应用，不做高频调用。

## 构建

构建产物是单文件 `DeskFlow.exe`，VLC 运行库会被压缩后嵌入可执行文件。

需要：

- Windows 10/11 x64。
- .NET Framework 4.8（Windows 自带）。
- Roslyn C# 编译器（例如 NuGet 包 `Microsoft.Net.Compilers` 3.11，解压到 `.cache\roslyn`）。
- VLC 3.0.21 win64 便携包，解压到 `.cache\vlc\vlc-3.0.21`。
- 官方 libwebp 1.6.0 Windows x64 包中的 `dwebp.exe`，放到 `assets\tools\dwebp.exe`。

然后运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1
```

输出：

- `build\DeskFlow.exe`：单文件可执行程序。
- `build\DeskFlow-1.0.1-win64.zip`：含程序、README 与许可文件的便携包。

## 已知限制

- 当前版本只把壁纸铺在主显示器上；多显示器扩展桌面暂未支持。
- 图片轮播不递归扫描子文件夹。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景。DeskFlow 会持续尝试应用完全透明样式；如果系统接管了任务栏绘制，任务栏会保持系统默认外观。这是系统限制，不影响壁纸本身。
- 任务栏透明属于未公开 API 的兼容实现，在部分自定义主题或第三方任务栏工具共存时可能失效。

## 许可

DeskFlow 以 **GNU GPL v3** 发布。它动态加载 VLC 3.0.21（GPLv2+），并使用 libwebp 的 `dwebp`（BSD-3-Clause）。完整许可见 [LICENSE](LICENSE) 与 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)，程序“设置 → 开源许可”中也可查看。

源码：<https://github.com/AchooLuv/DeskFlow>
