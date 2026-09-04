using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Data
{
    public class Numberconvertor
    {

        private static string[] onetonine = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

        private static string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        private static string[] Others = new string[] { "Hundred and", "Thousand", "Lakh", "Crore" };

        public static string rupees(long mInputstr)
        {
            return rupees((decimal)mInputstr);
        }

        public static string rupees(decimal mInputstr)
        {
            string returnValue = "";
            string Inputstr = "";
            string FndStr = "";
            Inputstr = Math.Round(mInputstr, 0).ToString();

            if (mInputstr <= 0)
                return "Zero";
            else
                Inputstr = Inputstr.Trim();

            Inputstr = ReversStr(Inputstr);

            if (Inputstr.Length > 7)
            {
                int i = 0;
                for (i = 1; i <= (int)(Inputstr.Length / 7); i++)
                {
                    int Start = (i * 7 - 7);
                    int endc = Inputstr.Length > (i * 7) ? (i * 7) : Inputstr.Length;

                    FndStr = Inputstr.Substring(Start, endc);

                    returnValue = sub1str(FndStr.Trim()) + returnValue;

                    if (Inputstr.Length > i * 7)
                        returnValue = Others[3] + " " + returnValue;
                }
                i = i - 1;
                if (Inputstr.Length > i * 7)
                {
                    FndStr = Inputstr.Substring(i * 7);
                    returnValue = sub1str(FndStr.Trim()) + returnValue;
                }
            }
            else
            {
                returnValue = sub1str(Inputstr);
            }

            returnValue = returnValue.Trim();

            if (returnValue.Length > 3 && returnValue.EndsWith(" and"))
            {
                returnValue = returnValue.Substring(0, returnValue.Length - 3);
            }
            return returnValue + " Only";
        }


        private static string ReversStr(string Inputstr)
        {

            string ret = "";
            for (int i = 0; i < Inputstr.Length; i++)
            {
                ret = Inputstr.Substring(i, 1) + ret;
            }
            return ret;
        }


        private static string sub1str(string Inputstr)
        {
            string ret = "";

            int lenstr = Inputstr.Length;
            int looprun = 1;
            switch (lenstr)
            {
                case 6:
                case 7:
                    looprun = 4;
                    break;
                case 4:
                case 5:
                    looprun = 3;
                    break;
                case 3:
                    looprun = 2;
                    break;
            }

            int totlen = 0;
            string temp;

            for (int i = 1; i <= looprun; i++)
            {
                switch (i)
                {

                    case 1:
                        totlen = Inputstr.Length > 1 ? 2 : 1;
                        ret = laststr(Inputstr.Substring(0, totlen)) + ret;
                        break;
                    case 2:
                        temp = laststr(Inputstr.Substring(2, 1));
                        ret = (temp.Length > 0 ? temp + Others[0] + " " : "") + ret;
                        break;
                    case 3:
                        totlen = Inputstr.Length > 4 ? 2 : 1;
                        temp = laststr(Inputstr.Substring(3, totlen));
                        ret = (temp.Length > 0 ? temp + Others[1] + " " : "") + ret;
                        break;
                    case 4:
                        totlen = Inputstr.Length > 6 ? 2 : 1;
                        temp = laststr(Inputstr.Substring(5, totlen));
                        ret = (temp.Length > 0 ? temp + Others[2] + " " : "") + ret;
                        break;
                }
            }
            return ret;
        }

        private static string laststr(string Inputstr)
        {

            string ret = "";
            if (Inputstr.Length == 2)
                Inputstr = Inputstr.Substring(Inputstr.Length - 1, 1) + Inputstr.Substring(0, 1);

            if (int.Parse(Inputstr) <= 19 && int.Parse(Inputstr) > 0)
                ret = onetonine[int.Parse(Inputstr)-1] + " ";
            else
            {
                string ten = Inputstr.Substring(0, 1);
                string ten1 = Inputstr.Substring(Inputstr.Length - 1, 1);
                if (int.Parse(ten) > 0)
                    ret = tens[int.Parse(ten) - 2] + " ";

                if (int.Parse(ten1) > 0)
                    ret = ret + onetonine[int.Parse(ten1)-1] + " ";
            }
            return ret;
        }



    }
}