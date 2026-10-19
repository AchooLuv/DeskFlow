# DeskFlow v1.0.2

界面细节与仓库展示优化，包含 v1.0.1 的全部启动、模式切换和默认静音修复。

## 本版改进

- **页面标题不再被副标题截断**：增大标题绘制区域并下移副标题、内容区，三个页面（图片轮播 / 动态壁纸 / 设置）的标题都能完整显示。
- **动态壁纸格式说明不再被裁切**：循环播放下方的格式支持改为单行小字号，完整展示 `MKV · MP4 · AVI · MOV · WMV · WEBM`。
- **README / GitHub SEO 优化**：重写仓库简介、功能说明、常见问题与英文摘要，补充 GitHub Topics 和项目主页链接，便于搜索“Windows 动态壁纸、视频壁纸、Live Wallpaper、Wallpaper Engine 替代品”等关键词。
- **更换 README 截图**：更新为 v1.0.2 的图片轮播和动态壁纸界面截图。
- 包含 v1.0.1 修复：
  - 非管理员启动更稳健（单实例锁权限、VLC 加载重试、数据目录回退）。
  - 点击左侧“图片轮播 / 动态壁纸”即可切换，不需要重新选择文件。
  - 动态壁纸默认音量为 0。

## 主要功能

- 图片轮播：WEBP、JPG/JPEG、PNG、BMP、GIF（静态首帧）；文件夹作为轮播源；3 秒–1 小时间隔；随机顺序。
- 动态壁纸：MKV、MP4、AVI、MOV、WMV、WEBM 等；0.25×–2.00×；音量 0–100%（默认静音）；默认循环。
- 任务栏默认完全透明（无毛玻璃），全屏/锁屏/电池自动暂停，托盘控制与开机自启。

## 下载

| 文件 | 说明 | SHA-256 |
| --- | --- | --- |
| `DeskFlow.exe` | 单文件便携版，约 59.8 MB | `7A690C552BEC49C1C3D38EAC29342F9101ED78F63AA89CC52A4BEB4C4F02EB5A` |
| `DeskFlow-1.0.2-win64.zip` | 含程序、README、双截图与许可 | `EAA0AA99DF5A04E8BC17B73CDF70C8C6E470B2CBD3DE386ABB76C4D0BD130A31` |

## 系统要求

- Windows 10（1809 及以上）/ Windows 11，64 位。
- 首次启动会释放约 136 MB 播放组件到 `%LOCALAPPDATA%\DeskFlow\runtime`；目录不可写时使用 `%TEMP%\DeskFlow`。
- 不支持 Windows 7/8.1。

## 已知限制

- 当前只把壁纸铺在主显示器；多显示器扩展桌面暂未支持。
- Windows 11 22H2（22621.1343）之后，微软限制了第三方程序修改任务栏背景。DeskFlow 会持续尝试完全透明样式；若系统接管绘制，任务栏会保持系统默认外观。
- 未购买代码签名证书，SmartScreen 可能提示“未知发布者”。请核对上面的 SHA-256。
- 任务栏透明使用未公开 API，与部分第三方任务栏工具或自定义主题共存时可能失效。

## 许可

GPL v3。VLC 3.0.21（GPLv2+）与 libwebp `dwebp`（BSD-3-Clause）的许可与来源见 `THIRD-PARTY-NOTICES.txt`，也可在程序“设置 → 开源许可”中查看。

源码：https://github.com/AchooLuv/DeskFlow
