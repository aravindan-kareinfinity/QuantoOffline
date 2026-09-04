using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportLibrary
{
    public class ThermalImage
    {
        string text;
        int height;
        int width;
        System.Drawing.StringAlignment stringAlignment;
        public ThermalImage(string text,int height,int width,
            System.Drawing.StringAlignment stringAlignment)
        {
            this.text = text;
            this.height = height;
            this.width = width;
            this.stringAlignment = stringAlignment;
        }

        public byte[] Draw(string fontname,int fontsize, FontStyle fontStyle)
        {
            byte[] returnvalue = null;
            using (var io = new System.IO.MemoryStream())
            {
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width,height))
                {
                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphic.DrawString(text, new System.Drawing.Font(fontname, fontsize, fontStyle),
                            new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                            new System.Drawing.RectangleF(0, 0, bitmap.Width, bitmap.Height),
                            new System.Drawing.StringFormat()
                            {
                                 Alignment = stringAlignment,
                                 LineAlignment = StringAlignment.Center
                            });
                    }
                    bitmap.Save(io, System.Drawing.Imaging.ImageFormat.Png);
                    returnvalue = io.ToArray();
                }
            }
            return returnvalue;
        }
    }
}
