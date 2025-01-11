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
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // submit
            // 
            this.submit.Location = new System.Drawing.Point(62, 115);
            this.submit.Name = "submit";
            this.submit.Size = new System.Drawing.Size(75, 23);
            this.submit.TabIndex = 0;
            this.submit.Text = "Ok";
            this.submit.UseVisualStyleBackColor = true;
            this.submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // tlsComboBox
            // 
            this.tlsComboBox.FormattingEnabled = true;
            this.tlsComboBox.Items.AddRange(new object[] {
            "TLS1.2",
            "TLS1.3"});
            this.tlsComboBox.Location = new System.Drawing.Point(40, 65);
            this.tlsComboBox.Name = "tlsComboBox";
            this.tlsComboBox.Size = new System.Drawing.Size(120, 21);
            this.tlsComboBox.TabIndex = 1;
            // 
            // tlslabel
            // 
            this.tlslabel.AutoSize = true;
            this.tlslabel.Location = new System.Drawing.Point(68, 49);
            this.tlslabel.Name = "tlslabel";
            this.tlslabel.Size = new System.Drawing.Size(64, 13);
            this.tlslabel.TabIndex = 2;
            this.tlslabel.Text = "TLS version";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(53, 92);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(96, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Enable logging";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // cacheTimeBox
            // 
            this.cacheTimeBox.Location = new System.Drawing.Point(50, 26);
            this.cacheTimeBox.Name = "cacheTimeBox";
            this.cacheTimeBox.Size = new System.Drawing.Size(100, 20);
            this.cacheTimeBox.TabIndex = 4;
            this.cacheTimeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cacheTimeBox_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Cache Time (hours)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 148);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
    }
}

