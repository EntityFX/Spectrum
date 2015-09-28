namespace EntityFX.Spectrum.UI
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
            this.spectrumVideoOutputPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.spectrumVideoOutputPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // spectrumVideoOutputPictureBox
            // 
            this.spectrumVideoOutputPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("spectrumVideoOutputPictureBox.Image")));
            this.spectrumVideoOutputPictureBox.Location = new System.Drawing.Point(16, 12);
            this.spectrumVideoOutputPictureBox.Name = "spectrumVideoOutputPictureBox";
            this.spectrumVideoOutputPictureBox.Size = new System.Drawing.Size(256, 192);
            this.spectrumVideoOutputPictureBox.TabIndex = 0;
            this.spectrumVideoOutputPictureBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.spectrumVideoOutputPictureBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.spectrumVideoOutputPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox spectrumVideoOutputPictureBox;
    }
}

