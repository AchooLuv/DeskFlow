using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class SlideshowPage : PageBase
    {
        private readonly AppSettings _settings;
        private readonly PreviewBox _preview;
        private readonly RoundedPanel _sideCard;
        private readonly RoundedPanel _folderCard;
        private readonly RoundedPanel _actionCard;
        private readonly Label _countValue;
        private readonly Label _countCaption;
        private readonly Label _intervalCaption;
        private readonly Label _intervalUnit;
        private readonly Label _shuffleCaption;
        private readonly Label _folderTitle;
        private readonly Label _folderPath;
        private readonly Label _statusTitle;
        private readonly Label _statusDetail;
        private readonly NumberField _interval;
        private readonly ToggleSwitch _shuffle;
        private readonly Chip[] _chips;
        private readonly UiButton _choose;
        private readonly UiButton _refresh;
        private readonly UiButton _apply;
        private readonly UiButton _pause;
        private readonly UiButton _next;
        private bool _updating;
        private string _scanFolder;
        private DateTime _scanStamp;
        private List<string> _scanFiles = new List<string>();
        private string _previewPath;
        private Bitmap _previewBitmap;

        public event Action Changed;
        public event Action TogglePauseRequested;
        public event Action NextRequested;
        public event Action RefreshRequested;
        public event Action ApplyRequested;

        public SlideshowPage(AppSettings settings)
        {
            _settings = settings;
            HeaderTitle = "图片轮播";
            HeaderSubtitle = "选择一组喜欢的图片，让桌面按节奏换换心情。";

            _preview = new PreviewBox
            {
                Badge = "未启用",
                BadgeColor = Theme.TextDim,
                Title = "图片轮播",
                Subtitle = "选择文件夹后开始轮播",
                Glyph = "\uE91B"
            };
            Controls.Add(_preview);

            _sideCard = Ui.Card(Theme.Background);
            Controls.Add(_sideCard);

            Label sideTitle = Ui.Label("轮播设置", Theme.H3, Theme.Text, Theme.Card);
            sideTitle.SetBounds(16, 12, 200, 24);
            _sideCard.Controls.Add(sideTitle);

            _countCaption = Ui.Label("图片数量", Theme.Small, Theme.TextMuted, Theme.Card);
            _countCaption.SetBounds(16, 46, 120, 20);
            _sideCard.Controls.Add(_countCaption);

            _countValue = Ui.Label("0 张", Theme.H3, Theme.Text, Theme.Card);
            _countValue.SetBounds(16, 64, 200, 24);
            _sideCard.Controls.Add(_countValue);

            _intervalCaption = Ui.Label("切换间隔", Theme.Small, Theme.TextMuted, Theme.Card);
            _intervalCaption.SetBounds(16, 94, 120, 20);
            _sideCard.Controls.Add(_intervalCaption);

            _interval = new NumberField
            {
                Minimum = 3,
                Maximum = 3600,
                Step = 5,
                Value = Math.Max(3, settings.IntervalSeconds)
            };
            _interval.SetBounds(16, 114, 132, 38);
            _interval.ValueChanged += delegate
            {
                if (_updating) return;
                _settings.IntervalSeconds = _interval.Value;
                UpdateChipSelection();
                RaiseChanged();
            };
            _sideCard.Controls.Add(_interval);

            _intervalUnit = Ui.Label("秒", Theme.Body, Theme.TextMuted, Theme.Card);
            _intervalUnit.SetBounds(156, 114, 30, 38);
            _sideCard.Controls.Add(_intervalUnit);

            _chips = new Chip[4];
            string[] chipText = { "10 秒", "30 秒", "1 分钟", "5 分钟" };
            int[] chipValue = { 10, 30, 60, 300 };
            int[] chipWidth = { 52, 52, 58, 58 };
            int chipX = 16;
            for (int i = 0; i < _chips.Length; i++)
            {
                Chip chip = new Chip { Text = chipText[i], Width = chipWidth[i] };
                int value = chipValue[i];
                chip.Click += delegate
                {
                    _interval.Value = value;
                    _settings.IntervalSeconds = value;
                    UpdateChipSelection();
                    RaiseChanged();
                };
                chip.Location = new Point(chipX, 162);
                chipX += chip.Width + 6;
                _chips[i] = chip;
                _sideCard.Controls.Add(chip);
            }

            _shuffleCaption = Ui.Label("随机顺序", Theme.Small, Theme.Text, Theme.Card);
            _shuffleCaption.SetBounds(16, 200, 150, 24);
            _sideCard.Controls.Add(_shuffleCaption);

            _shuffle = new ToggleSwitch { Width = 40, Height = 22 };
            _shuffle.SetBounds(220, 201, 40, 22);
            _shuffle.CheckedChanged += delegate
            {
                if (_updating) return;
                _settings.Shuffle = _shuffle.Checked;
                RaiseChanged();
            };
            _sideCard.Controls.Add(_shuffle);
            _shuffleCaption.Cursor = Cursors.Hand;
            _shuffleCaption.Click += delegate { _shuffle.Checked = !_shuffle.Checked; };

            _folderCard = Ui.Card(Theme.Background);
            Controls.Add(_folderCard);

            _folderTitle = Ui.Label("图片文件夹", Theme.SmallBold, Theme.Text, Theme.Card);
            _folderTitle.SetBounds(16, 10, 140, 20);
            _folderCard.Controls.Add(_folderTitle);

            _folderPath = Ui.Label("尚未选择文件夹", Theme.Small, Theme.TextMuted, Theme.Card);
            _folderPath.SetBounds(16, 34, 440, 22);
            _folderPath.AutoEllipsis = true;
            _folderCard.Controls.Add(_folderPath);

            _choose = new UiButton { Text = "选择文件夹", Style = UiButtonStyle.Primary, Width = 116, Height = 36 };
            _choose.Click += delegate { ChooseFolder(); };
            _folderCard.Controls.Add(_choose);

            _refresh = new UiButton { Text = "刷新", Style = UiButtonStyle.Ghost, Width = 72, Height = 36 };
            _refresh.Click += delegate
            {
                Action handler = RefreshRequested;
                if (handler != null) handler();
            };
            _folderCard.Controls.Add(_refresh);

            _actionCard = Ui.Card(Theme.Background);
            Controls.Add(_actionCard);

            _statusTitle = Ui.Label("尚未启用图片轮播", Theme.BodyBold, Theme.Text, Theme.Card);
            _statusTitle.SetBounds(18, 16, 420, 24);
            _actionCard.Controls.Add(_statusTitle);

            _statusDetail = Ui.Label("选择一个文件夹，DeskFlow 会自动识别 JPG、PNG、WebP 等图片。", Theme.Small, Theme.TextMuted, Theme.Card);
            _statusDetail.SetBounds(18, 40, 440, 22);
            _statusDetail.AutoEllipsis = true;
            _actionCard.Controls.Add(_statusDetail);

            _apply = new UiButton { Text = "应用轮播", Style = UiButtonStyle.Primary, Width = 104, Height = 38 };
            _apply.Click += delegate
            {
                Action handler = ApplyRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_apply);

            _pause = new UiButton { Text = "暂停轮播", Style = UiButtonStyle.Soft, Width = 112, Height = 38 };
            _pause.Click += delegate
            {
                Action handler = TogglePauseRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_pause);

            _next = new UiButton { Text = "下一张", Style = UiButtonStyle.Secondary, Width = 92, Height = 38 };
            _next.Click += delegate
            {
                Action handler = NextRequested;
                if (handler != null) handler();
            };
            _actionCard.Controls.Add(_next);

            UpdateChipSelection();
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
            _interval.Value = _settings.IntervalSeconds;
            _shuffle.Checked = _settings.Shuffle;
            _updating = false;
            UpdateChipSelection();
        }

        private void UpdateChipSelection()
        {
            int[] values = { 10, 30, 60, 300 };
            for (int i = 0; i < _chips.Length; i++)
                _chips[i].Selected = _settings.IntervalSeconds == values[i];
        }

        private void ChooseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择图片文件夹（仅读取该文件夹内的图片）";
                dialog.ShowNewFolderButton = false;
                if (!string.IsNullOrEmpty(_settings.ImageFolder) && Directory.Exists(_settings.ImageFolder))
                    dialog.SelectedPath = _settings.ImageFolder;
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                _settings.ImageFolder = dialog.SelectedPath;
                _settings.Mode = "slideshow";
                _folderPath.Text = dialog.SelectedPath;
                RaiseChanged();
            }
        }

        public void UpdateState(WallpaperController controller)
        {
            bool enabled = _settings.Mode == "slideshow";
            bool hasFolder = !string.IsNullOrEmpty(_settings.ImageFolder) && Directory.Exists(_settings.ImageFolder);
            List<string> files = hasFolder ? GetFolderFiles() : new List<string>();
            int count = Math.Max(controller.ImageCount, files.Count);
            Bitmap preview = enabled ? controller.CurrentImage : null;
            if (preview == null && files.Count > 0) preview = GetPreview(files[0]);
            _statusDetail.ForeColor = Theme.TextMuted;
            _preview.PreviewImage = preview;
            _preview.VideoMode = false;
            _preview.Glyph = "\uE91B";

            if (!enabled)
            {
                bool ready = hasFolder && count > 0;
                _preview.Badge = ready ? "已选择" : "未启用";
                _preview.BadgeColor = ready ? Theme.Accent : Theme.TextDim;
                _preview.Title = ready ? Path.GetFileName(files[0]) : "图片轮播";
                _preview.Subtitle = ready
                    ? count + " 张图片 · 点击“应用轮播”开始"
                    : "选择文件夹后开始轮播";
                _statusTitle.Text = ready ? "已选择图片文件夹" : "尚未启用图片轮播";
                _statusDetail.Text = ready
                    ? "点击“应用轮播”开始轮播。"
                    : "选择一个文件夹，DeskFlow 会自动识别 JPG、PNG、WebP 等图片。";
                _pause.Text = "暂停轮播";
            }
            else if (!hasFolder)
            {
                _preview.Badge = "需要文件夹";
                _preview.BadgeColor = Theme.Warning;
                _preview.Title = "尚未选择文件夹";
                _preview.Subtitle = "点击“选择文件夹”开始";
                _statusTitle.Text = "还没有可轮播的图片";
                _statusDetail.Text = "请选择一个包含图片的文件夹。";
                _pause.Text = "暂停轮播";
            }
            else if (count == 0)
            {
                _preview.Badge = "没有图片";
                _preview.BadgeColor = Theme.Warning;
                _preview.Title = "文件夹中没有支持的图片";
                _preview.Subtitle = "支持 WEBP、JPG、PNG、BMP、GIF";
                _statusTitle.Text = "没有找到可以播放的图片";
                _statusDetail.Text = "支持 WEBP、JPG/JPEG、PNG、BMP 与 GIF（静态首帧）。";
                _pause.Text = "暂停轮播";
            }
            else
            {
                bool paused = controller.IsPaused;
                _preview.Badge = paused ? "已暂停" : "轮播中";
                _preview.BadgeColor = paused ? Theme.Warning : Theme.Success;
                _preview.Title = controller.CurrentImagePath != null ? Path.GetFileName(controller.CurrentImagePath) : "图片轮播";
                _preview.Subtitle = count + " 张图片 · 每 " + _settings.IntervalSeconds + " 秒切换";
                _statusTitle.Text = paused ? "轮播已暂停" : "正在轮播";
                _statusDetail.Text = "共 " + count + " 张图片，当前间隔 " + _settings.IntervalSeconds + " 秒。";
                _pause.Text = paused ? "继续轮播" : "暂停轮播";
            }

            _countValue.Text = count + " 张";
            _folderPath.Text = hasFolder ? _settings.ImageFolder : "尚未选择文件夹";
            _folderPath.ForeColor = hasFolder ? Theme.TextMuted : Theme.TextDim;
            _pause.Enabled = enabled && count > 0;
            _next.Enabled = enabled && count > 0;
            _refresh.Enabled = enabled && hasFolder;
            _apply.Visible = !enabled && hasFolder && count > 0;
            _apply.Enabled = _apply.Visible;
            _choose.Text = hasFolder ? "更换文件夹" : "选择文件夹";
            LayoutPage();
        }

        private List<string> GetFolderFiles()
        {
            string folder = _settings.ImageFolder ?? string.Empty;
            DateTime stamp = DateTime.MinValue;
            try { if (Directory.Exists(folder)) stamp = Directory.GetLastWriteTimeUtc(folder); } catch { }
            if (!string.Equals(_scanFolder, folder, StringComparison.OrdinalIgnoreCase)
                || _scanStamp != stamp)
            {
                _scanFolder = folder;
                _scanStamp = stamp;
                _scanFiles = ImageLoader.ScanFolder(folder);
            }
            return _scanFiles;
        }

        private Bitmap GetPreview(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (string.Equals(_previewPath, path, StringComparison.OrdinalIgnoreCase)
                && _previewBitmap != null)
                return _previewBitmap;

            _preview.PreviewImage = null;
            if (_previewBitmap != null)
            {
                try { _previewBitmap.Dispose(); } catch { }
                _previewBitmap = null;
            }
            string error;
            _previewBitmap = ImageLoader.LoadForSize(path, 640, 360, out error);
            _previewPath = _previewBitmap != null ? path : null;
            return _previewBitmap;
        }

        public void ShowFolderEmptyHint()
        {
            _statusDetail.Text = "这个文件夹里没有支持的图片，请选择其他文件夹。";
            _statusDetail.ForeColor = Theme.Warning;
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

            int folderY = top + previewHeight + 16;
            _folderCard.SetBounds(margin, folderY, inner, 76);
            _choose.SetBounds(_folderCard.Width - 132, 20, 116, 36);
            _refresh.SetBounds(_folderCard.Width - 210, 20, 70, 36);
            _folderPath.Width = Math.Max(100, _folderCard.Width - 240);

            int actionY = folderY + 92;
            int actionHeight = Math.Max(92, Height - actionY - 18);
            _actionCard.SetBounds(margin, actionY, inner, actionHeight);
            int actionCenter = _actionCard.Height / 2;
            _statusTitle.Top = actionCenter - 24;
            _statusDetail.Top = actionCenter + 1;
            _apply.Top = actionCenter - 19;
            _pause.Top = actionCenter - 19;
            _next.Top = actionCenter - 19;
            _apply.SetBounds(_actionCard.Width - 340, actionCenter - 19, 104, 38);
            _pause.SetBounds(_actionCard.Width - 226, actionCenter - 19, 112, 38);
            _next.SetBounds(_actionCard.Width - 104, actionCenter - 19, 92, 38);
            _statusDetail.Width = Math.Max(180, _actionCard.Width - 382);
            _statusTitle.Width = Math.Max(180, _actionCard.Width - 382);

            _shuffle.Location = new Point(sideWidth - 58, 201);
            _intervalUnit.Left = 156;
        }
    }
}
