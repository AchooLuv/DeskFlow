# DeskFlow v1.0.18

完成文件夹扫描后台化、异步日志和发布者信息收尾；多显示器和 Windows 11 兼容性按你的要求暂不展开。

## 本版更新

- **文件夹扫描后台化**：图片文件夹枚举和排序移到线程池，切换文件夹不再阻塞 UI；扫描完成后自动刷新计数和预览。
- **日志异步写入**：日志改为后台单线程队列写入，不再在 UI 线程或 VLC 回调线程同步打开/追加文件。
- **WebP 缓存**：同一 WebP 文件复用 `%LOCALAPPDATA%\DeskFlow\cache\webp` 缓存，最多 12 个文件；GDI+ 失败只记录一次。
- **视频封面后台化**：截图和 JPEG 解码在后台完成，VLC 播放器句柄加锁，避免与 Stop/Dispose 并发。
- **文件夹扫描**、**异步预览**、**视频封面**、**日志**均不再占用 UI 线程。
- **发布者元数据**：Company / LegalCopyright 为 `AchooLuv`。
- 保留 VLC 事件代次校验、实例复用、壁纸挂载退避、显示设置处理、视频滑块轻量路径、3 秒全屏检测和资源释放。

## 仍未处理

- 多显示器扩展桌面。
- Windows 11 22H2+ 的任务栏绘制兼容性。
- 关闭任务栏透明后 Start 菜单可能重新应用旧透明外观（Explorer 缓存；不再自动重启 Explorer）。
- 代码签名证书。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `781478E65DC14F336A7E32FB2411FD931962A3AF0C9697A08245E7395AC34086` |
| `DeskFlow-1.0.18-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `35030D2531DEF301262121EEE4EB46698E92C761EAD323F9B52776439E27588F` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
