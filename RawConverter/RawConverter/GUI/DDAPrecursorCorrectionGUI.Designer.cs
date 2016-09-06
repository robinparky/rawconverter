namespace RawConverter.GUI
{
    partial class DDAPrecursorCorrectorGUI
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbExportedSpectrum = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbOutput = new System.Windows.Forms.ListBox();
            this.btPredict = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbExportedSpectrum);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(418, 568);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Exported Spectrum From Xcalibur ";
            // 
            // tbExportedSpectrum
            // 
            this.tbExportedSpectrum.AcceptsReturn = true;
            this.tbExportedSpectrum.AcceptsTab = true;
            this.tbExportedSpectrum.Location = new System.Drawing.Point(6, 19);
            this.tbExportedSpectrum.MaxLength = 65536;
            this.tbExportedSpectrum.Multiline = true;
            this.tbExportedSpectrum.Name = "tbExportedSpectrum";
            this.tbExportedSpectrum.Size = new System.Drawing.Size(406, 537);
            this.tbExportedSpectrum.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbOutput);
            this.groupBox2.Location = new System.Drawing.Point(493, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(418, 568);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Predicted precursor m/z and charge list";
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(6, 19);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(406, 537);
            this.lbOutput.TabIndex = 0;
            // 
            // btPredict
            // 
            this.btPredict.Location = new System.Drawing.Point(436, 233);
            this.btPredict.Name = "btPredict";
            this.btPredict.Size = new System.Drawing.Size(51, 61);
            this.btPredict.TabIndex = 2;
            this.btPredict.Text = "Predict";
            this.btPredict.UseVisualStyleBackColor = true;
            this.btPredict.Click += new System.EventHandler(this.btPredict_Click);
            // 
            // DDAPrecursorCorrectorGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 588);
            this.Controls.Add(this.btPredict);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DDAPrecursorCorrectorGUI";
            this.Text = "DDA Single Spectrum Precursor Corrector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbExportedSpectrum;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lbOutput;
        private System.Windows.Forms.Button btPredict;
    }
}