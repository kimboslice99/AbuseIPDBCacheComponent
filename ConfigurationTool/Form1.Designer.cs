using System.Windows.Forms;

namespace AbuseIPDBCacheComponent_ConfigurationTool
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.submit = new System.Windows.Forms.Button();
            this.tlsComboBox = new System.Windows.Forms.ComboBox();
            this.tlslabel = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.cacheTimeBox = new System.Windows.Forms.TextBox();
            this.labelCacheTimeHours = new System.Windows.Forms.Label();
            this.maxAge = new System.Windows.Forms.TextBox();
            this.minConfidenceScore = new System.Windows.Forms.TextBox();
            this.labelMaxAge = new System.Windows.Forms.Label();
            this.labelMinConfidenceScore = new System.Windows.Forms.Label();
            this.abuseipdbApiKey = new System.Windows.Forms.TextBox();
            this.apikeylabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // submit
            // 
            this.submit.Location = new System.Drawing.Point(89, 177);
            this.submit.Name = "submit";
            this.submit.Size = new System.Drawing.Size(75, 25);
            this.submit.TabIndex = 0;
            this.submit.Text = "Save";
            this.submit.UseVisualStyleBackColor = true;
            this.submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // tlsComboBox
            // 
            this.tlsComboBox.FormattingEnabled = true;
            this.tlsComboBox.Items.AddRange(new object[] {
            "TLS1.2",
            "TLS1.3"});
            this.tlsComboBox.Location = new System.Drawing.Point(127, 33);
            this.tlsComboBox.Name = "tlsComboBox";
            this.tlsComboBox.Size = new System.Drawing.Size(112, 21);
            this.tlsComboBox.TabIndex = 1;
            // 
            // tlslabel
            // 
            this.tlslabel.AutoSize = true;
            this.tlslabel.Location = new System.Drawing.Point(124, 17);
            this.tlslabel.Name = "tlslabel";
            this.tlslabel.Size = new System.Drawing.Size(64, 13);
            this.tlslabel.TabIndex = 2;
            this.tlslabel.Text = "TLS version";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(82, 154);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(96, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Enable logging";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // cacheTimeBox
            // 
            this.cacheTimeBox.Location = new System.Drawing.Point(12, 34);
            this.cacheTimeBox.Name = "cacheTimeBox";
            this.cacheTimeBox.Size = new System.Drawing.Size(96, 20);
            this.cacheTimeBox.TabIndex = 4;
            this.cacheTimeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.digit_KeyPress);
            // 
            // labelCacheTimeHours
            // 
            this.labelCacheTimeHours.AutoSize = true;
            this.labelCacheTimeHours.Location = new System.Drawing.Point(9, 18);
            this.labelCacheTimeHours.Name = "labelCacheTimeHours";
            this.labelCacheTimeHours.Size = new System.Drawing.Size(99, 13);
            this.labelCacheTimeHours.TabIndex = 5;
            this.labelCacheTimeHours.Text = "Cache Time (hours)";
            // 
            // maxAge
            // 
            this.maxAge.Location = new System.Drawing.Point(12, 84);
            this.maxAge.Name = "maxAge";
            this.maxAge.Size = new System.Drawing.Size(96, 20);
            this.maxAge.TabIndex = 6;
            this.maxAge.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.digit_KeyPress);
            // 
            // minConfidenceScore
            // 
            this.minConfidenceScore.Location = new System.Drawing.Point(127, 84);
            this.minConfidenceScore.Name = "minConfidenceScore";
            this.minConfidenceScore.Size = new System.Drawing.Size(112, 20);
            this.minConfidenceScore.TabIndex = 7;
            this.minConfidenceScore.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.digit_KeyPress);
            // 
            // labelMaxAge
            // 
            this.labelMaxAge.AutoSize = true;
            this.labelMaxAge.Location = new System.Drawing.Point(12, 68);
            this.labelMaxAge.Name = "labelMaxAge";
            this.labelMaxAge.Size = new System.Drawing.Size(82, 13);
            this.labelMaxAge.TabIndex = 8;
            this.labelMaxAge.Text = "Max Age (Days)";
            // 
            // labelMinConfidenceScore
            // 
            this.labelMinConfidenceScore.AutoSize = true;
            this.labelMinConfidenceScore.Location = new System.Drawing.Point(124, 68);
            this.labelMinConfidenceScore.Name = "labelMinConfidenceScore";
            this.labelMinConfidenceScore.Size = new System.Drawing.Size(112, 13);
            this.labelMinConfidenceScore.TabIndex = 9;
            this.labelMinConfidenceScore.Text = "Min Confidence Score";
            // 
            // abuseipdbApiKey
            // 
            this.abuseipdbApiKey.Location = new System.Drawing.Point(12, 126);
            this.abuseipdbApiKey.Name = "abuseipdbApiKey";
            this.abuseipdbApiKey.Size = new System.Drawing.Size(224, 20);
            this.abuseipdbApiKey.TabIndex = 10;
            // 
            // apikeylabel
            // 
            this.apikeylabel.AutoSize = true;
            this.apikeylabel.Location = new System.Drawing.Point(12, 110);
            this.apikeylabel.Name = "apikeylabel";
            this.apikeylabel.Size = new System.Drawing.Size(43, 13);
            this.apikeylabel.TabIndex = 11;
            this.apikeylabel.Text = "Api Key";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 212);
            this.Controls.Add(this.apikeylabel);
            this.Controls.Add(this.abuseipdbApiKey);
            this.Controls.Add(this.labelMinConfidenceScore);
            this.Controls.Add(this.labelMaxAge);
            this.Controls.Add(this.minConfidenceScore);
            this.Controls.Add(this.maxAge);
            this.Controls.Add(this.labelCacheTimeHours);
            this.Controls.Add(this.cacheTimeBox);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.tlslabel);
            this.Controls.Add(this.tlsComboBox);
            this.Controls.Add(this.submit);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button submit;
        private System.Windows.Forms.ComboBox tlsComboBox;
        private System.Windows.Forms.Label tlslabel;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox cacheTimeBox;
        private System.Windows.Forms.Label labelCacheTimeHours;
        private TextBox maxAge;
        private TextBox minConfidenceScore;
        private Label labelMaxAge;
        private Label labelMinConfidenceScore;
        private TextBox abuseipdbApiKey;
        private Label apikeylabel;
    }
}

