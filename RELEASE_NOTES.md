# DeskFlow v1.0.17

继续把剩余代码项处理完；多显示器和 Windows 11 兼容性按你的要求暂不展开。

## 本版更新

- **文件夹扫描后台化**：图片文件夹的枚举和排序移出 UI 线程；切换文件夹不再阻塞界面。
- **WebP 缓存保留**：同一 WebP 文件再次加载直接使用本地 BMP 缓存，不再反复启动 `dwebp.exe`；缓存最多 12 个文件。
- **视频封面后台化**：VLC 截图和 JPEG 解码在后台完成，播放器句柄加锁，避免与 Stop/Dispose 并发。
- **VLC 事件代次校验**：快速切换视频时，旧事件不会作用到新 player。
- **壁纸挂载退避**：最多重试 6 次，不再永久每 30 秒探测。
- **显示设置变化处理**：分辨率/DPI 变化后更新窗口并重新加载当前壁纸。
- **视频滑块轻量路径**：倍速、音量、循环调整不再触发完整设置/托盘/开机启动流程。
- **发布者元数据**：Company / LegalCopyright 为 `AchooLuv`。
- 保留 v1.0.14 的 VLC 回调线程安全、实例复用、异步预览、UI 刷新合并、资源释放和死代码清理。

## 仍未处理

- 多显示器扩展桌面。
- Windows 11 22H2+ 的任务栏绘制兼容性。
- 关闭任务栏透明后 Start 菜单可能重新应用旧透明外观（Explorer 缓存；不再自动重启 Explorer）。
- 同步日志写入。
- 代码签名证书。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `A6C15BA8418E15CC3CA8EBF5D9C9FF8E89EB5C8CA98552F3AD0CB310E2F0C211` |
| `DeskFlow-1.0.17-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `2BA555BD27907D8F4B190890C747433436030552D5E56B0F8EDC2129D54985F0` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
