using PDFGraphics;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using PdfSharp.Drawing;
using Quanto.Printer;
using Quanto;

namespace ReportLibrary
{
    public class DefaultPageSettings
    {
        public PaperSize PaperSize { get; set; }
        public System.Drawing.Printing.Margins Margins { get; set; }

        public System.Drawing.Rectangle MarginBound { get; set; }

        public bool Landscape { get; set; }
        public bool ThermalPrinter { get; set; }
        public XGraphicsUnit Unit { get; set; }
        public System.Drawing.Rectangle PageBounds { get; set; }

    }

    public class JSONPrinter
    {
        int pageindex = 0;
        JSONDocument document = null;
        [STAThread]
        public void Print(PrinterService.Printer printer,JSONDocument document)
        {
            this.document = document;
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings = new PrinterSettings();
            pd.PrinterSettings.PrinterName = printer.name;
            pd.PrinterSettings.DefaultPageSettings.PaperSize = new PaperSize("Custom", document.PageBounds.Width, document.PageBounds.Height);
            pd.PrinterSettings.DefaultPageSettings.Margins = new Margins(document.MarginBound.Left, document.MarginBound.Right,
    document.MarginBound.Top, document.MarginBound.Bottom);
            pd.PrintPage += new PrintPageEventHandler
               (this.pd_PrintPage);
            pd.PrintController = new StandardPrintController();
            pd.DocumentName = "Quanto POS";
            Logger.Current.Info("pd.Print - Starting..");
            pd.Print();
            Logger.Current.Info("pd.Print - Completed..");
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Logger.Current.Info("PrintPage - Starting..");
                DirectGraphic directGraphic = new DirectGraphic(e.Graphics);
                foreach (var item in document.Pages[pageindex].items)
                {
                    Logger.Current.Info("Print Page - " + item.type);
                    if(resettransform)
                    {
                        Render(item, directGraphic);
                        directGraphic.ResetTransform();
                        resettransform = false;
                    }
                    else
                    {
                        Render(item, directGraphic);
                    }
                }
                pageindex++;
                e.HasMorePages = document.Pages.Count > pageindex;
                Logger.Current.Info("PrintPage - Completed..");
            }
            catch(Exception exp)
            {
                Logger.Current.Error("PrintPage", exp);
            }
        }

        bool resettransform = false;

        public void Render(JSONPage.JSONElement jSONGraphicItem, DirectGraphic directGraphic)
        {
            switch(jSONGraphicItem.type)
            {
                case "DrawLine":
                    JSONGraphic.JSONLine line = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONLine>();
                    directGraphic.DrawLine(new Pen(line.color, line.width), line.x1, line.y1, line.x2, line.y2);
                    break;
                case "DrawCircle":
                    JSONGraphic.JSONRectangle rectangle = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONRectangle>();
                    directGraphic.DrawCircle(new Pen(rectangle.color, rectangle.width), rectangle.Rectangle);
                    break;
                case "DrawDottedLine":
                    JSONGraphic.JSONLine docttedline = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONLine>();
                    directGraphic.DrawLine(new Pen(docttedline.color, docttedline.width), docttedline.x1, docttedline.y1, docttedline.x2, docttedline.y2);
                    break;
                case "DrawRectangle":
                    JSONGraphic.JSONRectangle rectangle1 = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONRectangle>();
                    directGraphic.DrawRectangle(new Pen(rectangle1.color, rectangle1.width), rectangle1.Rectangle);
                    break;
                case "DrawString":
                    JSONGraphic.JSONString jsonstring = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONString>();
                    directGraphic.DrawString(jsonstring.value, new Font(jsonstring.fontname, jsonstring.fontsize, jsonstring.fontstyle), new SolidBrush(jsonstring.colour), jsonstring.point);
                    break;
                case "DrawStringRectangle":
                    JSONGraphic.JSONStringRectangle StringRectangle = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONStringRectangle>();
                    directGraphic.DrawString(StringRectangle.value, new Font(StringRectangle.fontname, StringRectangle.fontsize,
                        StringRectangle.fontstyle), new SolidBrush(StringRectangle.colour), StringRectangle.Rectangle);
                    break;
                case "DrawStringAngle":
                    JSONGraphic.JSONStringAngle stringangle = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONStringAngle>();
                    directGraphic.DrawString(stringangle.value, new Font(stringangle.fontname, stringangle.fontsize,
                    stringangle.fontstyle), new SolidBrush(stringangle.colour), stringangle.point.X, stringangle.point.Y, stringangle.angle);
                    break;
                case "DrawAlignedStringByPosition":
                    JSONGraphic.JSONString JSONStringposition = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONString>();
                    directGraphic.DrawString(JSONStringposition.value, new Font(JSONStringposition.fontname, JSONStringposition.fontsize,
                    JSONStringposition.fontstyle), new SolidBrush(JSONStringposition.colour), JSONStringposition.point,new StringFormat()
                    {
                        LineAlignment = JSONStringposition.LineAlignment,
                        Alignment = JSONStringposition.Alignment,
                        FormatFlags = JSONStringposition.FormatFlags,
                        Trimming = JSONStringposition.Trimming
                    });
                    break;
                case "DrawAlignedStringByRectangle":
                    JSONGraphic.JSONStringRectangle JSONStringrectangle = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONStringRectangle>();
                    directGraphic.DrawString(JSONStringrectangle.value, new Font(JSONStringrectangle.fontname, JSONStringrectangle.fontsize,
                    JSONStringrectangle.fontstyle), new SolidBrush(JSONStringrectangle.colour), JSONStringrectangle.Rectangle, new StringFormat()
                    {
                        LineAlignment = JSONStringrectangle.LineAlignment,
                        Alignment = JSONStringrectangle.Alignment
                    });
                    break;
                case "FillRectangle":
                    JSONGraphic.JSONRectangle fillrectangle = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONRectangle>();
                    directGraphic.FillRectangle(new SolidBrush(fillrectangle.color), fillrectangle.Rectangle);
                    break;
                case "DrawImageRect":
                    JSONGraphic.JSONImage jsonimagerect = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONImage>();
                    using (MemoryStream ms = new MemoryStream(jsonimagerect.Image))
                    {
                        directGraphic.DrawImage(Image.FromStream(ms),jsonimagerect.rect);
                    }
                    break;
                case "DrawImagePoint":
                    JSONGraphic.JSONImage jsonimagepoint = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONImage>();
                    using (MemoryStream ms = new MemoryStream(jsonimagepoint.Image))
                    {
                        directGraphic.DrawImage(Image.FromStream(ms), jsonimagepoint.point);
                    }
                    break;
                case "Rotate":
                    JSONGraphic.Rotate rotatetransaform = jSONGraphicItem.DeserializeObject<JSONGraphic.Rotate>();
                    directGraphic.RotateTransform(rotatetransaform.angle, rotatetransaform.point);
                    resettransform = true;
                    break;
                case "DrawImageUnscaled":
                    JSONGraphic.JSONImage jsonimageunscalled = jSONGraphicItem.DeserializeObject<JSONGraphic.JSONImage>();
                    using (MemoryStream ms = new MemoryStream(jsonimageunscalled.Image))
                    {
                        //directGraphic.DrawImageUnscaled(Image.FromStream(ms), jsonimageunscalled.rect);
                        directGraphic.DrawImage(Image.FromStream(ms), jsonimageunscalled.rect);
                    }
                    break;
                default:
                    Logger.Current.Info("Can't find the rendering option for " + jSONGraphicItem.type);
                    break;
            }
        }
    }
}