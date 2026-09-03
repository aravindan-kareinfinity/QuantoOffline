using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Drawing.Printing;
using PDFGraphics;

namespace ReportLibrary
{

    public class DOSReporter : IPDFReport
    {
        public PdfSharp.Drawing.XGraphicsUnit Unit { get; set; }
        public enum RangeAbnormality
        {
            Normal,
            Abnormal,
            Higher,
            Lower
        }
        public bool FilterAlternate { get; set; }
        public int TotaPages { get; set; }
        internal int AutoFixColumn = -1;
        internal Table currentTable = null;
        internal bool isTablePrinting = false;
        internal int currentTableRow = 0;
        internal StringFormat stringFormat = new StringFormat();
        internal int currentRow = 0;
        internal int currentIndex = 0;
        internal bool ShowGrandTotal = true;
        DateTime MinDateValue;
        private bool enableAutoPageSettings;
        private string autoPageSettingsKey;
        //private PageSettings defaultPageSetting = null;
        PageSettings defaultPage = null;
        private bool isUserSpecif;
        /*public void SetAutoPageSetting(bool enableAutoPageSettings, string autoPageSettingsKey, PageSettings defaultPageSetting, bool isUserSpecif)
        {
            this.enableAutoPageSettings=enableAutoPageSettings;
            this.autoPageSettingsKey=autoPageSettingsKey;
            defaultPage = defaultPageSetting;
            this.isUserSpecif = isUserSpecif;
            this.OnPageSettingChanged += new ClinicalPrint.PageSettingChanged(Reporter_OnPageSettingChanged);
            LoadDefaultSettings();
        }*/

        private string GetProfileConfig(string key)
        {
            return null;
        }

        private void LoadDefaultSettings()
        {

            string key = autoPageSettingsKey;

            string margin = GetProfileConfig(key);
            if (!string.IsNullOrEmpty(margin))
            {
                try
                {
                    System.Drawing.Printing.PageSettings ps = new System.Drawing.Printing.PageSettings();
                    System.Drawing.Printing.Margins margino = new System.Drawing.Printing.Margins();
                    string[] ms = margin.Split(',');
                    margino = new System.Drawing.Printing.Margins(int.Parse(ms[0]), int.Parse(ms[1]), int.Parse(ms[2]), int.Parse(ms[3]));
                    if (ms.Length == 8)
                        ps.PaperSize = new PaperSize(ms[5], int.Parse(ms[6]), int.Parse(ms[7]));
                    ps.Margins = margino;
                    ps.Landscape = ms[4].ToLower() == "true";
                    defaultPage = ps;
                }
                catch (Exception epx)
                {
                }
            }
        }


        public void SetAutoPageSetting(bool enableAutoPageSettings, string autoPageSettingsKey, bool isUserSpecif)
        {
            this.enableAutoPageSettings = enableAutoPageSettings;
            this.autoPageSettingsKey = autoPageSettingsKey;
            this.isUserSpecif = isUserSpecif;
            LoadDefaultSettings();
        }

        void Reporter_OnPageSettingChanged(PageSettings pageSetting)
        {
            string key = autoPageSettingsKey;
            //if (isUserSpecif) key = key + "-" + ClinicalPlus.Common.ApplicationContext.Current.UserID.ToString().Trim();
            ////key = key + "-" + pageSetting.PrinterSettings.PrinterName;

            //if (key.Length > 20) key = key.Substring(0, 20);

            //string margin = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", pageSetting.Margins.Left,
            //pageSetting.Margins.Right, pageSetting.Margins.Top, pageSetting.Margins.Bottom, pageSetting.Landscape, pageSetting.PaperSize.PaperName, pageSetting.PaperSize.Width, pageSetting.PaperSize.Height);
            //ClinicalPlus.Common.DBLookupUtils.SetProfileConfig(key, margin);

            if (defaultPage != null)
            {

                defaultPage.Margins = pageSetting.Margins;
                defaultPage.PaperSize = pageSetting.PaperSize;
                defaultPage.Landscape = pageSetting.Landscape;
            }
        }

        //public override System.Windows.Forms.ListViewItem ListViewTitle()
        //{
        //    ListViewItem lvi = new ListViewItem(Columns[0].Header);
        //    for (int i = 1; i < Columns.Count; i++)
        //    {
        //        lvi.SubItems.Add(Columns[i].Header);
        //    }
        //    return lvi;
        //}
        long organizationid;
        public DOSReporter(long organizationid)
        {
            this.organizationid = organizationid;
        }

        public DOSReporter(long organizationid,PageSettings defPage)
        {
            this.organizationid = organizationid;
            defaultPage = defPage;
        }


        public virtual void OnBeginPrint(PrintEventArgs e)
        {
            if (defaultPage != null)
            {
                //DefaultPageSettings.PaperSize = defaultPage.PaperSize;
                //DefaultPageSettings.Margins = defaultPage.Margins;
                //DefaultPageSettings.Landscape = defaultPage.Landscape;
                //if (this.PrinterSettings != null)
                //{
                //    this.PrinterSettings.DefaultPageSettings.PaperSize = defaultPage.PaperSize;
                //    this.PrinterSettings.DefaultPageSettings.Margins = defaultPage.Margins;
                //    this.PrinterSettings.DefaultPageSettings.Landscape = defaultPage.Landscape;
                //}
            }
        }


        public bool RenderTable(PrintPageEventArgs e, int mLines, float _currentX)
        {
            float titleRowHeight = GetRowHeight(TotalStyle.Font, e);
            return RenderTable(e, mLines, _currentX, titleRowHeight);
        }

        public bool RenderTable(PrintPageEventArgs e, int mLines, float _currentX, float titleRowHeight)
        {
            float bodyRowHeight = GetRowHeight(BodyStyle.Font, e);
            if (titleRowHeight == 0) titleRowHeight = bodyRowHeight;
            return RenderTable(e, mLines, _currentX, titleRowHeight, bodyRowHeight);
        }

        

        public bool RenderTable(PrintPageEventArgs e, int mLines, float _currentX, float titleRowHeight, float bodyRowHeight)
        {
            if (bodyRowHeight == 0)
                bodyRowHeight = GetRowHeight(BodyStyle.Font, e);

            if (titleRowHeight == 0)
                titleRowHeight = GetRowHeight(TotalStyle.Font, e);

            if (currentTable.config != null)
            {
                mLines = currentTable.config.mLines;
                _currentX = currentTable.config._currentX;
                titleRowHeight = currentTable.config.titleRowHeight;
                bodyRowHeight = currentTable.config.bodyRowHeight;
            }

            Pen vbPen = new Pen(Color.Black);
            int resumeIndex = currentTableRow;
            if (currentY + (currentTable.Total.Count * titleRowHeight) +
                (5 * bodyRowHeight) > e.MarginBounds.Bottom)
                return true;

            currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
            string lastGroup = null;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (currentTable.ShowHeader)
            {
                RectangleF headrectangle;
                for (int i = 0; i < currentTable.Columns.Count; i++)
                {
                    Column dcolumn = currentTable.Columns[i];
                    if (i != currentTable.GroupColumn && i != currentTable.BottomColumn)
                    {
                        headrectangle = new RectangleF(currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);

                        if (dcolumn.MergeCell > 0)
                        {
                            float mergecellwidth = 0;
                            int mergecell = dcolumn.MergeCell;
                            for (int mi = 0; mi < mergecell; mi++)
                            {
                                mergecellwidth += currentTable.Columns[i + mi].ColumnWidth;
                            }

                            headrectangle = new RectangleF(currentX, currentY, mergecellwidth, titleRowHeight / 2);
                            if (ShowGrid)
                            {
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, mergecellwidth, titleRowHeight);
                                eGraphics.DrawRectangle(Pens.Black, currentX, currentY, mergecellwidth, titleRowHeight);
                            }

                            stringFormat.Alignment = StringAlignment.Center;

                            eGraphics.DrawString(dcolumn.MergeText, dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont, HeaderStyle.Brush, headrectangle, stringFormat);


                            for (int mi = 0; mi < mergecell; mi++)
                            {
                                dcolumn = currentTable.Columns[i + mi];
                                headrectangle = new RectangleF(currentX, currentY + titleRowHeight / 2, dcolumn.ColumnWidth, titleRowHeight / 2);
                                stringFormat.Alignment = currentTable.ShowHeaderAsItem ? StringAlignment.Center : dcolumn.DataAlignment;

                                eGraphics.DrawString(dcolumn.Header, dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont, HeaderStyle.Brush, headrectangle, stringFormat);


                                eGraphics.DrawRectangle(Pens.Black, headrectangle.X, headrectangle.Y, headrectangle.Width, headrectangle.Height);

                                currentX = currentX + headrectangle.Width;
                            }
                            i += (mergecell - 1);
                        }
                        else
                        {
                            if (ShowGrid)
                            {
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);
                                eGraphics.DrawRectangle(Pens.Black, currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);
                            }
                            stringFormat.Alignment = currentTable.ShowHeaderAsItem ? StringAlignment.Center : dcolumn.DataAlignment;
                            float rstWidth = GetColumnWidth(dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont,
                                e.Graphics, dcolumn.Header);
                            if (currentTable.AutoFitHeight && rstWidth > headrectangle.Width)
                            {
                                Font hfont = dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont;
                                while (rstWidth > headrectangle.Width)
                                {
                                    hfont = new Font(hfont.Name, (float)(hfont.Size - 0.5), hfont.Style);
                                    rstWidth = GetColumnWidth(hfont, e.Graphics, dcolumn.Header);
                                }
                                eGraphics.DrawString(dcolumn.Header, hfont, HeaderStyle.Brush, headrectangle, stringFormat);
                            }
                            else
                            {
                                eGraphics.DrawString(dcolumn.Header, dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont, HeaderStyle.Brush, headrectangle, stringFormat);
                            }
                            currentX = currentX + headrectangle.Width;
                        }
                    }
                }
                currentX = _currentX;
                if (currentTable.ShowHeaderBottomLine && !ShowGrid)
                {
                    float totalColumnWidth = 0;
                    for (int i = 0; i < currentTable.Columns.Count; i++)
                        totalColumnWidth += (currentTable.BottomColumn == i ? 0 : currentTable.Columns[i].ColumnWidth);
                    eGraphics.DrawRectangle(Pens.Black, currentX, currentY + titleRowHeight, totalColumnWidth, 1);
                }
                currentY = currentY + titleRowHeight;
            }

            bool isOddRow = true;
            int startedcurrentTableRow = currentTableRow + mLines > currentTable.Rows.Count ? (currentTableRow + mLines - currentTable.Rows.Count) : 0;
            int maxRows = currentTableRow + mLines;
            for (; currentTableRow < currentTable.Rows.Count; currentTableRow++)
            {

                if (currentTable.GroupColumn >= 0)
                {
                    currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;

                    if (currentTable.Rows[currentTableRow][currentTable.GroupColumn] != null &&
                        currentTable.Rows[currentTableRow][currentTable.GroupColumn].ToString() != lastGroup)
                    {
                        isOddRow = !isOddRow;
                        RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentTable.Columns[currentTable.GroupColumn].ColumnWidth, titleRowHeight);
                        if (ShowGrid)
                        {
                            if (isOddRow)
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, currentTable.Columns[currentTable.GroupColumn].ColumnWidth, titleRowHeight);
                            eGraphics.DrawRectangle(vbPen, currentX, currentY, currentTable.Columns[currentTable.GroupColumn].ColumnWidth, titleRowHeight);
                        }
                        stringFormat.Alignment = StringAlignment.Near;
                        eGraphics.DrawString(currentTable.Rows[currentTableRow][currentTable.GroupColumn].ToString(),
                           currentTable.Columns[currentTable.GroupColumn].HeaderFont != null ?
                           currentTable.Columns[currentTable.GroupColumn].HeaderFont : HeaderStyle.Font,
                            HeaderStyle.Brush, bodyrectangle, stringFormat);
                        currentY = currentY + titleRowHeight;
                    }
                    lastGroup = currentTable.Rows[currentTableRow][currentTable.GroupColumn] == null ? "" : currentTable.Rows[currentTableRow][currentTable.GroupColumn].ToString();
                }

                isOddRow = !isOddRow;
                currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
                bool printExtraLine = currentTable.ExtraLineSpace != null && currentTable.ExtraLineSpace.Contains(currentTableRow);
                float rowHeight = bodyRowHeight;

                if (currentTable.AutoFitHeight)
                {
                    for (int j = 0; j < currentTable.Columns.Count; j++)
                    {
                        Column dcolumn = currentTable.Columns[j];
                        if (dcolumn.DataType == DataType.Abnormality || dcolumn.DataType == DataType.Table) continue;
                        if (currentTable.GroupColumn == j || currentTable.BottomColumn == j) continue;

                        object columnvalue = currentTable.Rows[currentTableRow][j];
                        if (columnvalue == null || columnvalue.ToString() == "") continue;


                        int charactersOnPage = 0;
                        int linesPerPage = 0;
                        // Sets the value of charactersOnPage to the number of characters 
                        // of stringToPrint that will fit within the bounds of the page.
                        SizeF sf = new SizeF(dcolumn.ColumnWidth, e.MarginBounds.Bottom - currentY);
                        SizeF psf = eGraphics.MeasureString(columnvalue.ToString(),
                            dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, sf, stringFormat,
                            out charactersOnPage, out linesPerPage);

                        if (linesPerPage > 1 && rowHeight < psf.Height)
                        {
                            rowHeight = psf.Height;
                            if (currentY + rowHeight + bodyRowHeight > e.MarginBounds.Bottom)
                            {
                                if (resumeIndex != currentTableRow)
                                    return true;
                            }
                        }
                    }
                }


                decimal columndvalue = 0;
                for (int j = 0; j < currentTable.Columns.Count; j++)
                {
                    Column dcolumn = currentTable.Columns[j];
                    if (currentTable.GroupColumn == j || currentTable.BottomColumn == j) continue;

                    object columnvalue = currentTable.Rows[currentTableRow][j];
                    if (dcolumn.ShowTotal && columnvalue != null)
                    {
                        if (!currentTable.Total.ContainsKey(j)) currentTable.Total.Add(j, 0);
                        if (decimal.TryParse(columnvalue.ToString(), out columndvalue))
                            currentTable.Total[j] = currentTable.Total[j] + columndvalue;
                    }

                    if (columnvalue == null)
                        columnvalue = "";

                    RectangleF bodyrectangle = new RectangleF(currentX, currentY, dcolumn.ColumnWidth, rowHeight);

                    if (ShowGrid)
                    {
                        if (isOddRow)
                            eGraphics.FillRectangle(Brushes.White, currentX, currentY, dcolumn.ColumnWidth, rowHeight);
                        eGraphics.DrawRectangle(vbPen, currentX, currentY, dcolumn.ColumnWidth, rowHeight);
                    }
                    stringFormat.Alignment = dcolumn.DataAlignment;

                    if (!string.IsNullOrEmpty(dcolumn.Format))
                        columnvalue = String.Format("{0:" + dcolumn.Format + "}", columnvalue);

                    if (dcolumn.DataType == DataType.Number && columnvalue.ToString() == "0") columnvalue = "";

                    if (currentTable.StrikeoutRows != null &&
                        currentTable.StrikeoutRows.ContainsKey(currentTableRow) && currentTable.StrikeoutRows[currentTableRow].Contains(j))
                    {
                        Font strikfont = new Font(
                            dcolumn.HeaderFont != null ? dcolumn.BodyFont : BodyStyle.Font, BodyStyle.Font.Style | FontStyle.Strikeout);
                        eGraphics.DrawString(columnvalue.ToString(), strikfont, BodyStyle.Brush, bodyrectangle, stringFormat);
                    }
                    else if (currentTable.MergeColumns != null &&
                        currentTable.MergeColumns.ContainsKey(currentTableRow))
                    {
                        if (currentTable.MergeColumns[currentTableRow].Count == 0)
                        {
                            bodyrectangle = new RectangleF(currentX, currentY, currentTable.Width, rowHeight);
                            if (isOddRow)
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, bodyrectangle.Width, rowHeight);
                            else
                                eGraphics.FillRectangle(Brushes.White, currentX, currentY, bodyrectangle.Width, rowHeight);

                            eGraphics.DrawRectangle(vbPen, currentX, currentY, bodyrectangle.Width, rowHeight);

                            eGraphics.DrawString(columnvalue.ToString(),
                                dcolumn.HeaderFont != null ? dcolumn.HeaderFont :
                                HeaderStyle.Font, HeaderStyle.Brush, bodyrectangle, stringFormat);
                            break;
                        }
                    }
                    else
                    {
                        if (dcolumn.DataType == DataType.Abnormality)
                        {
                            Font webdings = new Font("Wingdings", 11f);
                            RangeAbnormality rb = (RangeAbnormality)columnvalue;
                            string dispString = "αβ¬"; //high,low,abnormal
                            stringFormat.Alignment = StringAlignment.Center;
                            switch (rb)
                            {
                                case RangeAbnormality.Abnormal:
                                    dispString = "¬";
                                    break;
                                case RangeAbnormality.Higher:
                                    dispString = "α";
                                    break;
                                case RangeAbnormality.Lower:
                                    dispString = "β";
                                    break;
                                case RangeAbnormality.Normal:
                                    dispString = " ";
                                    break;
                            }
                            eGraphics.DrawString(dispString, webdings, BodyStyle.Brush, bodyrectangle, stringFormat);
                        }
                        else
                        if (dcolumn.DataType == DataType.Image)
                        {
                            if (columnvalue != null && !string.IsNullOrEmpty(columnvalue.ToString()))
                            {
                                string imgid = columnvalue.ToString();
                                throw new Exception("Image missing");
                                //var file = imageData.GetData(imgid.LongFromBase62());
                                //var img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(file.data));
                                //eGraphics.DrawImage(img, bodyrectangle);
                            }
                        }
                        if (dcolumn.DataType == DataType.Table)
                        {
                            RenderChildTable(e, eGraphics, bodyrectangle, columnvalue as ReportLibrary.DOSReporter.Table);
                        }
                        else
                        {
                            if (stringFormat.LineAlignment == StringAlignment.Center && rowHeight > bodyRowHeight)
                            {

                                int charactersOnPage = 0;
                                int linesPerPage = 0;
                                // Sets the value of charactersOnPage to the number of characters 
                                // of stringToPrint that will fit within the bounds of the page.
                                SizeF sf = new SizeF(bodyrectangle.Width, rowHeight);
                                SizeF psf = eGraphics.MeasureString(columnvalue.ToString(),
                                    dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, sf, stringFormat,
                                    out charactersOnPage, out linesPerPage);

                                if(linesPerPage>1)
                                {
                                    bool dospecialbreak = false;
                                    string svalue = columnvalue.ToString();
                                    var listofstrings = svalue.Split(new string[] { " ", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach(var str in listofstrings)
                                    {
                                        psf = eGraphics.MeasureString(columnvalue.ToString(),
                                    dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, sf, stringFormat,
                                    out charactersOnPage, out linesPerPage);
                                        if (linesPerPage > 1) dospecialbreak = true;
                                    }
                                    if(dospecialbreak)
                                    {
                                        var newValue = "";
                                        sf = new SizeF(bodyrectangle.Width, bodyRowHeight);
                                        while (!string.IsNullOrEmpty(svalue))
                                        {
                                            psf = eGraphics.MeasureString(svalue,
                                                    dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, sf, stringFormat,
                                                    out charactersOnPage, out linesPerPage);
                                            newValue += (newValue != "" ? " " : "") + svalue.Substring(0, charactersOnPage);
                                            if (svalue.Length == charactersOnPage)
                                                svalue = "";
                                            else
                                                svalue = svalue.Substring(charactersOnPage);
                                        }
                                        columnvalue = newValue;
                                    }
                                }

                                StringFormat format = new StringFormat(stringFormat);
                                format.LineAlignment = StringAlignment.Near;

                                bodyrectangle.Height = rowHeight;
                                eGraphics.DrawString(columnvalue.ToString(),
                                   dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, BodyStyle.Brush,
                                   bodyrectangle, format);

                            }
                            else
                            {
                                eGraphics.DrawString(columnvalue.ToString(),
                                   dcolumn.BodyFont != null ? dcolumn.BodyFont : BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                            }
                        }
                    }

                    if (printExtraLine && columnvalue.ToString().IndexOf("\r\n") > 0)
                    {
                        bodyrectangle.Y = bodyrectangle.Y + bodyrectangle.Height;
                        columnvalue = columnvalue.ToString().Substring(columnvalue.ToString().IndexOf("\r\n") + 2);
                        eGraphics.DrawString(columnvalue.ToString(), new Font("verdana", (float)8), BodyStyle.Brush, bodyrectangle, stringFormat);
                    }
                    currentX = currentX + bodyrectangle.Width;
                }
                currentY = currentY + rowHeight;




                if (currentTable.BottomColumn >= 0)
                {
                    object columnvalue = currentTable.Rows[currentTableRow][currentTable.BottomColumn];
                    if (columnvalue == null)
                        columnvalue = "";


                    if (!string.IsNullOrEmpty(columnvalue.ToString()))
                    {
                        float totalColumnWidth = 0;
                        for (int i = 0; i < currentTable.Columns.Count; i++)
                        {
                            if (i != currentTable.GroupColumn && i != currentTable.BottomColumn)
                                totalColumnWidth += currentTable.Columns[i].ColumnWidth;
                        }

                        currentX = _currentX;
                        RectangleF bodyrectangle = new RectangleF(currentX, currentY, totalColumnWidth, bodyRowHeight);
                        if (currentTable.Columns[currentTable.BottomColumn].AddExtraGap)
                        {
                            bodyrectangle = new RectangleF(bodyrectangle.Left, bodyrectangle.Top, bodyrectangle.Width, bodyrectangle.Height + (bodyrectangle.Height / 10));
                        }

                        if (ShowGrid)
                        {
                            if (isOddRow)
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, totalColumnWidth, bodyRowHeight);
                            eGraphics.DrawRectangle(vbPen, currentX, currentY, totalColumnWidth, bodyRowHeight);
                        }
                        stringFormat.Alignment = StringAlignment.Near;

                        if (currentTable.Columns[0].Name == "S.No")
                        {
                            currentX = currentX + currentTable.Columns[0].ColumnWidth;
                            bodyrectangle = new RectangleF(currentX, currentY, totalColumnWidth - currentTable.Columns[0].ColumnWidth, bodyrectangle.Height);
                        }



                        if (currentTable.Columns[currentTable.BottomColumn].BodyFont != null)
                            eGraphics.DrawString(columnvalue.ToString(), currentTable.Columns[currentTable.BottomColumn].BodyFont, BodyStyle.Brush, bodyrectangle, stringFormat);
                        else
                            eGraphics.DrawString(columnvalue.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);

                        currentY = currentY + bodyrectangle.Height;
                    }
                    else
                    {
                        if (printExtraLine) currentY = currentY + bodyRowHeight;
                    }
                }
                else
                {
                    if (printExtraLine) currentY = currentY + bodyRowHeight;
                }

                if (currentY + bodyRowHeight > e.MarginBounds.Bottom || maxRows == (currentTableRow + 1))
                {
                    currentTableRow++;
                    return true;
                }
            }

            if (startedcurrentTableRow > 0)
            {
                for (var drowindex = 0; drowindex < startedcurrentTableRow; drowindex++)
                {
                    currentX = _currentX;
                    for (int j = 0; j < currentTable.Columns.Count; j++)
                    {
                        Column dcolumn = currentTable.Columns[j];
                        if (currentTable.GroupColumn == j || currentTable.BottomColumn == j) continue;

                        RectangleF bodyrectangle = new RectangleF(currentX, currentY, dcolumn.ColumnWidth, bodyRowHeight);

                        if (isOddRow)
                            eGraphics.FillRectangle(Brushes.White, currentX, currentY, dcolumn.ColumnWidth, bodyRowHeight);
                        eGraphics.DrawRectangle(vbPen, currentX, currentY, dcolumn.ColumnWidth, bodyRowHeight);
                        currentX += bodyrectangle.Width;
                    }
                    currentY += bodyRowHeight;
                }
            }
            if (currentTable.Total.Count > 0)
            {
                currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
                float totalColumnWidth = 0;
                int i = 0;
                for (i = 0; i < currentTable.Columns.Count; i++)
                {
                    if (currentTable.Columns[i].ShowTotal) break;
                    totalColumnWidth += currentTable.Columns[i].ColumnWidth;
                }
                if (ShowGrid)
                {
                    eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, totalColumnWidth, bodyRowHeight);
                    eGraphics.DrawRectangle(vbPen, currentX, currentY, totalColumnWidth, bodyRowHeight);
                }
                stringFormat.Alignment = StringAlignment.Near;

                RectangleF bodyrectangle = new RectangleF(currentX, currentY, totalColumnWidth, bodyRowHeight);
                eGraphics.DrawString("Total", TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);

                currentX = currentX + totalColumnWidth;
                stringFormat.Alignment = StringAlignment.Far;
                for (; i < currentTable.Columns.Count; i++)
                {
                    if (i == currentTable.GroupColumn || i == currentTable.BottomColumn) continue;
                    eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                    eGraphics.DrawRectangle(vbPen, currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                    if (currentTable.Columns[i].ShowTotal)
                    {
                        string displayText = "";
                        if (currentTable.Total.ContainsKey(i))
                            displayText = currentTable.Columns[i].Format.Length > 0 ? currentTable.Total[i].ToString(currentTable.Columns[i].Format) : currentTable.Total[i].ToString();
                        bodyrectangle = new RectangleF(currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                        eGraphics.DrawString(displayText, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
                    }
                    currentX = currentX + currentTable.Columns[i].ColumnWidth;
                }
                currentY = currentY + bodyRowHeight;
            }

            return false;
        }

        private void RenderChildTable(PrintPageEventArgs e, IGraphic eGraphics, RectangleF bodyrectangle, Table table)
        {
            float titleRowHeight = bodyrectangle.Height / 2;
            float bodyRowHeight = bodyrectangle.Height / 2;
            float x = currentX;
            float y = currentY;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column dcolumn = table.Columns[i];
                RectangleF headrectangle = new RectangleF(x, y, (float)dcolumn.ColumnWidth, titleRowHeight);
                eGraphics.FillRectangle(Brushes.LightGray, x, y, (float)dcolumn.ColumnWidth, titleRowHeight);
                eGraphics.DrawRectangle(Pens.Black, x, y, (float)dcolumn.ColumnWidth, titleRowHeight);
                stringFormat.Alignment = StringAlignment.Center;
                eGraphics.DrawString(dcolumn.Header, dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont, HeaderStyle.Brush, headrectangle, stringFormat);
                x = x + headrectangle.Width;
            }
            y += titleRowHeight;
            foreach (var row in table.Rows)
            {
                x = currentX;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column dcolumn = table.Columns[i];
                    RectangleF headrectangle = new RectangleF(x, y, (float)dcolumn.ColumnWidth, bodyRowHeight);
                    eGraphics.DrawRectangle(Pens.Black, x, y, (float)dcolumn.ColumnWidth, bodyRowHeight);
                    stringFormat.Alignment = dcolumn.DataAlignment;
                    var displayvalue = string.IsNullOrEmpty(dcolumn.Format) ? row[i].ToString() : (String.Format("{0:" + dcolumn.Format + "}", row[i]));
                    eGraphics.DrawString(displayvalue, dcolumn.BodyFont == null ? BodyStyle.Font : dcolumn.BodyFont, HeaderStyle.Brush, headrectangle, stringFormat);
                    x = x + headrectangle.Width;
                }
                y += bodyRowHeight;
            }
        }

        public bool RenderBillTable(PrintPageEventArgs e, int height, float _currentX)
        {
            Pen vbPen = new Pen(Color.Black);
            float titleRowHeight = GetRowHeight(TotalStyle.Font, e);
            float bodyRowHeight = GetRowHeight(BodyStyle.Font, e);
            if (currentY + (currentTable.Total.Count * titleRowHeight) +
                (3 * bodyRowHeight) > e.MarginBounds.Bottom)
                return true;

            float tableWidth = e.MarginBounds.Right - _currentX;
            int tableFullWidth = 0;

            currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
            float maxY = currentY + height;
            if (currentTable.ShowHeader)
            {
                for (int i = 0; i < currentTable.Columns.Count; i++)
                {
                    Column dcolumn = currentTable.Columns[i];
                    if (i != currentTable.GroupColumn && i != currentTable.BottomColumn)
                    {
                        //dcolumn.ColumnWidth = (int) (tableWidth * dcolumn.ColumnWidth / 100);
                        tableFullWidth += dcolumn.ColumnWidth;
                        RectangleF headrectangle = new RectangleF(currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);
                        eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);
                        eGraphics.DrawRectangle(Pens.Black, currentX, currentY, (float)dcolumn.ColumnWidth, titleRowHeight);
                        stringFormat.Alignment = currentTable.ShowHeaderAsItem ? StringAlignment.Center : dcolumn.DataAlignment;
                        eGraphics.DrawString(dcolumn.Header, dcolumn.HeaderFont == null ? HeaderStyle.Font : dcolumn.HeaderFont, HeaderStyle.Brush, headrectangle, stringFormat);
                        currentX = currentX + headrectangle.Width;
                    }
                }
                currentY = currentY + titleRowHeight;
            }
            float headerStartY = currentY;

            for (; currentTableRow < currentTable.Rows.Count; currentTableRow++)
            {
                currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
                for (int j = 0; j < currentTable.Columns.Count; j++)
                {
                    Column dcolumn = currentTable.Columns[j];
                    if (currentTable.GroupColumn == j || currentTable.BottomColumn == j) continue;

                    object columnvalue = currentTable.Rows[currentTableRow][j];
                    if (columnvalue == null)
                        columnvalue = "";

                    RectangleF bodyrectangle = new RectangleF(currentX, currentY, dcolumn.ColumnWidth, bodyRowHeight);

                    stringFormat.Alignment = dcolumn.DataAlignment;
                    if (!string.IsNullOrEmpty(dcolumn.Format))
                        columnvalue = String.Format("{0:" + dcolumn.Format + "}", columnvalue);
                    if (dcolumn.DataType == DataType.Number && columnvalue.ToString() == "0") columnvalue = "";

                    eGraphics.DrawString(columnvalue.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);

                    currentX = currentX + bodyrectangle.Width;
                }
                currentY = currentY + bodyRowHeight;

                if (currentY + bodyRowHeight > maxY)
                {
                    currentTableRow++;
                    if (currentTable.Rows.Count > currentTableRow)
                    {
                        currentX = _currentX;
                        for (int j = 0; j < currentTable.Columns.Count; j++)
                        {
                            Column dcolumn = currentTable.Columns[j];
                            Rectangle bodyrectangle = new Rectangle((int)currentX, (int)headerStartY, dcolumn.ColumnWidth, (int)(currentY - headerStartY));
                            currentX = currentX + bodyrectangle.Width;
                            eGraphics.DrawRectangle(Pens.Black, bodyrectangle);
                        }

                        RectangleF rectf = new RectangleF(_currentX, currentY + bodyRowHeight, tableFullWidth, bodyRowHeight);
                        StringFormat sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        eGraphics.DrawString("Continue...", BodyStyle.Font, BodyStyle.Brush, rectf, sf);

                        return true;
                    }
                }
            }

            currentX = _currentX;
            for (int j = 0; j < currentTable.Columns.Count; j++)
            {
                Column dcolumn = currentTable.Columns[j];
                Rectangle bodyrectangle = new Rectangle((int)currentX, (int)headerStartY, dcolumn.ColumnWidth, height + 1);
                currentX = currentX + bodyrectangle.Width;
                eGraphics.DrawRectangle(Pens.Black, bodyrectangle);
            }
            currentY = headerStartY + height;
            if (currentTable.Total.Count > 0)
            {
                currentX = _currentX;// defaultLeft.Value + BillPrintInfo.PrintDesign[currentIndex].Left;
                float totalColumnWidth = 0;
                int i = 0;
                for (i = 0; i < currentTable.Columns.Count; i++)
                {
                    if (currentTable.Columns[i].ShowTotal) break;
                    totalColumnWidth += currentTable.Columns[i].ColumnWidth;
                }

                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, totalColumnWidth, bodyRowHeight);
                eGraphics.DrawRectangle(vbPen, currentX, currentY, totalColumnWidth, bodyRowHeight);

                stringFormat.Alignment = StringAlignment.Near;

                RectangleF bodyrectangle = new RectangleF(currentX, currentY, totalColumnWidth, bodyRowHeight);
                eGraphics.DrawString("Total", TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);

                currentX = currentX + totalColumnWidth;
                stringFormat.Alignment = StringAlignment.Far;
                for (; i < currentTable.Columns.Count; i++)
                {
                    eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                    eGraphics.DrawRectangle(vbPen, currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                    if (currentTable.Columns[i].ShowTotal)
                    {
                        string displayText = currentTable.Columns[i].Format.Length > 0 ? currentTable.Total[i].ToString(currentTable.Columns[i].Format) : currentTable.Total[i].ToString();
                        bodyrectangle = new RectangleF(currentX, currentY, currentTable.Columns[i].ColumnWidth, bodyRowHeight);
                        eGraphics.DrawString(displayText, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
                    }
                    currentX = currentX + currentTable.Columns[i].ColumnWidth;
                }
                currentY = currentY + bodyRowHeight;
            }

            return false;
        }


        public class Table
        {
            public class Config
            {
                public int mLines;
                public float _currentX;
                public float titleRowHeight;
                public float bodyRowHeight;
                public float _currentY;
            }

            public Config config = null;
            public bool ShowHeader = true;
            public bool ShowHeaderBottomLine = false;
            public List<Column> Columns = new List<Column>();
            public List<Title> ReportTitle = new List<Title>();
            public List<SortedList<int, object>> Rows = new List<SortedList<int, object>>();
            public int CurrentRow = 0;
            public int GroupColumn = -1;
            public int BottomColumn = -1;
            public bool RuntimeTotal = true;
            public SortedList<int, List<int>> MergeColumns;
            public SortedList<int, List<int>> StrikeoutRows;
            public List<int> ExtraLineSpace;
            public int Width = 0;
            public SortedList<int, Decimal> Total = new SortedList<int, decimal>();
            public bool ShowHeaderAsItem;
            public Column AddColumn(string name, string header, DataType datatype, string format, StringAlignment alignment
                    , bool showtotal, int columnWidth)
            {
                if (this.Columns.Count != GroupColumn && this.Columns.Count != BottomColumn)
                    Width = Width + columnWidth;

                Column clmn = new Column(name, header, datatype, format, alignment, showtotal, columnWidth);
                Columns.Add(clmn);
                if (clmn.ShowTotal) Total.Add(Columns.Count - 1, 0);
                return clmn;
            }

            public void AddTitle(string displayText, int row, int widthPercentage, TitleMode titlemode, StringAlignment alignment, Font font, Brush brush)
            {
                ReportTitle.Add(new Title(displayText, row, widthPercentage, titlemode, alignment, font, brush));
            }
            public void AddRow(object[] values)
            {
                SortedList<int, object> value = new SortedList<int, object>();
                for (int i = 0; i < values.Length; i++)
                {
                    if (Columns[i].ShowTotal && values[i] != null)
                    {
                        Total[i] = Total[i] + decimal.Parse(values[i].ToString());
                    }
                    value.Add(i, values[i]);
                }
                Rows.Add(value);
                RuntimeTotal = false;
            }

            public bool AutoFitHeight { get; set; }

            public int LabRowIndexReset { get; set; }
        }

        public enum TitleMode
        {
            ReportTitle,
            PageTitle,
            Both
        }
        public class Title
        {
            public string DisplayText = string.Empty;
            public int RowNumber;
            public StringAlignment Alignment = StringAlignment.Near;
            public Style Style = new Style();
            public int Width = 100;
            public TitleMode TitleMode = TitleMode.ReportTitle;
            public Title(string displayText, int row, int widthPercentage, TitleMode titlemode, StringAlignment alignment, Font font, Brush brush)
            {
                DisplayText = displayText;
                RowNumber = row;
                Alignment = alignment;
                Style = new Style();
                Style.Brush = brush;
                Style.Font = font;
                TitleMode = titlemode;
                Width = widthPercentage;
            }

        }

        public class Style : ICloneable
        {
            public Font Font = new Font("Verdana", 10f);
            public Brush Brush = Brushes.Black;
            public Style()
            {
            }
            public Style(string font, float fontsize, FontStyle style, Brush brush)
            {
                this.Font = new Font(font, fontsize, style);
                this.Brush = brush;
            }
            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        public enum DataType
        {
            String,
            Number,
            Date,
            Abnormality,
            Image,
            Table
        }

        public enum ColumnAlignment
        {
            Left,
            Right,
            Center
        }

        public class Column
        {
            public string Name = string.Empty;
            public string Header = string.Empty;
            public DataType DataType = DataType.String;
            public string Format = string.Empty;
            public StringAlignment Alignment = StringAlignment.Near;
            public bool ShowTotal = false;
            public int ColumnWidth = 25;
            public float PixelWidth = 0;
            public bool OwnerDraw = false;
            public bool IsSerialNo = false;
            public bool ShowEmptyForZero = false;
            public Font HeaderFont = null;
            public Font BodyFont = null;
            public bool AddExtraGap = false;
            public bool DetailsContent = false;
            public bool DetailsContentGrouping = false;
            public StringAlignment DataAlignment
            {
                get
                {
                    if (DataType == DOSReporter.DataType.Number || Alignment == StringAlignment.Far) return StringAlignment.Far;
                    return StringAlignment.Near;
                }
            }

            public int MergeCell { get; set; }
            public string MergeText { get; set; }

            public Column(string name, string header, DataType datatype, string format, StringAlignment alignment
                , bool showtotal, int columnWidth)
            {
                Name = name;
                Header = header;
                DataType = datatype;
                Format = format;
                Alignment = alignment;
                ShowTotal = showtotal;
                ColumnWidth = columnWidth;
            }
        }
        public Style HeaderStyle = new Style("verdana", 11f, FontStyle.Bold, Brushes.Black);
        public Style TotalStyle = new Style();
        public Style BodyStyle = new Style();
        public List<Column> Columns = new List<Column>();
        public List<Title> ReportTitle = new List<Title>();
        public List<Title> ReportFooter = new List<Title>();
        public SortedList<int, decimal> PageTotal = new SortedList<int, decimal>();
        public SortedList<int, decimal> GrandTotal = new SortedList<int, decimal>();
        public SortedList<string, Style> ColumnStyle = new SortedList<string, Style>();

        //Data retreival...
        public IEnumerable ie = null;
        public IEnumerator ien = null;
        public float currentX = 0;
        public float currentY = 0;
        public long SerialNumber = 0;
        public int currentpage = 1;
        public bool dataSourceInitialized = false;
        public bool ShowGrid = true;
        public bool ShowNullAsEmpty = false;
        private object dataSource = null;

        public object DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
                ie = null;
                ien = null;
            }
        }
        public float GetColumnWidth(Font fntReportBodyFont, Graphics g, string value)
        {
            return g.MeasureString(value, fntReportBodyFont).Width;
        }

        public bool ColumnWidthAsPercentage = false;
        public float GetColumnWidth(Font fntReportBodyFont, PrintPageEventArgs e, int noofChar)
        {
            if (ColumnWidthAsPercentage)
            {
                return GetPercentWidth(noofChar, e.MarginBounds);
            }
            return eGraphics.MeasureString(new string('W', noofChar), fntReportBodyFont).Width + 2;
        }
        public float GetRowHeight(Font fntReportBodyFont, PrintPageEventArgs e)
        {
            if (FixedRowHeight < 0)
                return eGraphics.MeasureString("W", fntReportBodyFont, 300).Height + 2;
            return FixedRowHeight;
        }

        public float FixedRowHeight = -1;

        #region Report Title

        public void AddFooter(string displayText, int row, int widthPercentage, TitleMode titlemode)
        {
            AddFooter(displayText, row, widthPercentage, titlemode, StringAlignment.Center, new Font("Verdana", 3f), Brushes.Black);
        }

        public void AddFooter(string displayText, int row, int widthPercentage, TitleMode titlemode, StringAlignment alignment, Font font, Brush brush)
        {
            ReportFooter.Add(new Title(displayText, row, widthPercentage, titlemode, alignment, font, brush));
        }

        public void AddTitle(string displayText, int row, int widthPercentage, TitleMode titlemode)
        {
            AddTitle(displayText, row, widthPercentage, titlemode, StringAlignment.Center, new Font("Verdana", 12f), Brushes.Black);
        }

        public void AddTitle(string displayText, int row, int widthPercentage, TitleMode titlemode, StringAlignment alignment, Font font, Brush brush)
        {
            Title title = new Title(displayText, row, widthPercentage, titlemode, alignment, font, brush);
            if (ReportTitle.Exists(e => e.RowNumber == title.RowNumber && e.Width == title.Width))
            {
                ReportTitle.RemoveAll(e => e.RowNumber == title.RowNumber && e.Width == title.Width);
            }
            ReportTitle.Add(title);
        }
        public Column AddColumn(string name, string header, DataType datatype, string format, StringAlignment alignment
                , bool showtotal, int columnWidth)
        {
            Column clmn = new Column(name, header, datatype, format, alignment, showtotal, columnWidth);
            Columns.Add(clmn);
            return clmn;
        }

        public Column AddColumn(string name, string header, DataType datatype, int columnWidth, string format)
        {
            return AddColumn(name, header, datatype, format, StringAlignment.Near, false, columnWidth);
        }

        public Column AddColumn(string name, string header, DataType datatype, int columnWidth)
        {
            return AddColumn(name, header, datatype, "", StringAlignment.Near, false, columnWidth);
        }

        public virtual void RenderTitle(System.Drawing.Printing.PrintPageEventArgs e, TitleMode titleMode)
        {
            RenderTitle(e, titleMode, true);
        }

        public virtual void RenderTitle(System.Drawing.Printing.PrintPageEventArgs e, TitleMode titleMode, bool resetX)
        {
            int currentRow = 1;
            currentX = e.MarginBounds.Left;
            if (resetX) currentY = e.MarginBounds.Top;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            float lastTitleHeight = 0;
            for (int i = 0; i < ReportTitle.Count; i++)
            {
                if (titleMode == TitleMode.PageTitle && ReportTitle[i].TitleMode == TitleMode.ReportTitle) continue;
                if (titleMode == TitleMode.ReportTitle && ReportTitle[i].TitleMode == TitleMode.PageTitle) continue;

                if (currentRow != ReportTitle[i].RowNumber)
                {
                    currentY = currentY + (GetRowHeight(ReportTitle[i].Style.Font, e) * (ReportTitle[i].RowNumber - currentRow));
                    currentX = e.MarginBounds.Left;
                }
                //titleRect = new RectangleF(startX + iReportLogoWidth , startY, e.MarginBounds.Width - LogoWidth, TitleHeight);
                RectangleF titlerec = new RectangleF(currentX, currentY, GetPercentWidth(ReportTitle[i].Width, e.MarginBounds), GetRowHeight(ReportTitle[i].Style.Font, e));
                currentRow = ReportTitle[i].RowNumber;
                stringFormat.Alignment = ReportTitle[i].Alignment;
                eGraphics.DrawString(ReportTitle[i].DisplayText, ReportTitle[i].Style.Font, ReportTitle[i].Style.Brush, titlerec, stringFormat);
                currentX = currentX + titlerec.Width;
                lastTitleHeight = titlerec.Height;
            }
            currentY = currentY + lastTitleHeight;
        }

        public virtual void RenderFooter(System.Drawing.Printing.PrintPageEventArgs e, TitleMode titleMode)
        {
            int currentRow = 1;
            currentX = e.MarginBounds.Left;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            float lastTitleHeight = 0;
            for (int i = ReportFooter.Count - 1; i >= 0; i--)
            {
                if (currentRow != ReportFooter[i].RowNumber)
                {
                    currentY = currentY - (GetRowHeight(ReportFooter[i].Style.Font, e) * (ReportFooter[i].RowNumber - currentRow));
                    currentX = e.MarginBounds.Left;
                }
                //titleRect = new RectangleF(startX + iReportLogoWidth , startY, e.MarginBounds.Width - LogoWidth, TitleHeight);
                RectangleF titlerec = new RectangleF(currentX, currentY, GetPercentWidth(ReportFooter[i].Width, e.MarginBounds), GetRowHeight(ReportFooter[i].Style.Font, e));
                currentRow = ReportFooter[i].RowNumber;
                stringFormat.Alignment = ReportFooter[i].Alignment;
                eGraphics.DrawString(ReportFooter[i].DisplayText, ReportFooter[i].Style.Font, ReportFooter[i].Style.Brush, titlerec, stringFormat);
                currentX = currentX + titlerec.Width;
                lastTitleHeight = titlerec.Height;
            }
        }

        public virtual void RenderHeader(System.Drawing.Printing.PrintPageEventArgs e)
        {
            RenderHeader(e, Columns);
        }

        private string lastDetailContent = null;
        private Column DetailContentColumn = null;
        public void RenderHeader(System.Drawing.Printing.PrintPageEventArgs e, List<Column> source)
        {
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            currentX = e.MarginBounds.Left;
            float headerHight = GetRowHeight(HeaderStyle.Font, e);

            if (AutoFixColumn >= 0)
            {
                float totalColSize = 0;
                for (int i = 0; i < source.Count; i++)
                {
                    Column dcolumn = source[i];
                    if (dcolumn.DetailsContent || i == AutoFixColumn) continue;
                    dcolumn.PixelWidth = GetColumnWidth(HeaderStyle.Font, e, dcolumn.ColumnWidth);
                    if (e.MarginBounds.Width < dcolumn.PixelWidth) dcolumn.PixelWidth = e.MarginBounds.Width;
                    totalColSize = totalColSize + dcolumn.PixelWidth;
                }

                if (e.MarginBounds.Width - totalColSize > 0)
                {
                    source[AutoFixColumn].PixelWidth = e.MarginBounds.Width - totalColSize;
                }
                else
                {
                    source[AutoFixColumn].PixelWidth = GetColumnWidth(HeaderStyle.Font, e, source[AutoFixColumn].ColumnWidth); ;
                }
            }

            for (int i = 0; i < source.Count; i++)
            {
                Column dcolumn = source[i];
                if (dcolumn.DetailsContent)
                {
                    if (dcolumn.DetailsContentGrouping)
                    {
                        lastDetailContent = null;
                        DetailContentColumn = dcolumn;
                    }
                    continue;
                }

                if (currentpage == 1)
                {
                    SerialNumber = 0;

                    if (AutoFixColumn < 0)
                    {
                        dcolumn.PixelWidth = GetColumnWidth(HeaderStyle.Font, e, dcolumn.ColumnWidth);
                        if (e.MarginBounds.Width < dcolumn.PixelWidth) dcolumn.PixelWidth = e.MarginBounds.Width;
                    }
                    if (dcolumn.ShowTotal && !GrandTotal.ContainsKey(i))
                    {
                        GrandTotal.Add(i, 0);
                        PageTotal.Add(i, 0);
                    }
                }
                //if(dcolumn.ShowTotal) PageTotal[i] = 0;
                RectangleF headrectangle = new RectangleF(currentX, currentY, dcolumn.PixelWidth, headerHight);
                if (ShowGrid)
                {
                    eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, dcolumn.PixelWidth, headerHight);
                    eGraphics.DrawRectangle(Pens.Black, currentX, currentY, dcolumn.PixelWidth, headerHight);
                }
                stringFormat.Alignment = StringAlignment.Center;
                eGraphics.DrawString(dcolumn.Header, HeaderStyle.Font, HeaderStyle.Brush, headrectangle, stringFormat);
                currentX = currentX + headrectangle.Width;
            }
            currentY = currentY + headerHight;
        }
        #endregion

        public int GetPercentWidth(int percent, Rectangle rectangle)
        {
            return rectangle.Width * percent / 100;
        }


        public bool InitDataSource()
        {
            if (dataSourceInitialized) return true;
            dataSourceInitialized = true;
            if (dataSource == null) return false;
            ie = DataSourceHelper.GetResolvedDataSource(dataSource, null);
            if (ie == null) return false;
            ien = ie.GetEnumerator();
            return true;
        }

        public decimal GetDecimalValue(string value)
        {
            decimal newValue = 0;
            if (decimal.TryParse(value, out newValue)) return newValue;
            return 0;
        }

        public virtual void RenderPageTotal(System.Drawing.Printing.PrintPageEventArgs e, SortedList<int, decimal> total)
        {
            RenderPageTotal(e, total, "Total");
        }
        public virtual void RenderPageTotal(System.Drawing.Printing.PrintPageEventArgs e, SortedList<int, decimal> total, string showTotalDisplay)
        {
            //if (total.Count == 0) return;
            currentX = e.MarginBounds.Left;
            Pen vbPen = new Pen(Color.Black);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            float bodyRowHeight = GetRowHeight(TotalStyle.Font, e);
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, GetColumnWidth(TotalStyle.Font, e, 8), bodyRowHeight);
            float totalColumnWidth = 0;
            int i = 0;
            for (i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].ShowTotal || Columns[i].DetailsContent) break;
                totalColumnWidth += Columns[i].PixelWidth;
            }

            //if (i == Columns.Count) return;
            if (ShowGrid)
            {
                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, totalColumnWidth, bodyRowHeight);
                eGraphics.DrawRectangle(vbPen, currentX, currentY, totalColumnWidth, bodyRowHeight);
            }
            stringFormat.Alignment = StringAlignment.Near;

            if (!string.IsNullOrEmpty(showTotalDisplay))
                eGraphics.DrawString(showTotalDisplay, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);

            currentX = currentX + totalColumnWidth;
            stringFormat.Alignment = StringAlignment.Far;
            for (; i < Columns.Count; i++)
            {
                if (Columns[i].DetailsContent) continue;
                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, Columns[i].PixelWidth, bodyRowHeight);
                eGraphics.DrawRectangle(vbPen, currentX, currentY, Columns[i].PixelWidth, bodyRowHeight);
                if (Columns[i].ShowTotal)
                {
                    string displayText = Columns[i].Format.Length > 0 ? total[i].ToString(Columns[i].Format) : total[i].ToString();
                    total[i] = 0;
                    bodyrectangle = new RectangleF(currentX, currentY, Columns[i].PixelWidth, bodyRowHeight);
                    eGraphics.DrawString(displayText, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
                }
                currentX = currentX + Columns[i].PixelWidth;
            }

        }
        public bool IsOverrided = false;
        public bool EnablePageNumber = true;
        public int PageNo;
        public int PageCount;
        PDFGraphics.IGraphic eGraphics;


        public void ShowPageNumber(bool islastPage, float bottommargin, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (!EnablePageNumber) return;
            if (islastPage && this.PageNo == 1) return;
            float bodyRowHeight = GetRowHeight(BodyStyle.Font, e);
            bottommargin = e.PageSettings.Margins.Bottom;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (currentY > e.PageBounds.Bottom - bottommargin)
            {
                bottommargin = currentY - (e.PageBounds.Bottom - bottommargin - 25);
            }
            RectangleF bodyrectangle = new RectangleF(0, e.PageBounds.Bottom - bottommargin - 25, e.PageBounds.Right, bottommargin);
            if (TotaPages <= 0)
            {
                eGraphics.DrawString(this.PageNo.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
            }

            stringFormat.Alignment = StringAlignment.Far;
            bodyrectangle = new RectangleF(e.MarginBounds.Left, e.PageBounds.Bottom - bottommargin - 25, e.MarginBounds.Right - e.MarginBounds.Left, bottommargin);


            if (!e.HasMorePages)
            {
                if (TotaPages <= 0)
                    eGraphics.DrawString("End.,", BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                else
                    eGraphics.DrawString(string.Format("{0}/{1}", PageNo, TotaPages), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                this.PageNo = 1;
            }
            else
            {
                if (TotaPages <= 0)
                    eGraphics.DrawString("Cont.,", BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                else
                    eGraphics.DrawString(string.Format("{0}/{1}", PageNo, TotaPages), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                this.PageNo++;
            }
        }

        public void ShowPageNumberInMargin(bool islastPage, float bottommargin, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (!EnablePageNumber) return;
            if (islastPage && this.PageNo == 1) return;
            float bodyRowHeight = GetRowHeight(BodyStyle.Font, e);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(0, e.MarginBounds.Bottom - bottommargin - 25, e.MarginBounds.Right, bottommargin);
            if (this.TotaPages <= 0)
            {
                eGraphics.DrawString(this.PageNo.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
            }

            stringFormat.Alignment = StringAlignment.Far;
            bodyrectangle = new RectangleF(e.MarginBounds.Left, e.MarginBounds.Bottom - bottommargin - 25, e.MarginBounds.Right - e.MarginBounds.Left, bottommargin);

            if (!e.HasMorePages)
            {
                if (TotaPages <= 0)
                    eGraphics.DrawString("End.,", BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                else
                    eGraphics.DrawString(string.Format("{0}/{1}", PageNo, TotaPages), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                this.PageNo = 1;
            }
            else
            {
                if (TotaPages <= 0)
                    eGraphics.DrawString("Cont.,", BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                else
                    eGraphics.DrawString(string.Format("{0}/{1}", PageNo, TotaPages), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                this.PageNo++;
            }
        }

        public PDFGraphics.IGraphic PDFGraphic
        {

            get
            {
                return eGraphics;
            }
            set
            {
                eGraphics = value;
            }
        }
        public virtual void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            float bodyRowHeight = GetRowHeight(BodyStyle.Font, e);
            if (IsOverrided)
            {
                ShowPageNumber(!e.HasMorePages, bodyRowHeight, e);
                return;
            }
            currentY = e.MarginBounds.Top;
            float endY = e.MarginBounds.Bottom;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            if (currentpage == 1)
                RenderTitle(e, TitleMode.ReportTitle);
            else
                RenderTitle(e, TitleMode.PageTitle);

            RenderHeader(e);

            if (!InitDataSource()) return;
            if (ien == null) return;
            Pen vbPen = new Pen(Color.Black);
            bool isOddRow = true;
            object columnvalue = null;
            object curobj = null;

            if (PageTotal.Count > 0) endY = endY - bodyRowHeight;
            while (ien.MoveNext())
            {
                SerialNumber++;

                curobj = ien.Current;
                currentX = e.MarginBounds.Left;

                if (DetailContentColumn != null)
                {
                    if (DetailContentColumn.Format.Length > 0)
                        columnvalue = DataSourceHelper.Eval(curobj, DetailContentColumn.Name, "{0:" + DetailContentColumn.Format + "}");
                    else
                        columnvalue = DataSourceHelper.Eval(curobj, DetailContentColumn.Name);
                    if (columnvalue == null) columnvalue = "";
                    if (lastDetailContent != columnvalue.ToString())
                    {
                        lastDetailContent = columnvalue.ToString();
                        isOddRow = !isOddRow;
                        RectangleF bodyrectanglex = new RectangleF(currentX, currentY, e.MarginBounds.Right - e.MarginBounds.Left, bodyRowHeight);
                        if (ShowGrid)
                        {
                            if (isOddRow)
                                eGraphics.FillRectangle(Brushes.LightGray, bodyrectanglex);
                            eGraphics.DrawRectangle(vbPen, bodyrectanglex.X, bodyrectanglex.Y, bodyrectanglex.Width, bodyrectanglex.Height);
                            stringFormat.Alignment = DetailContentColumn.DataAlignment;
                            eGraphics.DrawString(columnvalue.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectanglex, stringFormat);
                        }
                        currentY = currentY + bodyRowHeight;
                    }
                }
                isOddRow = !isOddRow;

                for (int i = 0; i < Columns.Count; i++)
                {
                    Column dcolumn = Columns[i];
                    if (dcolumn.IsSerialNo)
                    {
                        if (dcolumn.Format.Length > 0)
                            columnvalue = string.Format("{0:" + dcolumn.Format + "}", SerialNumber);
                        else
                            columnvalue = SerialNumber;
                    }
                    else
                    {
                        bool showData = true;
                        if (ShowNullAsEmpty)
                        {
                            columnvalue = DataSourceHelper.Eval(curobj, dcolumn.Name);
                            if (columnvalue == null)
                            {
                                showData = false;
                            }
                            else
                            {
                                if (dcolumn.DataType == DataType.Date && ((DateTime)columnvalue) <= MinDateValue)
                                    showData = false;
                                if (dcolumn.DataType == DataType.Number && (Convert.ToDecimal(columnvalue) <= 0))
                                    showData = false;
                            }

                        }

                        if (showData)
                        {
                            if (dcolumn.Format.Length > 0)
                                columnvalue = DataSourceHelper.Eval(curobj, dcolumn.Name, "{0:" + dcolumn.Format + "}");
                            else
                                columnvalue = DataSourceHelper.Eval(curobj, dcolumn.Name);
                        }
                        else
                        {
                            columnvalue = null;
                        }
                    }
                    if (columnvalue == null)
                        columnvalue = "";

                    if (dcolumn.DetailsContent)
                    {
                        if (!dcolumn.DetailsContentGrouping)
                        {
                            lastDetailContent = columnvalue.ToString();
                            isOddRow = !isOddRow;
                            currentY = currentY + bodyRowHeight;
                            currentX = e.MarginBounds.Left;
                            RectangleF bodyrectanglex = new RectangleF(currentX, currentY, e.MarginBounds.Right - e.MarginBounds.Left, bodyRowHeight);
                            if (ShowGrid)
                            {
                                if (isOddRow)
                                    eGraphics.FillRectangle(Brushes.LightGray, bodyrectanglex);
                                eGraphics.DrawRectangle(vbPen, bodyrectanglex.X, bodyrectanglex.Y, bodyrectanglex.Width, bodyrectanglex.Height);
                                stringFormat.Alignment = dcolumn.DataAlignment;
                                eGraphics.DrawString(columnvalue.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectanglex, stringFormat);
                            }
                        }
                    }
                    else
                    {
                        if (dcolumn.ShowTotal)
                        {
                            PageTotal[i] = PageTotal[i] + GetDecimalValue(columnvalue.ToString());
                            GrandTotal[i] = GrandTotal[i] + GetDecimalValue(columnvalue.ToString());
                        }
                        RectangleF bodyrectangle = new RectangleF(currentX, currentY, dcolumn.PixelWidth, bodyRowHeight);

                        if (ShowGrid)
                        {
                            if (isOddRow)
                                eGraphics.FillRectangle(Brushes.LightGray, currentX, currentY, dcolumn.PixelWidth, bodyRowHeight);
                            eGraphics.DrawRectangle(vbPen, currentX, currentY, dcolumn.PixelWidth, bodyRowHeight);
                        }
                        stringFormat.Alignment = dcolumn.DataAlignment;
                        eGraphics.DrawString(columnvalue.ToString(), BodyStyle.Font, BodyStyle.Brush, bodyrectangle, stringFormat);
                        currentX = currentX + bodyrectangle.Width;
                    }
                }
                currentY = currentY + bodyRowHeight;
                if (currentY > endY)
                {
                    if (PageTotal.Count > 0)
                        RenderPageTotal(e, PageTotal);
                    e.HasMorePages = true;
                    break;
                }
            }
            if (!e.HasMorePages)
            {
                if (PageTotal.Count > 0)
                {
                    RenderPageTotal(e, PageTotal);
                    currentY = currentY + bodyRowHeight;
                }
                if (ShowGrandTotal && currentpage > 1 && GrandTotal.Count > 0)
                    RenderPageTotal(e, GrandTotal, "Grand Total");
                dataSourceInitialized = false;
                PageCount = currentpage;
                currentpage = 1;
            }
            else
            {
                currentpage++;
            }
            ShowPageNumber(!e.HasMorePages, bodyRowHeight, e);
        }
    }
}
