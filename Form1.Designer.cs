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
            this.lblApiKey_Gemini = new System.Windows.Forms.Label();
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
            this.txtApiKey_MistralPixtral = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbModel_MistralPixtral = new System.Windows.Forms.ComboBox();
            this.linkLabel_MistralPixtral = new System.Windows.Forms.LinkLabel();
            this.lblApiKey_MistralPixtral = new System.Windows.Forms.Label();
            this.txtApiKey_Llama4 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbModel_Llama4 = new System.Windows.Forms.ComboBox();
            this.linkLabel_Llama4 = new System.Windows.Forms.LinkLabel();
            this.lblApiKey_Llama4 = new System.Windows.Forms.Label();
            this.btnQuotaBoard = new System.Windows.Forms.Button();
            this.lblActivationKeyboardKey = new System.Windows.Forms.Label();
            this.cmbActivationKeyboardKey = new System.Windows.Forms.ComboBox();
            this.lblActivationMouseButton = new System.Windows.Forms.Label();
            this.cmbActivationMouseButton = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numIdleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOverlaySeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(130, 146);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(216, 22);
            this.txtApiKey.TabIndex = 9;
            this.txtApiKey.TabStop = false;
            // 
            // lblApiKey_Gemini
            // 
            this.lblApiKey_Gemini.AutoSize = true;
            this.lblApiKey_Gemini.Location = new System.Drawing.Point(30, 152);
            this.lblApiKey_Gemini.Name = "lblApiKey_Gemini";
            this.lblApiKey_Gemini.Size = new System.Drawing.Size(94, 12);
            this.lblApiKey_Gemini.TabIndex = 1;
            this.lblApiKey_Gemini.Text = "Gemini API Key：";
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(352, 152);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(41, 12);
            this.lblModel.TabIndex = 2;
            this.lblModel.Text = "模型：";
            // 
            // cmbModel
            // 
            this.cmbModel.FormattingEnabled = true;
            this.cmbModel.Items.AddRange(new object[] {
            "gemini-3.1-flash-lite",
            "gemini-3-flash-preview",
            "gemini-2.5-flash-lite",
            "gemini-2.5-flash",
            "gemini-2.5-pro"});
            this.cmbModel.Location = new System.Drawing.Point(399, 149);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(143, 20);
            this.cmbModel.TabIndex = 3;
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new System.Drawing.Point(10, 297);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(75, 28);
            this.btnCapture.TabIndex = 4;
            this.btnCapture.Text = "擷取 + 翻譯";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Visible = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // txtResult
            // 
            this.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResult.Location = new System.Drawing.Point(10, 331);
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
            this.picturePreview.Location = new System.Drawing.Point(371, 331);
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
            this.label3.Location = new System.Drawing.Point(405, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(211, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "按著設定鍵，即可啟動框選擷取。";
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
            this.lblTokens.Location = new System.Drawing.Point(91, 307);
            this.lblTokens.Name = "lblTokens";
            this.lblTokens.Size = new System.Drawing.Size(73, 12);
            this.lblTokens.TabIndex = 17;
            this.lblTokens.Text = "消耗Tokens: -";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.linkLabel1.Location = new System.Drawing.Point(548, 149);
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
            // txtApiKey_MistralPixtral
            //
            this.txtApiKey_MistralPixtral.Location = new System.Drawing.Point(130, 174);
            this.txtApiKey_MistralPixtral.Name = "txtApiKey_MistralPixtral";
            this.txtApiKey_MistralPixtral.Size = new System.Drawing.Size(216, 22);
            this.txtApiKey_MistralPixtral.TabIndex = 9;
            this.txtApiKey_MistralPixtral.TabStop = false;
            // 
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(352, 180);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "模型：";
            // 
            // cmbModel_MistralPixtral
            // 
            this.cmbModel_MistralPixtral.FormattingEnabled = true;
            this.cmbModel_MistralPixtral.Items.AddRange(new object[] {
            "mistral-large-2512"});
            this.cmbModel_MistralPixtral.Location = new System.Drawing.Point(399, 177);
            this.cmbModel_MistralPixtral.Name = "cmbModel_MistralPixtral";
            this.cmbModel_MistralPixtral.Size = new System.Drawing.Size(143, 20);
            this.cmbModel_MistralPixtral.TabIndex = 3;
            // 
            // linkLabel_MistralPixtral
            // 
            this.linkLabel_MistralPixtral.AutoSize = true;
            this.linkLabel_MistralPixtral.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.linkLabel_MistralPixtral.Location = new System.Drawing.Point(548, 177);
            this.linkLabel_MistralPixtral.Name = "linkLabel_MistralPixtral";
            this.linkLabel_MistralPixtral.Size = new System.Drawing.Size(172, 16);
            this.linkLabel_MistralPixtral.TabIndex = 18;
            this.linkLabel_MistralPixtral.TabStop = true;
            this.linkLabel_MistralPixtral.Tag = "";
            this.linkLabel_MistralPixtral.Text = "前往取得Mistral API key";
            this.linkLabel_MistralPixtral.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // lblApiKey_MistralPixtral
            //
            this.lblApiKey_MistralPixtral.AutoSize = true;
            this.lblApiKey_MistralPixtral.Location = new System.Drawing.Point(2, 180);
            this.lblApiKey_MistralPixtral.Name = "lblApiKey_MistralPixtral";
            this.lblApiKey_MistralPixtral.Size = new System.Drawing.Size(116, 12);
            this.lblApiKey_MistralPixtral.TabIndex = 20;
            this.lblApiKey_MistralPixtral.Text = "Mistral Vision API Key：";
            // 
            // txtApiKey_Llama4
            //
            this.txtApiKey_Llama4.Location = new System.Drawing.Point(130, 202);
            this.txtApiKey_Llama4.Name = "txtApiKey_Llama4";
            this.txtApiKey_Llama4.Size = new System.Drawing.Size(216, 22);
            this.txtApiKey_Llama4.TabIndex = 9;
            this.txtApiKey_Llama4.TabStop = false;
            // 
            // label9
            //
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(352, 208);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "模型：";
            // 
            // cmbModel_Llama4
            // 
            this.cmbModel_Llama4.FormattingEnabled = true;
            this.cmbModel_Llama4.Items.AddRange(new object[] {
            "meta-llama/llama-4-scout-17b-16e-instruct"});
            this.cmbModel_Llama4.Location = new System.Drawing.Point(399, 205);
            this.cmbModel_Llama4.Name = "cmbModel_Llama4";
            this.cmbModel_Llama4.Size = new System.Drawing.Size(143, 20);
            this.cmbModel_Llama4.TabIndex = 3;
            // 
            // linkLabel_Llama4
            // 
            this.linkLabel_Llama4.AutoSize = true;
            this.linkLabel_Llama4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.linkLabel_Llama4.Location = new System.Drawing.Point(548, 205);
            this.linkLabel_Llama4.Name = "linkLabel_Llama4";
            this.linkLabel_Llama4.Size = new System.Drawing.Size(177, 16);
            this.linkLabel_Llama4.TabIndex = 18;
            this.linkLabel_Llama4.TabStop = true;
            this.linkLabel_Llama4.Tag = "";
            this.linkLabel_Llama4.Text = "前往取得Llama 4 API key";
            this.linkLabel_Llama4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // lblApiKey_Llama4
            //
            this.lblApiKey_Llama4.AutoSize = true;
            this.lblApiKey_Llama4.Location = new System.Drawing.Point(26, 208);
            this.lblApiKey_Llama4.Name = "lblApiKey_Llama4";
            this.lblApiKey_Llama4.Size = new System.Drawing.Size(98, 12);
            this.lblApiKey_Llama4.TabIndex = 20;
            this.lblApiKey_Llama4.Text = "Llama 4 API Key：";
            // 
            // 
            // btnQuotaBoard
            // 
            this.btnQuotaBoard.Location = new System.Drawing.Point(319, 8);
            this.btnQuotaBoard.Name = "btnQuotaBoard";
            this.btnQuotaBoard.Size = new System.Drawing.Size(111, 23);
            this.btnQuotaBoard.TabIndex = 21;
            this.btnQuotaBoard.Text = "今日引擎使用量";
            this.btnQuotaBoard.UseVisualStyleBackColor = true;
            this.btnQuotaBoard.Click += new System.EventHandler(this.btnQuotaBoard_Click);
            //
            // lblActivationKeyboardKey
            //
            this.lblActivationKeyboardKey.AutoSize = true;
            this.lblActivationKeyboardKey.Location = new System.Drawing.Point(8, 70);
            this.lblActivationKeyboardKey.Name = "lblActivationKeyboardKey";
            this.lblActivationKeyboardKey.Size = new System.Drawing.Size(65, 12);
            this.lblActivationKeyboardKey.TabIndex = 22;
            this.lblActivationKeyboardKey.Text = "啟動鍵盤：";
            //
            // cmbActivationKeyboardKey
            //
            this.cmbActivationKeyboardKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbActivationKeyboardKey.FormattingEnabled = true;
            this.cmbActivationKeyboardKey.Location = new System.Drawing.Point(75, 66);
            this.cmbActivationKeyboardKey.Name = "cmbActivationKeyboardKey";
            this.cmbActivationKeyboardKey.Size = new System.Drawing.Size(90, 20);
            this.cmbActivationKeyboardKey.TabIndex = 23;
            //
            // lblActivationMouseButton
            //
            this.lblActivationMouseButton.AutoSize = true;
            this.lblActivationMouseButton.Location = new System.Drawing.Point(175, 70);
            this.lblActivationMouseButton.Name = "lblActivationMouseButton";
            this.lblActivationMouseButton.Size = new System.Drawing.Size(65, 12);
            this.lblActivationMouseButton.TabIndex = 24;
            this.lblActivationMouseButton.Text = "啟動滑鼠：";
            //
            // cmbActivationMouseButton
            //
            this.cmbActivationMouseButton.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbActivationMouseButton.FormattingEnabled = true;
            this.cmbActivationMouseButton.Location = new System.Drawing.Point(242, 66);
            this.cmbActivationMouseButton.Name = "cmbActivationMouseButton";
            this.cmbActivationMouseButton.Size = new System.Drawing.Size(110, 20);
            this.cmbActivationMouseButton.TabIndex = 25;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 544);
            this.Controls.Add(this.cmbActivationMouseButton);
            this.Controls.Add(this.lblActivationMouseButton);
            this.Controls.Add(this.cmbActivationKeyboardKey);
            this.Controls.Add(this.lblActivationKeyboardKey);
            this.Controls.Add(this.btnQuotaBoard);
            this.Controls.Add(this.lblApiKey_Llama4);
            this.Controls.Add(this.lblApiKey_MistralPixtral);
            this.Controls.Add(this.linkLabel_Llama4);
            this.Controls.Add(this.linkLabel_MistralPixtral);
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
            this.Controls.Add(this.cmbModel_Llama4);
            this.Controls.Add(this.cmbModel_MistralPixtral);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbModel);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.txtApiKey_Llama4);
            this.Controls.Add(this.txtApiKey_MistralPixtral);
            this.Controls.Add(this.lblApiKey_Gemini);
            this.Controls.Add(this.txtApiKey);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "ScreenOCRTranslator V01.001";
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
        private System.Windows.Forms.Label lblApiKey_Gemini;
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
        private System.Windows.Forms.TextBox txtApiKey_MistralPixtral;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbModel_MistralPixtral;
        private System.Windows.Forms.LinkLabel linkLabel_MistralPixtral;
        private System.Windows.Forms.Label lblApiKey_MistralPixtral;
        private System.Windows.Forms.TextBox txtApiKey_Llama4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbModel_Llama4;
        private System.Windows.Forms.LinkLabel linkLabel_Llama4;
        private System.Windows.Forms.Label lblApiKey_Llama4;
        private System.Windows.Forms.Button btnQuotaBoard;
        private System.Windows.Forms.Label lblActivationKeyboardKey;
        private System.Windows.Forms.ComboBox cmbActivationKeyboardKey;
        private System.Windows.Forms.Label lblActivationMouseButton;
        private System.Windows.Forms.ComboBox cmbActivationMouseButton;
    }
}

