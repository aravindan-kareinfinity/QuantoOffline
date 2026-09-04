using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Quanto.Printer;

namespace Quanto
{
    public class PDFGraphicPrint
    {

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int capability);
        private const int PHYSICALOFFSETX = 112; // Physical Printable Area x margin
        private const int PHYSICALOFFSETY = 113; // Physical Printable Area y margin

         private static void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Get an XGraphics object for the page
            Graphics graphics = e.Graphics;
            IntPtr hdc = graphics.GetHdc();
            int xOffset = GetDeviceCaps(hdc, PHYSICALOFFSETX);
            int yOffset = GetDeviceCaps(hdc, PHYSICALOFFSETY);

            graphics.ReleaseHdc(hdc);
            float hardMarginX = xOffset * 100 / graphics.DpiX;
            float hardMarginY = yOffset * 100 / graphics.DpiY;
            graphics.TranslateTransform(-hardMarginX, -hardMarginY);

            // Recall: Width and Height are exchanged when settings.Landscape is true.
            XSize pageSize = new XSize(e.PageSettings.Bounds.Width / 100.0 * 72,
                                       e.PageSettings.Bounds.Height / 100.0 * 72);
            float scale = 100f / 72f; // Taken from the MigraDoc/PdfSharp documentation
            graphics.ScaleTransform(scale, scale);

            XGraphics gfx = XGraphics.FromGraphics(graphics, pageSize);
            e.HasMorePages = Render(gfx);
        }

        private static bool Render(XGraphics gfx)
        {
            return true;
        }

        static PdfDocument document;
        static int currentpage = 0;
        internal static bool SendFileToPrinter(PrinterService.Printer printer, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                document = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
                
                PrintDialog printerDialog = new PrintDialog();

                printerDialog.AllowSomePages = true;
                printerDialog.ShowHelp = false;
                printerDialog.PrinterSettings = new PrinterSettings();
                printerDialog.AllowPrintToFile = true;
                printerDialog.PrinterSettings.PrintToFile = true;

                DialogResult result = printerDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var printDoc = new PrintDocument();
                    printDoc.PrintPage += printDoc_PrintPage;
                    printDoc.PrinterSettings = printerDialog.PrinterSettings;
                    printDoc.Print();
                }

            }
            return true;
        }

    }
}
