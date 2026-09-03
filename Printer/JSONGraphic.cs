using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PDFGraphics
{
    public class JSONDocument
    {
        public Rectangle MarginBound { get; set; }
        public Rectangle PageBounds { get; set; }
        public List<JSONPage> Pages { get; set; }
        public int noofcopy { get; set; }
        public JSONDocument()
        {
            Pages = new List<JSONPage>();
        }
        public JSONPage AddPage()
        {
            JSONPage page = new JSONPage();
            Pages.Add(page);
            return page;
        }
    }
    public class JSONPage
    {
        public PageOrientation Orientation { get; set; }
        public enum PageOrientation
        {
            Portrait = 0,
            Landscape = 1
        }
        public enum PageSize
        {
            Undefined = 0,
            A0 = 1,
            A1 = 2,
            A2 = 3,
            A3 = 4,
            A4 = 5,
            A5 = 6,
            RA0 = 7,
            RA1 = 8,
            RA2 = 9,
            RA3 = 10,
            RA4 = 11,
            RA5 = 12,
            B0 = 13,
            B1 = 14,
            B2 = 15,
            B3 = 16,
            B4 = 17,
            B5 = 18,
            Quarto = 100,
            Foolscap = 101,
            Executive = 102,
            GovernmentLetter = 103,
            Letter = 104,
            Legal = 105,
            Ledger = 106,
            Tabloid = 107,
            Post = 108,
            Crown = 109,
            LargePost = 110,
            Demy = 111,
            Medium = 112,
            Royal = 113,
            Elephant = 114,
            DoubleDemy = 115,
            QuadDemy = 116,
            STMT = 117,
            Folio = 120,
            Statement = 121,
            Size10x14 = 122
        }
        public PageSize pagesize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public class JSONElement
        {
            public string type { get; set; }
            public string value { get; set; }
            public JSONElement()
            {

            }
            public JSONElement(string type, object element)
            {
                this.type = type;
                this.value = Newtonsoft.Json.JsonConvert.SerializeObject(element);
            }
            public T DeserializeObject<T>()
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            }
        }
        public void AddItem(string type, object element)
        {
            this.items.Add(new JSONElement(type, element));
        }
        public List<JSONElement> items;
        public JSONPage()
        {
            this.items = new List<JSONElement>();
        }
    }

    public class JSONGraphic : PDFGraphics.IGraphic
    {
        System.Drawing.Graphics InternalGP;
        JSONPage page;

        public JSONGraphic(JSONPage page, System.Drawing.Graphics graphic)
        {
            this.page = page;
            this.InternalGP = graphic;
        }
        public JSONGraphic(JSONPage page, System.Drawing.Graphics graphic, XGraphicsUnit unit)
        {
            this.page = page;
            this.InternalGP = graphic;
            switch (unit)
            {
                case XGraphicsUnit.Millimeter:
                    graphic.PageUnit = GraphicsUnit.Millimeter;
                    break;
                case XGraphicsUnit.Inch:
                    graphic.PageUnit = GraphicsUnit.Inch;
                    break;
                case XGraphicsUnit.Point:
                    graphic.PageUnit = GraphicsUnit.Point;
                    break;
                case XGraphicsUnit.Presentation:
                    graphic.PageUnit = GraphicsUnit.Pixel;
                    break;
            }
        }

        public class JSONGraphicItem
        {
            public JSONGraphicItem()
            {

            }
            public Color color { get; set; }
            public float width { get; set; }
        }
        public class JSONLine : JSONGraphicItem
        {

            public float x1 { get; set; }
            public float y1 { get; set; }
            public float x2 { get; set; }
            public float y2 { get; set; }
            public JSONLine() : base()
            {

            }
            public JSONLine(Pen pen, float x1, float y1, float x2, float y2)
            {
                this.color = pen.Color;
                this.width = pen.Width;
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
            }
        }
        public class JSONRectangle : JSONGraphicItem
        {
            public System.Drawing.Rectangle Rectangle { get; set; }
            public JSONRectangle()
            {

            }
            public JSONRectangle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
            {
                this.color = pen.Color;
                this.width = pen.Width;
                this.Rectangle = rect;
            }
            public JSONRectangle(System.Drawing.Pen pen, System.Drawing.RectangleF rect)
            {
                this.color = pen.Color;
                this.width = pen.Width;
                this.Rectangle = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            }
        }
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            page.AddItem("DrawLine", new JSONLine(pen, x1, y1, x2, y2));
        }
        public void DrawCircle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
        {
            page.AddItem("DrawCircle", new JSONRectangle(pen, rect));
        }
        public void DrawDottedLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            page.AddItem("DrawDottedLine", new JSONLine(pen, x1, y1, x2, y2));
        }
        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            page.AddItem("DrawRectangle", new JSONRectangle(pen, rect));
        }
        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            page.AddItem("DrawRectangle", new JSONRectangle(pen, new Rectangle() { Height = (int)height, Width = (int)width, X = (int)x, Y = (int)y }));
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            page.AddItem("DrawRectangle", new JSONRectangle(pen, new Rectangle() { Height = (int)height, Width = (int)width, X = (int)x, Y = (int)y }));
        }
        public void DrawRectangles(Pen pen, Rectangle[] rects)
        {
            foreach (var item in rects)
                page.AddItem("DrawRectangle", new JSONRectangle(pen, item));
        }
        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            foreach (var item in rects)
                page.AddItem("DrawRectangle", new JSONRectangle(pen, new Rectangle()
                {
                    Height = (int)item.Height,
                    Width = (int)item.Width,
                    X = (int)item.X,
                    Y = (int)item.Y
                }));
        }
        public class JSONString
        {
            public Color colour { get; set; }
            public string value { get; set; }
            public string fontname { get; set; }
            public float fontsize { get; set; }
            public FontStyle fontstyle { get; set; }
            public PointF point { get; set; }
            public StringAlignment Alignment { get; set; }
            public StringAlignment LineAlignment { get; set; }
            public StringFormatFlags FormatFlags { get; set; }
            public StringTrimming Trimming { get; set; }
        }
        public class JSONStringRectangle : JSONString
        {
            public RectangleF Rectangle { get; set; }
        }
        public class JSONStringAngle : JSONString
        {
            public float angle { get; set; }
        }
        public void DrawString(string s, Font font, Brush brush, PointF point)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONString jSONString = new JSONString()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                point = point,
                colour = solidBrush.Color
            };
            page.AddItem("DrawString", jSONString);
        }
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONStringRectangle jSONString = new JSONStringRectangle()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                Rectangle = layoutRectangle,
                colour = solidBrush.Color
            };
            page.AddItem("DrawStringRectangle", jSONString);
        }
        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONString jSONString = new JSONString()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                point = new PointF(x, y),
                colour = solidBrush.Color
            };
            page.AddItem("DrawString", jSONString);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y, float angle)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONStringAngle jSONString = new JSONStringAngle()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                point = new PointF(x, y),
                colour = solidBrush.Color,
                angle = angle
            };
            page.AddItem("DrawStringAngle", jSONString);
        }


        public XStringFormat Convert(StringFormat format)
        {
            XStringFormat xformat = new XStringFormat();
            xformat.Alignment = (XStringAlignment)format.Alignment;
            xformat.LineAlignment = (XLineAlignment)format.LineAlignment;
            return xformat;
        }
        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONString jSONString = new JSONString()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                point = point,
                colour = solidBrush.Color,
                LineAlignment = format.LineAlignment,
                Alignment = format.Alignment
            };
            page.AddItem("DrawAlignedStringByPosition", jSONString);
        }
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONStringRectangle jSONString = new JSONStringRectangle()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                Rectangle = layoutRectangle,
                colour = solidBrush.Color,
                LineAlignment = format.LineAlignment,
                Alignment = format.Alignment
            };
            page.AddItem("DrawAlignedStringByRectangle", jSONString);
        }



        public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
        {
            SolidBrush solidBrush = brush as SolidBrush;
            JSONString jSONString = new JSONString()
            {
                value = s,
                fontname = font.Name,
                fontsize = font.Size,
                fontstyle = font.Style,
                point = new PointF(x, y),
                colour = solidBrush.Color,
                LineAlignment = format.LineAlignment,
                Alignment = format.Alignment
            };
            page.AddItem("DrawAlignedStringByPosition", jSONString);
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), rect));

        }
        public void FillRectangle(Brush brush, RectangleF rect)
        {
            page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), rect));
        }
        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), new RectangleF(x, y, width, height)));
        }
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), new Rectangle(x, y, width, height)));
        }
        public void FillRectangles(Brush brush, Rectangle[] rects)
        {
            foreach (Rectangle rect in rects)
                page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), new Rectangle(rect.X, rect.Y, rect.Width, rect.Height)));
        }
        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            foreach (RectangleF rect in rects)
                page.AddItem("FillRectangle", new JSONRectangle(new Pen((SolidBrush)brush, 1), new RectangleF(rect.X, rect.Y, rect.Width, rect.Height)));
        }
        public SizeF MeasureString(string text, Font font)
        {
            return this.InternalGP.MeasureString(text, font);
        }
        public SizeF MeasureString(string text, Font font, int width)
        {
            return this.InternalGP.MeasureString(text, font, width);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea);
        }
        public SizeF MeasureString(string text, Font font, int width, StringFormat format)
        {
            return this.InternalGP.MeasureString(text, font, width, format);
        }
        public SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat)
        {
            return this.InternalGP.MeasureString(text, font, origin, stringFormat);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea, stringFormat);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
        }
        public class Rotate
        {
            public float angle { get; set; }
            public Point point { get; set; }
        }
        public class JSONImage
        {
            public byte[] Image { get; set; }
            public Rectangle rect { get; set; }
            public Point point { get; set; }
        }
        public void DrawImage(Image img, RectangleF rect)
        {
            JSONImage jSONImage = new JSONImage();
            jSONImage.rect = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                jSONImage.Image = ms.ToArray();
            }
            page.AddItem("DrawImageRect", jSONImage);
        }
        public void DrawImage(Image img, Point point)
        {
            JSONImage jSONImage = new JSONImage();
            jSONImage.point = point;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                jSONImage.Image = ms.ToArray();
            }
            page.AddItem("DrawImagePoint", jSONImage);
        }
        public float DpiX
        {
            get
            {
                return this.InternalGP.DpiX;
            }
        }
        public float DpiY
        {
            get
            {
                return this.InternalGP.DpiY;
            }
        }
        public void DrawImageUnscaled(Image img, Rectangle rect)
        {
            JSONImage jSONImage = new JSONImage();
            jSONImage.rect = rect;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                jSONImage.Image = ms.ToArray();
            }
            page.AddItem("DrawImageUnscaled", jSONImage);
        }
        public void Save()
        {

        }

        public void RotateTransform(float angle)
        {
            page.AddItem("Rotate", new Rotate() { angle = angle });
        }

        public void RotateTransform(float angle, Point point)
        {
            page.AddItem("Rotate", new Rotate() { angle = angle, point = point });
        }

        public void ResetTransform()
        {

        }
    }
}