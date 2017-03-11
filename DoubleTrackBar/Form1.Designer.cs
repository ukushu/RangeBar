namespace DoubleTrackBar
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
            this.zzzzRangeBarOriginal1 = new Zzzz.ZzzzRangeBarOriginal();
            this.uksRangeBar1 = new DoubleTrackBar.UksRangeBar();
            this.SuspendLayout();
            // 
            // zzzzRangeBarOriginal1
            // 
            this.zzzzRangeBarOriginal1.DivisionNum = 10;
            this.zzzzRangeBarOriginal1.HeightOfBar = 8;
            this.zzzzRangeBarOriginal1.HeightOfMark = 24;
            this.zzzzRangeBarOriginal1.HeightOfTick = 6;
            this.zzzzRangeBarOriginal1.InnerColor = System.Drawing.Color.LightGreen;
            this.zzzzRangeBarOriginal1.Location = new System.Drawing.Point(13, 13);
            this.zzzzRangeBarOriginal1.Name = "zzzzRangeBarOriginal1";
            this.zzzzRangeBarOriginal1.Orientation = Zzzz.ZzzzRangeBarOriginal.RangeBarOrientation.horizontal;
            this.zzzzRangeBarOriginal1.RangeMaximum = 5;
            this.zzzzRangeBarOriginal1.RangeMinimum = 3;
            this.zzzzRangeBarOriginal1.ScaleOrientation = Zzzz.ZzzzRangeBarOriginal.TopBottomOrientation.bottom;
            this.zzzzRangeBarOriginal1.Size = new System.Drawing.Size(688, 64);
            this.zzzzRangeBarOriginal1.TabIndex = 0;
            this.zzzzRangeBarOriginal1.TotalMaximum = 10;
            this.zzzzRangeBarOriginal1.TotalMinimum = 0;
            // 
            // uksRangeBar1
            // 
            this.uksRangeBar1.DivisionNum = 10;
            this.uksRangeBar1.HeightOfBar = 8;
            this.uksRangeBar1.HeightOfMark = 24;
            this.uksRangeBar1.HeightOfTick = 6;
            this.uksRangeBar1.InnerColor = System.Drawing.Color.LightGreen;
            this.uksRangeBar1.Location = new System.Drawing.Point(12, 83);
            this.uksRangeBar1.Name = "uksRangeBar1";
            this.uksRangeBar1.Orientation = DoubleTrackBar.UksRangeBar.RangeBarOrientation.Horizontal;
            this.uksRangeBar1.RangeMaximum = 5;
            this.uksRangeBar1.RangeMinimum = 3;
            this.uksRangeBar1.ScaleOrientation = DoubleTrackBar.UksRangeBar.TopBottomOrientation.Bottom;
            this.uksRangeBar1.Size = new System.Drawing.Size(689, 64);
            this.uksRangeBar1.TabIndex = 1;
            this.uksRangeBar1.TotalMaximum = 10;
            this.uksRangeBar1.TotalMinimum = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 172);
            this.Controls.Add(this.uksRangeBar1);
            this.Controls.Add(this.zzzzRangeBarOriginal1);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        private Zzzz.ZzzzRangeBarOriginal zzzzRangeBarOriginal1;
        private UksRangeBar uksRangeBar1;

    }
}

