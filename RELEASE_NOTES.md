# DeskFlow v1.0.13

紧急修复 v1.0.12 中“重启 Explorer”导致的黑屏/桌面重启问题。

## 本版更新

- **移除强制重启 Explorer**：v1.0.12 在切换任务栏透明度时会杀掉所有 `explorer.exe` 再启动，可能导致任务栏消失、桌面黑屏、壁纸重新挂载，甚至短时间无响应。v1.0.13 完全删除了这段逻辑。
- **关闭透明时写入非透明值**：为避免 Explorer 缓存旧的 `TaskbarAcrylicOpacity=0`，关闭时把该项写为 `255`（最大不透明度/模糊），不再删除后让 Start 菜单重新读到 0。
- 保留任务栏原生透明度开启逻辑（`TaskbarAcrylicOpacity=0`），Start 菜单打开时不会持续闪烁。
- 保留 v1.0.7 的按钮修复、v1.0.6 的预览修复、精简 runtime、普通权限启动修复。
- 早期启动实验保持移除。

> 关闭透明后如果任务栏仍显示旧外观，重启一次 Explorer 或重新登录即可，但 DeskFlow 不会再自动重启你的桌面。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `27D9F59F4AE4D36FAF97287777455E15FB816338B330C875A73098A219D511A9` |
| `DeskFlow-1.0.13-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `8820AF6664F65F9A22DCBB85CC3190C1A85F3E2FBFFBC322DF71BCB17552EE4E` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`，之后启动无需等待。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- `TaskbarAcrylicOpacity` 依赖 Windows“透明效果”开启；如果系统设置关闭了透明效果，任务栏不会全透明。
- 关闭透明后可能需要重启 Explorer/重新登录才能完全回到系统默认外观；DeskFlow 不再自动重启 Explorer。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果需要这些功能，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
