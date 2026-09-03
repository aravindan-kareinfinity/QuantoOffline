using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportLibrary
{
    public static class PosExt
    {
        public static void Enlarged(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)32);
            bw.Write(text.ToCharArray(), 0, text.Length);
            //bw.Write(AsciiControlChars.Newline);
        }

        public static void Boldenlarged(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)47);
            bw.Write((byte)33);
            bw.Write((byte)32);
            bw.Write(text.ToCharArray(), 0, text.Length);
            //bw.Write(AsciiControlChars.Newline);
        }

        public static void Boldlarge(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)47);
            bw.Write((byte)33);
            bw.Write((byte)48);
            bw.Write(text.ToCharArray(), 0, text.Length);
            //bw.Write(AsciiControlChars.Newline);
        }

        public static void High(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)16);
            bw.Write(text.ToCharArray(), 0, text.Length); //Width,enlarged
            //bw.Write(AsciiControlChars.Newline);
        }
        
        public static void LargeText(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)48);
            bw.Write(text.ToCharArray(), 0, text.Length);
            //bw.Write(AsciiControlChars.Newline);
        }
        public static void CenterText(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)'a');
            bw.Write((char)1);
            bw.Write(text.ToCharArray(), 0, text.Length);
            bw.Write(AsciiControlChars.Newline);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)'a');
            bw.Write((char)0);
        }
        public static void FeedLines(this BinaryWriter bw, int lines)
        {
            if (lines > 0)
            {
                bw.Write(AsciiControlChars.Newline);
                bw.Write(AsciiControlChars.Escape);
                bw.Write('d');
                bw.Write((byte)lines - 1);
            }
        }

        public static void PageBegin(this BinaryWriter bw)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)64);
        }
        public static void PageEnd(this BinaryWriter bw)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write(AsciiControlChars.EndOfTransmission);
            
        }

        public static void OnlyBarcode(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        {
            string barcodetext = "";
            text = text.Trim();
            int nostart = text.IndexOfAny("0123456789".ToCharArray());
            if (nostart == -1)
            {
                barcodetext = "\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Trim().Length + 2) + "{A" + text.Trim();
            }
            else
            {
                string leftpart = text.Substring(0, nostart);
                string rightpart = text.Substring(nostart);
                barcodetext = "\u001dhQ\u001dw\u0002\u001dkI" +
                    (char)(text.Trim().Length + 4) +
                    "{B" + leftpart + "{A" + rightpart;
            }
            CenterText(bw, barcodetext);
        }
        public static void BarcodeAndText(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        {
            text = text.Trim();
            string barcodetext = "\t\u001dw\u0003\u001dhF\u001dkH" + ((char)text.Length) + text;
            CenterText(bw, barcodetext);
        }
        public static void DirectBarcode(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        {
            text = text.Trim();
            //string barcodetext = "\t\u001dw\u0003\u001dhF\u001dkH" + ((char)text.Length) + text;
            //CenterText(bw, barcodetext);

            //barcodetext = "\u001b@\u001dH\u0001\u001dhQ\u001dw\u0002\u001dkI\n{B0{A" + text + "\u001d\0";
            //CenterText(bw, barcodetext);

            var barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Trim().Length + 2) + "{A" + text.Trim();
            CenterText(bw, barcodetext);
        }

        //public static void DirectBarcode(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        //{
        //    string barcodetext = "\u001b@\u001dH\u0001\u001dhQ\u001dw\u0002\u001dkI\n{B0{A" + text + "\u001d\0";
        //    CenterText(bw, barcodetext);

        //    barcodetext = barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Trim().Length + 2) + "{A" + text.Trim();
        //    CenterText(bw, barcodetext);
        //}
        public static void Barcode(this BinaryWriter bw, String text, bool newline = true, bool center = true)
        {

            // with out barcode text printing on top
            // string barcodetext = "\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Length + 12) + "{A" + text + "{B" + "12345678";

            //string barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Length + 12) + "{A" + text + "{B" + "12345678";

            string barcodetext = "";
            text = text.Trim();
            int nostart = text.IndexOfAny("0123456789".ToCharArray());
            if (nostart == -1)
            {
                barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" + (char)(text.Trim().Length + 2) + "{A" + text.Trim();
            }
            else
            {
                string leftpart = text.Substring(0, nostart);
                string rightpart = text.Substring(nostart);
                barcodetext = "\u001dH\u0001\u001df\0\u001dhQ\u001dw\u0002\u001dkI" +
                    (char)(text.Trim().Length + 4) +
                    "{B" + leftpart + "{A" + rightpart;
            }
            CenterText(bw, barcodetext);
        }

        public static void Barcode2D(this BinaryWriter bw, String QrData, bool newline = true, bool center = true)
        {
            string ESC = Convert.ToString((char)27);

            string GS = Convert.ToString((char)29);
            //string center = ESC + "a" + (char)1; //align center
            string left = ESC + "a" + (char)0; //align left
            string bold_on = ESC + "E" + (char)1; //turn on bold mode
            string bold_off = ESC + "E" + (char)0; //turn off bold mode
            string cut = ESC + "d" + (char)1 + GS + "V" + (char)66; //add 1 extra line before par

            Encoding m_encoding = Encoding.GetEncoding("iso-8859-1"); //set encoding for QRCode
            int store_len = (QrData).Length + 3;
            byte store_pL = (byte)(store_len % 256);
            byte store_pH = (byte)(store_len / 256);
            var barcodetext = "";
            string initp = ESC + (char)64; //initialize printer
            barcodetext += initp; //initialize printer
            barcodetext += m_encoding.GetString(new byte[] { 29, 40, 107, 4, 0, 49, 65, 50, 0 });
            barcodetext += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 67, 8 });
            barcodetext += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 69, 48 });
            barcodetext += m_encoding.GetString(new byte[] { 29, 40, 107, store_pL, store_pH, 49, 80, 48 });
            barcodetext += QrData;
            barcodetext += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 81, 48 });

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
            bw.Write(' ');
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (line)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void SmallText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void CondensedText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)4);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void DoubleCondensedText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)1);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        
        public static void ItalicText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)100);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void BoldText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)47);
            bw.Write(text.ToCharArray(), 0, text.Length);
            //bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void UnderlineText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)141);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void DoubleHeightText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)16);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void DoubleWidthText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)32);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }
        public static void EmphasizeText(this BinaryWriter bw, string text, bool newline = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)141);
            bw.Write(text.ToCharArray(), 0, text.Length);
            if (newline)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void Cutter(this BinaryWriter bw)
        {
            bw.Write(AsciiControlChars.GroupSeparator);
            bw.Write('V');
            bw.Write((byte)66);
            bw.Write((byte)3);
        }

        private static int ParseHexString(string hexNumber)
        {
            hexNumber = hexNumber.Replace("x", string.Empty);
            int result = 0;
            int.TryParse(hexNumber, System.Globalization.NumberStyles.HexNumber, null, out result);
            return result;
        }
        internal static void DrawUnicode(this BinaryWriter bw,string specialHeader)
        {
            var items = specialHeader.Split(new char[] { ',',' ' });
            foreach (var item in items)
            {
                bw.Write((char)ParseHexString(item));
            }
        }

        public static void BixolonHeader(this BinaryWriter bw)
        {
            // 1B 1D 5C 1C 7A 09 42 49 58 4F 1D 21 10 4C 4F 4E 50 52 49 4E 54 45 52 44
            bw.Write((char)0x1B);  bw.Write((char)0x1D); bw.Write((char)0x5C);
            bw.Write((char)0x1C);  bw.Write((char)0x7A);  bw.Write((char)0x09);
            bw.Write((char)0x42);  bw.Write((char)0x49);  bw.Write((char)0x58);
            bw.Write((char)0x4F);  bw.Write((char)0x1D); bw.Write((char)0x21);
            bw.Write((char)0x10); bw.Write((char)0x4C); bw.Write((char)0x4F);
            bw.Write((char)0x4E); bw.Write((char)0x50); bw.Write((char)0x52);
            bw.Write((char)0x49); bw.Write((char)0x4E); bw.Write((char)0x54);
            bw.Write((char)0x45);  bw.Write((char)0x52);  bw.Write((char)0x44);
        }
        public static string GetCutter()
        {
            char[] buytearray = new char[] { AsciiControlChars.GroupSeparator,'V',(char)66, (char)3 };
            return new string(buytearray);
        }
		
        public static void WriteLogo(this BinaryWriter bw,byte[] image,int scale)
        {
            if (image != null)
            {
                using (MemoryStream msg = new MemoryStream(image))
                {
                    BitmapData data = GetBitmapData(msg, scale);
                    if (data == null) return;
                    System.Collections.BitArray dots = data.Dots;
                    byte[] width = BitConverter.GetBytes(data.Width);

                    int offset = 0;
                    //MemoryStream stream = new MemoryStream();
                    //BinaryWriter bw = new BinaryWriter(stream);

                    bw.Write((char)0x1B);
                    //bw.Write('@');
                    bw.Write((byte)'a');
                    bw.Write((char)1);


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
                    bw.Write((byte)60);

                    //bw.Write(AsciiControlChars.Newline);
                    bw.Write(AsciiControlChars.Escape);
                    bw.Write((byte)'a');
                    bw.Write((char)0);
                }
            }
            //bw.Flush();
            //byte[] bytes = stream.ToArray();
            //return logo + Encoding.Default.GetString(bytes);
        }

        public static BitmapData GetBitmapData(MemoryStream bmpFileName, double multiplier)
        {
            try
            {
                using (var bitmap = (Bitmap)Bitmap.FromStream(bmpFileName))
                {
                    var threshold = 127;
                    var index = 0;
                    double scale = multiplier == -1 ? 1 : ((double)(multiplier / (double)bitmap.Width));
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
            }catch(Exception exp)
            {
                return null;
            }
        }

        public static BitmapData GetBitmapData(MemoryStream bmpFileName)
        {
            try
            {
                using (var bitmap = (Bitmap)Bitmap.FromStream(bmpFileName))
                {
                    var threshold = 127;
                    var index = 0;
                    double scale = 1;
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
            catch (Exception exp)
            {
                return null;
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
