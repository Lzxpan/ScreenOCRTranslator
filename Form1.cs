using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

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
        private bool isQPressed = false;
        private bool isLeftMouseDown = false;
        private bool isSelecting = false;
        private Rectangle lastCapturedRegion;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbModel.SelectedIndex = 5;
            cmbLanguage.SelectedIndex = 3;
            cmbTranslationMode.SelectedIndex = 0; // 預設 OCR 模式
            // 載入儲存的 API Key 和模型
            txtApiKey.Text = Properties.Settings.Default.ApiKey;
            string savedModel = Properties.Settings.Default.ModelName;           
            if (!string.IsNullOrEmpty(savedModel))
            {
                int index = cmbModel.Items.IndexOf(savedModel);
                if (index >= 0)
                    cmbModel.SelectedIndex = index;
            }
            cmbTranslationMode.SelectedIndex = Properties.Settings.Default.TranslationModeIndex;
            cmbLanguage.SelectedIndex = Properties.Settings.Default.LanguageModeIndex;

            monitorTimer = new System.Windows.Forms.Timer();
            monitorTimer.Interval = 200; // 每 200ms 檢查一次滑鼠
            monitorTimer.Tick += MonitorTimer_Tick;
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.KeyUp += GlobalHook_KeyUp;
            globalHook.MouseDownExt += GlobalHook_MouseDownExt;
            globalHook.MouseUpExt += GlobalHook_MouseUpExt;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ApiKey = txtApiKey.Text.Trim();
            Properties.Settings.Default.ModelName = cmbModel.SelectedItem?.ToString();
            Properties.Settings.Default.TranslationModeIndex = cmbTranslationMode.SelectedIndex;
            Properties.Settings.Default.LanguageModeIndex = cmbLanguage.SelectedIndex;
            Properties.Settings.Default.Save(); // 寫入設定
            globalHook?.Dispose();
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
                // 將圖片放大 4 倍
                //Bitmap scaled = new Bitmap(image, image.Width * 4, image.Height * 4);

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
                using (var engine = new TesseractEngine(@"./tessdata", _language, EngineMode.TesseractOnly))
                {
                    using (var page = engine.Process(binary))
                    {
                        return page.GetText();
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

        private async Task HandleCapturedImage(Bitmap captured)
        {
            picturePreview.Image = captured;

            string apiKey = txtApiKey.Text.Trim();
            string modelName = cmbModel.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(modelName))
            {
                geminiClient = new GeminiClient(apiKey, modelName);
            }
            else
            {
                MessageBox.Show("請輸入 API Key 並選擇模型");
                return;
            }

            string translated = null;

            int selectedMode = cmbTranslationMode.SelectedIndex; // 翻譯模式切換 (ComboBox)

            if (selectedMode == 0) // OCR 模式
            {
                txtResult.Text = "辨識中...";
                string langCode = GetSelectedLanguageCode();
                var ocr = new TesseractOcrProcessor(langCode, picturePreview);
                string text = ocr.PerformOCR(captured);
                txtResult.Text = text;

                if (geminiClient == null)
                {
                    MessageBox.Show("GeminiClient 未初始化！");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    txtResult.AppendText("\r\n\r\n翻譯中...\r\n");

                    string prompt = $"請將以下內容翻譯成繁體中文（只輸出翻譯結果）：\n\n{text}";

                    try
                    {
                        translated = await geminiClient.TranslateText(prompt);
                        txtResult.AppendText($"\r\n\r\n翻譯結果：\r\n{translated}");
                    }
                    catch (Exception ex)
                    {
                        txtResult.AppendText($"\r\n\r\n翻譯失敗: {ex.Message}");
                    }
                }
            }
            else if (selectedMode == 1) // AI 直接圖像翻譯
            {

                txtResult.Text = "AI 圖像翻譯中...";

                try
                {
                    translated = await geminiClient.SendImageForOCRAndTranslate(captured);
                    txtResult.AppendText($"\r\n\r\n翻譯結果：\r\n{translated}");
                }
                catch (Exception ex)
                {
                    txtResult.AppendText($"\r\n\r\nAI 圖像翻譯失敗：{ex.Message}");
                }
            }

            // 顯示翻譯結果到畫面上（畫上去）
            if (!string.IsNullOrWhiteSpace(translated))
            {
                DrawTranslatedText(translated, lastCapturedRegion);
            }
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
                isQPressed = true;
        }

        private void GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
                isQPressed = false;
        }

        private void GlobalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left && isQPressed)
            {
                isLeftMouseDown = true;
                StartSelectionOverlay(); // 👉 進入選取模式
                e.Handled = true;
            }
        }

        private void GlobalHook_MouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseDown = false;
            }
        }

        private void StartSelectionOverlay()
        {
            if (isSelecting) return;
            isSelecting = true;

            var selector = new SelectionForm();
            selector.OnSelectionCompleted += (img, region) =>
            {
                lastCapturedRegion = region; // ✅ 設定選取區域
                HandleCapturedImage(img);
                isSelecting = false; // 解鎖
            };
            selector.FormClosed += (s, e) => isSelecting = false; // 雙保險
            selector.Show();
        }

        private void DrawTranslatedText(string translated, Rectangle region)
        {
            if (string.IsNullOrWhiteSpace(translated)) return;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                Font font = new Font("Microsoft JhengHei", 12, FontStyle.Bold);
                Brush brush = Brushes.DeepSkyBlue;
                Brush bgBrush = new SolidBrush(Color.FromArgb(180, Color.Black));

                SizeF textSize = g.MeasureString(translated, font);

                float x = region.X + (region.Width - textSize.Width) / 2;
                float y = region.Y + (region.Height - textSize.Height) / 2;

                g.FillRectangle(bgBrush, x - 10, y - 10, textSize.Width + 20, textSize.Height + 20);
                g.DrawString(translated, font, brush, new PointF(x, y));
            }
        }

    }
}


