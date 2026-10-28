# DeskFlow v1.0.16

继续完成 WebP 缓存、视频封面后台化和发布者信息处理；多显示器和 Windows 11 兼容性按你的要求暂不展开。

## 本版更新

- **WebP 解码缓存**：WebP 解码结果缓存到 `%LOCALAPPDATA%\DeskFlow\cache\webp`，同一文件再次加载不再反复启动 `dwebp.exe`；缓存最多保留 12 个文件。
- **WebP GDI+ 失败记忆**：系统不支持 WebP 时只记录一次失败，后续直接走缓存/`dwebp`，不再每张图重复异常和日志。
- **视频封面后台处理**：截图和 JPEG 解码移到后台线程，完成后投递回 UI；VLC 播放器句柄增加锁，避免与 Stop/Dispose 并发释放。
- **发布者信息**：程序集 Company 已为 `AchooLuv`，文件属性显示发布者为 AchooLuv。Windows SmartScreen 的“已验证发布者”仍需要代码签名证书，当前没有证书。
- **保留 v1.0.15**：VLC 事件代次校验、壁纸挂载退避、显示设置变化处理、视频滑块轻量路径、3 秒全屏检测。
- **保留 v1.0.14**：VLC 回调线程安全、实例复用、异步图片预览、UI 刷新合并、资源释放和死代码清理。

## 仍未处理

- 多显示器扩展桌面。
- Windows 11 22H2+ 的任务栏绘制兼容性。
- 关闭任务栏透明后 Start 菜单可能重新应用旧透明外观（Explorer 缓存；不再自动重启 Explorer）。
- 代码签名证书。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `81976A771476EFA703C165B63D6C78ECEABDF2FB875BB78D6109E43BF75D4F3A` |
| `DeskFlow-1.0.16-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `C131F783B9FC3600931C2D0EBA6E906E5C87FAE52C109C1DD4B515FDDCD0E611` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
