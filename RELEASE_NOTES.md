# DeskFlow v1.0.14

代码检查与性能清理版本，不改变界面和任务栏设置行为。

## 本版更新

- **VLC 回调线程安全**：libVLC 事件回调不再直接调用 libVLC API，也不再直接操作播放器句柄；事件统一投递回 UI 线程，避免回调重入和播放器释放后的悬空调用。
- **复用 VLC 实例**：切换视频/图片时只停止媒体、释放 player/media，不再每次 `libvlc_new`/`libvlc_release`，减少切换卡顿和模块引用增长。
- **图片预览异步加载**：图片页的 640×360 预览改为后台解码，不再在 UI 线程同步处理大图。
- **合并 UI 刷新**：播放状态、预览和设置变化触发的多次刷新合并为一次，减少重复布局和绘制。
- **自动启动只在设置变化时写注册表**：拖动视频倍速/音量时不再反复打开并写入 `HKCU\...\Run`。
- **壁纸维护间隔 4 秒 → 10 秒**，减少空闲唤醒。
- **清理死代码**：删除无调用的任务栏轮询、侧栏计时器、VLC/窗口/控制器属性、未使用字体与 P/Invoke。
- **资源释放**：页面预览位图、Toast 计时器、LicenseForm 字体、缩略图 Shell COM 对象现在会正确释放。
- **省略号测量**：`Theme.Ellipsis` 从逐字符测量改为二分查找，减少长文件名绘制开销。
- 任务栏透明仍保持 v1.0.13 的安全实现，不会自动重启 Explorer。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `E6E31F40751A6AD3FFE864B32CCD7A314CD300FA5FF7566BFF55F9BF64F5DA46` |
| `DeskFlow-1.0.14-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `8CBC82A2F846564FF4D91290AE7F5CEB2B3E0000F041C66554ED801CAB9FD8D4` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- 关闭任务栏完全透明后，开始菜单仍可能重新应用旧的透明外观；这是 Windows shell 缓存行为，当前版本不自动重启 Explorer。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果需要这些功能，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
