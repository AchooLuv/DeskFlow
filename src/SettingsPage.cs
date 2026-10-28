using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class SettingsPage : PageBase
    {
        private readonly AppSettings _settings;
        private readonly RoundedPanel _appearanceCard;
        private readonly RoundedPanel _performanceCard;
        private readonly RoundedPanel _aboutCard;
        private readonly SettingRow _taskbarRow;
        private readonly SettingRow _trayRow;
        private readonly SettingRow _autostartRow;
        private readonly SettingRow _switchModeRow;
        private readonly SettingRow _fullscreenRow;
        private readonly SettingRow _batteryRow;
        private readonly Label _versionLabel;
        private readonly Label _platformLabel;
        private readonly Label _engineLabel;
        private readonly LinkLabel _link;
        private readonly Label _note;
        private readonly Label _perfHint;
        private readonly UiButton _openLogs;
        private readonly UiButton _license;
        private readonly UiButton _reset;
        private bool _updating;

        public event Action Changed;
        public event Action ResetRequested;

        public SettingsPage(AppSettings settings)
        {
            _settings = settings;
            HeaderTitle = "设置";
            HeaderSubtitle = "调整运行方式。所有改动都会立即生效。";

            _appearanceCard = Ui.Card(Theme.Background);
            Controls.Add(_appearanceCard);
            Label appearanceTitle = Ui.Label("外观与行为", Theme.H3, Theme.Text, Theme.Card);
            appearanceTitle.SetBounds(16, 12, 220, 24);
            _appearanceCard.Controls.Add(appearanceTitle);

            _taskbarRow = new SettingRow
            {
                Title = "任务栏完全透明",
                Description = "去除毛玻璃，让任务栏与壁纸融合"
            };
            _taskbarRow.Checked = settings.TaskbarTransparent;
            _taskbarRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.TaskbarTransparent = _taskbarRow.Checked;
                RaiseChanged();
            };
            _appearanceCard.Controls.Add(_taskbarRow);

            _trayRow = new SettingRow
            {
                Title = "关闭窗口时最小化到托盘",
                Description = "壁纸继续运行，双击托盘图标恢复"
            };
            _trayRow.Checked = settings.MinimizeToTray;
            _trayRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.MinimizeToTray = _trayRow.Checked;
                RaiseChanged();
            };
            _appearanceCard.Controls.Add(_trayRow);

            _autostartRow = new SettingRow
            {
                Title = "开机自动启动",
                Description = "登录 Windows 后自动恢复壁纸"
            };
            _autostartRow.Checked = settings.AutoStart;
            _autostartRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.AutoStart = _autostartRow.Checked;
                RaiseChanged();
            };
            _appearanceCard.Controls.Add(_autostartRow);

            _switchModeRow = new SettingRow
            {
                Title = "点击菜单直接切换壁纸",
                Description = "关闭后点击只打开页面，由“应用为壁纸”切换"
            };
            _switchModeRow.Checked = settings.SwitchModeOnNavClick;
            _switchModeRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.SwitchModeOnNavClick = _switchModeRow.Checked;
                RaiseChanged();
            };
            _appearanceCard.Controls.Add(_switchModeRow);

            _performanceCard = Ui.Card(Theme.Background);
            Controls.Add(_performanceCard);
            Label performanceTitle = Ui.Label("性能", Theme.H3, Theme.Text, Theme.Card);
            performanceTitle.SetBounds(16, 12, 220, 24);
            _performanceCard.Controls.Add(performanceTitle);

            _fullscreenRow = new SettingRow
            {
                Title = "全屏程序运行时暂停",
                Description = "游戏或全屏视频时自动暂停"
            };
            _fullscreenRow.Checked = settings.PauseFullscreen;
            _fullscreenRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.PauseFullscreen = _fullscreenRow.Checked;
                RaiseChanged();
            };
            _performanceCard.Controls.Add(_fullscreenRow);

            _batteryRow = new SettingRow
            {
                Title = "使用电池时暂停",
                Description = "拔掉电源后暂停，延长续航"
            };
            _batteryRow.Checked = settings.PauseOnBattery;
            _batteryRow.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.PauseOnBattery = _batteryRow.Checked;
                RaiseChanged();
            };
            _performanceCard.Controls.Add(_batteryRow);

            _perfHint = Ui.Label("全屏检测每 2 秒检查一次，空闲时几乎不占用 CPU。", Theme.Tiny, Theme.TextDim, Theme.Card);
            _perfHint.SetBounds(16, 170, 300, 34);
            _performanceCard.Controls.Add(_perfHint);

            _aboutCard = Ui.Card(Theme.Background);
            Controls.Add(_aboutCard);
            Label aboutTitle = Ui.Label("关于 DeskFlow", Theme.H3, Theme.Text, Theme.Card);
            aboutTitle.SetBounds(18, 14, 220, 24);
            _aboutCard.Controls.Add(aboutTitle);

            _versionLabel = Ui.Label("DeskFlow 1.0.18", Theme.BodyBold, Theme.Text, Theme.Card);
            _versionLabel.SetBounds(18, 46, 240, 22);
            _aboutCard.Controls.Add(_versionLabel);

            _platformLabel = Ui.Label("Windows 10 / 11 · 64 位 · 仅主显示器", Theme.Small, Theme.TextMuted, Theme.Card);
            _platformLabel.SetBounds(18, 68, 360, 22);
            _aboutCard.Controls.Add(_platformLabel);

            _engineLabel = Ui.Label("播放引擎 VLC 3.0.21 · WebP libwebp 1.6.0", Theme.Small, Theme.TextMuted, Theme.Card);
            _engineLabel.SetBounds(18, 90, 400, 22);
            _aboutCard.Controls.Add(_engineLabel);

            _link = new LinkLabel
            {
                Text = "github.com/AchooLuv/DeskFlow",
                Font = Theme.Small,
                LinkColor = Theme.AccentHover,
                ActiveLinkColor = Color.White,
                VisitedLinkColor = Theme.AccentHover,
                BackColor = Theme.Card,
                AutoSize = false
            };
            _link.SetBounds(18, 112, 300, 22);
            _link.LinkClicked += delegate
            {
                try { Process.Start(new ProcessStartInfo("https://github.com/AchooLuv/DeskFlow") { UseShellExecute = true }); }
                catch { }
            };
            _aboutCard.Controls.Add(_link);

            _note = Ui.Label("DeskFlow 是开源软件。第三方组件版权与许可见程序目录或仓库。", Theme.Tiny, Theme.TextDim, Theme.Card);
            _note.SetBounds(18, 136, 500, 22);
            _aboutCard.Controls.Add(_note);

            _openLogs = new UiButton { Text = "打开日志", Style = UiButtonStyle.Ghost, Width = 96, Height = 36 };
            _openLogs.Click += delegate
            {
                try
                {
                    string dir = Path.GetDirectoryName(Log.LogPath);
                    Directory.CreateDirectory(dir);
                    Process.Start("explorer.exe", "\"" + dir + "\"");
                }
                catch { }
            };
            _aboutCard.Controls.Add(_openLogs);

            _license = new UiButton { Text = "开源许可", Style = UiButtonStyle.Ghost, Width = 96, Height = 36 };
            _license.Click += delegate
            {
                using (LicenseForm form = new LicenseForm())
                    form.ShowDialog(FindForm());
            };
            _aboutCard.Controls.Add(_license);

            _reset = new UiButton { Text = "恢复默认设置", Style = UiButtonStyle.Danger, Width = 128, Height = 36 };
            _reset.Click += delegate
            {
                if (MessageBox.Show(FindForm(), "恢复默认设置会清除当前壁纸选择与任务栏设置，是否继续？", "DeskFlow",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                AppSettings defaults = new AppSettings();
                _settings.Mode = defaults.Mode;
                _settings.ImageFolder = defaults.ImageFolder;
                _settings.IntervalSeconds = defaults.IntervalSeconds;
                _settings.Shuffle = defaults.Shuffle;
                _settings.VideoPath = defaults.VideoPath;
                _settings.PlaybackRate = defaults.PlaybackRate;
                _settings.Volume = defaults.Volume;
                _settings.Loop = defaults.Loop;
                _settings.TaskbarTransparent = defaults.TaskbarTransparent;
                _settings.MinimizeToTray = defaults.MinimizeToTray;
                _settings.AutoStart = defaults.AutoStart;
                _settings.SwitchModeOnNavClick = defaults.SwitchModeOnNavClick;
                _settings.PauseFullscreen = defaults.PauseFullscreen;
                _settings.PauseOnBattery = defaults.PauseOnBattery;
                ApplySettingsToControls();
                Action handler = ResetRequested;
                if (handler != null) handler();
            };
            _aboutCard.Controls.Add(_reset);
        }

        private void RaiseChanged()
        {
            Action handler = Changed;
            if (handler != null) handler();
        }

        public void ApplySettingsToControls()
        {
            _updating = true;
            _taskbarRow.Checked = _settings.TaskbarTransparent;
            _trayRow.Checked = _settings.MinimizeToTray;
            _autostartRow.Checked = _settings.AutoStart;
            _switchModeRow.Checked = _settings.SwitchModeOnNavClick;
            _fullscreenRow.Checked = _settings.PauseFullscreen;
            _batteryRow.Checked = _settings.PauseOnBattery;
            _updating = false;
        }

        protected override void LayoutPage()
        {
            int margin = ContentMargin;
            int inner = Math.Max(500, Width - margin * 2);
            int top = ContentTop;
            int gap = 16;
            int column = (inner - gap) / 2;
            const int cardHeight = 292;

            _appearanceCard.SetBounds(margin, top, column, cardHeight);
            _performanceCard.SetBounds(margin + column + gap, top, column, cardHeight);

            int rowWidth = column - 32;
            _taskbarRow.SetBounds(16, 44, rowWidth, 58);
            _trayRow.SetBounds(16, 106, rowWidth, 58);
            _autostartRow.SetBounds(16, 168, rowWidth, 58);
            _switchModeRow.SetBounds(16, 230, rowWidth, 58);

            _fullscreenRow.SetBounds(16, 44, rowWidth, 58);
            _batteryRow.SetBounds(16, 106, rowWidth, 58);
            _perfHint.Width = rowWidth;

            int aboutY = top + cardHeight + gap;
            int aboutHeight = Math.Max(150, Height - aboutY - 18);
            _aboutCard.SetBounds(margin, aboutY, inner, aboutHeight);
            _openLogs.SetBounds(_aboutCard.Width - 350, _aboutCard.Height - 52, 96, 36);
            _license.SetBounds(_aboutCard.Width - 244, _aboutCard.Height - 52, 96, 36);
            _reset.SetBounds(_aboutCard.Width - 136, _aboutCard.Height - 52, 118, 36);
            _note.Width = Math.Max(220, _aboutCard.Width - 280);
            _link.Width = Math.Max(200, _aboutCard.Width - 300);
        }
    }
}
