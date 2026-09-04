using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportLibrary
{
    public class DosFormatParser
    {
        static string rootDirectory = "";
        static DosFormatParser()
        {
            rootDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName).Directory.FullName;
        }

        System.IO.FileInfo defaultConfigFile = null;
        string defaultFileName = "";
        public DosFormatParser(string fileName)
        {
            string filePath = System.IO.Path.Combine(rootDirectory, fileName);
            defaultFileName = fileName;
            if (System.IO.File.Exists(filePath) && defaultConfigFile == null)
            {
                defaultConfigFile = new System.IO.FileInfo(filePath);
            }
        }


        private string GetFormatFileName()
        {
            string printerName = "Laser";
            printerName = System.Text.RegularExpressions.Regex.Replace(printerName, "[^0-9A-Za-z]+", ",");
            string filePath = System.IO.Path.Combine(rootDirectory, printerName + "." + defaultFileName);
            if (System.IO.File.Exists(filePath))
                return filePath;

            if (defaultConfigFile != null && defaultConfigFile.Exists)
                return defaultConfigFile.FullName;

            return null;
        }

        public DosFormat Parse(long organizationid)
        {
            string fileName = GetFormatFileName();
            if (string.IsNullOrEmpty(fileName))
                return null;
            return ParseXML(organizationid,System.IO.File.ReadAllText(fileName));
        }

        public static DosFormat.Column ParseColumn(System.Xml.XmlNode column)
        {
            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            document.LoadXml(column.OuterXml);


            DosFormat.Column df = new DosFormat.Column();
            df.width = document.DocumentElement.Attributes["Width"] != null ? float.Parse(document.DocumentElement.Attributes["Width"].InnerText) : 0;
            df.height = document.DocumentElement.Attributes["Height"] != null ? float.Parse(document.DocumentElement.Attributes["Height"].InnerText) : 0;
            df.border = document.DocumentElement.Attributes["ShowBorder"] != null ? document.DocumentElement.Attributes["ShowBorder"].InnerText == "True" : false;
            var header = document.SelectSingleNode("//Header");

            if (header != null)
            {
                df.PageHeader = new DosFormat.Header();
                df.PageHeader.Parse(header);
            }
            var body = document.SelectSingleNode("//Body");
            if (body != null)
            {
                df.PageBody = new DosFormat.Body();
                df.PageBody.Parse(body);
            }

            var bodyEmptyLine = document.SelectSingleNode("//BodyEmptyLine");
            if (bodyEmptyLine != null)
            {
                df.PageBodyEmptyLine = new DosFormat.Header();
                df.PageBodyEmptyLine.Parse(bodyEmptyLine);
            }

            var footer = document.SelectSingleNode("//Footer");
            if (footer != null)
            {
                df.PageFooter = new DosFormat.Footer();
                df.PageFooter.Parse(footer);
            }
            return df;
        }


        public static DosFormat ParsePartialDocument(string content)
        {
            DosFormat df = new DosFormat();
            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            if(content.Trim().ToUpper().StartsWith("<LINE>"))
                content = @"<Header>" + content + @"</Header>";
            else
                content = @"<Header><Line>" + content + @"</Line></Header>";
            document.LoadXml(content);
            var header = document.SelectSingleNode("//Header");
            if (header != null)
            {
                df.PageHeader = new DosFormat.Header();
                df.PageHeader.Parse(header);
            }
            return df;
        }
        public static DosFormat ParseXML(long organizationid, string content)
        {
            try
            {
                System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                document.LoadXml(content);
                var columns = document.SelectNodes("//Column");
                var rows = document.SelectNodes("//Row");
                DosFormat df = new DosFormat();


                var pagesetting = document.SelectSingleNode("//PageSetting");
                if (pagesetting != null)
                {
                    df.PageSetting = new DosFormat.PageSettings();
                    df.PageSetting.Parse(pagesetting);
                }

                if (columns.Count == 0 && rows.Count == 0)
                {
                    var header = document.SelectSingleNode("//Header");
                    if (header != null)
                    {
                        df.PageHeader = new DosFormat.Header();
                        df.PageHeader.Parse(header);
                    }


                    var body = document.SelectSingleNode("//Body");
                    if (body != null)
                    {
                        df.PageBody = new DosFormat.Body();
                        df.PageBody.Parse(body);
                    }

                    var bodyEmptyLine = document.SelectSingleNode("//BodyEmptyLine");
                    if (bodyEmptyLine != null)
                    {
                        df.PageBodyEmptyLine = new DosFormat.Header();
                        df.PageBodyEmptyLine.Parse(bodyEmptyLine);
                    }

                    var footer = document.SelectSingleNode("//Footer");
                    if (footer != null)
                    {
                        df.PageFooter = new DosFormat.Footer();
                        df.PageFooter.Parse(footer);
                    }

                    if (df.PageHeader == null && df.PageFooter == null && df.PageBody == null)
                    {
                        return ParsePartialDocument(content);
                    }
                }
                else if (columns.Count > 0)
                {
                    df.Columnes = new List<DosFormat.Column>();
                    foreach (System.Xml.XmlNode item in columns)
                    {
                        df.Columnes.Add(ParseColumn(item));
                    }
                }
                else if (rows.Count > 0)
                {
                    df.Rows = new List<DosFormat.Column>();
                    foreach (System.Xml.XmlNode item in rows)
                    {
                        df.Rows.Add(ParseColumn(item));
                    }
                }
                df.isValid = true;
                return df;
            }
            catch (Exception exp)
            {
                try
                {
                    return ParsePartialDocument(content);

                }
                catch (Exception fullexp)
                {
                    
                }
            }
            return new DosFormat();
        }

        internal bool HasFormat()
        {
            return (!string.IsNullOrEmpty(GetFormatFileName()));
        }
    }
}
