using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PDFGraphics
{
    public interface IGraphic
    {
        void DrawRectangle(System.Drawing.Pen pen, System.Drawing.Rectangle rect);
        void DrawCircle(System.Drawing.Pen pen, System.Drawing.Rectangle rect);
        void DrawRectangle(System.Drawing.Pen pen, int x, int y, int width, int height);
        void DrawRectangle(System.Drawing.Pen pen, float x, float y, float width, float height);
        void DrawRectangles(System.Drawing.Pen pen, System.Drawing.Rectangle[] rects);
        void DrawRectangles(System.Drawing.Pen pen, System.Drawing.RectangleF[] rects);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point, System.Drawing.StringFormat format);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat format);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y);
        void DrawString(string s, Font font, Brush brush, float x, float y, float angle);
        void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, System.Drawing.StringFormat format);
        void RotateTransform(float angle);
        void RotateTransform(float angle, Point point);
        void ResetTransform();
        void FillRectangle(System.Drawing.Brush brush, System.Drawing.Rectangle rect);
        void FillRectangle(System.Drawing.Brush brush, System.Drawing.RectangleF rect);
        void FillRectangle(System.Drawing.Brush brush, int x, int y, int width, int height);
        void FillRectangle(System.Drawing.Brush brush, float x, float y, float width, float height);
        void FillRectangles(System.Drawing.Brush brush, System.Drawing.Rectangle[] rects);
        void FillRectangles(System.Drawing.Brush brush, System.Drawing.RectangleF[] rects);
        void DrawLine(Pen pen, float x1, float y1, float x2, float y2);
        void Save();
        SizeF MeasureString(string text, Font font);
        SizeF MeasureString(string text, Font font, int width);
        SizeF MeasureString(string text, Font font, SizeF layoutArea);
        SizeF MeasureString(string text, Font font, int width, StringFormat format);
        SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat);
        SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat);
        SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled);
        void DrawImageUnscaled(Image img, Rectangle rect);
        void DrawImage(Image img, RectangleF rect);
        void DrawImage(Image img, Point point);
        float DpiX { get; }
        float DpiY { get; }

        void DrawDottedLine(Pen black, float v1, float v2, float v3, float bottom);
    }
    public class DirectGraphic : PDFGraphics.IGraphic
    {
        System.Drawing.Graphics InternalGP;
        public DirectGraphic(System.Drawing.Graphics graphic)
        {
            this.InternalGP = graphic;
        }
        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            this.InternalGP.DrawRectangle(pen, rect);
        }
        public void DrawCircle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
        {
            this.InternalGP.DrawEllipse(pen, rect);
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            this.InternalGP.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            this.InternalGP.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawRectangles(Pen pen, Rectangle[] rects)
        {
            this.InternalGP.DrawRectangles(pen, rects);
        }

        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            this.InternalGP.DrawRectangles(pen, rects);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point)
        {

            this.InternalGP.DrawString(s, font, brush, point);
        }
        public void DrawString(string s, Font font, Brush brush, float x, float y, float angle)
        {
            this.InternalGP.RotateTransform(angle);
            DrawString(s, font, brush, x, y);
            this.InternalGP.ResetTransform();
        }
        System.Drawing.Drawing2D.GraphicsState currentstate = null;
        public void RotateTransform(float angle)
        {
            currentstate = this.InternalGP.Save();
            this.InternalGP.RotateTransform(angle);
        }
        public void RotateTransform(float angle, Point point)
        {
            currentstate = this.InternalGP.Save();
            Matrix result = new Matrix();
            result.RotateAt(angle, point);
            this.InternalGP.Transform = result;
        }
        public void ResetTransform()
        {
            if (currentstate != null)
                this.InternalGP.Restore(currentstate);
            currentstate = null;
        }

        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            this.InternalGP.DrawString(s, font, brush, point, format);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
        {
            this.InternalGP.DrawString(s, font, brush, layoutRectangle);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            this.InternalGP.DrawString(s, font, brush, layoutRectangle, format);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            this.InternalGP.DrawString(s, font, brush, x, y);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
        {
            this.InternalGP.DrawString(s, font, brush, x, y, format);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            this.InternalGP.FillRectangle(brush, rect);
        }

        public void FillRectangle(Brush brush, RectangleF rect)
        {
            this.InternalGP.FillRectangle(brush, rect);
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            this.InternalGP.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            this.InternalGP.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangles(Brush brush, Rectangle[] rects)
        {
            this.InternalGP.FillRectangles(brush, rects);
        }

        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            this.InternalGP.FillRectangles(brush, rects);
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

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            this.InternalGP.DrawLine(pen, x1, y1, x2, y2);
        }
        public void DrawImageUnscaled(Image img, Rectangle rect)
        {
            this.InternalGP.DrawImageUnscaled(img, rect);
        }

        public void DrawImage(Image img, RectangleF rect)
        {
            this.InternalGP.DrawImage(img, rect);
        }
        public void DrawImage(Image img, Point point)
        {
            this.InternalGP.DrawImage(img, point);
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

        public void Save()
        {
        }

        public void DrawDottedLine(Pen black, float v1, float v2, float v3, float bottom)
        {
            DrawLine(black, v1, v2, v3, bottom);
        }
    }

}
