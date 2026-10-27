# DeskFlow v1.0.11

改用 Windows 原生任务栏透明度设置，避免开始菜单反复重置导致的闪烁。

## 本版更新

- **改用原生 `TaskbarAcrylicOpacity`**：Windows 10 支持在
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`
  下设置 `TaskbarAcrylicOpacity = 0`，由 Explorer 直接使用，不再依赖每 0.8 秒调用
  `SetWindowCompositionAttribute` 强制覆盖。
- **移除任务栏定时重设**：不会再因为 Start 菜单和 DeskFlow 互相覆盖而闪烁。
- 开启“任务栏完全透明”时写入原生透明度值；关闭或退出 DeskFlow 时恢复原值/删除该项。
- 保留 `SetWindowCompositionAttribute` 作为即时生效和 Explorer 重建时的兼容处理。
- 保留 v1.0.7 的按钮修复、v1.0.6 的预览修复、精简 runtime、普通权限启动修复。
- 早期启动实验保持移除。

> 如果首次开启后任务栏没有立即变化，重启一次 Explorer 或重新登录即可让原生设置生效；之后 Start 菜单不会反复改变它。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `331ABF42CAFD73FAF981D97E3097AC8D1AC31A5CB3A8C86023DE0B1757F215AB` |
| `DeskFlow-1.0.11-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `0A1CF200567220D13ACBA2C2755E678E7A8CD86AA3FDCCF3FE96783E55FC1406` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`，之后启动无需等待。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- `TaskbarAcrylicOpacity` 依赖 Windows“透明效果”开启；如果系统设置关闭了透明效果，任务栏不会全透明。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果需要这些功能，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
