using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Drawing.Printing;
using System.Data;


namespace ReportLibrary
{
    public class DotMatrixReporter : DOSReporter
    {
        
        public class PrinterConfig
        {
            public string PrinterName;
            public int TopMargin = 50;
            public int LeftMargin = 50;
            public int BottomMargin = 50;
            public int RightMargin = 50;
            public int Width = 850;
            public int Height = 1100;
            public bool IsLandscape = false;
        }

        public enum DisplayTextAlign
        {
            Left,
            Rigtht,
            Center
        }

        public enum FontSize
        {
            Normal,
            Big,
            Small
        }

        public class DesignerElement
        {

            public DesignerElement(DosFormat.Content citem)
            {
                DisplayText = citem.Text;
            }

            public string DisplayText { get; set; }
            public DisplayTextAlign DisplayTextAlign { get; set; }
            public Color ForeColor { get; set; }
            public float Left { get; set; }
            public int Width { get; set; }

            internal Font GetFont()
            {
                return new Font("verdana", 10f);
            }
        }

        DefaultPageSettings defaultPageSettings = new DefaultPageSettings();
        Font webdings = new Font("Webdings", 12f);
        PrinterConfig lrd;
        DesignerElement currentElement;
        string configName = "CustomFinalPrint";
        public DotMatrixReporter(long organizatoinid,string configName, PrinterConfig lrdConfig)
            : this(organizatoinid,lrdConfig)
        {
            this.configName = configName;
        }

        public DefaultPageSettings DefaultPageSettings
        {
            get
            {
                return defaultPageSettings;
            }

            set
            {
                defaultPageSettings = value;
            }
        }

        public DotMatrixReporter(long organizationid):base(organizationid)
        {

        }
        public DotMatrixReporter(long organizationid,PrinterConfig lrdConfig)
            : base(organizationid)
        {
            lrd = lrdConfig;
            DefaultPageSettings.PaperSize = new PaperSize(configName, lrd.Width, lrd.Height);
            DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(lrd.LeftMargin, lrd.RightMargin, lrd.TopMargin, lrd.BottomMargin);
            if (DefaultPageSettings.PaperSize.Width <= 550)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 7.5f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 7.5f, BodyStyle.Font.Style);
            }
            else if (DefaultPageSettings.PaperSize.Width <= 650)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 8.5f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 8.5f, BodyStyle.Font.Style);
            }
            else if (DefaultPageSettings.PaperSize.Width <= 750)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 9f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 9f, BodyStyle.Font.Style);
            }
        }

        public override void OnBeginPrint(PrintEventArgs e)
        {
            //DefaultPageSettings.PaperSize = new PaperSize(configName, lrd.Width, lrd.Height);
            //DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(lrd.LeftMargin, lrd.RightMargin, lrd.TopMargin, lrd.BottomMargin);
            base.OnBeginPrint(e);
        }

        private int currentRow = 0;
        private int currentIndex = 0;

        float defaultHeight = 0;
        bool IsMultiLinePrinting = false;
        string StringToPrint = string.Empty;
        private bool DisplayMultiLine(PrintPageEventArgs e)
        {
            IsMultiLinePrinting = true;
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Near;
            int charactersOnPage = 0;
            int linesPerPage = 0;
            // Sets the value of charactersOnPage to the number of characters 0
            // of stringToPrint that will fit within the bounds of the page.
            SizeF sf = new SizeF(currentElement.Width, e.MarginBounds.Bottom - currentY);
            SizeF psf = PDFGraphic.MeasureString(StringToPrint, GetCurrentFont, sf, stringFormat,
                out charactersOnPage, out linesPerPage);

            RectangleF bodyrectangle = new RectangleF(currentX, currentY - 2, psf.Width, psf.Height + 15);

            // Draws the string within the bounds of the page
            PDFGraphic.DrawString(StringToPrint.Substring(0, charactersOnPage), GetCurrentFont, BodyStyle.Brush,
               bodyrectangle, stringFormat);

            // Remove the portion of the string that has been printed.
            StringToPrint = StringToPrint.Substring(charactersOnPage);

            currentY = currentY + bodyrectangle.Height;
            // Check to see if more pages are to be printed.
            IsMultiLinePrinting = (StringToPrint.Length > 0);
            return IsMultiLinePrinting;
        }

        private void Display(Graphics g, string text)
        {
            stringFormat.Alignment = CurrentAligntment;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            PDFGraphic.DrawString(text, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private StringAlignment CurrentAligntment
        {
            get
            {
                switch (currentElement.DisplayTextAlign)
                {
                    case DisplayTextAlign.Center:
                        return StringAlignment.Center;
                    case DisplayTextAlign.Left:
                        return StringAlignment.Near;
                }
                return StringAlignment.Far;
            }
        }

        public int GetPercentWidth(int percent)
        {
            return currentElement.Width * percent / 100;
        }


        private void Display(Graphics g, decimal value)
        {
            Display(g, value, null);
        }

        private void DisplayFullNumber(Graphics g, decimal value, String format)
        {
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            string val = string.IsNullOrEmpty(format) ? value.ToString() : string.Format(format, value);
            PDFGraphic.DrawString(val, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, decimal value, String format)
        {
            if (value == 0) return;
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            string val = string.IsNullOrEmpty(format) ? value.ToString() : string.Format(format, value);
            PDFGraphic.DrawString(val, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, bool value, string displayText)
        {
            string dispText = value ? "a" : "r";
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, GetColumnWidth(webdings, g, dispText + dispText), defaultHeight);
            PDFGraphic.DrawString(dispText, webdings, BodyStyle.Brush, bodyrectangle, stringFormat);
            if (string.IsNullOrEmpty(displayText) == false)
            {
                bodyrectangle = new RectangleF(currentX + GetColumnWidth(GetCurrentFont, g, dispText + dispText), currentY, currentElement.Width, defaultHeight);
                PDFGraphic.DrawString(displayText, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            }
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, DateTime value, string format)
        {
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.Alignment = CurrentAligntment;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            PDFGraphic.DrawString(value.ToString(format), GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
        }


        private void DrawCell(float x,float y, float width, float height)
        {
            PDFGraphic.DrawRectangle(new Pen(Color.Black), x, y, width, height);
        }

        private void DrawCell(int width, float height)
        {
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
        }

        private void DrawCell(string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            PDFGraphic.FillRectangle(Brushes.LightGray, currentX, currentY, width, height);
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
            stringFormat.Alignment = sf;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
            PDFGraphic.DrawString(title, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
        }

        private void DisplayText(string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            PDFGraphic.FillRectangle(Brushes.LightGray, currentX, currentY, width, height);
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
            stringFormat.Alignment = sf;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
            PDFGraphic.DrawString(title, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
        }

        private void DisplayText(DosFormat.Content citem, string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            Style _style = (Style)TotalStyle.Clone();
            if (!string.IsNullOrEmpty(citem.Style))
            {
                _style.Font = citem.GetStyle(_style.Font);
                float rowheight = GetRowHeight(_style.Font, e);
                height = rowheight;
            }
            stringFormat.Alignment = sf;
            stringFormat.LineAlignment = StringAlignment.Near;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
            PDFGraphic.DrawString(title, _style.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
        }

        private Font GetCurrentFont
        {
            get
            {
                return currentElement.GetFont();
            }
        }


        public float GetColumnWidth(Font fntReportBodyFont, Graphics g, int noofChar)
        {
            return g.MeasureString(new string('w', noofChar), fntReportBodyFont).Width + 2;
        }


        private bool isPrintRemainingDetails;

        private void PrintLine(Graphics g, PrintPageEventArgs e)
        {
            if (endX < currentElement.Width)
                currentElement.Width = (int)endX - (int)currentX;

            Pen pen = new Pen(currentElement.ForeColor.IsEmpty ? Color.Black : currentElement.ForeColor);
            pen.Width = 1;
            int v = 0;
            if (int.TryParse(currentElement.DisplayText, out v))
            {
                switch (v)
                {
                    case 0:
                        PDFGraphic.DrawLine(pen, currentX, currentY, currentX + currentElement.Width, currentY);
                        break;
                    case 1:
                        PDFGraphic.DrawLine(pen, currentX, currentY, currentX + currentElement.Width, currentY);
                        PDFGraphic.DrawLine(pen, currentX, currentY + 2, currentX + currentElement.Width, currentY + 2);
                        break;
                    case 2:
                        pen.Width = 2;
                        PDFGraphic.DrawLine(pen, currentX, currentY, currentX + currentElement.Width, currentY);
                        break;
                    case 3:
                        PDFGraphic.DrawLine(pen, 0, currentY, endX, currentY);
                        break;
                    case 4:
                        PDFGraphic.DrawLine(pen, 0, currentY, endX, currentY);
                        PDFGraphic.DrawLine(pen, 0, currentY + 2, endX, currentY + 2);
                        break;
                    case 5:
                        pen.Width = 2;
                        PDFGraphic.DrawLine(pen, 0, currentY, endX, currentY);
                        break;
                }
            }
            else
            {
                PDFGraphic.DrawLine(pen, currentX, currentY, currentX + currentElement.Width, currentY);
            }
        }

        private object dataSourceList = null;
        IEnumerator currentDataSourceList = null;
        public object DataSourceList
        {
            get
            {
                return dataSourceList;
            }
            set
            {
                dataSourceList = value;
                ie = null;
                ien = null;
            }
        }

        public string DataSourceItemMember { get; set; }

        float? defaultLeft = null;
        IEnumerator currentBodyDataSource = null;
        public Object DataItems;
        public Object DataItem;
        public DosFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                var style = format.GetStyle(format.PageHeader.Style);
                if (style != null)
                {
                    HeaderStyle.Font = style;
                }
                style = format.GetStyle(format.PageBody.Style);
                if (style != null)
                {
                    BodyStyle.Font = style;
                }
                style = format.GetStyle(format.PageSetting.Style);
                if (style != null)
                {
                    TotalStyle.Font = style;
                }
            }
        }
        public DosFormat format;
        public SortedDictionary<string, object> Additional;
        int currentbodyIndex = 0;
        float endX = 0;
        float currentLeft = 0;
        public PrintingMode Mode { get; set; }
        public enum PrintingMode
        {
            Normal,
            Label
        }

        public void OnPrintLabelPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentDataSourceList == null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(dataSourceList, null);
                currentDataSourceList = ie.GetEnumerator();
                currentDataSourceList.Reset();
                if (!currentDataSourceList.MoveNext())
                    return;
                DataSource = currentDataSourceList.Current;
            }

            currentY = e.MarginBounds.Top;
            currentX = e.MarginBounds.Left;
            currentLeft = e.MarginBounds.Left;
            float endY = e.MarginBounds.Bottom;
            endX = e.MarginBounds.Right;
            defaultHeight = GetRowHeight(BodyStyle.Font, e);

            if (format.PageBody == null)
                format.PageBody = new DosFormat.Body()
                {
                    Items = new List<DosFormat.Line>()
                };

            e.HasMorePages = true;
            format.PageBody.AvailableRows = 0;
            
            while (true)
            {
                float labelheight = currentY;
                for (int columnindex = 0; columnindex < format.PageSetting.NoofColumns; columnindex++)
                {
                    currentY = labelheight;
                    for (var currentbodyIndex = 0; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                    {
                        var item = format.PageBody.Items[currentbodyIndex];
                        currentX = currentLeft + (columnindex * format.PageSetting.ColumnWidth);
                        foreach (var citem in item.Items)
                        {
                            RenderContent(citem, DataSource, e, e.MarginBounds.Width);
                        }
                        currentY += defaultHeight;
                    }

                    DrawCell(currentLeft + (columnindex * format.PageSetting.ColumnWidth),
                        labelheight, format.PageSetting.ColumnWidth, format.PageSetting.ColumnHeight);

                    if (currentDataSourceList.MoveNext())
                    {
                        DataSource = currentDataSourceList.Current;
                    }
                    else
                    {
                        e.HasMorePages = false;
                        break;
                    }
                }
                currentY = labelheight+ format.PageSetting.ColumnHeight;
                if (e.HasMorePages == true)
                {
                    if(currentY+ format.PageSetting.ColumnHeight> e.MarginBounds.Bottom)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public override void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (Mode == PrintingMode.Label)
            {
                OnPrintLabelPage(e);
                return;
            }

            if (currentDataSourceList == null && DataSource == null && dataSourceList != null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(dataSourceList, null);
                currentDataSourceList = ie.GetEnumerator();
                currentDataSourceList.Reset();
                currentDataSourceList.MoveNext();
                DataSource = currentDataSourceList.Current;

                DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
            }

            if (currentBodyDataSource == null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(DataItems, null);
                if (ie != null)
                    currentBodyDataSource = ie.GetEnumerator();
            }

            EnablePageNumber = false;
            currentY = e.MarginBounds.Top;
            currentX = e.MarginBounds.Left;
            currentLeft = e.MarginBounds.Left;
            float endY = e.MarginBounds.Bottom;
            endX = e.MarginBounds.Right;
            defaultHeight = GetRowHeight(BodyStyle.Font, e);
            if (format.Columnes != null && format.Columnes.Count > 0)
            {
                float columnwidth = 0;
                currentLeft = e.MarginBounds.Left - 20;
                currentX = currentLeft;
                for (int i = 0; i < format.Columnes.Count; i++)
                {
                    currentY = e.MarginBounds.Top;
                    currentX = currentLeft + columnwidth + (i * 30);
                    float width = (float)e.MarginBounds.Width * (float)format.Columnes[i].width / 100;
                    endX = currentX + width;
                    PrintColumn(format.Columnes[i], currentX, width, e);
                    columnwidth += width;
                    if (i != format.Columnes.Count - 1)
                    {
                        PDFGraphic.DrawDottedLine(Pens.Black, currentX + width + 15, 0, currentX + width + 15, e.PageBounds.Bottom);
                    }
                }
                e.HasMorePages = false;
                if (currentDataSourceList != null)
                {
                    if (currentDataSourceList.MoveNext())
                    {
                        e.HasMorePages = true;
                        DataSource = currentDataSourceList.Current;
                        DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
                        currentBodyDataSource = null;
                    }
                }
                return;
                //e.HasMorePages = false;
                //PageCount = 1;
                //currentpage = 1;
                //return;
            }
            else if (format.Rows != null && format.Rows.Count > 0)
            {
                float rowheight = 0;

                currentY = e.MarginBounds.Top;
                for (int i = 0; i < format.Rows.Count; i++)
                {
                    currentX = e.MarginBounds.Left;
                    endX = e.MarginBounds.Right;
                    currentY = e.MarginBounds.Top + rowheight + (i * 30);
                    float height = (float)e.MarginBounds.Height * (float)format.Rows[i].height / 100;
                    PrintColumn(format.Rows[i], currentX, e.MarginBounds.Width, e);
                    rowheight += height;
                    if (i != format.Rows.Count - 1)
                    {
                        PDFGraphic.DrawDottedLine(Pens.Black, e.MarginBounds.Left, rowheight, e.MarginBounds.Right, rowheight);
                    }
                }
                e.HasMorePages = false;
                PageCount = 1;
                currentpage = 1;
                return;
            }
            if (currentpage == 1)
            {
                currentRow = 0;
                currentIndex = 0;

                if (!defaultLeft.HasValue)
                    defaultLeft = currentLeft;

                PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);
            }
            else
            {
                PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);
                currentTableRow = _bodycurrentTableRow;
                currentTable = _bodycurrentTable;
                _bodycurrentTableRow = 0;
                _bodycurrentTable = null;
                //if (isTablePrinting)
                //{
                if (RenderTable(e, format.PageBody.Length, currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
                {
                    currentY += defaultHeight;
                    e.HasMorePages = true;
                    currentpage++;
                    //return;
                }
                else
                {
                    currentY += defaultHeight;
                    currentX = currentLeft;
                    currentbodyIndex++;
                }
                currentIndex++;
                //}
            }

            if (format.PageBody == null)
                format.PageBody = new DosFormat.Body()
                {
                    Items = new List<DosFormat.Line>()
                };

            format.PageBody.AvailableRows = 0;
            //if (currentpage == 1)
            //{
            if (!e.HasMorePages)
            {
                ShowGrid = !format.PageBody.DisableGrid;
                for (; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                {
                    var item = format.PageBody.Items[currentbodyIndex];
                    if (item.IsTable)
                    {
                        if (!string.IsNullOrEmpty(item.field))
                        {

                            var tablesource = ReportLibrary.DataSourceHelper.Eval(DataSource, item.field);
                            if (tablesource != null)
                            {
                                ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(tablesource, null);
                                if (ie != null)
                                    currentBodyDataSource = ie.GetEnumerator();

                            }
                        }
                        currentTable = new Table();
                        currentTable.AutoFitHeight = format.PageBody.WrapRow;
                        currentTableRow = 0;
                        List<DosFormat.Content> removable = new List<DosFormat.Content>();
                        foreach (var citem in item.Items)
                        {
                            if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                            {
                                bool hide = false;
                                if (!string.IsNullOrEmpty(citem.Show))
                                {
                                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                                    if (show != null && !(bool)show)
                                        hide = true;
                                }
                                if (!hide && !string.IsNullOrEmpty(citem.Hide))
                                {
                                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                                    if (show != null && (bool)show)
                                        hide = true;
                                }
                                if (hide)
                                {
                                    removable.Add(citem);
                                    continue;
                                }
                                var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                                clmn.BodyFont = item.GetStyle();
                                if (clmn.BodyFont != null)
                                    clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);


                                clmn.MergeCell = citem.MergeCell;
                                clmn.MergeText = citem.MergeText;
                                currentTable.Columns.Add(clmn);
                            }
                        }
                        removable.ForEach(ex => item.Items.Remove(ex));

                        int sno = 0;
                        while (currentBodyDataSource != null && currentBodyDataSource.MoveNext())
                        {
                            DataItem = currentBodyDataSource.Current;
                            var columnValues = new SortedList<int, object>();
                            currentTable.Rows.Add(columnValues);
                            sno += 1;
                            foreach (var citem in item.Items)
                            {
                                if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                                {
                                    object currentvalue = null;
                                    if (citem.Format.Length > 0)
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                                    else
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);
                                    if (currentvalue == null && citem.Name == "sno")
                                        currentvalue = sno;
                                    columnValues.Add(columnValues.Count, currentvalue);
                                }
                            }
                        }
                        if (RenderTable(e, format.PageBody.Length, currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
                        {
                            e.HasMorePages = true;
                            break;
                        }
                        else
                        {
                            //currentY += defaultHeight;
                            currentX = currentLeft;
                        }
                    }
                    else
                    {

                        foreach (var citem in item.Items)
                        {
                            RenderContent(citem, DataSource, e, e.MarginBounds.Width);
                        }
                        currentX = currentLeft;
                        if(!item.Items.TrueForAll(ex=>ex.Type == DosFormat.CotnentType.Image))
                            currentY += defaultHeight;

                    }
                    format.PageBody.AvailableRows++;
                }
                //currentY += defaultHeight;
                currentX = currentLeft;
            }
            //}
            //else
            //{
            //    currentY += defaultHeight;
            //    currentX = currentLeft;
            //}

            _bodycurrentTableRow = currentTableRow;
            _bodycurrentTable = currentTable;

            if((format.PageFooter.EOD && !e.HasMorePages) || !format.PageFooter.EOD)
                PrintFooter(e, format.PageFooter, e.MarginBounds.Left, e.MarginBounds.Width);

            if (currentDataSourceList != null)
            {
                if (currentDataSourceList.MoveNext())
                {
                    e.HasMorePages = true;
                    DataSource = currentDataSourceList.Current;
                    DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
                    currentBodyDataSource = null;
                }
            }

            //e.HasMorePages = !(currentIndex + 1 >= BillPrintInfo.PrintDesign.Count);
            //ShowPageNumber(!e.HasMorePages, defaultHeight, e);
            if (e.HasMorePages)
            {
                currentpage++;
            }
            else
            {
                PageCount = currentpage;
                currentpage = 1;
            }

        }

        int _bodycurrentTableRow = -1;
        Table _bodycurrentTable = null;
        private void PrintColumn(DosFormat.Column column, float left, float width, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentBodyDataSource == null && DataItems != null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(DataItems, null);
                currentBodyDataSource = ie.GetEnumerator();
            }
            if (currentBodyDataSource != null)
            {
                currentBodyDataSource.Reset();
            }
            currentbodyIndex = 0;
            currentTableRow = 0;
            currentRow = 0;
            currentIndex = 0;

            defaultLeft = left;

            if (column.border)
            {
                Rectangle bodyrectanglen = new Rectangle((int)left - 5, e.MarginBounds.Top, (int)width + 10, (int)column.height);
                PDFGraphic.DrawRectangle(Pens.Black, bodyrectanglen);
            }

            PrintHeader(e, column.PageHeader, left, width);


            if (column.PageBody == null)
            {
                column.PageBody = new DosFormat.Body();
                column.PageBody.Items = new List<DosFormat.Line>();
            }
            column.PageBody.AvailableRows = 0;
            if (currentpage == 1)
            {
                for (; currentbodyIndex < column.PageBody.Items.Count; currentbodyIndex++)
                {
                    var item = column.PageBody.Items[currentbodyIndex];
                    if (item.IsTable)
                    {
                        currentTable = new Table();
                        currentTable.AutoFitHeight = format.PageBody.WrapRow;
                        foreach (var citem in item.Items)
                        {
                            if (citem.Type == DosFormat.CotnentType.Field)
                            {
                                var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, StringAlignment.Near, citem.Total, citem.Length);

                                clmn.BodyFont = item.GetStyle();
                                if (clmn.BodyFont != null)
                                    clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);

                                if (citem.Align == "right")
                                {
                                    clmn.Alignment = StringAlignment.Far;
                                }
                                currentTable.Columns.Add(clmn);
                            }
                        }

                        while (currentBodyDataSource.MoveNext())
                        {
                            DataItem = currentBodyDataSource.Current;
                            var columnValues = new SortedList<int, object>();
                            currentTable.Rows.Add(columnValues);
                            foreach (var citem in item.Items)
                            {
                                if (citem.Type == DosFormat.CotnentType.Field)
                                {
                                    object currentvalue = null;
                                    if (citem.Format.Length > 0)
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                                    else
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);

                                    columnValues.Add(columnValues.Count, currentvalue);
                                }
                            }
                        }
                        if (RenderTable(e, column.PageBody.Length, currentX, column.PageBody.HeaderHeight, column.PageBody.LineHeight))
                        {
                            e.HasMorePages = true;
                            break;
                        }
                    }
                    else
                    {
                        while (currentBodyDataSource.MoveNext())
                        {
                            DataItem = currentBodyDataSource.Current;
                            foreach (var citem in item.Items)
                            {
                                RenderContent(citem, DataItem, e, width);
                            }
                            currentX = left;
                            currentY += defaultHeight;
                        }
                    }
                    column.PageBody.AvailableRows++;
                }
                currentY += defaultHeight;
                currentX = left;
            }
            else
            {
                currentY += defaultHeight;
                currentX = left;
            }
            if (column.PageFooter == null)
            {
                column.PageFooter = new DosFormat.Footer();
                column.PageFooter.Items = new List<DosFormat.Line>();
            }
            PrintFooter(e, column.PageFooter, left, width);
        }

        private void RenderContent(DosFormat.Content citem, object currentDS, PrintPageEventArgs e, float width)
        {
            if (currentX + citem.Length > endX)
                citem.Length = (int)(endX - currentX);

            if (citem.Length < 0)
                citem.Length = 0;

            switch (citem.Type)
            {
                case DosFormat.CotnentType.Field:
                    object columnvalue = null;
                    if (citem.Format.Length > 0)
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name, "{0:" + citem.Format + "}");
                    else
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                    if (columnvalue != null)
                    {
                        DisplayText(citem, columnvalue.ToString(), citem.Length, citem.GetHeight(defaultHeight), citem.GetAlignment(), e);
                    }
                    break;
                case DosFormat.CotnentType.Line:
                    currentElement = new DesignerElement(citem);
                    currentElement.Left = currentX;
                    currentElement.Width = (int)width;
                    PrintLine(e.Graphics, e);
                    break;
                case DosFormat.CotnentType.Duplicate:
                    string stringText = PrintUtils.Replicate(citem.Text, citem.Length);
                    DisplayText(citem, stringText, citem.Length, defaultHeight, citem.GetAlignment(), e);
                    break;
                case DosFormat.CotnentType.Word:
                    DisplayText(citem, citem.Text, citem.Length, citem.GetHeight(defaultHeight), citem.GetAlignment(), e);
                    break;
                case DosFormat.CotnentType.Image:
                    if(string.IsNullOrEmpty(citem.Source) && !string.IsNullOrEmpty(citem.Name))
                    {
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                        if (columnvalue != null)
                        {
                            citem.Source = columnvalue.ToString();
                            drawImage(citem);
                        }
                    }
                    else
                    {
                        drawImage(citem);
                    }
                    break;
                case DosFormat.CotnentType.Logo:
                    drawlogo(citem);
                    break;
                case DosFormat.CotnentType.Paragraph:
                    StringToPrint = citem.Text;
                    currentElement = new DesignerElement(citem);
                    currentElement.Width = citem.Length;
                    while (DisplayMultiLine(e)) ;
                    break;

            }
            currentX += citem.Length;
        }

        private void drawtable(DosFormat.Line line, PrintPageEventArgs e, float width)
        {
            //foreach (var citem in line.Items)
            //{
            //    if (currentX + citem.Length > endX)
            //        citem.Length = (int)(endX - currentX);

            //    if (citem.Length < 0)
            //        citem.Length = 0;
            //    DrawCell(citem.Length, defaultHeight);
            //    RenderContent(citem, DataSource, e, width);
            //}

            currentTableRow = 0;
            currentTable = new Table();
            foreach (var citem in line.Items)
            {
                if (citem.Type == DosFormat.CotnentType.Field)
                {
                    var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, StringAlignment.Near, citem.Total, citem.Length);
                    clmn.BodyFont = line.GetStyle();
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.Align == "right")
                    {
                        clmn.Alignment = StringAlignment.Far;
                    }
                    currentTable.Columns.Add(clmn);
                }
            }


            var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(ReportLibrary.DataSourceHelper.Eval(DataSource, line.field), "");
            var tableDataSource = ie.GetEnumerator();
            while (tableDataSource.MoveNext())
            {
                DataItem = tableDataSource.Current;
                var columnValues = new SortedList<int, object>();
                currentTable.Rows.Add(columnValues);
                foreach (var citem in line.Items)
                {
                    if (citem.Type == DosFormat.CotnentType.Field)
                    {
                        object currentvalue = null;
                        if (citem.Format.Length > 0)
                            currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                        else
                            currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);

                        columnValues.Add(columnValues.Count, currentvalue);
                    }
                }
            }
            RenderTable(e, 0, currentX);
        }

        private void drawImage(DosFormat.Content citem)
        {
            var parts = citem.Text.Split(',');
            
            if (parts.Length == 4 && Additional != null && Additional.ContainsKey("IMG-" + citem.Source))
            {

                Rectangle bodyrectanglen = new Rectangle(int.Parse(parts[0]) + (int)currentX, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));

                PDFGraphic.DrawImageUnscaled(Additional["IMG-" + citem.Source] as Image, bodyrectanglen);
            }
        }



        private void drawlogo(DosFormat.Content citem)
        {
            var parts = citem.Text.Split(',');
            if (parts.Length == 4 && Additional != null && Additional.ContainsKey("LOGO"))
            {

                Rectangle bodyrectanglen = new Rectangle(int.Parse(parts[0]) + (int)currentX, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));

                PDFGraphic.DrawImageUnscaled(Additional["LOGO"] as Image, bodyrectanglen);
            }
        }

        private void PrintHeader(PrintPageEventArgs e, DosFormat.Header header, float left, float width)
        {
            foreach (var item in header.Items)
            {
                bool hide = false;
                if (!string.IsNullOrEmpty(item.Show))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Show);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.Hide))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Hide);
                    if (show != null && (bool)show)
                        hide = true;
                }
                if (hide) continue;
                if (item.IsTable)
                {
                    drawtable(item, e, width);
                }
                else
                {
                    foreach (var citem in item.Items)
                    {
                        hide = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            if (show != null && !(bool)show)
                                hide = true;
                        }
                        if (!hide && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                hide = true;
                        }
                        if (hide) continue;
                        RenderContent(citem, DataSource, e, width);
                    }
                }
                currentY += defaultHeight;
                currentX = left;
            }

        }
        private void PrintFooter(PrintPageEventArgs e, DosFormat.Footer footer, float left, float width)
        {
            if (footer == null)
                return;
            currentX = left;
            foreach (var item in footer.Items)
            {
                bool hide = false;
                if (!string.IsNullOrEmpty(item.Show))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Show);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.Hide))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Hide);
                    if (show != null && (bool)show)
                        hide = true;
                }
                if (hide) continue;

                if (item.IsTable)
                {
                    drawtable(item, e, width);
                }
                else
                {
                    foreach (var citem in item.Items)
                    {
                        hide = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            if (show != null && !(bool)show)
                                hide = true;
                        }
                        if (!hide && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                hide = true;
                        }
                        if (hide) continue;
                        RenderContent(citem, DataSource, e, width);
                    }
                }
                currentY += defaultHeight;
                currentX = left;
            }

        }
    }
}
