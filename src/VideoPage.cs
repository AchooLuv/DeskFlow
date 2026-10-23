using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class VideoPage : PageBase
    {
        private readonly AppSettings _settings;
        private readonly PreviewBox _preview;
        private readonly RoundedPanel _sideCard;
        private readonly RoundedPanel _fileCard;
        private readonly RoundedPanel _actionCard;
        private readonly Label _speedCaption;
        private readonly Label _speedValue;
        private readonly Label _volumeCaption;
        private readonly Label _volumeValue;
        private readonly Label _loopCaption;
        private readonly Label _formatHint;
        private readonly Label _fileTitle;
        private readonly Label _filePath;
        private readonly Label _statusTitle;
        private readonly Label _statusDetail;
        private readonly ModernSlider _speed;
        private readonly ModernSlider _volume;
        private readonly ToggleSwitch _loop;
        private readonly UiButton _choose;
        private readonly UiButton _apply;
        private readonly UiButton _pause;
        private readonly UiButton _restart;
        private bool _updating;
        private string _thumbPath;
        private string _thumbTriedPath;
        private Bitmap _thumbBitmap;
        private bool _thumbLoading;
        private int _thumbGeneration;

        public event Action Changed;
        public event Action TogglePauseRequested;
        public event Action RestartRequested;
        public event Action ApplyRequested;

        public VideoPage(AppSettings settings)
        {
            _settings = settings;
            HeaderTitle = "动态壁纸";
            HeaderSubtitle = "把喜欢的视频放到桌面，声音、倍速与循环都由你决定。";

            _preview = new PreviewBox
            {
                Badge = "未启用",
                BadgeColor = Theme.TextDim,
                Title = "动态壁纸",
                Subtitle = "选择一个视频文件开始播放",
                VideoMode = true,
                Glyph = "\uE714"
            };
            Controls.Add(_preview);

            _sideCard = Ui.Card(Theme.Background);
            Controls.Add(_sideCard);

            Label sideTitle = Ui.Label("播放设置", Theme.H3, Theme.Text, Theme.Card);
            sideTitle.SetBounds(16, 12, 200, 24);
            _sideCard.Controls.Add(sideTitle);

            _speedCaption = Ui.Label("播放倍率", Theme.Small, Theme.TextMuted, Theme.Card);
            _speedCaption.SetBounds(16, 44, 120, 20);
            _sideCard.Controls.Add(_speedCaption);

            _speedValue = Ui.Label("1.00×", Theme.SmallBold, Theme.AccentHover, Theme.Card);
            _speedValue.SetBounds(180, 44, 80, 20);
            _speedValue.TextAlign = ContentAlignment.MiddleRight;
            _sideCard.Controls.Add(_speedValue);

            _speed = new ModernSlider { Minimum = 25, Maximum = 200, Step = 5, Value = 100 };
            _speed.SetBounds(16, 66, 244, 28);
            _speed.ValueChanged += delegate
            {
                if (_updating) return;
                _settings.PlaybackRate = _speed.Value / 100.0;
                _speedValue.Text = _settings.PlaybackRate.ToString("0.00") + "×";
                RaiseChanged();
            };
            _sideCard.Controls.Add(_speed);

            _volumeCaption = Ui.Label("音量", Theme.Small, Theme.TextMuted, Theme.Card);
            _volumeCaption.SetBounds(16, 106, 120, 20);
            _sideCard.Controls.Add(_volumeCaption);

            _volumeValue = Ui.Label("0%", Theme.SmallBold, Theme.Text, Theme.Card);
            _volumeValue.SetBounds(180, 106, 80, 20);
            _volumeValue.TextAlign = ContentAlignment.MiddleRight;
            _sideCard.Controls.Add(_volumeValue);

            _volume = new ModernSlider { Minimum = 0, Maximum = 100, Step = 1, Value = 0 };
            _volume.SetBounds(16, 128, 244, 28);
            _volume.ValueChanged += delegate
            {
                if (_updating) return;
                _settings.Volume = _volume.Value;
                _volumeValue.Text = _settings.Volume + "%";
                RaiseChanged();
            };
            _sideCard.Controls.Add(_volume);

            _loopCaption = Ui.Label("循环播放", Theme.Body, Theme.Text, Theme.Card);
            _loopCaption.SetBounds(16, 172, 150, 24);
            _sideCard.Controls.Add(_loopCaption);

            _loop = new ToggleSwitch { Width = 40, Height = 22 };
            _loop.SetBounds(220, 173, 40, 22);
            _loop.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.Loop = _loop.Checked;
                RaiseChanged();
            };
            _sideCard.Controls.Add(_loop);
            _loopCaption.Cursor = Cursors.Hand;
            _loopCaption.Click += delegate { _loop.Checked = !_loop.Checked; };

            _formatHint = Ui.Label("MKV · MP4 · AVI · MOV · WMV · WEBM", Theme.Tiny2, Theme.TextDim, Theme.Card);
            _formatHint.SetBounds(16, 202, 244, 18);
            _sideCard.Controls.Add(_formatHint);

            _fileCard = Ui.Card(Theme.Background);
            Controls.Add(_fileCard);

            _fileTitle = Ui.Label("视频文件", Theme.SmallBold, Theme.Text, Theme.Card);
            _fileTitle.SetBounds(16, 10, 140, 20);
            _fileCard.Controls.Add(_fileTitle);

            _filePath = Ui.Label("尚未选择视频", Theme.Small, Theme.TextMuted, Theme.Card);
            _filePath.SetBounds(16, 34, 440, 22);
            _filePath.AutoEllipsis = true;
            _fileCard.Controls.Add(_filePath);

            _choose = new UiButton { Text = "选择视频", Style = UiButtonStyle.Primary, Width = 116, Height = 36 };
            _choose.Click += delegate { ChooseVideo(); };
            _fileCard.Controls.Add(_choose);

            _actionCard = Ui.Card(Theme.Background);
            Controls.Add(_actionCard);

            _statusTitle = Ui.Label("动态壁纸未启用", Theme.BodyBold, Theme.Text, Theme.Card);
            _statusTitle.SetBounds(18, 16, 420, 24);
            _actionCard.Controls.Add(_statusTitle);

            _statusDetail = Ui.Label("VLC 播放引擎支持常见视频格式，并优先使用硬件解码。", Theme.Small, Theme.TextMuted, Theme.Card);
            _statusDetail.SetBounds(18, 40, 440, 22);
            _statusDetail.AutoEllipsis = true;
            _actionCard.Controls.Add(_statusDetail);

            _apply = new UiButton { Text = "应用视频", Style = UiButtonStyle.Primary, Width = 104, Height = 38 };
            _apply.Click += delegate
            {
                Action handler = ApplyRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_apply);

            _pause = new UiButton { Text = "暂停播放", Style = UiButtonStyle.Soft, Width = 104, Height = 38 };
            _pause.Click += delegate
            {
                Action handler = TogglePauseRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_pause);

            _restart = new UiButton { Text = "重新开始", Style = UiButtonStyle.Secondary, Width = 104, Height = 38 };
            _restart.Click += delegate
            {
                Action handler = RestartRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_restart);

            ApplySettingsToControls();
        }

        private void RaiseChanged()
        {
            Action handler = Changed;
            if (handler != null) handler();
        }

        public void ApplySettingsToControls()
        {
            _updating = true;
            _speed.Value = (int)Math.Round(_settings.PlaybackRate * 100);
            _volume.Value = _settings.Volume;
            _loop.Checked = _settings.Loop;
            _updating = false;
            _speedValue.Text = _settings.PlaybackRate.ToString("0.00") + "×";
            _volumeValue.Text = _settings.Volume + "%";
        }

        private void ChooseVideo()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择动态壁纸视频";
                dialog.Filter = "视频文件|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm;*.flv;*.m4v;*.ts;*.mpg;*.mpeg|所有文件|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (File.Exists(_settings.VideoPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(_settings.VideoPath);
                    dialog.FileName = Path.GetFileName(_settings.VideoPath);
                }
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                _settings.VideoPath = dialog.FileName;
                _settings.Mode = "video";
                _filePath.Text = dialog.FileName;
                RaiseChanged();
            }
        }

        public void UpdateState(WallpaperController controller)
        {
            bool enabled = _settings.Mode == "video";
            bool hasFile = !string.IsNullOrEmpty(_settings.VideoPath) && File.Exists(_settings.VideoPath);
            bool active = enabled && controller.IsVideoActive;
            bool paused = controller.IsPaused;

            Bitmap preview = enabled ? controller.VideoCover : null;
            if (preview == null && hasFile) preview = GetThumbnail();
            _preview.VideoMode = true;
            _preview.Glyph = "\uE714";
            _preview.PreviewImage = preview;

            if (!enabled)
            {
                _preview.Badge = hasFile ? "已选择" : "未启用";
                _preview.BadgeColor = hasFile ? Theme.Accent : Theme.TextDim;
                _preview.Title = hasFile ? Path.GetFileName(_settings.VideoPath) : "动态壁纸";
                _preview.Subtitle = hasFile
                    ? "点击“应用视频”开始播放"
                    : "选择一个视频文件开始播放";
                _statusTitle.Text = hasFile ? "已选择视频文件" : "动态壁纸未启用";
                _statusDetail.Text = hasFile
                    ? "点击“应用视频”开始播放。"
                    : "VLC 播放引擎支持常见视频格式，并优先使用硬件解码。";
            }
            else if (!hasFile)
            {
                _preview.Badge = "文件不存在";
                _preview.BadgeColor = Theme.Warning;
                _preview.Title = "找不到视频文件";
                _preview.Subtitle = "请重新选择视频";
                _statusTitle.Text = "视频文件不可用";
                _statusDetail.Text = "文件可能已被移动或删除，请重新选择。";
            }
            else
            {
                _preview.Badge = active ? (paused ? "已暂停" : "动态壁纸") : "准备中";
                _preview.BadgeColor = active ? (paused ? Theme.Warning : Theme.Success) : Theme.TextDim;
                _preview.Title = Path.GetFileName(_settings.VideoPath);
                string detail = Ui.FormatSize(new FileInfo(_settings.VideoPath).Length);
                long length = controller.VideoLength;
                if (length > 0) detail += "  ·  " + Ui.FormatDuration(length);
                detail += _settings.Loop ? "  ·  循环播放" : "  ·  单次播放";
                _preview.Subtitle = detail;
                _statusTitle.Text = active ? (paused ? "动态壁纸已暂停" : "动态壁纸正在播放") : "正在准备视频";
                _statusDetail.Text = active
                    ? "倍率 " + _settings.PlaybackRate.ToString("0.00") + "×  ·  音量 " + _settings.Volume + "%  ·  "
                        + (_settings.Loop ? "循环" : "单次") + "  ·  硬件解码优先"
                    : "VLC 正在打开视频，请稍候。";
            }

            _filePath.Text = hasFile ? _settings.VideoPath : "尚未选择视频";
            _filePath.ForeColor = hasFile ? Theme.TextMuted : Theme.TextDim;
            _pause.Text = paused ? "继续播放" : "暂停播放";
            _pause.Enabled = active;
            _restart.Enabled = enabled && hasFile;
            _apply.Visible = !enabled && hasFile;
            _apply.Enabled = _apply.Visible;
            _choose.Text = hasFile ? "更换视频" : "选择视频";
            LayoutPage();
        }

        private Bitmap GetThumbnail()
        {
            string path = _settings.VideoPath;
            if (string.IsNullOrEmpty(path)) return null;
            if (string.Equals(_thumbPath, path, StringComparison.OrdinalIgnoreCase))
            {
                if (_thumbBitmap != null) return _thumbBitmap;
                if (_thumbLoading || string.Equals(_thumbTriedPath, path, StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            _preview.PreviewImage = null;
            if (_thumbBitmap != null)
            {
                try { _thumbBitmap.Dispose(); } catch { }
                _thumbBitmap = null;
            }
            _thumbPath = path;
            _thumbTriedPath = path;
            _thumbLoading = true;
            int generation = ++_thumbGeneration;
            Task.Run(delegate { return ThumbnailLoader.Load(path, 640, 360); })
                .ContinueWith(delegate(Task<Bitmap> task)
                {
                    Bitmap bitmap = task.Result;
                    if (IsDisposed || generation != _thumbGeneration)
                    {
                        if (bitmap != null) bitmap.Dispose();
                        return;
                    }
                    _thumbLoading = false;
                    if (bitmap != null)
                    {
                        _thumbBitmap = bitmap;
                        _preview.PreviewImage = bitmap;
                        _preview.Invalidate();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            return null;
        }

        protected override void LayoutPage()
        {
            int margin = ContentMargin;
            int inner = Math.Max(400, Width - margin * 2);
            int top = ContentTop;
            int previewWidth = (int)(inner * 0.60f);
            int sideWidth = inner - previewWidth - 16;
            const int previewHeight = 236;

            _preview.SetBounds(margin, top, previewWidth, previewHeight);
            _sideCard.SetBounds(margin + previewWidth + 16, top, sideWidth, previewHeight);
            _speed.Width = Math.Max(120, sideWidth - 32);
            _volume.Width = Math.Max(120, sideWidth - 32);
            _speedValue.Left = sideWidth - 96;
            _volumeValue.Left = sideWidth - 96;
            _loop.Location = new Point(sideWidth - 58, 173);
            _formatHint.Width = Math.Max(120, sideWidth - 32);

            int fileY = top + previewHeight + 16;
            _fileCard.SetBounds(margin, fileY, inner, 76);
            _choose.SetBounds(_fileCard.Width - 132, 20, 116, 36);
            _filePath.Width = Math.Max(100, _fileCard.Width - 160);

            int actionY = fileY + 92;
            int actionHeight = Math.Max(92, Height - actionY - 18);
            _actionCard.SetBounds(margin, actionY, inner, actionHeight);
            int actionCenter = _actionCard.Height / 2;
            _statusTitle.Top = actionCenter - 24;
            _statusDetail.Top = actionCenter + 1;
            _apply.SetBounds(_actionCard.Width - 340, actionCenter - 19, 104, 38);
            _pause.SetBounds(_actionCard.Width - 226, actionCenter - 19, 104, 38);
            _restart.SetBounds(_actionCard.Width - 114, actionCenter - 19, 104, 38);
            _statusTitle.Width = Math.Max(180, _actionCard.Width - 382);
            _statusDetail.Width = Math.Max(180, _actionCard.Width - 382);
        }
    }
}
