# DeskFlow v1.0.5

桌面出现前启动、导航切换开关，以及完整的普通权限修复。

## 本版更新

- **更早启动**：开启“开机自动启动”后，DeskFlow 会创建一个“登录时”计划任务，不再只依赖资源管理器启动项。程序会在登录阶段启动，预加载 VLC，并在 WorkerW 桌面层出现前保持隐藏；桌面就绪后立即挂载，避免先闪出普通窗口或临时默认桌面。
  - 首次开启需要一次管理员授权（UAC）。
  - 如果拒绝授权，会自动回退到原来的 `HKCU\...\Run` 启动方式。
- **新增“点击菜单直接切换壁纸”开关**：默认关闭。关闭时，点击左侧“图片轮播 / 动态壁纸”只会打开对应页面，需要点击页面内的“应用为壁纸”按钮才会切换；开启后恢复原来的点击直接切换行为。
- **新增页面内“应用为壁纸”按钮**：图片轮播页与动态壁纸页都可以显式应用当前配置，避免误点导航就切换。
- **普通权限启动修复保留**：发布文件为 Medium 完整性，不再因为构建目录的 Low 标签导致双击无法写入 `%LOCALAPPDATA%`。
- **数据目录回退保留**：`%LOCALAPPDATA%\DeskFlow` 不可写时优先使用 `%LOCALAPPDATA%\DeskFlow-<用户SID>`，最后才使用 `%TEMP%\DeskFlow`。
- **错误可诊断**：播放引擎释放失败时，完整异常写入 `%TEMP%\DeskFlow-error.log` 或 `%LOCALAPPDATA%\DeskFlow-error.log`。

## 精简版范围

已测试容器 MKV、MP4、AVI、MOV、WMV、WEBM，以及 H.264、HEVC、AV1、VP8、VP9、MPEG-2 编码。播放和封面截图正常。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 19.5 MB | `8EFF8B830F221D8E84A91213297FBCAF344F00F6CB18D7674B0B1283D68DBDC2` |
| `DeskFlow-1.0.5-slim-win64.zip` | 含程序、README、三张截图与许可，约 19.4 MB | `566FB6EC0DB5E5E162F0F956B0F2FC02C4390E9C929DB8B312915978DA1934FF` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 40 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime\vlc-3.0.21-slim2`，之后启动无需等待。
- 不需要单独安装 VLC 或 .NET；使用 Windows 自带的 .NET Framework 4.8。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- 登录计划任务首次创建/删除需要管理员授权；拒绝后会回退到普通启动项。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景。DeskFlow 会持续尝试完全透明样式；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 精简版不包含 VLC GUI、网络访问、蓝光和字幕相关插件；如果需要这些功能，请使用完整版构建 `build.ps1`。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
