namespace ScreenOCRTranslator
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.lblApiKey = new System.Windows.Forms.Label();
            this.lblModel = new System.Windows.Forms.Label();
            this.cmbModel = new System.Windows.Forms.ComboBox();
            this.btnCapture = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.numIdleSeconds = new System.Windows.Forms.NumericUpDown();
            this.picturePreview = new System.Windows.Forms.PictureBox();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.cmbTranslationMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numOverlaySeconds = new System.Windows.Forms.NumericUpDown();
            this.lblTokens = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numIdleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOverlaySeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(71, 155);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(216, 22);
            this.txtApiKey.TabIndex = 9;
            this.txtApiKey.TabStop = false;
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(8, 158);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(57, 12);
            this.lblApiKey.TabIndex = 1;
            this.lblApiKey.Text = "API Key：";
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(24, 188);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(41, 12);
            this.lblModel.TabIndex = 2;
            this.lblModel.Text = "模型：";
            // 
            // cmbModel
            // 
            this.cmbModel.FormattingEnabled = true;
            this.cmbModel.Items.AddRange(new object[] {
            "gemini-2.5-flash-lite",
            "gemini-2.5-flash",
            "gemini-2.5-pro",
            "gemini-3-flash-preview",
            "gemini-3-pro-preview"});
            this.cmbModel.Location = new System.Drawing.Point(71, 185);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(143, 20);
            this.cmbModel.TabIndex = 3;
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new System.Drawing.Point(10, 211);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(75, 23);
            this.btnCapture.TabIndex = 4;
            this.btnCapture.Text = "擷取 + 翻譯";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Visible = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // txtResult
            // 
            this.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResult.Location = new System.Drawing.Point(10, 240);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.txtResult.Size = new System.Drawing.Size(355, 201);
            this.txtResult.TabIndex = 0;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(10, 7);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 6;
            this.btnStartStop.Text = "啟動";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Visible = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(91, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 12);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "已停止";
            this.lblStatus.Visible = false;
            // 
            // numIdleSeconds
            // 
            this.numIdleSeconds.DecimalPlaces = 1;
            this.numIdleSeconds.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numIdleSeconds.Location = new System.Drawing.Point(10, 36);
            this.numIdleSeconds.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numIdleSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numIdleSeconds.Name = "numIdleSeconds";
            this.numIdleSeconds.Size = new System.Drawing.Size(75, 22);
            this.numIdleSeconds.TabIndex = 8;
            this.numIdleSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numIdleSeconds.Value = new decimal(new int[] {
            15,
            0,
            0,
            65536});
            this.numIdleSeconds.Visible = false;
            // 
            // picturePreview
            // 
            this.picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picturePreview.Location = new System.Drawing.Point(371, 240);
            this.picturePreview.Name = "picturePreview";
            this.picturePreview.Size = new System.Drawing.Size(545, 201);
            this.picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picturePreview.TabIndex = 9;
            this.picturePreview.TabStop = false;
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "繁體中文",
            "簡體中文",
            "日文",
            "英文"});
            this.cmbLanguage.Location = new System.Drawing.Point(10, 94);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(121, 20);
            this.cmbLanguage.TabIndex = 10;
            // 
            // cmbTranslationMode
            // 
            this.cmbTranslationMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTranslationMode.FormattingEnabled = true;
            this.cmbTranslationMode.Items.AddRange(new object[] {
            "OCR 模式（省 token）",
            "AI 圖像翻譯（高精確）"});
            this.cmbTranslationMode.Location = new System.Drawing.Point(10, 120);
            this.cmbTranslationMode.Name = "cmbTranslationMode";
            this.cmbTranslationMode.Size = new System.Drawing.Size(168, 20);
            this.cmbTranslationMode.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(137, 97);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "OCR辨識語系，使用AI路徑時請無視。";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "翻譯路徑選擇，AI路經無視語言，一率翻成繁中。";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(211, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "按著Q+滑鼠左鍵，即可啟動框選擷取。";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(145, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 12);
            this.label4.TabIndex = 15;
            this.label4.Text = "翻譯覆蓋顯示(秒)：";
            // 
            // numOverlaySeconds
            // 
            this.numOverlaySeconds.Location = new System.Drawing.Point(260, 8);
            this.numOverlaySeconds.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numOverlaySeconds.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numOverlaySeconds.Name = "numOverlaySeconds";
            this.numOverlaySeconds.Size = new System.Drawing.Size(53, 22);
            this.numOverlaySeconds.TabIndex = 16;
            this.numOverlaySeconds.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // lblTokens
            // 
            this.lblTokens.AutoSize = true;
            this.lblTokens.Location = new System.Drawing.Point(91, 216);
            this.lblTokens.Name = "lblTokens";
            this.lblTokens.Size = new System.Drawing.Size(73, 12);
            this.lblTokens.TabIndex = 17;
            this.lblTokens.Text = "消耗Tokens: -";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.linkLabel1.Location = new System.Drawing.Point(317, 158);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(172, 16);
            this.linkLabel1.TabIndex = 18;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Tag = "";
            this.linkLabel1.Text = "前往取得Gemini API key";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(145, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(281, 12);
            this.label5.TabIndex = 19;
            this.label5.Text = "文在文字顯示區域按下滑鼠右鍵可立即關閉顯示文字";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 450);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.lblTokens);
            this.Controls.Add(this.numOverlaySeconds);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbTranslationMode);
            this.Controls.Add(this.cmbLanguage);
            this.Controls.Add(this.picturePreview);
            this.Controls.Add(this.numIdleSeconds);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.cmbModel);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.lblApiKey);
            this.Controls.Add(this.txtApiKey);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "ScreenOCRTranslator V0.93b";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numIdleSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOverlaySeconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cmbModel;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.NumericUpDown numIdleSeconds;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.ComboBox cmbTranslationMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numOverlaySeconds;
        private System.Windows.Forms.Label lblTokens;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label5;
    }
}

