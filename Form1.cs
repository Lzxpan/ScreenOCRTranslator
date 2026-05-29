using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Tesseract;
using static GeminiClient;

namespace ScreenOCRTranslator
{
    public partial class Form1 : Form
    {
        private GeminiClient geminiClient;
        private System.Windows.Forms.Timer monitorTimer;
        private Point lastMousePosition;
        private DateTime lastMoveTime;
        private bool isMonitoring = false;
        private IKeyboardMouseEvents globalHook;
        private bool _activationKeyboardPressed = false;
        private Keys _activationKeyboardKey = Keys.Q;
        private MouseButtons _activationMouseButton = MouseButtons.Left;
        private bool isSelecting = false;
        private Rectangle lastCapturedRegion;
        private TranslationOverlayForm _overlay;
        private CursorHintForm _cursorHint;
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;
        private bool _allowExit = false;
        private DailyQuotaTracker _quotaTracker;
        private QuotaBoardForm _quotaBoard;
        private const string GeminiDefaultModel = "gemini-3.1-flash-lite";
        private const string MistralVisionDefaultModel = "mistral-large-2512";

        public Form1()
        {
            InitializeComponent();
        }

        private static readonly Keys[] ActivationKeyboardKeys = new Keys[]
        {
            Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M,
            Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
            Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
            Keys.Space, Keys.ControlKey, Keys.Menu, Keys.ShiftKey
        };

        private static readonly MouseButtons[] ActivationMouseButtons = new MouseButtons[]
        {
            MouseButtons.Left,
            MouseButtons.Middle,
            MouseButtons.XButton1,
            MouseButtons.XButton2
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeActivationKeyOptions();
            InitializeGeminiModelOptions();
            cmbModel.SelectedIndex = 0;
            cmbLanguage.SelectedIndex = 3;
            cmbTranslationMode.SelectedIndex = 1; // 預設 OCR 模式
            cmbModel_MistralPixtral.Items.Clear();
            cmbModel_MistralPixtral.Items.AddRange(new object[]
            {
                MistralVisionDefaultModel
            });

            cmbModel_Llama4.Items.Clear();
            cmbModel_Llama4.Items.AddRange(new object[]
            {
                "meta-llama/llama-4-scout-17b-16e-instruct"
            });

            cmbModel_MistralPixtral.SelectedIndex = 0;
            cmbModel_Llama4.SelectedIndex = 0;

            // 載入儲存的 API Key 和模型
            txtApiKey.Text = Properties.Settings.Default.ApiKey;
            string savedModel = NormalizeGeminiModel(Properties.Settings.Default.ModelName);
            if (!string.IsNullOrEmpty(savedModel))
            {
                int index = cmbModel.Items.IndexOf(savedModel);
                if (index >= 0)
                    cmbModel.SelectedIndex = index;
            }

            txtApiKey_MistralPixtral.Text = Properties.Settings.Default.ApiKey_MistralPixtral;
            string savedMistralPixtralModel = NormalizeMistralVisionModel(Properties.Settings.Default.ModelName_MistralPixtral);
            if (!string.IsNullOrEmpty(savedMistralPixtralModel))
            {
                int index = cmbModel_MistralPixtral.Items.IndexOf(savedMistralPixtralModel);
                if (index >= 0)
                    cmbModel_MistralPixtral.SelectedIndex = index;
            }

            txtApiKey_Llama4.Text = Properties.Settings.Default.ApiKey_Llama4;
            string savedLlama4Model = Properties.Settings.Default.ModelName_Llama4;
            if (!string.IsNullOrEmpty(savedLlama4Model))
            {
                int index = cmbModel_Llama4.Items.IndexOf(savedLlama4Model);
                if (index >= 0)
                    cmbModel_Llama4.SelectedIndex = index;
            }

            cmbTranslationMode.SelectedIndex = Properties.Settings.Default.TranslationModeIndex;
            cmbLanguage.SelectedIndex = Properties.Settings.Default.LanguageModeIndex;
            numOverlaySeconds.Value = Math.Max(numOverlaySeconds.Minimum,
                Math.Min(numOverlaySeconds.Maximum, Properties.Settings.Default.OverlaySeconds));
            LoadActivationKeySettings();

            monitorTimer = new System.Windows.Forms.Timer();
            monitorTimer.Interval = 200; // 每 200ms 檢查一次滑鼠
            monitorTimer.Tick += MonitorTimer_Tick;
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.KeyUp += GlobalHook_KeyUp;
            globalHook.MouseDownExt += GlobalHook_MouseDownExt;
            globalHook.MouseUpExt += GlobalHook_MouseUpExt;
            linkLabel1.Links.Clear();
            linkLabel1.Links.Add(0, linkLabel1.Text.Length, "https://aistudio.google.com/api-keys"); // LinkData 存網址
            linkLabel_MistralPixtral.Text = "前往取得Mistral API key";
            linkLabel_MistralPixtral.Links.Clear();
            linkLabel_MistralPixtral.Links.Add(0, linkLabel_MistralPixtral.Text.Length, "https://console.mistral.ai/api-keys/");
            linkLabel_Llama4.Text = "前往取得Groq API key";
            linkLabel_Llama4.Links.Clear();
            linkLabel_Llama4.Links.Add(0, linkLabel_Llama4.Text.Length, "https://console.groq.com/keys");

            InitializeTrayIcon();

            _quotaTracker = DailyQuotaTracker.LoadOrCreate(Path.Combine(Application.StartupPath, "usage_daily.json"));
            _quotaTracker.EnsureToday();
        }

        private void InitializeGeminiModelOptions()
        {
            cmbModel.Items.Clear();
            cmbModel.Items.AddRange(new object[]
            {
                GeminiDefaultModel,
                "gemini-3-flash-preview",
                "gemini-2.5-flash-lite",
                "gemini-2.5-flash",
                "gemini-2.5-pro"
            });
        }

        private static string NormalizeGeminiModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return "";
            string trimmed = model.Trim();
            if (string.Equals(trimmed, "gemini-3.1-flash-lite-preview", StringComparison.OrdinalIgnoreCase))
                return GeminiDefaultModel;
            if (string.Equals(trimmed, "gemini-3-pro-preview", StringComparison.OrdinalIgnoreCase))
                return GeminiDefaultModel;
            return trimmed;
        }

        private static string NormalizeMistralVisionModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return "";
            string trimmed = model.Trim();
            if (string.Equals(trimmed, "pixtral-large-latest", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "pixtral-large-2411", StringComparison.OrdinalIgnoreCase))
            {
                return MistralVisionDefaultModel;
            }
            return trimmed;
        }

        private void InitializeActivationKeyOptions()
        {
            cmbActivationKeyboardKey.Items.Clear();
            foreach (var key in ActivationKeyboardKeys)
                cmbActivationKeyboardKey.Items.Add(GetKeyDisplayName(key));

            cmbActivationMouseButton.Items.Clear();
            foreach (var button in ActivationMouseButtons)
                cmbActivationMouseButton.Items.Add(GetMouseDisplayName(button));

            cmbActivationKeyboardKey.SelectedIndexChanged += (s, e) =>
            {
                _activationKeyboardKey = GetSelectedActivationKeyboardKey();
                _activationKeyboardPressed = false;
                UpdateActivationHint();
            };

            cmbActivationMouseButton.SelectedIndexChanged += (s, e) =>
            {
                _activationMouseButton = GetSelectedActivationMouseButton();
                UpdateActivationHint();
            };
        }

        private void LoadActivationKeySettings()
        {
            _activationKeyboardKey = ParseActivationKeyboardKey(Properties.Settings.Default.ActivationKeyboardKey);
            _activationMouseButton = ParseActivationMouseButton(Properties.Settings.Default.ActivationMouseButton);

            SelectActivationKeyboardKey(_activationKeyboardKey);
            SelectActivationMouseButton(_activationMouseButton);
            UpdateActivationHint();
        }

        private Keys GetSelectedActivationKeyboardKey()
        {
            int index = cmbActivationKeyboardKey.SelectedIndex;
            if (index >= 0 && index < ActivationKeyboardKeys.Length)
                return ActivationKeyboardKeys[index];
            return Keys.Q;
        }

        private MouseButtons GetSelectedActivationMouseButton()
        {
            int index = cmbActivationMouseButton.SelectedIndex;
            if (index >= 0 && index < ActivationMouseButtons.Length)
                return ActivationMouseButtons[index];
            return MouseButtons.Left;
        }

        private static Keys ParseActivationKeyboardKey(string saved)
        {
            if (!string.IsNullOrWhiteSpace(saved) &&
                Enum.TryParse(saved, true, out Keys key) &&
                ActivationKeyboardKeys.Contains(key))
            {
                return key;
            }
            return Keys.Q;
        }

        private static MouseButtons ParseActivationMouseButton(string saved)
        {
            if (!string.IsNullOrWhiteSpace(saved) &&
                Enum.TryParse(saved, true, out MouseButtons button) &&
                ActivationMouseButtons.Contains(button))
            {
                return button;
            }
            return MouseButtons.Left;
        }

        private void SelectActivationKeyboardKey(Keys key)
        {
            int index = Array.IndexOf(ActivationKeyboardKeys, key);
            cmbActivationKeyboardKey.SelectedIndex = index >= 0 ? index : Array.IndexOf(ActivationKeyboardKeys, Keys.Q);
        }

        private void SelectActivationMouseButton(MouseButtons button)
        {
            int index = Array.IndexOf(ActivationMouseButtons, button);
            cmbActivationMouseButton.SelectedIndex = index >= 0 ? index : Array.IndexOf(ActivationMouseButtons, MouseButtons.Left);
        }

        private static string GetKeyDisplayName(Keys key)
        {
            switch (key)
            {
                case Keys.D0: return "0";
                case Keys.D1: return "1";
                case Keys.D2: return "2";
                case Keys.D3: return "3";
                case Keys.D4: return "4";
                case Keys.D5: return "5";
                case Keys.D6: return "6";
                case Keys.D7: return "7";
                case Keys.D8: return "8";
                case Keys.D9: return "9";
                case Keys.Space: return "space";
                case Keys.ControlKey: return "ctrl";
                case Keys.Menu: return "alt";
                case Keys.ShiftKey: return "shift";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private static string GetMouseDisplayName(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left: return "Left";
                case MouseButtons.Middle: return "Middle";
                case MouseButtons.XButton1: return "XButton1";
                case MouseButtons.XButton2: return "XButton2";
                default: return MouseButtons.Left.ToString();
            }
        }

        private void UpdateActivationHint()
        {
            label3.Text = $"按著{GetKeyDisplayName(_activationKeyboardKey)}+滑鼠{GetMouseDisplayName(_activationMouseButton)}，即可啟動框選擷取。";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_allowExit && e.CloseReason == CloseReason.UserClosing)
            {
                var choice = MessageBox.Show(
                    "請選擇：\n是(Y)：關閉程式\n否(N)：縮小到右下角常駐\n取消：維持目前視窗",
                    "關閉選項",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (choice == DialogResult.Yes)
                {
                    _allowExit = true;
                }
                else if (choice == DialogResult.No)
                {
                    e.Cancel = true;
                    HideToTray();
                    return;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            Properties.Settings.Default.ApiKey = txtApiKey.Text.Trim();
            Properties.Settings.Default.ModelName = cmbModel.SelectedItem?.ToString();
            Properties.Settings.Default.ApiKey_MistralPixtral = txtApiKey_MistralPixtral.Text.Trim();
            Properties.Settings.Default.ModelName_MistralPixtral = cmbModel_MistralPixtral.SelectedItem?.ToString();
            Properties.Settings.Default.ApiKey_Llama4 = txtApiKey_Llama4.Text.Trim();
            Properties.Settings.Default.ModelName_Llama4 = cmbModel_Llama4.SelectedItem?.ToString();
            Properties.Settings.Default.TranslationModeIndex = cmbTranslationMode.SelectedIndex;
            Properties.Settings.Default.LanguageModeIndex = cmbLanguage.SelectedIndex;
            Properties.Settings.Default.OverlaySeconds = (int)numOverlaySeconds.Value;
            Properties.Settings.Default.ActivationKeyboardKey = _activationKeyboardKey.ToString().ToLowerInvariant();
            Properties.Settings.Default.ActivationMouseButton = _activationMouseButton.ToString();
            Properties.Settings.Default.Save(); // 寫入設定
            _quotaTracker?.Save();
            globalHook?.Dispose();
            _cursorHint?.Dispose();
            _trayIcon?.Dispose();
            _trayMenu?.Dispose();
        }

        private void btnQuotaBoard_Click(object sender, EventArgs e)
        {
            ShowQuotaBoard();
        }

        private void ShowQuotaBoard()
        {
            _quotaTracker?.EnsureToday();
            if (_quotaBoard == null || _quotaBoard.IsDisposed)
            {
                _quotaBoard = new QuotaBoardForm();
                _quotaBoard.FormClosed += (s, e) => _quotaBoard = null;
            }

            var rows = _quotaTracker?.GetSnapshot(BuildLlmCredentials()) ?? new List<DailyQuotaEntry>();
            _quotaBoard.UpdateRows(rows, DateTime.Now);
            _quotaBoard.Show();
            _quotaBoard.BringToFront();
        }

        private void InitializeTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("開啟主視窗", null, (s, e) => ShowFromTray());
            _trayMenu.Items.Add("結束", null, (s, e) => ExitApplication());

            _trayIcon = new NotifyIcon
            {
                Text = "ScreenOCRTranslator",
                Icon = this.Icon,
                Visible = false,
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void HideToTray()
        {
            ShowInTaskbar = false;
            Hide();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = true;
                _trayIcon.ShowBalloonTip(1200, "ScreenOCRTranslator", "已縮小到右下角常駐。", ToolTipIcon.Info);
            }
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
            }
        }

        private void ExitApplication()
        {
            _allowExit = true;
            _trayIcon.Visible = false;
            Close();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (WindowState == FormWindowState.Minimized)
            {
                HideToTray();
            }
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            // 顯示選取視窗
            using (var selector = new SelectionForm())
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    Rectangle captureRect = selector.SelectedRectangle;

                    // 擷取畫面該範圍
                    Bitmap captured = new Bitmap(captureRect.Width, captureRect.Height);
                    using (Graphics g = Graphics.FromImage(captured))
                    {
                        g.CopyFromScreen(captureRect.Location, Point.Empty, captureRect.Size);
                    }
                    lastCapturedRegion = captureRect; // ✅ 存起來方便之後畫文字用
                    picturePreview.Image = captured; // 顯示原圖以供除錯

                    // OCR 辨識（使用選擇的語言）
                    string langCode = GetSelectedLanguageCode();
                    var ocr = new TesseractOcrProcessor(langCode, picturePreview);
                    txtResult.Text = "辨識中...";
                    string text = ocr.PerformOCR(captured);
                    txtResult.Text = text;
                }
            }
        }

        private Bitmap CaptureCursorArea(int width = 300, int height = 150)
        {
            Point cursorPos = Cursor.Position;

            int x = Math.Max(0, cursorPos.X - width / 2);
            int y = Math.Max(0, cursorPos.Y - height / 2);

            Rectangle captureRect = new Rectangle(x, y, width, height);
            Bitmap bmp = new Bitmap(captureRect.Width, captureRect.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(captureRect.Location, Point.Empty, captureRect.Size);
            }

            return bmp;
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (!isMonitoring)
            {
                // ⚠ 初始化 GeminiClient
                string apiKey = txtApiKey.Text.Trim();
                string model = cmbModel.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(model))
                {
                    MessageBox.Show("請輸入 API Key 並選擇模型");
                    return;
                }

                geminiClient = new GeminiClient(apiKey, model); // ✅ 修正點

                isMonitoring = true;
                btnStartStop.Text = "停止";
                lblStatus.Text = "滑鼠偵測中...";
                lblStatus.ForeColor = Color.LightGreen;
                lastMousePosition = Cursor.Position;
                lastMoveTime = DateTime.Now;
                monitorTimer.Start();
            }
            else
            {
                isMonitoring = false;
                btnStartStop.Text = "啟動";
                lblStatus.Text = "已停止";
                lblStatus.ForeColor = Color.DarkRed;
                monitorTimer.Stop();
            }
        }

        private async void MonitorTimer_Tick(object sender, EventArgs e)
        {
            var currentPos = Cursor.Position;

            if (currentPos != lastMousePosition)
            {
                lastMousePosition = currentPos;
                lastMoveTime = DateTime.Now;
                return;
            }

            int idleSeconds = (int)numIdleSeconds.Value;
            if ((DateTime.Now - lastMoveTime).TotalSeconds >= idleSeconds)
            {
                monitorTimer.Stop(); // 暫停監聽避免重複觸發
                lblStatus.Text = "擷取中...";

                Bitmap img = CaptureCursorArea();

                txtResult.Text = "辨識中...";
                string langCode = GetSelectedLanguageCode();
                var ocr = new TesseractOcrProcessor(langCode, picturePreview); // 這樣圖片才會顯示
                string text = ocr.PerformOCR(img);
                txtResult.Text = text;

                //string result = await geminiClient.SendImageForOCRAndTranslate(img);
                //txtResult.Text = result;

                lastMoveTime = DateTime.Now;
                monitorTimer.Start(); // 再次啟動
                lblStatus.Text = "滑鼠偵測中...";
            }
        }

        public class TesseractOcrProcessor
        {
            private readonly string _language;
            private readonly PictureBox _previewControl;

            public TesseractOcrProcessor(string language = "eng", PictureBox previewControl = null)
            {
                _language = language;
                _previewControl = previewControl;
            }

            public string PerformOCR(Bitmap image)
            {
                // 將圖片放大 3 倍
                Bitmap scaled = new Bitmap(image, image.Width * 3, image.Height * 3);

                // 轉為灰階
                Bitmap gray = new Bitmap(image.Width, image.Height);
                using (Graphics g = Graphics.FromImage(gray))
                {
                    var colorMatrix = new ColorMatrix(new float[][]
                    {
                        new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                        new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                        new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                        new float[] { 0, 0, 0, 1, 0 },
                        new float[] { 0, 0, 0, 0, 1 }
                    });

                    var attributes = new ImageAttributes();
                    attributes.SetColorMatrix(colorMatrix);

                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }

                // 加上銳化處理
                gray = ApplySharpenFilter(gray);

                // 自動 threshold：計算整張圖片的平均灰階亮度
                int total = 0;
                for (int y = 0; y < gray.Height; y++)
                {
                    for (int x = 0; x < gray.Width; x++)
                    {
                        Color pixel = gray.GetPixel(x, y);
                        total += pixel.R; // 灰階圖 RGB 相同
                    }
                }

                int avg = total / (gray.Width * gray.Height);
                int threshold = avg; // 使用平均亮度作為臨界值（類似 adaptive）

                // 黑白二值化
                Bitmap binary = new Bitmap(gray.Width, gray.Height);
                for (int y = 0; y < gray.Height; y++)
                {
                    for (int x = 0; x < gray.Width; x++)
                    {
                        Color pixel = gray.GetPixel(x, y);
                        Color newColor = (pixel.R > threshold) ? Color.White : Color.Black;
                        binary.SetPixel(x, y, newColor);
                    }
                }

                if (IsMostlyDark(binary))
                {
                    binary = InvertImage(binary);
                }

                // 顯示處理後圖片（方便 debug）
                _previewControl.Image = binary; // ✅ 這裡要先傳入 previewControl（例如 PictureBox）

                // 使用 Tesseract 進行辨識
                using (var engine = new TesseractEngine(TessdataResourceExtractor.EnsureTessdata(), _language, EngineMode.LstmOnly))
                {
                    // 小區域框選：PSM 6 通常最穩
                    engine.DefaultPageSegMode = PageSegMode.SingleBlock;

                    // 避免截圖被當成低 DPI 影像
                    engine.SetVariable("user_defined_dpi", "300");

                    // .NET 10 使用 Tesseract netstandard API，需先把 Bitmap 轉為 PNG bytes 再載入 Pix。
                    byte[] pngBytes;
                    using (var ms = new MemoryStream())
                    {
                        binary.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        pngBytes = ms.ToArray();
                    }

                    using (var pix = Pix.LoadFromMemory(pngBytes))
                    using (var page = engine.Process(pix))
                    {
                        var text = page.GetText();

                        // 你可以用它做 debug / 自動挑參數（可先印出來）
                        var conf = page.GetMeanConfidence(); // 0~1

                        return text;
                    }
                }
            }

            public static Bitmap ApplySharpenFilter(Bitmap image)
            {
                Bitmap sharpenImage = new Bitmap(image.Width, image.Height);

                // 銳化矩陣 (Laplacian kernel)
                float[][] kernel =
                {
                    new float[] { -1, -1, -1 },
                    new float[] { -1,  9, -1 },
                    new float[] { -1, -1, -1 }
                };

                int w = image.Width;
                int h = image.Height;

                for (int y = 1; y < h - 1; y++)
                {
                    for (int x = 1; x < w - 1; x++)
                    {
                        float r = 0, g = 0, b = 0;

                        for (int ky = -1; ky <= 1; ky++)
                        {
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                Color pixel = image.GetPixel(x + kx, y + ky);
                                float k = kernel[ky + 1][kx + 1];

                                r += pixel.R * k;
                                g += pixel.G * k;
                                b += pixel.B * k;
                            }
                        }

                        int rr = Math.Min(255, Math.Max(0, (int)r));
                        int gg = Math.Min(255, Math.Max(0, (int)g));
                        int bb = Math.Min(255, Math.Max(0, (int)b));

                        sharpenImage.SetPixel(x, y, Color.FromArgb(rr, gg, bb));
                    }
                }

                return sharpenImage;
            }
        }

        private string GetSelectedLanguageCode()
        {
            switch (cmbLanguage.SelectedItem?.ToString())
            {
                case "繁體中文": return "chi_tra";
                case "簡體中文": return "chi_sim";
                case "日文": return "jpn";
                case "英文": return "eng";
                default:
                    return "eng";
            }
        }

        public static Bitmap InvertImage(Bitmap original)
        {
            Bitmap inverted = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    Color invertedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    inverted.SetPixel(x, y, invertedColor);
                }
            }

            return inverted;
        }

        public static bool IsMostlyDark(Bitmap image)
        {
            long totalBrightness = 0;
            int pixelCount = image.Width * image.Height;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    int brightness = (c.R + c.G + c.B) / 3;
                    totalBrightness += brightness;
                }
            }

            int avg = (int)(totalBrightness / pixelCount);
            return avg < 128; // 小於 128 表示整體偏暗，可視為黑底白字
        }

        private class DownscaleResult
        {
            public Bitmap Image;
            public int SrcW, SrcH;
            public Rectangle InkBounds;
            public int LineCount;
            public float EstLineH;
            public float Scale;
            public bool Cropped;
        }

        // 你呼叫 AI 前用這個縮圖（支援 Debug + 可裁切）
        private static DownscaleResult DownscaleForAi(
            Bitmap src,
            int targetLinePx = 22,      // 想縮更小就降這個：18~24 建議
            int minLinePx = 14,         // 保護下限：多行小字不要縮到看不清
            bool cropToInk = true,      // ✅ 建議開：大量留白會被砍掉，縮圖會更有效
            int cropPadPx = 6,          // 裁切邊界留白
            int maxWidth = 1200,
            int maxHeight = 1200,
            int maxPixels = 350_000,    // 想更小就降：例如 250_000
            float minScaleHard = 0.12f  // 最低縮放保險（避免變太小）
        )
        {
            if (src == null) return null;

            var (inkBounds, lineCount) = EstimateTextBoundsAndLineCount(src);

            // 估算單行行高（用 inkBounds，單行/少字會比較準）
            float estLineH = Math.Max(1f, (float)inkBounds.Height / Math.Max(1, lineCount));

            // 1) 先決定裁切矩形（可選）
            Rectangle cropRect = new Rectangle(0, 0, src.Width, src.Height);
            bool cropped = false;

            if (cropToInk && inkBounds.Width > 0 && inkBounds.Height > 0)
            {
                // 加 padding，避免切到筆畫
                int left = Math.Max(0, inkBounds.Left - cropPadPx);
                int top = Math.Max(0, inkBounds.Top - cropPadPx);
                int right = Math.Min(src.Width, inkBounds.Right + cropPadPx);
                int bottom = Math.Min(src.Height, inkBounds.Bottom + cropPadPx);
                cropRect = Rectangle.FromLTRB(left, top, right, bottom);

                // 避免裁太小（極端情況）
                if (cropRect.Width >= 20 && cropRect.Height >= 20)
                    cropped = true;
                else
                    cropRect = new Rectangle(0, 0, src.Width, src.Height);
            }

            // 2) 用行高決定縮放：希望縮到 targetLinePx，但不低於 minLinePx
            //    scaleWanted：把 estLineH 縮到 targetLinePx
            //    scaleMinByLine：保證縮完後行高 >= minLinePx
            float scaleWanted = targetLinePx / estLineH;
            float scaleMinByLine = minLinePx / estLineH;

            // 我們只做「縮小」，不放大，所以 clamp 到 <= 1
            float scaleByLine = Math.Min(1f, scaleWanted);

            // 但也不要縮到小於 minLinePx（縮太小 AI 反而看不清）
            scaleByLine = Math.Max(scaleByLine, Math.Min(1f, scaleMinByLine));

            // 3) 尺寸與像素上限保護（用裁切後的尺寸來算）
            int baseW = cropRect.Width;
            int baseH = cropRect.Height;

            float scaleByDim = Math.Min(1f, Math.Min((float)maxWidth / baseW, (float)maxHeight / baseH));

            long pixels = (long)baseW * baseH;
            float scaleByPixels = (pixels > maxPixels)
                ? (float)Math.Sqrt(maxPixels / (double)pixels)
                : 1f;

            // 最終縮放取最小（最嚴格）
            float scale = Math.Min(scaleByLine, Math.Min(scaleByDim, scaleByPixels));

            // 最低縮放保險
            scale = Math.Max(scale, minScaleHard);

            // 4) 先裁切，再縮放（都會產生新 bitmap）
            Bitmap working = CropBitmap(src, cropRect);     // 一律 new
            Bitmap resized = working;

            if (scale < 0.999f)
            {
                int newW = Math.Max(1, (int)Math.Round(working.Width * scale));
                int newH = Math.Max(1, (int)Math.Round(working.Height * scale));

                resized = ResizeBitmap(working, newW, newH);
                working.Dispose();
            }

            return new DownscaleResult
            {
                Image = resized,
                SrcW = src.Width,
                SrcH = src.Height,
                InkBounds = inkBounds,
                LineCount = Math.Max(1, lineCount),
                EstLineH = estLineH,
                Scale = scale,
                Cropped = cropped
            };
        }

        private static Bitmap CropBitmap(Bitmap src, Rectangle rect)
        {
            // rect 若剛好是全圖，也會 clone 一份，確保呼叫端可 Dispose
            var dst = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
            }
            return dst;
        }


        private static Bitmap ResizeBitmap(Bitmap src, int newW, int newH)
        {
            var dst = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawImage(src, new Rectangle(0, 0, newW, newH));
            }
            return dst;
        }

        /// <summary>
        /// 會同時嘗試「深色字」與「淺色字」兩種 ink，選擇更像文字的那個（避免把黑底條當成文字）
        /// </summary>
        private static (Rectangle inkBounds, int lineCount) EstimateTextBoundsAndLineCount(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;

            // 背景亮度：取四邊取樣中位數
            List<int> samples = new List<int>();
            int stepEdgeX = Math.Max(1, w / 60);
            int stepEdgeY = Math.Max(1, h / 60);

            for (int x = 0; x < w; x += stepEdgeX)
            {
                samples.Add(Luma(bmp.GetPixel(x, 0)));
                samples.Add(Luma(bmp.GetPixel(x, h - 1)));
            }
            for (int y = 0; y < h; y += stepEdgeY)
            {
                samples.Add(Luma(bmp.GetPixel(0, y)));
                samples.Add(Luma(bmp.GetPixel(w - 1, y)));
            }
            samples.Sort();
            int bg = samples.Count > 0 ? samples[samples.Count / 2] : 255;

            int tol = 40; // 30~60 可調
            int step = Math.Max(1, Math.Min(w, h) / 350);

            // 兩種候選：darkInk(比背景暗) / lightInk(比背景亮)
            var candDark = MeasureInkCandidate(bmp, bg, tol, step, wantLightInk: false);
            var candLight = MeasureInkCandidate(bmp, bg, tol, step, wantLightInk: true);

            // 選「更像文字」的：ink 佔比要小（文字通常是少數），但不能太少到是雜訊
            InkCandidate best = ChooseBetterCandidate(candDark, candLight);

            if (!best.Valid)
                return (new Rectangle(0, 0, w, h), 1);

            return (best.Bounds, Math.Max(1, best.Lines));
        }

        private struct InkCandidate
        {
            public bool Valid;
            public Rectangle Bounds;
            public int Lines;
            public double FillRatio; // 0~1
        }

        private static InkCandidate ChooseBetterCandidate(InkCandidate a, InkCandidate b)
        {
            // 合理文字佔比範圍（太大多半是背景塊，太小多半是雜訊）
            bool aOk = a.Valid && a.FillRatio >= 0.002 && a.FillRatio <= 0.45;
            bool bOk = b.Valid && b.FillRatio >= 0.002 && b.FillRatio <= 0.45;

            if (aOk && bOk) return (a.FillRatio <= b.FillRatio) ? a : b;
            if (aOk) return a;
            if (bOk) return b;

            // 退一步：至少挑一個 valid 的
            if (a.Valid && b.Valid) return (a.FillRatio <= b.FillRatio) ? a : b;
            if (a.Valid) return a;
            if (b.Valid) return b;
            return default;
        }

        private static InkCandidate MeasureInkCandidate(Bitmap bmp, int bg, int tol, int step, bool wantLightInk)
        {
            int w = bmp.Width, h = bmp.Height;

            int minX = w, minY = h, maxX = -1, maxY = -1;

            bool[] rowInk = new bool[h];
            long inkTotal = 0;
            long sampleTotal = 0;

            for (int y = 0; y < h; y += step)
            {
                int inkCountRow = 0;
                int sampleCountRow = 0;

                for (int x = 0; x < w; x += step)
                {
                    sampleCountRow++;
                    int l = Luma(bmp.GetPixel(x, y));

                    bool isInk = wantLightInk
                        ? (l > bg + tol)
                        : (l < bg - tol);

                    if (isInk)
                    {
                        inkCountRow++;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }

                inkTotal += inkCountRow;
                sampleTotal += sampleCountRow;

                if (sampleCountRow > 0 && inkCountRow > sampleCountRow * 0.02) // 2%
                    rowInk[y] = true;
            }

            if (maxX < 0 || sampleTotal == 0)
                return default;

            // 行數：rowInk 連續區塊
            int lines = 0;
            bool inLine = false;
            int gap = 0;
            int maxGap = Math.Max(2, 3 * step);

            for (int y = 0; y < h; y++)
            {
                bool has = rowInk[y];
                if (has)
                {
                    if (!inLine)
                    {
                        inLine = true;
                        lines++;
                    }
                    gap = 0;
                }
                else if (inLine)
                {
                    gap++;
                    if (gap > maxGap) inLine = false;
                }
            }
            if (lines < 1) lines = 1;

            var bounds = Rectangle.FromLTRB(
                Math.Max(0, minX - step * 3),
                Math.Max(0, minY - step * 3),
                Math.Min(w, maxX + step * 3),
                Math.Min(h, maxY + step * 3)
            );

            return new InkCandidate
            {
                Valid = bounds.Width > 0 && bounds.Height > 0,
                Bounds = bounds,
                Lines = lines,
                FillRatio = inkTotal / (double)sampleTotal
            };
        }

        private static int Luma(Color c) => (c.R * 299 + c.G * 587 + c.B * 114) / 1000;

        private List<LlmCredential> BuildLlmCredentials()
        {
            var list = new List<LlmCredential>();

            // 1) Gemini
            if (!string.IsNullOrWhiteSpace(txtApiKey.Text) && cmbModel.SelectedItem != null)
            {
                list.Add(new LlmCredential
                {
                    Provider = LlmProvider.Gemini,
                    ApiKey = txtApiKey.Text.Trim(),
                    Model = cmbModel.SelectedItem.ToString(),
                    BaseUrl = "",
                    DisplayName = "Gemini"
                });
            }

            // 2) Mistral Vision
            if (!string.IsNullOrWhiteSpace(txtApiKey_MistralPixtral.Text) && cmbModel_MistralPixtral.SelectedItem != null)
            {
                list.Add(new LlmCredential
                {
                    Provider = LlmProvider.MistralPixtral,
                    ApiKey = txtApiKey_MistralPixtral.Text.Trim(),
                    Model = cmbModel_MistralPixtral.SelectedItem.ToString(),
                    BaseUrl = "https://api.mistral.ai/v1",
                    DisplayName = "Mistral Vision"
                });
            }

            // 3) Groq Llama4
            if (!string.IsNullOrWhiteSpace(txtApiKey_Llama4.Text) && cmbModel_Llama4.SelectedItem != null)
            {
                list.Add(new LlmCredential
                {
                    Provider = LlmProvider.GroqLlama4,
                    ApiKey = txtApiKey_Llama4.Text.Trim(),
                    Model = cmbModel_Llama4.SelectedItem.ToString(),
                    BaseUrl = "https://api.groq.com/openai/v1",
                    DisplayName = "Groq Llama4"
                });
            }

            return list;
        }

        private async Task<GeminiResult> TranslateTextWithFallbackAsync(string prompt)
        {
            var credentials = BuildLlmCredentials();
            if (credentials.Count == 0)
            {
                return new GeminiResult
                {
                    HttpStatus = 0,
                    Error = "請至少填入一組可用的 API Key 與模型",
                    Text = "錯誤：請至少填入一組可用的 API Key 與模型"
                };
            }

            GeminiResult last = null;
            for (int i = 0; i < credentials.Count; i++)
            {
                var c = credentials[i];
                txtResult.AppendText($"\r\n[嘗試] {c.DisplayName} / {c.Model}\r\n");

                try
                {
                    GeminiResult gr;
                    if (c.Provider == LlmProvider.Gemini)
                    {
                        var client = new GeminiClient(c.ApiKey, c.Model);
                        gr = await client.TranslateTextEx(prompt);
                    }
                    else
                    {
                        var client = new OpenAiCompatibleClient(c.BaseUrl, c.ApiKey, c.Model, c.DisplayName);
                        gr = await client.TranslateTextEx(prompt);
                    }

                    if (gr != null && gr.HttpStatus == 200 && string.IsNullOrWhiteSpace(gr.Error))
                    {
                        _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, gr.Usage, true, null);
                        _quotaTracker?.Save();
                        return gr;
                    }

                    last = gr;
                    _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, gr?.Usage, false, gr?.Error);
                    _quotaTracker?.Save();
                    bool canSwitch = LlmErrorPolicy.ShouldSwitchProvider(gr);
                    if (canSwitch)
                    {
                        if (i < credentials.Count - 1)
                        {
                            txtResult.AppendText($"[切換] {c.DisplayName} 呼叫失敗，改用下一個已設定模型。\r\n");
                            continue;
                        }

                        return new GeminiResult
                        {
                            HttpStatus = gr?.HttpStatus ?? 0,
                            Error = "所有已設定模型/API KEY皆無法使用",
                            Text = "錯誤：所有已設定模型/API KEY皆無法使用"
                        };
                    }

                    return gr;
                }
                catch (Exception ex)
                {
                    _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, null, false, ex.Message);
                    _quotaTracker?.Save();
                    last = new GeminiResult
                    {
                        HttpStatus = 0,
                        Error = ex.Message,
                        Text = "錯誤：" + ex.Message
                    };

                    if (LlmErrorPolicy.IsRetryableNetworkError(ex) && i < credentials.Count - 1)
                    {
                        txtResult.AppendText($"[切換] {c.DisplayName} 網路或連線失敗，改用下一組 API Key。\r\n");
                        continue;
                    }

                    return last;
                }
            }

            return last ?? new GeminiResult
            {
                HttpStatus = 429,
                Error = "所有已設定模型/API KEY皆無法使用",
                Text = "錯誤：所有已設定模型/API KEY皆無法使用"
            };
        }

        private async Task<GeminiResult> TranslateImageWithFallbackAsync(Bitmap image)
        {
            var credentials = BuildLlmCredentials();
            if (credentials.Count == 0)
            {
                return new GeminiResult
                {
                    HttpStatus = 0,
                    Error = "請至少填入一組可用的 API Key 與模型",
                    Text = "錯誤：請至少填入一組可用的 API Key 與模型"
                };
            }

            GeminiResult last = null;
            for (int i = 0; i < credentials.Count; i++)
            {
                var c = credentials[i];
                txtResult.AppendText($"\r\n[嘗試] {c.DisplayName} / {c.Model}\r\n");

                try
                {
                    GeminiResult gr;
                    if (c.Provider == LlmProvider.Gemini)
                    {
                        var client = new GeminiClient(c.ApiKey, c.Model);
                        gr = await client.SendImageForOCRAndTranslateEx(image);
                    }
                    else
                    {
                        var client = new OpenAiCompatibleClient(c.BaseUrl, c.ApiKey, c.Model, c.DisplayName);
                        gr = await client.SendImageForOCRAndTranslateEx(image);
                    }

                    if (gr != null && gr.HttpStatus == 200 && string.IsNullOrWhiteSpace(gr.Error))
                    {
                        _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, gr.Usage, true, null);
                        _quotaTracker?.Save();
                        return gr;
                    }

                    last = gr;
                    _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, gr?.Usage, false, gr?.Error);
                    _quotaTracker?.Save();
                    bool canSwitch = LlmErrorPolicy.ShouldSwitchProvider(gr);
                    if (canSwitch)
                    {
                        if (i < credentials.Count - 1)
                        {
                            txtResult.AppendText($"[切換] {c.DisplayName} 呼叫失敗，改用下一個已設定模型。\r\n");
                            continue;
                        }

                        return new GeminiResult
                        {
                            HttpStatus = gr?.HttpStatus ?? 0,
                            Error = "所有已設定模型/API KEY皆無法使用",
                            Text = "錯誤：所有已設定模型/API KEY皆無法使用"
                        };
                    }

                    return gr;
                }
                catch (Exception ex)
                {
                    _quotaTracker?.RecordResult(DailyQuotaTracker.MapProvider(c.Provider, c.Model), c.Model, null, false, ex.Message);
                    _quotaTracker?.Save();
                    last = new GeminiResult
                    {
                        HttpStatus = 0,
                        Error = ex.Message,
                        Text = "錯誤：" + ex.Message
                    };

                    if (LlmErrorPolicy.IsRetryableNetworkError(ex) && i < credentials.Count - 1)
                    {
                        txtResult.AppendText($"[切換] {c.DisplayName} 網路或連線失敗，改用下一組 API Key。\r\n");
                        continue;
                    }

                    return last;
                }
            }

            return last ?? new GeminiResult
            {
                HttpStatus = 429,
                Error = "所有已設定模型/API KEY皆無法使用",
                Text = "錯誤：所有已設定模型/API KEY皆無法使用"
            };
        }

        private async Task HandleCapturedImage(Bitmap captured)
        {
            picturePreview.Image = captured;

            try
            {
                string translated = null;

                int selectedMode = cmbTranslationMode.SelectedIndex; // 翻譯模式切換 (ComboBox)

                if (selectedMode == 0) // OCR 模式
                {
                    txtResult.Text = "辨識中...";
                    string langCode = GetSelectedLanguageCode();
                    var ocr = new TesseractOcrProcessor(langCode, picturePreview);
                    string text = ocr.PerformOCR(captured);
                    txtResult.Text = text;

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        txtResult.AppendText("\r\n\r\n翻譯中...\r\n");
                        string prompt = $"請將以下內容翻譯成繁體中文（只輸出翻譯後的中文，不需解釋或開場白）：\n\n{text}";

                        try
                        {
                            ShowCursorHint("翻譯中...", Color.Gold);
                            var gr = await TranslateTextWithFallbackAsync(prompt);

                            if (!string.IsNullOrWhiteSpace(gr.Error) || gr.HttpStatus != 200)
                            {
                                txtResult.AppendText($"\r\n\r\n翻譯失敗（HTTP {gr.HttpStatus}）：\r\n{gr.Error}\r\n");
                                translated = "翻譯失敗";

                                if (gr.IsDailyQuotaExceeded)
                                    txtResult.AppendText("\r\n[判斷] 免費層「當日請求數」已達上限（常見 20/日）。\r\n");
                                if (gr.RetryAfterSeconds.HasValue)
                                    txtResult.AppendText($"[建議] 退避等待：{gr.RetryAfterSeconds.Value} 秒後再試（若為日配額，等待秒數不一定有用）。\r\n");

                                lblStatus.Text = $"429/配額：{(gr.IsDailyQuotaExceeded ? "當日上限" : "請稍後再試")}";
                                await ShowCursorFailureThenHideAsync();
                            }
                            else
                            {
                                HideCursorHint();
                                translated = gr.Text;
                                if (gr.Usage != null)
                                {
                                    UpdateTokensUi(gr.Usage);
                                }

                                txtResult.AppendText($"\r\n\r\n翻譯結果：\r\n{translated}");
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateTokensUi(null);
                            txtResult.AppendText($"\r\n\r\n翻譯失敗: {ex.Message}");
                            translated = "翻譯失敗";
                            await ShowCursorFailureThenHideAsync();
                        }
                    }
                }
                else if (selectedMode == 1) // AI 直接圖像翻譯
                {
                    txtResult.Text = "AI 圖像翻譯中...\r\n";

                    try
                    {
                        DownscaleResult ds = null;

                        try
                        {
                            ds = DownscaleForAi(
                                captured,
                                targetLinePx: 16,
                                minLinePx: 14,
                                cropToInk: true,
                                cropPadPx: 6,
                                maxPixels: 200_000
                            );

                            txtResult.AppendText(
                                $"[原始框選圖] {ds.SrcW}x{ds.SrcH}px\r\n" +
                                $"[InkBounds] {ds.InkBounds} lines={ds.LineCount} estLineH={ds.EstLineH:F1}px\r\n" +
                                $"[裁切] {(ds.Cropped ? "Y" : "N")}  [縮放] scale={ds.Scale:F3}\r\n" +
                                $"[AI送出圖] {ds.Image.Width}x{ds.Image.Height}px\r\n" +
                                $"(提示：右邊預覽若為實際大小)\r\n\r\n"
                            );

                            picturePreview.Image = (Bitmap)ds.Image.Clone();

                            ShowCursorHint("翻譯中...", Color.Gold);
                            var gr = await TranslateImageWithFallbackAsync(ds.Image);

                            if (!string.IsNullOrWhiteSpace(gr.Error) || gr.HttpStatus != 200)
                            {
                                txtResult.AppendText($"\r\n\r\nAI 圖像翻譯失敗（HTTP {gr.HttpStatus}）：\r\n{gr.Error}\r\n");
                                translated = "翻譯失敗";

                                if (gr.IsDailyQuotaExceeded)
                                    txtResult.AppendText("\r\n[判斷] 免費層「當日請求數」已達上限（常見 20/日）。\r\n");
                                if (gr.RetryAfterSeconds.HasValue)
                                    txtResult.AppendText($"[建議] 退避等待：{gr.RetryAfterSeconds.Value} 秒後再試（若為日配額，等待秒數不一定有用）。\r\n");

                                lblStatus.Text = $"429/配額：{(gr.IsDailyQuotaExceeded ? "當日上限" : "請稍後再試")}";
                                await ShowCursorFailureThenHideAsync();
                            }
                            else
                            {
                                HideCursorHint();
                                translated = gr.Text;

                                if (gr.Usage != null)
                                {
                                    UpdateTokensUi(gr.Usage);
                                }
                            }
                        }
                        finally
                        {
                            ds?.Image?.Dispose();
                        }

                        if (!string.IsNullOrWhiteSpace(translated) && translated != "翻譯失敗")
                        {
                            txtResult.AppendText($"\r\n\r\n翻譯結果：\r\n{translated}");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateTokensUi(null);
                        txtResult.AppendText($"\r\n\r\nAI 圖像翻譯失敗：{ex.Message}");
                        translated = "翻譯失敗";
                        await ShowCursorFailureThenHideAsync();
                    }
                }

                if (!string.IsNullOrWhiteSpace(translated) && !translated.StartsWith("錯誤："))
                {
                    DrawTranslatedText(translated, lastCapturedRegion);
                }
                else if (!string.IsNullOrWhiteSpace(translated))
                {
                    SafeStatus(translated); // 把錯誤留在狀態列
                }
            }
            finally
            {
                HideCursorHint();
            }
        }

        private void ShowCursorHint(string text, Color backColor)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowCursorHint(text, backColor)));
                return;
            }

            if (_cursorHint == null || _cursorHint.IsDisposed)
            {
                _cursorHint = new CursorHintForm();
            }

            _cursorHint.UpdateMessage(text, backColor, Cursor.Position);
            if (!_cursorHint.Visible)
            {
                _cursorHint.Show();
            }
        }

        private void HideCursorHint()
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideCursorHint));
                return;
            }

            if (_cursorHint != null && !_cursorHint.IsDisposed)
            {
                _cursorHint.Hide();
            }
        }

        private async Task ShowCursorFailureThenHideAsync()
        {
            ShowCursorHint("翻譯失敗", Color.IndianRed);
            await Task.Delay(2000);
            HideCursorHint();
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _activationKeyboardKey)
                _activationKeyboardPressed = true;
        }

        private void GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _activationKeyboardKey)
                _activationKeyboardPressed = false;
        }

        private void GlobalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            // ✅ 右鍵：若游標在 overlay 範圍內，立即關閉
            if (e.Button == MouseButtons.Right)
            {
                var ov = _overlay; // local snapshot
                if (ov != null && !ov.IsDisposed && ov.Visible)
                {
                    Point p = Cursor.Position;
                    if (ov.Bounds.Contains(p))
                    {
                        CloseOverlay();
                        e.Handled = true; // 避免右鍵穿透到底下程式彈選單
                        return;
                    }
                }
            }

            if (e.Button == _activationMouseButton && _activationKeyboardPressed)
            {
                StartSelectionOverlay(); // 👉 進入選取模式
                e.Handled = true;
            }
        }

        private void GlobalHook_MouseUpExt(object sender, MouseEventExtArgs e)
        {
        }

        private void SafeStatus(string msg)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => SafeStatus(msg))); return; }

            // 你已有 lblStatus 在用（監聽中/擷取中），這裡直接沿用
            lblStatus.Text = msg;
        }

        private void StartSelectionOverlay()
        {
            if (isSelecting) return;
            isSelecting = true;

            var selector = new SelectionForm();
            bool completed = false;

            selector.OnSelectionCompleted += async (img, region) =>
            {
                completed = true;
                lastCapturedRegion = region;

                try
                {
                    await HandleCapturedImage(img);   // ✅ 等翻譯/流程跑完
                }
                catch (Exception ex)
                {
                    // 避免 async void 事件吃掉例外
                    SafeStatus($"處理失敗：{ex.Message}");
                }
                finally
                {
                    isSelecting = false;              // ✅ 只在流程完成後才解鎖
                }
            };

            selector.FormClosed += (s, e) =>
            {
                // 只有「使用者取消/沒完成選取」才在這裡解鎖
                if (!completed) isSelecting = false;
            };

            selector.Show();
        }

        private void CloseOverlay()
        {
            // 如果 Form1 都快被關了，就不要做事
            if (this.IsDisposed) return;

            // 全域 hook 可能不在 UI thread：一律轉回 UI thread 做 Close/Dispose
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(CloseOverlay)); }
                catch { /* 可能正在關閉程式，忽略 */ }
                return;
            }

            // 用 local snapshot，避免 _overlay 在中途被別的地方設成 null
            var ov = _overlay;
            if (ov == null) return;

            // 先把欄位清掉，避免 re-entrancy（例如 Close() 觸發 FormClosed 又來 CloseOverlay）
            _overlay = null;

            try
            {
                if (!ov.IsDisposed)
                {
                    ov.Close();   // 讓它正常走關閉流程
                }
            }
            catch
            {
                // close 途中被關/被 dispose 都可能，吞掉即可
            }
            finally
            {
                try { ov.Dispose(); } catch { }
            }
        }

        private void UpdateTokensUi(GeminiUsage usage)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateTokensUi(usage)));
                return;
            }

            if (lblTokens == null) return;

            if (usage == null)
            {
                lblTokens.Text = "Tokens: -";
                return;
            }

            string p = usage.PromptTokenCount?.ToString() ?? "-";
            string c = usage.CandidatesTokenCount?.ToString() ?? "-";
            string t = usage.TotalTokenCount?.ToString() ?? "-";

            string th = usage.ThoughtsTokenCount?.ToString();
            string tool = usage.ToolUsePromptTokenCount?.ToString();
            string cache = usage.CachedContentTokenCount?.ToString();

            var extra = new List<string>();
            if (!string.IsNullOrEmpty(th)) extra.Add($"thoughts={th}");
            if (!string.IsNullOrEmpty(tool)) extra.Add($"tool={tool}");
            if (!string.IsNullOrEmpty(cache)) extra.Add($"cache={cache}");

            lblTokens.Text = extra.Count == 0
                ? $"Tokens: prompt={p}, out={c}, total={t}"
                : $"Tokens: prompt={p}, out={c}, {string.Join(", ", extra)}, total={t}";
        }

        private void DrawTranslatedText(string translated, Rectangle region)
        {
            if (string.IsNullOrWhiteSpace(translated)) return;

            CloseOverlay(); // ✅ 統一用安全版

            int durationMs = GetOverlayDurationMs();
            _overlay = new TranslationOverlayForm(translated, region, durationMs);

            // 可選但建議：overlay 自己關閉後，把引用清掉，避免你後面判斷 _overlay 時卡在已 disposed 的物件
            _overlay.FormClosed += (s, e) =>
            {
                if (ReferenceEquals(_overlay, s)) _overlay = null;
            };

            _overlay.Show();
        }

        private int GetOverlayDurationMs()
        {
            int sec = 10;
            try { sec = (int)numOverlaySeconds.Value; } catch { }
            sec = Math.Max(1, sec);
            return sec * 1000;
        }

        private class TranslationOverlayForm : Form
        {
            private readonly string _text;
            private Font _font;

            private readonly int _padding = 12;
            private readonly int _durationMs; // 幾秒後自動消失（可調）
            private Timer _closeTimer;

            private const int WS_EX_TRANSPARENT = 0x20;
            private const int WS_EX_NOACTIVATE = 0x08000000;

            public TranslationOverlayForm(string text, Rectangle region, int durationMs)
            {
                _text = text ?? "";
                _durationMs = Math.Max(200, durationMs); // 200ms 做個保護下限，避免有人設到 0

                StartPosition = FormStartPosition.Manual;
                Bounds = region;

                FormBorderStyle = FormBorderStyle.None;
                TopMost = true;
                ShowInTaskbar = false;

                BackColor = Color.Black;
                Opacity = 0.85;

                // 視窗建立後再算字型（此時 ClientSize 才準）
                Shown += (s, e) =>
                {
                    UpdateFontToFit();
                    StartAutoCloseTimer();
                    Invalidate();
                };
            }

            // 更保險：不搶焦點
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            private void StartAutoCloseTimer()
            {
                _closeTimer?.Stop();
                _closeTimer?.Dispose();

                _closeTimer = new Timer();
                _closeTimer.Interval = _durationMs;
                _closeTimer.Tick += (s, e) =>
                {
                    _closeTimer.Stop();
                    Close();
                };
                _closeTimer.Start();
            }

            private void UpdateFontToFit()
            {
                // 你可改字型：日文/中文建議微軟正黑體 / Meiryo / Yu Gothic
                string fontName = "Microsoft JhengHei";
                var style = FontStyle.Bold;

                var available = new Size(
                    Math.Max(1, ClientSize.Width - _padding * 2),
                    Math.Max(1, ClientSize.Height - _padding * 2)
                );

                float best = FindBestFontSize(_text, fontName, style, available, min: 6f, max: 128f);

                _font?.Dispose();
                _font = new Font(fontName, best, style, GraphicsUnit.Point);
            }

            private static float FindBestFontSize(string text, string fontName, FontStyle style, Size area, float min, float max)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return min;

                // 用 1pt 精度即可（要更細可改 0.5）
                float low = min, high = max, best = min;

                var flags = TextFormatFlags.WordBreak
                          | TextFormatFlags.NoPadding
                          | TextFormatFlags.TextBoxControl;

                for (int i = 0; i < 12; i++) // 12 次二分搜尋很夠
                {
                    float mid = (low + high) / 2f;

                    using (var f = new Font(fontName, mid, style, GraphicsUnit.Point))
                    {
                        // MeasureText 會依 area 寬度自動換行，回傳需要的高度
                        Size measured = TextRenderer.MeasureText(text, f, area, flags);

                        bool fits = measured.Width <= area.Width && measured.Height <= area.Height;

                        if (fits)
                        {
                            best = mid;
                            low = mid;
                        }
                        else
                        {
                            high = mid;
                        }
                    }
                }

                return Math.Max(min, best);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (_font == null) return;

                var rect = Rectangle.Inflate(ClientRectangle, -_padding, -_padding);

                var flags = TextFormatFlags.HorizontalCenter
                          | TextFormatFlags.VerticalCenter
                          | TextFormatFlags.WordBreak
                          | TextFormatFlags.NoPadding
                          | TextFormatFlags.TextBoxControl;

                // 用 TextRenderer 畫字，跟 MeasureText 一致（避免你遇到「算得出來但畫不出來」的落差）
                TextRenderer.DrawText(e.Graphics, _text, _font, rect, Color.DeepSkyBlue, flags);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _font?.Dispose();
                    _closeTimer?.Stop();
                    _closeTimer?.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private class CursorHintForm : Form
        {
            private readonly Label _label;

            public CursorHintForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                StartPosition = FormStartPosition.Manual;
                AutoSize = true;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                Padding = new Padding(0);

                _label = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.IndianRed,
                    Font = new Font("Microsoft JhengHei", 10f, FontStyle.Bold),
                    Padding = new Padding(10, 6, 10, 6)
                };

                Controls.Add(_label);
            }

            protected override bool ShowWithoutActivation => true;

            public void UpdateMessage(string text, Color backColor, Point cursorPos)
            {
                _label.Text = text;
                _label.BackColor = backColor;
                Location = new Point(cursorPos.X + 14, cursorPos.Y + 14);
            }
        }

        private class QuotaBoardForm : Form
        {
            private readonly DataGridView _grid;
            private readonly Label _title;
            private readonly Button _btnRefresh;

            public QuotaBoardForm()
            {
                Text = "今日各引擎使用量";
                Width = 980;
                Height = 520;
                StartPosition = FormStartPosition.CenterParent;

                _title = new Label
                {
                    AutoSize = true,
                    Location = new Point(12, 12),
                    Text = "今日各引擎使用量"
                };

                _btnRefresh = new Button
                {
                    Text = "重新整理",
                    Width = 90,
                    Height = 24,
                    Location = new Point(860, 8)
                };
                _btnRefresh.Click += (s, e) => { /* 由外部按鈕重開即可刷新 */ };

                _grid = new DataGridView
                {
                    Location = new Point(12, 40),
                    Width = 940,
                    Height = 430,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                _grid.Columns.Add("Provider", "引擎");
                _grid.Columns.Add("Model", "模型");
                _grid.Columns.Add("UsedSuccessRequests", "已用(成功)");
                _grid.Columns.Add("UsedFailedRequests", "失敗");
                _grid.Columns.Add("UsedTotalTokens", "消耗Tokens");
                _grid.Columns.Add("DailyLimit", "上限");
                _grid.Columns.Add("RpmLimit", "RPM");
                _grid.Columns.Add("LastError", "最後錯誤");

                Controls.Add(_title);
                Controls.Add(_btnRefresh);
                Controls.Add(_grid);
            }

            public void UpdateRows(List<DailyQuotaEntry> rows, DateTime now)
            {
                _title.Text = $"今日各引擎使用量（{now:yyyy-MM-dd}）";
                _grid.Rows.Clear();

                foreach (var r in rows)
                {
                    _grid.Rows.Add(
                        r.Provider,
                        r.Model,
                        r.UsedSuccessRequests,
                        r.UsedFailedRequests,
                        r.UsedTotalTokens,
                        r.DailyLimit <= 0 ? "-" : r.DailyLimit.ToString(),
                        r.RpmLimit.HasValue ? r.RpmLimit.Value.ToString() : "-",
                        string.IsNullOrWhiteSpace(r.LastError) ? "-" : r.LastError
                    );
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = e.Link.LinkData as string ?? e.Link.ToString();
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                // 使用 ProcessStartInfo 確保以預設瀏覽器開啟
                var psi = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("無法開啟網址: " + ex.Message);
            }
        }
    }
}
