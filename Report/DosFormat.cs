using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Printing;

namespace ReportLibrary
{
    [Serializable]
    public class DosFormat
    {
        
        public Font GetStyle(string Style)
        {
            string[] str = Style.Split(',');
            Font f = null;
            if (str.Length == 3)
            {
                switch (str[2].ToLower())
                {
                    case "bold":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                    case "regular":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                    case "underline":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                    case "italic":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                }
                f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
            }
            return f;
        }

        MemoryStream ms = null;
        public byte[] RenderBytes(object dataSource, Object Items, SortedDictionary<string, object> Additional, bool addcutter)
        {
            ms = new MemoryStream();
            //using (var ms = new MemoryStream())
            using (var bw = new CodeBinaryWriter(ms))
            {
                if (string.IsNullOrEmpty(this.PageHeader.SpecialHeader))
                {
                    // Reset the printer bws (NV images are not cleared)
                    bw.Write(AsciiControlChars.Escape);
                    bw.Write('@');
                }
                else
                {
                    if(this.PageHeader.SpecialHeader.ToLower() == "bixolon")
                        bw.BixolonHeader();
                    else
                        bw.DrawUnicode(this.PageHeader.SpecialHeader);                
                }
                
                Render(bw, dataSource, Items, Additional);

                if (addcutter)
                {
                    bw.Write(AsciiControlChars.Escape);
                    // Feed 3 vertical motion units and cut the paper with a 1 point cut
                    bw.Write(AsciiControlChars.GroupSeparator);
                    bw.Write('V');
                    bw.Write((byte)66);
                    bw.Write((byte)3);
                }
                bw.Flush();
                return ms.ToArray();
            }
        }
        private bool Render(CodeBinaryWriter bw, object dataSource, Object Items, SortedDictionary<string, object> Additional)
        {
            DosFormat format = Clone();
            var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(Items, null);
            if (ie == null) return false;
            format.PageBody.AvailableRows = 0;
            var ien = ie.GetEnumerator();
            while (ien.MoveNext())
                format.PageBody.AvailableRows++;

            ien = ie.GetEnumerator();

            if(format.PageHeader !=null)
                format.PageHeader.Render(dataSource, Additional, bw);
            while (true)
            {
                if (format.PageBody.Render(ien, Additional, bw))
                {
                    if (format.PageBody.EmptyLines > 0)
                        bw.FeedLines(format.PageBody.EmptyLines);
                    break;
                }
                bw.FeedLines(format.PageFooter.Length);
            }

            if(format.PageFooter != null)
                format.PageFooter.Render(dataSource, Additional, bw);
            return true;
        }
        public string Render(Object dataSource, Object Items, SortedDictionary<string, object> Additional)
        {
            DosFormat format = Clone();
            CodeWriter cw = new CodeWriter();
            var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(Items, null);
            

            if (format.PageBody != null)
            {
                format.PageBody.AvailableRows = 0;
                var ien = ie.GetEnumerator();
                while (ien.MoveNext())
                    format.PageBody.AvailableRows++;
            }

            if(format.PageHeader != null)
                format.PageHeader.Render(dataSource, Additional, cw);
            if (ie != null && format.PageBody != null)
            {
                var ien = ie.GetEnumerator();
                while (true)
                {
                    if (format.PageBody.Render(ien, Additional, cw))
                    {
                        if (format.PageBody.EmptyLines > 0)
                            cw.AddLines(format.PageBody.EmptyLines);
                        break;
                    }
                    cw.AddLines(format.PageFooter.Length);
                }
            }

            if (format.PageFooter != null)
                format.PageFooter.Render(dataSource, Additional, cw);
            return cw.ToCode();
        }

        public static byte[] Serialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Seek(0, 0);
                bytes = ms.ToArray();
            }
            return bytes;
        }

        public static object Deserialize(byte[] bytes)
        {

            System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream(bytes);

            // create new BinaryFormatter
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
                        = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            // set memory stream position to starting point
            _MemoryStream.Position = 0;

            // Deserializes a stream into an object graph and return as a object.
            return _BinaryFormatter.Deserialize(_MemoryStream);
        }


        public DosFormat Clone()
        {
            var bytes = Serialize(this);
            var obj = Deserialize(bytes);
            return obj as DosFormat;
        }

        public DefaultPageSettings GetDefaultSetting()
        {
            ReportLibrary.DefaultPageSettings setting = new DefaultPageSettings();
            if (PageSetting == null)
                PageSetting = new PageSettings();
            setting.Landscape = PageSetting.IsLandscape;

            setting.Margins = new Margins(PageSetting.LeftMargin, PageSetting.RightMargin, PageSetting.TopMargin, PageSetting.BottomMargin);
            setting.PageBounds = new Rectangle(0, 0, PageSetting.Width, PageSetting.Height);
            setting.MarginBound = new Rectangle(PageSetting.LeftMargin, PageSetting.TopMargin,
                PageSetting.Width - (PageSetting.LeftMargin + PageSetting.RightMargin), PageSetting.Height - (PageSetting.BottomMargin + PageSetting.TopMargin));
            setting.PaperSize = new PaperSize("Custom", PageSetting.Width, PageSetting.Height);
            return setting;
        }

        public List<string> GetImages()
        {
            List<string> ret = new List<string>();
            if (PageHeader != null && PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageHeader.Items));
            }

            if (PageFooter != null && PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageFooter.Items));
            }

            if (PageBody != null && PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageBody.Items));
            }

            if (Columnes != null)
            {
                foreach (var item in Columnes)
                {
                    if (item.PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
                    {
                        ret.AddRange(GetImages(item.PageHeader.Items));
                    }
                    if (item.PageFooter != null)
                    {
                        if (item.PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
                        {
                            ret.AddRange(GetImages(item.PageFooter.Items));
                        }
                    }
                    if (item.PageBody != null)
                    {
                        if (item.PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Image)))
                        {
                            ret.AddRange(GetImages(item.PageBody.Items));
                        }
                    }
                }
            }
            return ret;
        }

        public List<string> GetImages(List<Line> source)
        {
            List<string> ret = new List<string>();

            foreach (var item in source)
            {
                foreach(var content in item.Items.FindAll(e=>e.Type == CotnentType.Image))
                {
                    ret.Add(content.Source);
                }
            }
            return ret;
        }
        public bool HasLogo()
        {

            if (PageHeader != null && PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                return true;
            else if (PageFooter != null && PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                return true;
            else if (PageBody != null && PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                return true;
            else if (Columnes != null)
            {
                foreach (var item in Columnes)
                {
                    if (item.PageHeader != null && item.PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                        return true;
                    if (item.PageFooter != null && item.PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                        return true;
                    if (item.PageBody != null && item.PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.DosFormat.CotnentType.Logo)))
                        return true;
                }
            }
            
            return false;
        }
        public PageSettings PageSetting { get; set; }
        public Header PageHeader { get; set; }
        public Body PageBody { get; set; }
        public Footer PageFooter { get; set; }
        public bool Valid { get; set; }
        public string Exceptions { get; set; }
        public List<Column> Columnes { get; set; }
        public List<Column> Rows { get; set; }
        public class Column
        {
            public float width { get; set; }
            public float height { get; set; }
            public bool border { get; set; }
            public Header PageHeader { get; set; }
            public Body PageBody { get; set; }
            public Footer PageFooter { get; set; }
            public Header PageBodyEmptyLine { get; set; }
        }
        [Serializable]
        public class LineCollection
        {
            public List<Line> Items { get; set; }
            public virtual void Parse(System.Xml.XmlNode xmlNode)
            {
                Items = new List<Line>();
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    if (item.Name.ToLower() == "line")
                        Items.Add(new Line(item));
                }
            }

            public string GetAttribValue(System.Xml.XmlNode xmlNode, string key)
            {
                foreach (System.Xml.XmlAttribute aitem in xmlNode.Attributes)
                {
                    if (aitem.Name.ToLower() == key.ToLower()) return aitem.Value;
                }
                return "";
            }
        }
        [Serializable]
        public class Header : LineCollection
        {
            public Header()
            {

            }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;

                SpecialHeader = GetAttribValue(xmlNode, "SpecialHeader");

                base.Parse(xmlNode);
            }
            public string Style { get; set; }
            public float LineHeight { get; set; }
            public string SpecialHeader { get; set; }
            
            internal void Render(object PageValues, SortedDictionary<string, object> Additional, CodeWriter cw)
            {
                foreach (var item in Items)
                {
                    item.Render(PageValues, Additional, cw);
                    cw.AddLines();
                }
            }
            internal void Render(object PageValues, SortedDictionary<string, object> Additional, CodeBinaryWriter bw)
            {
                foreach (var item in Items)
                {
                    //if (item.Render(PageValues, Additional, bw))
                    //    bw.FeedLines(1);

                    if (item.IsTable && !string.IsNullOrEmpty(item.field))
                    {

                        var items = ReportLibrary.DataSourceHelper.Eval(PageValues, item.field);

                        var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(items, null);
                        if (ie != null)
                        {
                            Table table = new Table();
                            List<Line> lstline = new List<Line>();
                            lstline.Add(item);
                            table.Items = lstline;

                            var ien = ie.GetEnumerator();
                            while (ien.MoveNext())
                                table.AvailableRows++;
                            ien = ie.GetEnumerator();
                            table.Length = table.AvailableRows;
                            table.Render(PageValues, ien, Additional, bw);
                        }
                    }
                    else
                    {
                        if (item.Render(PageValues, Additional, bw))
                            bw.FeedLines(1);
                    }

                }

            }
        }
        [Serializable]
        public class PageSettings : LineCollection
        {
            public string PrinterName;
            public int TopMargin = 10;
            public int LeftMargin = 10;
            public int BottomMargin = 10;
            public int RightMargin = 10;
            public int Width = 800;
            public int NoofColumns = 3;
            public int ColumnWidth = 100;
            public int ColumnHeight = 100;
            public int Height = 1000;
            public bool IsLandscape = false;
            public bool IsThermal = false;
            public string Style { get; set; }
            public string[] fixedbilltypes { get; set; }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    if (item.Name.ToLower() == "setting")
                    {
                        int.TryParse(GetAttribValue(item, "topmargin"), out TopMargin);
                        int.TryParse(GetAttribValue(item, "leftmargin"), out LeftMargin);
                        int.TryParse(GetAttribValue(item, "bottommargin"), out BottomMargin);
                        int.TryParse(GetAttribValue(item, "rightmargin"), out RightMargin);
                        int.TryParse(GetAttribValue(item, "width"), out Width);
                        int.TryParse(GetAttribValue(item, "height"), out Height);
                        int.TryParse(GetAttribValue(item, "noofcolumns"), out NoofColumns);
                        int.TryParse(GetAttribValue(item, "columnwidth"), out ColumnWidth);
                        int.TryParse(GetAttribValue(item, "columnheight"), out ColumnHeight);

                        bool.TryParse(GetAttribValue(item, "landscape"), out IsLandscape);
                        bool.TryParse(GetAttribValue(item, "thermal"), out IsThermal);
                        Style = GetAttribValue(item, "style");

                        var ftypes = GetAttribValue(item, "fixedbilltypes");
                        if(!string.IsNullOrEmpty(ftypes))
                        fixedbilltypes = ftypes.Split(',');
                    }
                }
                base.Parse(xmlNode);
            }
        }
        [Serializable]
        public class Body : LineCollection
        {
            int length = 10;
            public bool DisableGrid { get; set; }
            public bool WrapRow { get; set; }
            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int AvailableRows { get; set; }

            public bool IsTable { get; set; }
            int printedItems = 0;
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if(float.TryParse(GetAttribValue(xmlNode, "HeaderHeight"),out height))
                    HeaderHeight = height;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);

                DisableGrid = GetAttribValue(xmlNode, "DisableGrid") == "True";
                WrapRow = GetAttribValue(xmlNode, "WrapRow") == "True";
                base.Parse(xmlNode);
            }

            public int EmptyLines
            {
                get
                {
                    return length - (AvailableRows % length);
                }
            }

            public string Style { get; set; }
            public float HeaderHeight { get;  set; }

            public float LineHeight { get; set; }

            internal bool Render(System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, CodeWriter cw)
            {
                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        item.Render(ien.Current, Additional, cw);
                        cw.AddLines();
                    }
                    if (printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
            internal bool Render(System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, CodeBinaryWriter bw)
            {
                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        if (item.Render(ien.Current, Additional, bw))
                            bw.FeedLines(1);
                    }
                    if (printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        [Serializable]
        public class Table : LineCollection
        {
            int length = 10;

            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int AvailableRows { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public bool IsTable { get; set; }
            int printedItems = 0;
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                HasValue = GetAttribValue(xmlNode, "HasValue");
                HideValue = GetAttribValue(xmlNode, "HideValue");
                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);
                base.Parse(xmlNode);
            }

            public int EmptyLines
            {
                get
                {
                    return length - (AvailableRows % length);
                }
            }

            internal bool Render(object PageValues,System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, CodeWriter cw)
            {
                if (!String.IsNullOrEmpty(HasValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value.GetType().Name == "Boolean" && (bool)Value == false)
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(HideValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HideValue);
                    if (Value == null && Additional.ContainsKey(HideValue))
                        Value = Additional[HideValue];

                    if (Value.GetType().Name == "Boolean" && (bool)Value == true)
                    {
                        return false;
                    }
                }

                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        item.Render(ien.Current, Additional, cw);
                        cw.AddLines();
                    }
                    if (printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
            internal bool Render(object PageValues, System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, CodeBinaryWriter bw)
            {
                if (!String.IsNullOrEmpty(HasValue) && ien == null)
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value.GetType().Name == "Boolean" && (bool)Value == false)
                    {
                        return false;
                    }
                }
                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        if (item.Render(ien.Current, Additional, bw))
                            bw.FeedLines(1);
                    }
                    if (printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [Serializable]
        public class Footer : LineCollection
        {
            int length = 0;

            public int Length
            {
                get { return length; }
                set { length = value; }
            }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;

                int.TryParse(GetAttribValue(xmlNode, "length"), out length);

                EOD = GetAttribValue(xmlNode, "EOD").ToLower() == "true";
                base.Parse(xmlNode);
            }

            public bool EOD { get; set; }

            public string Style { get; set; }
            public float LineHeight { get; set; }
            internal void Render(object PageValues, SortedDictionary<string, object> Additional, CodeWriter cw)
            {
                foreach (var item in Items)
                {
                    item.Render(PageValues, Additional, cw);
                    cw.AddLines();
                }
            }
            internal void Render(object PageValues, SortedDictionary<string, object> Additional, CodeBinaryWriter bw)
            {
                foreach (var item in Items)
                {
                    if (item.IsTable && !string.IsNullOrEmpty(item.field))
                    {

                        var items = ReportLibrary.DataSourceHelper.Eval(PageValues, item.field);

                        var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(items, null);
                        if (ie != null)
                        {
                            Table table = new Table();
                            List<Line> lstline = new List<Line>();
                            lstline.Add(item);
                            table.Items = lstline;

                            var ien = ie.GetEnumerator();
                            while (ien.MoveNext())
                                table.AvailableRows++;
                            ien = ie.GetEnumerator();
                            table.Length = table.AvailableRows;
                            table.Render(PageValues,ien, Additional, bw);
                        }
                    }
                    else
                    {
                        if (item.Render(PageValues, Additional, bw))
                            bw.FeedLines(1);
                    }
                    
                }
            }
        }
        [Serializable]
        public class Line
        {
            public Font GetStyle()
            {
                if (string.IsNullOrEmpty(Style)) return null;
                string[] str = Style.Split(',');
                Font f = null;
                if (str.Length == 3)
                {
                    switch (str[2].ToLower())
                    {
                        case "bold":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                        case "regular":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                        case "underline":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                        case "italic":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                    }
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                }
                return f;
            }
            public string GetAttribValue(System.Xml.XmlNode xmlNode, string key)
            {
                foreach (System.Xml.XmlAttribute aitem in xmlNode.Attributes)
                {
                    if (aitem.Name.ToLower() == key.ToLower()) return aitem.Value;
                }
                return "";
            }
            public string field { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public float Height { get; set; }
            public string Style { get; set; }
            int length = 0;
            public List<Content> Items { get; set; }
            public Line(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode,"Style");
                Hide = GetAttribValue(xmlNode, "Hide");
                Show = GetAttribValue(xmlNode, "Show");
                float height;
                if (float.TryParse(GetAttribValue(xmlNode, "Height"), out height))
                    Height = height;
                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                if (IsTable)
                {
                    field = GetAttribValue(xmlNode, "field");
                }
                IsImage = GetAttribValue(xmlNode, "Type") == "Image";
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);
                HasValue = GetAttribValue(xmlNode,"HasValue");
                HideValue = GetAttribValue(xmlNode, "HideValue");
                Items = new List<Content>();
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    Items.Add(new Content(item));
                }
            }
            public string Hide { get; set; }
            public string Show { get; set; }
            public bool IsTable { get; set; }
            public bool IsImage { get; set; }
            internal void Render(Object PageValues, SortedDictionary<string, object> Additional, CodeWriter cw)
            {
                foreach (var item in Items)
                {
                    cw.ContinueStatement(item.Render(PageValues, Additional));
                }
                if (length > 1)
                    cw.AddLines(length - 1);
            }
            internal bool Render(Object PageValues, SortedDictionary<string, object> Additional, CodeBinaryWriter bw)
            {
                if(!String.IsNullOrEmpty(HasValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(Show))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, Show);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(HideValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HideValue);
                    if (Value == null && Additional.ContainsKey(HideValue))
                        Value = Additional[HideValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == true))
                    {
                        return false;
                    }
                }

                foreach (var item in Items)
                {
                    string str = item.Render(PageValues, Additional, true);
                    if (item.TrueType)
                    {
                        str = item.Render(PageValues, Additional, false);
                        ReportLibrary.ThermalImage thermalImage = new ThermalImage(str, item.Height, item.Length, item.Align.ToLower() == "center" ?
                            StringAlignment.Center : StringAlignment.Near);
                        var bytes = thermalImage.Draw("Verdana", 15, FontStyle.Bold);
                        bw.WriteLogo(bytes, -1);
                    }
                    else if (item.Style.ToLower() == "contensed")
                        bw.CondensedText(str, false);
                    else if (item.Style.ToLower() == "doublecontensed")
                        bw.DoubleCondensedText(str, false);
                    else if (item.Style.ToLower() == "bold")
                        bw.BoldText(str, false);
                    else if (item.Style.ToLower() == "small")
                        bw.SmallText(str, false);
                    else if (item.Style.ToLower() == "large")
                        bw.LargeText(str);
                    else if (item.Style.ToLower() == "boldlarge")
                        bw.Boldlarge(str);
                    else if (item.Style.ToLower() == "high")
                        bw.High(str);
                    else if (item.Style.ToLower() == "enlarge")
                        bw.Enlarged(str);
                    else if (item.Style.ToLower() == "boldenlarge")
                        bw.Boldenlarged(str);
                    else if (item.Style.ToLower() == "pagebegin")
                        bw.PageBegin();
                    else if (item.Style.ToLower() == "bixheader")
                        bw.BixolonHeader();
                    else if (item.Style.ToLower() == "pageend")
                        bw.PageEnd();
                    else if (item.Style.ToLower() == "big")
                        bw.Boldlarge(str);
                    else if (item.Type == CotnentType.Barcode)
                        bw.Barcode(str);
                    else if (item.Type == CotnentType.PlainBarcode)
                        bw.DirectBarcode(str);
                    else if (item.Type == CotnentType.OnlyBarcode)
                        bw.OnlyBarcode(str);
                    else if (item.Type == CotnentType.Logo)
                    {
                        if (Additional.ContainsKey("LOGO"))
                            bw.WriteLogo((byte[])Additional["LOGO"], int.Parse(item.Text));
                    }
                    else if (item.Type == CotnentType.Image)
                    {

                        if (string.IsNullOrEmpty(item.Source) && !string.IsNullOrEmpty(item.Name))
                        {
                            if (str != null)
                            {
                                item.Source = str.ToString();
                            }
                        }

                        if (Additional.ContainsKey("IMG-" + item.Source))
                        {
                            int scale;
                            if (!int.TryParse(item.Text, out scale))
                                scale = 1;
                            bw.WriteLogo((byte[])Additional["IMG-" + item.Source], scale);
                        }

                        if (Additional.ContainsKey(item.Source))
                        {
                            int scale;
                            if (!int.TryParse(item.Text, out scale))
                                scale = 1;
                            bw.WriteLogo((byte[])Additional[item.Source], scale);
                        }
                    }
                    else if (item.Type == CotnentType.Cutter)
                        bw.Cutter();
                    else
                        bw.NormalFont(str, false);
                }
                if (length > 1)
                    bw.FeedLines(length - 1);
                return true;
            }
        }
        public enum CotnentType
        {
            Word,
            SpecialCharactor,
            Field,
            Reverse,
            Duplicate,
            Line,
            Paragraph,
            Logo,
            Table,
            Image,
			Barcode,
            Cutter,
            Dynamicfield,
            PlainBarcode,
            Dynamictable,
            OnlyBarcode
        }
        [Serializable]
        public class Content
        {
            SortedDictionary<string, string> keyvalue;
            private string GetAttributeValue(string key)
            {
                if (!keyvalue.ContainsKey(key.ToLower()))
                    return "";
                return keyvalue[key.ToLower()];
            }
            public Content()
            {

            }

            internal float GetHeight(float defaultHeight)
            {
                if (Height > defaultHeight) return Height;
                return defaultHeight;
            }

            public Content(System.Xml.XmlNode item)
            {
                keyvalue = new SortedDictionary<string, string>();
                foreach (System.Xml.XmlAttribute aitem in item.Attributes)
                {
                    if (!keyvalue.ContainsKey(aitem.Name.ToLower()))
                        keyvalue.Add(aitem.Name.ToLower(), aitem.Value);
                }
                switch (GetAttributeValue("Type"))
                {
                    case "Duplicate":
                        Type = CotnentType.Duplicate;
                        break;
                    case "Field":
                        Type = CotnentType.Field;
                        break;
                    case "Logo":
                        Type = CotnentType.Logo;
                        break;
                    case "Barcode":
                        Type = CotnentType.Barcode;
                        break;
                    case "PlainBarcode":
                        Type = CotnentType.PlainBarcode;
                        break;
                    case "OnlyBarcode":
                        Type = CotnentType.OnlyBarcode;
                        break;
                    case "Cutter":
                        Type = CotnentType.Cutter;
                        break;					
                    case "Image":
                        Type = CotnentType.Image;
                        break;
                    case "Table":
                        Type = CotnentType.Table;
                        break;
                    case "Reverse":
                        Type = CotnentType.Reverse;
                        break;
                    case "SpecialCharactor":
                        Type = CotnentType.SpecialCharactor;
                        break;
                    case "Line":
                        Type = CotnentType.Line;
                        break;
                    case "Paragraph":
                        Type = CotnentType.Paragraph;
                        break;
                    case "Word":
                        Type = CotnentType.Word;
                        break;
                    case "Dynamicfield":
                        Type = CotnentType.Dynamicfield;
                        break;
                    case "Dynamictable":
                        Type = CotnentType.Dynamictable;
                        break;
                }
                int length;
                if (int.TryParse(GetAttributeValue("Length"), out length))
                    Length = length;
                if (GetAttributeValue("Text") != "")
                    Text = GetAttributeValue("Text");
                else
                    Text = item.InnerText;
                Source = GetAttributeValue("src");
                Format = GetAttributeValue("Format");
                Align = GetAttributeValue("Align");
                Name = GetAttributeValue("Name");
                Style = GetAttributeValue("Style");
                Multiline = GetAttributeValue("Multiline");
                HasValue = GetAttributeValue("HasValue");
                HideValue = GetAttributeValue("HideValue");
                Hide = GetAttributeValue("Hide");
                Show = GetAttributeValue("Show");
                Total = GetAttributeValue("Total") == "true";
                TrueType = GetAttributeValue("TrueType") == "true";
                MergeText = GetAttributeValue("MergeText");
                if (!string.IsNullOrEmpty(MergeText) && int.TryParse(GetAttributeValue("MergeCell"),out length))
                    MergeCell = length;

                if (!string.IsNullOrEmpty(GetAttributeValue("FontSize")) && int.TryParse(GetAttributeValue("FontSize"), out length))
                    FontSize = length;

                if (!string.IsNullOrEmpty(GetAttributeValue("Height")) && int.TryParse(GetAttributeValue("Height"), out length))
                    Height = length;

                FontName = GetAttributeValue("FontName");

                DataType = DOSReporter.DataType.String;
                switch (GetAttributeValue("DataType").ToLower())
                {
                    case "number":
                        DataType = DOSReporter.DataType.Number;
                        break;
                    case "date":
                        DataType = DOSReporter.DataType.Date;
                        break;
                    case "image":
                        DataType = DOSReporter.DataType.Image;
                        break;
                }
            }
            public CotnentType Type { get; set; }
            public int Length { get; set; }
            public string Text { get; set; }
            public string Format { get; set; }
            public string Source { get; set; }
            public bool TrueType { get; set; }
            public string Align { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public object Value { get; set; }
            public string Style { get; set; }
            public string Name { get; set; }
            public string Multiline { get; set; }
            public bool Total { get; set; }
            public int MergeCell { get; set; }
            public string Show { get; set; }
            public string Hide { get; set; }
            public string MergeText { get; set; }
            public string FontName { get; private set; }
            public DOSReporter.DataType DataType { get; set; }
            public int Height { get;  set; }
            public int FontSize { get;  set; }

            public StringAlignment GetAlignment()
            {
                switch (Align)
                {
                    case "right":
                        return StringAlignment.Far;
                    case "center":
                        return StringAlignment.Center;
                    default:
                        return StringAlignment.Near;
                }
            }
            public Font GetStyle(Font source)
            {
                string[] str = Style.Split(',');
                Font f = source;
                if (str.Length == 3)
                {
                    switch (str[2].ToLower())
                    {
                        case "bold":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                        case "regular":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                        case "underline":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                        case "italic":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                    }
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                }
                return f;
            }
            private string GetAlignmentText(string input)
            {
                if (Length > 0)
                {
                    switch (Align)
                    {
                        case "right":
                            return PrintUtils.RightText(input, Length);
                        case "center":
                            return PrintUtils.CenterText(input, Length);
                        default:
                            return PrintUtils.DisplayText(input, Length);
                    }
                }
                return input;
            }

            private bool IsNumericType(Type type)
            {
                TypeCode typeCode = System.Type.GetTypeCode(type);
                //The TypeCode of numerical types are between SByte (5) and Decimal (15).
                return (int)typeCode >= 5 && (int)typeCode <= 15;
            }

            public string Render(Object values, SortedDictionary<string, object> Additional)
            {
                return Render(values, Additional, false);
            }
            public string Render(Object values, SortedDictionary<string, object> Additional, bool isThermal)
            {
                if (!string.IsNullOrEmpty(HasValue))
                {
                    Value = ReportLibrary.DataSourceHelper.Eval(values, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (IsNumericType(Value.GetType()))
                    {
                        if (Convert.ToDecimal(Value) == 0)
                            return "";
                    }
                    else if (Value.GetType().Name == "Boolean" && (bool)Value == false)
                    {
                        return "";
                    }
                }

                if (!String.IsNullOrEmpty(Show))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(values, Show);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return "";
                    }
                }

                if (!string.IsNullOrEmpty(HideValue))
                {
                    Value = ReportLibrary.DataSourceHelper.Eval(values, HideValue);
                    if (Value == null && Additional.ContainsKey(HideValue))
                        Value = Additional[HideValue];

                    if (IsNumericType(Value.GetType()))
                    {
                        if (Convert.ToDecimal(Value) == 0)
                            return "";
                    }
                    else if (Value.GetType().Name == "Boolean" && (bool)Value == true)
                    {
                        return "";
                    }
                }


                string stringText = "";
                switch (Type)
                {
                    case CotnentType.Duplicate:
                        stringText = PrintUtils.Replicate(Text, Length);
                        break;
                    case CotnentType.Field:
                        Value = ReportLibrary.DataSourceHelper.Eval(values, Name);
                        if (Value == null && Additional.ContainsKey(HasValue))
                            Value = Additional[HasValue];
                        if (Value != null)
                        {
                            if (!string.IsNullOrEmpty(Format))
                                stringText = string.Format("{0:" + Format + "}", Value);
                            else
                                stringText = Value.ToString();
                        }
                        break;
                    case CotnentType.Reverse:
                        stringText = PrintUtils.Reverse;
                        break;
                    case CotnentType.SpecialCharactor:
                        stringText = "";
                        switch (GetAttributeValue("type").ToLower())
                        {
                            case "":
                            case "string":
                                stringText = Text;
                                break;
                            case "bytes":
                            case "byte":
                                foreach (var item in Text.Split(','))
                                {
                                    stringText += (char)int.Parse(item);
                                }
                                break;
                        }

                        break;
                    case CotnentType.Word:
                        stringText = Text;
                        break;
                    case CotnentType.Logo:
                        stringText = Text;
                        break;
                    case CotnentType.Barcode:
                    case CotnentType.PlainBarcode:
                    case CotnentType.OnlyBarcode:
                        Value = ReportLibrary.DataSourceHelper.Eval(values, Name);
                        if (Value == null && Additional.ContainsKey(HasValue))
                            Value = Additional[HasValue];
                        if (Value != null)
                        {
                            stringText = Value.ToString();
                        }
                        break;
                }

                if(isThermal)
                    stringText = GetAlignmentText(stringText);

                if(isThermal && 
                    !string.IsNullOrEmpty(Multiline) && stringText.Length>Length)
                {
                    var parts = Multiline.Split(':');
                    int leftspace = int.Parse(parts[0]);
                    int width = int.Parse(parts[1]);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(stringText.Substring(0, Length));
                    stringText = stringText.Substring(Length);
                    while (true)
                    {
                        stringBuilder.Append(new string(' ', leftspace));
                        if (stringText.Length > width)
                        {
                            stringBuilder.AppendLine(stringText.Substring(0, width));
                            stringText = stringText.Substring(width);
                        }
                        else
                        {
                            stringBuilder.Append(stringText);
                            break;
                        }
                    }
                    stringText = stringBuilder.ToString();
                }

                if (isThermal)
                {
                    if (Style == "Contensed")
                        stringText = PrintUtils.ContenseString(stringText);
                    if (Style == "Emphosed")
                        stringText = PrintUtils.BoldString(stringText);
                    if (Style == "Bold")
                        stringText = PrintUtils.BoldContenseString(stringText);
                }
                return stringText;
            }
        }

        public Header PageBodyEmptyLine { get; set; }
        public bool isValid { get;  set; }
    }
}
