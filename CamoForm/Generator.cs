using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamoForm {
    class Generator {

        public List<string> GetFileNames() {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".png";
            ofd.Multiselect = true;
            ofd.ShowDialog();
            return ofd.FileNames.ToList<string>();
        }

        public Bitmap[] LoadImages(List<string> fileNames) {
            Bitmap[] imgs = new Bitmap[fileNames.Count];
            for(int i = 0; i < imgs.Length; i++) {
                imgs[i] = (Bitmap) Image.FromFile(fileNames[i], true);
            }

            return imgs;
        }

        public Bitmap Superimpose(Bitmap largeBmp, Bitmap smallBmp, Random rand) {
            using (Graphics g = Graphics.FromImage(largeBmp)) {
                g.CompositingMode = CompositingMode.SourceOver;
                smallBmp = new Bitmap(smallBmp, new Size(smallBmp.Width, smallBmp.Height));
                using (Bitmap rotatedImage = RotateImg(smallBmp, rand.Next(0, 370), Color.Transparent)) {
                    g.DrawImage(rotatedImage, rand.Next(-500, largeBmp.Width + 100), rand.Next(-500, largeBmp.Height + 100));
                }
                smallBmp.Dispose();
            }
            return largeBmp;
        }

        public string TrimFileDirectory(string file) {
            int index = file.IndexOf('\\');
            var lent = file.Length;
            if (index== -1) {
                return file;
            }else {
                return TrimFileDirectory(file.Substring(index +1));
            }
        }

        public Bitmap RotateImg(Bitmap bmp, float angle, Color bkColor) {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;

            PixelFormat pf = default(PixelFormat);
            if (bkColor == Color.Transparent) {
                pf = PixelFormat.Format32bppArgb;
            }
            else {
                pf = bmp.PixelFormat;
            }

            float sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // this function takes radians
            float cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // this one too
            float newImgWidth = sin * bmp.Height + cos * bmp.Width;
            float newImgHeight = sin * bmp.Width + cos * bmp.Height;
            float originX = 0f;
            float originY = 0f;

            if (angle > 0) {
                if (angle <= 90)
                    originX = sin * bmp.Height;
                else {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * bmp.Width;
                }
            }
            else {
                if (angle >= -90)
                    originY = sin * bmp.Width;
                else {
                    originX = newImgWidth - sin * bmp.Height;
                    originY = newImgHeight;
                }
            }

            Bitmap newImg = new Bitmap((int)newImgWidth, (int)newImgHeight, pf);
            using (Graphics g = Graphics.FromImage(newImg)) {
                g.Clear(bkColor);
                g.TranslateTransform(originX, originY); // offset the origin to our calculated values
                g.RotateTransform(angle); // set up rotate
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.DrawImageUnscaled(bmp, 0, 0); // draw the image at 0, 0
                bmp.Dispose();
                return newImg;
            }
            
        }

    }
}
