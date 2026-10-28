# DeskFlow v1.0.15

继续处理剩余性能与稳定性问题；按你的要求，多显示器和 Windows 11 兼容性暂不展开。

## 本版更新

- **VLC 事件代次校验**：快速切换视频时，旧播放器投递到 UI 线程的延迟事件不会再作用于新播放器。
- **壁纸挂载退避**：未挂载时最多重试 6 次，不再永久每 30 秒向 Progman 发送探测消息；Explorer 重建时会重置计数。
- **显示设置变化处理**：分辨率/DPI 变化后更新壁纸窗口并重新加载当前图片或视频。
- **视频滑块走轻量路径**：倍速、音量、循环调整不再触发自动启动注册表、托盘重建和完整设置刷新。
- **全屏检测从 2 秒调整为 3 秒**，减少空闲唤醒。
- 保留 v1.0.14 的 VLC 回调线程安全、实例复用、异步图片预览、UI 刷新合并、资源释放和死代码清理。

## 仍未处理

- 多显示器扩展桌面。
- Windows 11 22H2+ 的任务栏绘制兼容性。
- 关闭任务栏透明后 Start 菜单可能重新应用旧透明外观（Windows shell 缓存；不再自动重启 Explorer）。
- 代码签名。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `AB080715F9264DE7A802D6C4D524AE6C59117050AA47A50DE67E5718DA9DB4E7` |
| `DeskFlow-1.0.15-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `5193B5CA5326F1AC43876A7C390E29C2BA71F3DC7807EFA833578BAAC92F45D3` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
