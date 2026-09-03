using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public static class PosExt
    {
        public static void Enlarged(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)32);
            bw.Write(text);
            bw.Write(AsciiControlChars.Newline);
        }
        public static void High(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)16);
            bw.Write(text); //Width,enlarged
            bw.Write(AsciiControlChars.Newline);
        }
        public static void LargeText(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)48);
            bw.Write(text);
            bw.Write(AsciiControlChars.Newline);
        }
        public static void CenterText(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)'a');
            bw.Write((char)1);
            bw.Write(text);
            bw.Write(AsciiControlChars.Newline);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)'a');
            bw.Write((char)0);
        }
        public static void FeedLines(this BinaryWriter bw, int lines)
        {
            bw.Write(AsciiControlChars.Newline);
            if (lines > 0)
            {
                bw.Write(AsciiControlChars.Escape);
                bw.Write('d');
                bw.Write((byte)lines - 1);
            }
        }

        public static void Barcode(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        {
            // with out barcode text printing on top
            // string barcodetext = "\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Length + 12) + "{A" + text + "{B" + "12345678";
            
            //string barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Length + 12) + "{A" + text + "{B" + "12345678";

            string barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Trim().Length + 8) + "{B" + "PHK/" + "{A" + text.Trim();
            CenterText(bw, barcodetext);
        }
        public static void Finish(this BinaryWriter bw)
        {
            bw.FeedLines(1);
            //bw.NormalFont("---  Thank You, Come Again ---");
            bw.FeedLines(1);
            bw.Write(AsciiControlChars.Newline);
        }

        private static int Len(string v)
        {
            return v.Length;
        }

        private static string Mid(string v1, int v2, int num10)
        {
            return v1.Substring(v2-1, num10-1);
        }

        public static void NormalFont(this BinaryWriter bw, string text, bool line = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)8);
            bw.Write(" " + text);
            if (line)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void PicaText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void CondensedText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)4);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void ItalicText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)100);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void BoldText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)47);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void UnderlineText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)141);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void DoubleHeightText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)16);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void DoubleWidthText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)32);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void EmphasizeText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)141);
            bw.Write(text);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void WriteLogo(this BinaryWriter bw)
        {
            string logo = "";
            if (!File.Exists(@"C:\bitmap.bmp"))
                return;
            BitmapData data = GetBitmapData(@"C:\bitmap.bmp");
            System.Collections.BitArray dots = data.Dots;
            byte[] width = BitConverter.GetBytes(data.Width);

            int offset = 0;
            //MemoryStream stream = new MemoryStream();
            //BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((char)0x1B);
            bw.Write('@');

            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)24);

            while (offset < data.Height)
            {
                bw.Write((char)0x1B);
                bw.Write('*');         // bit-image mode
                bw.Write((byte)33);    // 24-dot double-density
                bw.Write(width[0]);  // width low byte
                bw.Write(width[1]);  // width high byte

                for (int x = 0; x < data.Width; ++x)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; ++b)
                        {
                            int y = (((offset / 8) + k) * 8) + b;
                            // Calculate the location of the pixel we want in the bit array.
                            // It'll be at (y * width) + x.
                            int i = (y * data.Width) + x;

                            // If the image is shorter than 24 dots, pad with zero.
                            bool v = false;
                            if (i < dots.Length)
                            {
                                v = dots[i];
                            }
                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }

                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write((char)0x0A);
            }
            // Restore the line spacing to the default of 30 dots.
            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)30);

            //bw.Flush();
            //byte[] bytes = stream.ToArray();
            //return logo + Encoding.Default.GetString(bytes);
        }

        public static BitmapData GetBitmapData(string bmpFileName)
        {
            using (var bitmap = (Bitmap)Bitmap.FromFile(bmpFileName))
            {
                var threshold = 127;
                var index = 0;
                double multiplier = 570; // this depends on your printer model. for Beiyang you should use 1000
                double scale = (double)(multiplier / (double)bitmap.Width);
                int xheight = (int)(bitmap.Height * scale);
                int xwidth = (int)(bitmap.Width * scale);
                var dimensions = xwidth * xheight;
                var dots = new BitArray(dimensions);

                for (var y = 0; y < xheight; y++)
                {
                    for (var x = 0; x < xwidth; x++)
                    {
                        var _x = (int)(x / scale);
                        var _y = (int)(y / scale);
                        var color = bitmap.GetPixel(_x, _y);
                        var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        dots[index] = (luminance < threshold);
                        index++;
                    }
                }

                return new BitmapData()
                {
                    Dots = dots,
                    Height = (int)(bitmap.Height * scale),
                    Width = (int)(bitmap.Width * scale)
                };
            }
        }

        public class BitmapData
        {
            public BitArray Dots
            {
                get;
                set;
            }

            public int Height
            {
                get;
                set;
            }

            public int Width
            {
                get;
                set;
            }
        }
        
    }
}
