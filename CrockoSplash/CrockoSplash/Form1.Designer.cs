using System.Drawing;
using System.Windows.Forms;

namespace CrockoSplash
{
    partial class CrockoSplashSplash
    {
        #region IMGS
        Bitmap _logoImg;
        #endregion

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
            _logoImg = new Bitmap("..\\..\\logo.png");
            this.SuspendLayout();
            // 
            // CrockoSplashSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CrockoSplashSplash";
            this.Opacity = 1;// 0.5D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CrockoSplash";
            this.ResumeLayout(false);
        }

        #endregion

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        //https://stackoverflow.com/questions/38265506/draw-a-rotated-image-directly-to-graphics-object-without-creating-a-new-bitmap

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            System.Drawing.Graphics formGraphics = e.Graphics;
            //formGraphics.FillEllipse(myBrush, new Rectangle(0, 0, 400, 300));

            /*string drawString = "Sample Text";
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            float x = 150.0F;
            float y = 50.0F;
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            formGraphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);*/

            myBrush.Dispose();
            formGraphics.DrawImage(_logoImg, 10, 10);
            formGraphics.Dispose();
        }

    }
}

