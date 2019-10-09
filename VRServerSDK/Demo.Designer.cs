namespace VRServerSDK
{
    partial class Demo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.logLabel = new System.Windows.Forms.Label();
            this.serverStatusTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.experienceDataTextBox = new System.Windows.Forms.TextBox();
            this.experienceDataLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(310, 63);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(282, 412);
            this.LogTextBox.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 5000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // logLabel
            // 
            this.logLabel.AutoSize = true;
            this.logLabel.Location = new System.Drawing.Point(308, 48);
            this.logLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(23, 12);
            this.logLabel.TabIndex = 1;
            this.logLabel.Text = "Log";
            // 
            // serverStatusTextBox
            // 
            this.serverStatusTextBox.Location = new System.Drawing.Point(9, 491);
            this.serverStatusTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.serverStatusTextBox.Multiline = true;
            this.serverStatusTextBox.Name = "serverStatusTextBox";
            this.serverStatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.serverStatusTextBox.Size = new System.Drawing.Size(583, 104);
            this.serverStatusTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 476);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server Status";
            // 
            // experienceDataTextBox
            // 
            this.experienceDataTextBox.Location = new System.Drawing.Point(11, 62);
            this.experienceDataTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.experienceDataTextBox.Multiline = true;
            this.experienceDataTextBox.Name = "experienceDataTextBox";
            this.experienceDataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.experienceDataTextBox.Size = new System.Drawing.Size(292, 412);
            this.experienceDataTextBox.TabIndex = 4;
            // 
            // experienceDataLabel
            // 
            this.experienceDataLabel.AutoSize = true;
            this.experienceDataLabel.Location = new System.Drawing.Point(7, 48);
            this.experienceDataLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.experienceDataLabel.Name = "experienceDataLabel";
            this.experienceDataLabel.Size = new System.Drawing.Size(95, 12);
            this.experienceDataLabel.TabIndex = 5;
            this.experienceDataLabel.Text = "Experience Data";
            // 
            // startButton
            // 
            this.startButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startButton.Location = new System.Drawing.Point(9, 16);
            this.startButton.Margin = new System.Windows.Forms.Padding(2);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(582, 30);
            this.startButton.TabIndex = 6;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // Demo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 602);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.experienceDataLabel);
            this.Controls.Add(this.experienceDataTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverStatusTextBox);
            this.Controls.Add(this.logLabel);
            this.Controls.Add(this.LogTextBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Demo";
            this.Text = "Cloud VR Server Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Demo_FormClosing);
            this.Load += new System.EventHandler(this.Demo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2; //AosenUpdate
        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.TextBox serverStatusTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox experienceDataTextBox;
        private System.Windows.Forms.Label experienceDataLabel;
        private System.Windows.Forms.Button startButton;
    }
}

