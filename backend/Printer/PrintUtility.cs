using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace Quanto
{

    public static class PrintUtils
    {
        public static int PageLength = 50;
        public static int PageStartFrom(int currentPageIndex)
        {
            return (currentPageIndex-1) * PageLength + 1;
        }

        public static int PageStartTo(int currentPageIndex)
        {
            return currentPageIndex * PageLength;
        }

        public static string BoldString(string title)
        {
            StringBuilder sb = new StringBuilder();
                char[] Char = new char[5];
                Char[0] = (char)27;
                Char[1] = 'E';
                Char[2] = (char)27;
                Char[3] = 'W';
                Char[4] = (char)1;
                sb.Append(Char);
                sb.Append(title);
                Char[0] = (char)27;
                Char[1] = 'W';
                Char[2] = (char)0;
                Char[3] = (char)27;
                Char[4] = 'F';
                sb.Append(Char);
                return sb.ToString();
        }


        public static string ContenseString(string title)
        {
            StringBuilder sb = new StringBuilder();
            char[] Char = new char[3];
            Char[0] = (char)27;
            Char[1] = '!';
            Char[2] = (char)5;
            sb.Append(Char);

            sb.Append(title);

            Char[0] = (char)27;
            Char[1] = '!';
            Char[2] = (char)0;
            sb.Append(Char);
            return sb.ToString();
        }

        public static string BoldContenseString(string title)
        {
            StringBuilder sb = new StringBuilder();
            char[] Char = new char[5];
            Char[0] = (char)27;
            Char[1] = 'E';
            Char[2] = (char)27;
            Char[3] = 'W';
            Char[4] = (char)1;
            sb.Append(Char);

            sb.Append(ContenseString(title));

            Char[0] = (char)27;
            Char[1] = 'W';
            Char[2] = (char)0;
            Char[3] = (char)27;
            Char[4] = 'F';
            sb.Append(Char);
            return sb.ToString();
        }

        public static char Eject
        {
            get
            {
                return (char)12;
            }
        }

        public static int ForwardLine
        {
            get
            {
                return 10;
            }
        }

        public static string Reverse
        {
            get
            {
                char[] Char = new char[3];
                Char[0] = (char)27;
                Char[1] = 'j';
                Char[2] = (char)255;
                return new string(Char);
            }
        }

        public static string GetNumberBlock(string format, int length, object args)
        {
            decimal decvalue = 0;
            decimal.TryParse(args.ToString(), out decvalue);
            string str = string.Format(format, decvalue);
            if(!format.StartsWith("N"))  str = str.Replace(".0000", ".00");
            if (str.Length < length)
                str = (new string(' ', length - str.Length)) + str;
            return str;
        }

        public static string DisplayText(string input, int length)
        {
            input = input == null ? "" : input;
            if (input.Length > length) return input.Substring(0,length);
            StringBuilder sb = new StringBuilder(input);
            sb.Insert(input.Length, " ", length - input.Length);
            return sb.ToString();
        }

        public static string RightText(string input, int length)
        {
            input = input == null ? "" : input;
            if (input.Length >= length) return input.Substring(0,length);
            input = (new string(' ', length - input.Length)) + input;
            return input;
        }

        public static string CenterText(string input, int length)
        {
            input = input == null ? "" : input.Trim();
            if(input.Length < length)
            {
                int spacewidth = length/2 - input.Length/2;
                if(spacewidth>0)
                {
                    string sp = new string(' ',spacewidth);
                    input = sp+input+sp;
                }
                if(input.Length<length) input = input + (new string(' ',length-input.Length));
            }else
            {
                input = input.Substring(0,length);
            }
            return input;
        }

        public static string GetDescription(object value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                  (DescriptionAttribute[])fi.GetCustomAttributes(
                  typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static void WriteFile(string content, string fileName)
        {
            FileInfo t = new FileInfo(fileName);
            StreamWriter Tex = t.CreateText();
            Tex.Write(content);
            Tex.Close();
        }
        public static string Replicate(string str, int noofTimes)
        {
            string tabstring = "";
            for (int i = 0; i < noofTimes; i++)
                tabstring += str;
            return tabstring;
        }
        public static string CamelCase(string input)
        {
            return input.Substring(0, 1).ToLower() + input.Substring(1);
        }
    }

    public class CodeWriter
    {
        int tabIndent = 0;
        StringBuilder sb = null;
        string tabstring = "";
        public Graphics g;
        public CodeWriter()
        {
            sb = new StringBuilder();
        }

        private static int? dosFontWidth;
        private int DosFontWidth
        {
            get
            {
                if (!dosFontWidth.HasValue)
                {

                    dosFontWidth = (int)( g.MeasureString(new string('W', 1), new Font("courier new",8)).Width);

                   
                }
                return dosFontWidth.Value;
            }
        
        }

        public void AddGraphicText(string s, RectangleF layoutRectangle, StringFormat format,string separator)
        {
            int width = (int) (layoutRectangle.Width / dosFontWidth);
            switch (format.Alignment)
            {
                case StringAlignment.Center:
                    sb.Append(PrintUtils.CenterText(s, width));
                    break;
                case StringAlignment.Far:
                    sb.Append(PrintUtils.RightText(s, width));
                    break;
                case StringAlignment.Near:
                    sb.Append(PrintUtils.DisplayText(s, width));
                    break;
            }
            sb.Append(separator);
        }

        public StringBuilder StringBuilder
        {
            get
            {
                return sb;
            }
        }

        public void AddLoadPrinter()
        {
            sb.Append((char)27);
            sb.Append("j");
            sb.Append((char)255);
        }

        public void AddCenterText(string text, int length)
        {
            sb.Append(PrintUtils.CenterText(text, length));
        }
        public void AddText(string text, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
        }
        public void AddText(string text, int length, string separator)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(separator);
        }
        public void AddRightText(object text,string format, int length, string separator)
        {
            text = string.Format(format,text).Replace(",","");
            if(text.ToString() == "0") text = "";
            sb.Append(PrintUtils.RightText(text.ToString(), length));
            sb.Append(separator);
        }
        public void AddTextBlock(string text, string value, string seperotor, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(seperotor);
            sb.Append(value);
        }

        public void AddTextBlock(string format, int length, string args)
        {
            string str = string.Format(format, args);
            sb.Append(PrintUtils.DisplayText(str, length));
        }

        public void AddNumberBlock(string format, int length, object args)
        {
            if (args == null || args.ToString() == "")
            {
                string str = string.Format(format, args);
                sb.Append(PrintUtils.RightText(str, length));
                return;
            }
            sb.Append(PrintUtils.GetNumberBlock( format,  length,  args));
        }

        public void AddTextBlock(string format, int length, params object[] args)
        {
            string str = string.Format(format, args);
            sb.Append(PrintUtils.DisplayText(str,length));
        }

        public void AddTextBlock(string text, string value, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(value);
        }

        public void AddFormatedBlock(string text, object value, string seperotor,string format, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(seperotor);
            sb.Append(string.Format(format,value));
        }
        public void AddLines(int noofTimes)
        {
            for (int i = 0; i < noofTimes; i++)
            {
                AddLines();
            }
        }
        public void AddLines()
        {
            sb.Append("\r\n");
        }
        public void AddLines(char separator, int length)
        {
            sb.Append(new string(separator,length));
            sb.Append("\r\n");
        }
        public void AddLine(string text)
        {
            sb.Append(text);
            sb.Append("\r\n");
        }
        public void AddFormatedBlock(string text, object value,string format)
        {
            sb.Append(text);
            sb.Append(string.Format(format, value));
        }

        public string GetTabString()
        {
            if (tabstring.Length != tabIndent)
            {
                tabstring = PrintUtils.Replicate("\t", tabIndent);
            }
            return tabstring;
        }
        public void AddIndent()
        {
            tabIndent++;
        }

        public void AddBlock(string str)
        {
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
            tabIndent++;
        }
        public void CloseBlock(string str)
        {
            tabIndent--;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void CloseBlock()
        {
            tabIndent--;
            sb.AppendFormat("{0}", GetTabString());
            sb.Append("\r\n}");
        }


        public void AddBlock()
        {
            sb.AppendFormat("{0}", GetTabString());
            sb.Append("{\r\n");
            tabIndent++;
        }

        public void CloseBlock(int noofTimes)
        {
            for (int i = 0; i < noofTimes; i++)
            {
                CloseBlock();
            }
        }
        public void AddIndent(string statement)
        {
            tabIndent++;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statement);
        }
        public void AddIndent(string format, params object[] arg)
        {
            tabIndent++;
            string str = string.Format(format, arg);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }
        public void ContinueStatement(string statement)
        {
            sb.Append(statement);
        }
        public void ContinueStatement(string format, params object[] args)
        {
            string str = string.Format(format, args);
            sb.Append(str);
        }
        public void AddStatement(string statement)
        {
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statement);
        }
        public void AddStatement(string statement, params object[] args)
        {
            string str = string.Format(statement, args);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void Endindent()
        {
            if (tabIndent > 0) tabIndent--;
        }
        public void Endindent(string statemment)
        {
            if (tabIndent > 0) tabIndent--;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statemment);
        }
        public void Endindent(string statemment, params object[] args)
        {
            if (tabIndent > 0) tabIndent--;
            string str = string.Format(statemment, args);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void AddHeaders(string statement)
        {
            sb.AppendFormat("\r\n{0}", statement);
        }
        public void AddHeaders(string statement, params object[] args)
        {
            string str = string.Format(statement, args);
            sb.AppendFormat("\r\n{0}", str);
        }
        public string ToCode()
        {
            return sb.ToString();
        }
    }


    public class CodeBytesWriter
    {
        int tabIndent = 0;
        StringBuilder sb = null;
        string tabstring = "";
        public Graphics g;
        public CodeBytesWriter()
        {
            sb = new StringBuilder();
        }

        private static int? dosFontWidth;
        private int DosFontWidth
        {
            get
            {
                if (!dosFontWidth.HasValue)
                {

                    dosFontWidth = (int)(g.MeasureString(new string('W', 1), new Font("courier new", 8)).Width);


                }
                return dosFontWidth.Value;
            }

        }

        public void AddGraphicText(string s, RectangleF layoutRectangle, StringFormat format, string separator)
        {
            int width = (int)(layoutRectangle.Width / dosFontWidth);
            switch (format.Alignment)
            {
                case StringAlignment.Center:
                    sb.Append(PrintUtils.CenterText(s, width));
                    break;
                case StringAlignment.Far:
                    sb.Append(PrintUtils.RightText(s, width));
                    break;
                case StringAlignment.Near:
                    sb.Append(PrintUtils.DisplayText(s, width));
                    break;
            }
            sb.Append(separator);
        }

        public StringBuilder StringBuilder
        {
            get
            {
                return sb;
            }
        }

        public void AddLoadPrinter()
        {
            sb.Append((char)27);
            sb.Append("j");
            sb.Append((char)255);
        }

        public void AddCenterText(string text, int length)
        {
            sb.Append(PrintUtils.CenterText(text, length));
        }
        public void AddText(string text, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
        }
        public void AddText(string text, int length, string separator)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(separator);
        }
        public void AddRightText(object text, string format, int length, string separator)
        {
            text = string.Format(format, text).Replace(",", "");
            if (text.ToString() == "0") text = "";
            sb.Append(PrintUtils.RightText(text.ToString(), length));
            sb.Append(separator);
        }
        public void AddTextBlock(string text, string value, string seperotor, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(seperotor);
            sb.Append(value);
        }

        public void AddTextBlock(string format, int length, string args)
        {
            string str = string.Format(format, args);
            sb.Append(PrintUtils.DisplayText(str, length));
        }

        public void AddNumberBlock(string format, int length, object args)
        {
            if (args == null || args.ToString() == "")
            {
                string str = string.Format(format, args);
                sb.Append(PrintUtils.RightText(str, length));
                return;
            }
            sb.Append(PrintUtils.GetNumberBlock(format, length, args));
        }

        public void AddTextBlock(string format, int length, params object[] args)
        {
            string str = string.Format(format, args);
            sb.Append(PrintUtils.DisplayText(str, length));
        }

        public void AddTextBlock(string text, string value, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(value);
        }

        public void AddFormatedBlock(string text, object value, string seperotor, string format, int length)
        {
            sb.Append(PrintUtils.DisplayText(text, length));
            sb.Append(seperotor);
            sb.Append(string.Format(format, value));
        }
        public void AddLines(int noofTimes)
        {
            for (int i = 0; i < noofTimes; i++)
            {
                AddLines();
            }
        }
        public void AddLines()
        {
            sb.Append("\r\n");
        }
        public void AddLines(char separator, int length)
        {
            sb.Append(new string(separator, length));
            sb.Append("\r\n");
        }
        public void AddLine(string text)
        {
            sb.Append(text);
            sb.Append("\r\n");
        }
        public void AddFormatedBlock(string text, object value, string format)
        {
            sb.Append(text);
            sb.Append(string.Format(format, value));
        }

        public string GetTabString()
        {
            if (tabstring.Length != tabIndent)
            {
                tabstring = PrintUtils.Replicate("\t", tabIndent);
            }
            return tabstring;
        }
        public void AddIndent()
        {
            tabIndent++;
        }

        public void AddBlock(string str)
        {
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
            tabIndent++;
        }
        public void CloseBlock(string str)
        {
            tabIndent--;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void CloseBlock()
        {
            tabIndent--;
            sb.AppendFormat("{0}", GetTabString());
            sb.Append("\r\n}");
        }


        public void AddBlock()
        {
            sb.AppendFormat("{0}", GetTabString());
            sb.Append("{\r\n");
            tabIndent++;
        }

        public void CloseBlock(int noofTimes)
        {
            for (int i = 0; i < noofTimes; i++)
            {
                CloseBlock();
            }
        }
        public void AddIndent(string statement)
        {
            tabIndent++;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statement);
        }
        public void AddIndent(string format, params object[] arg)
        {
            tabIndent++;
            string str = string.Format(format, arg);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }
        public void ContinueStatement(string statement)
        {
            sb.Append(statement);
        }
        public void ContinueStatement(string format, params object[] args)
        {
            string str = string.Format(format, args);
            sb.Append(str);
        }
        public void AddStatement(string statement)
        {
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statement);
        }
        public void AddStatement(string statement, params object[] args)
        {
            string str = string.Format(statement, args);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void Endindent()
        {
            if (tabIndent > 0) tabIndent--;
        }
        public void Endindent(string statemment)
        {
            if (tabIndent > 0) tabIndent--;
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), statemment);
        }
        public void Endindent(string statemment, params object[] args)
        {
            if (tabIndent > 0) tabIndent--;
            string str = string.Format(statemment, args);
            sb.AppendFormat("\r\n{0}{1}", GetTabString(), str);
        }

        public void AddHeaders(string statement)
        {
            sb.AppendFormat("\r\n{0}", statement);
        }
        public void AddHeaders(string statement, params object[] args)
        {
            string str = string.Format(statement, args);
            sb.AppendFormat("\r\n{0}", str);
        }
        public string ToCode()
        {
            return sb.ToString();
        }
    }
}


