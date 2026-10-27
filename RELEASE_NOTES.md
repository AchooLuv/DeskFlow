# DeskFlow v1.0.12

修复关闭任务栏完全透明后，打开开始菜单又变透明的问题。

## 本版更新

- **关闭透明时重启 Explorer**：`TaskbarAcrylicOpacity` 由 Explorer 在启动时读取；只删除注册表值不会清掉正在运行的 Explorer 缓存，所以关闭后 Start 仍可能使用旧的 0 值。v1.0.12 在切换该设置时重启 Explorer，让新值立即生效。
- **避免把 DeskFlow 自己的 0 当成用户原值**：之前如果注册表里已经是 0，关闭时会被当作“原值”恢复，导致继续透明。本版将 0 视为 DeskFlow 自身状态，关闭时直接删除/恢复默认。
- **保留原生透明度实现**：开启时写入 `TaskbarAcrylicOpacity=0`，由 Explorer 直接使用，不再定时强制覆盖，因此 Start 打开时不会持续闪烁。
- 保留 v1.0.7 的按钮修复、v1.0.6 的预览修复、精简 runtime、普通权限启动修复。
- 早期启动实验保持移除。

> 切换“任务栏完全透明”时任务栏会短暂重建（Explorer 重启）；这是让原生设置真正生效所必需的。之后 Start 菜单不会改变任务栏透明度。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `57E79F359FC8731CF23808289192217618DE0BBDB107BC02287C6831E06AAC47` |
| `DeskFlow-1.0.12-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `CEC7B9F7E15A33A7362EAA98B3909402E4DB7CE247EAA59606FB95A6E1C16084` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`，之后启动无需等待。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- `TaskbarAcrylicOpacity` 依赖 Windows“透明效果”开启；如果系统设置关闭了透明效果，任务栏不会全透明。
- 切换任务栏透明度会重启 Explorer，可能关闭已打开的“文件资源管理器”窗口。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果需要这些功能，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
