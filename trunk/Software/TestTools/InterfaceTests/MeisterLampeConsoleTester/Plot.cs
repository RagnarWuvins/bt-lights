using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MeisterLampeConsoleTester
{
    class Plot
    {
        // public properties
        public Bitmap Axes;
        public int Width
        {
            get { return Axes.Width; }
            set
            {
                Bitmap b = new Bitmap(value, this.Height);
                Graphics g = Graphics.FromImage((Image)b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(Axes, 0, 0, value, this.Height);
                g.Dispose();
            }
        }
        public int Height
        {
            get { return Axes.Height; }
            set
            {
                Bitmap b = new Bitmap(this.Width, value);
                Graphics g = Graphics.FromImage((Image)b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(Axes, 0, 0, this.Width, value);
                g.Dispose();
            }
        }
        // Private properties
        private Graphics mGraphics;
        private Form mForm = new Form();
        private PictureBox mPicBox;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Plot(int width, int height)
        {
            Axes = new Bitmap(width, height);
            mGraphics = Graphics.FromImage(Axes);
            Rectangle screenRectangle = mForm.RectangleToScreen(mForm.ClientRectangle);
            int titleHeight = screenRectangle.Top - mForm.Top;

            mForm.Size = new System.Drawing.Size(this.Width, this.Height + titleHeight);
            mForm.Text = "Image Viewer";
            mPicBox = new PictureBox();
            mPicBox.Image = Axes;
            mPicBox.Dock = DockStyle.Fill;

            mForm.Controls.Add(mPicBox);
            mForm.Show();
        }

        public void Init()
        {
            Axes = new Bitmap(this.Width, this.Height);
            mGraphics = Graphics.FromImage(Axes);
        }

        public void AddLine(int[] line)
        {
            AddLine(line, Pens.Black);
        }

        public void AddLine(int[] line, Pen pen)
        {            
            int step = this.Width / (line.Length);
            Font f = new Font(FontFamily.GenericSansSerif, 10);

            for (int i = 0; i < line.Length; i++)
            {
                if (i < line.Length - 1)
                {                    
                    mGraphics.DrawLine(pen, i * step, line[i], (i + 1) * step, line[i + 1]);
                }
                mGraphics.DrawString(String.Format("{0}", line[i]), f, Brushes.Black, i * step, line[i]);
            }
            mPicBox.Invalidate();
            mForm.Invalidate();
        }
    }
}
