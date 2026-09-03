using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Quanto
{
    public class TallyServer
    {
        static TallyServer instance;
        public static TallyServer Instance
        {
            get
            {
                if (instance == null)
                    instance = new TallyServer();
                return instance;
            }
        }

        public TallyVocher Transfer(TallyVocher voucher)
        {
            foreach(var item in voucher.groups)
            {
                string error;
                if (Transfer(voucher.url, item.xml, out error))
                    item.success = true;
                else
                    item.error = error;
            }

            foreach (var item in voucher.ledgers)
            {
                string error;
                if (Transfer(voucher.url, item.xml, out error))
                    item.success = true;
                else
                    item.error = error;
            }

            foreach (var item in voucher.vouchers)
            {
                string error;
                if (Transfer(voucher.url, item.xml, out error))
                    item.success = true;
                else
                    item.error = error;
            }

            return voucher;
        }

        public static string SerializeXML(object objectValue, Type type)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            XmlSerializer s = new XmlSerializer(type);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            s.Serialize(stream, objectValue);
            stream.Position = 0;
            doc.Load(stream);
            return doc.InnerXml;
        }

        public static Object DeSerializeXML(string objectXML, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(objectXML)) return null;
                System.IO.StringReader read = new StringReader(objectXML);
                XmlSerializer s = new XmlSerializer(type);
                System.Xml.XmlReader reader = new System.Xml.XmlTextReader(read);
                return s.Deserialize(reader);
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public bool Transfer(string URL, string xml,out string error)
        {
            error = "";
            string filename = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", ".log.xml");
            System.IO.File.AppendAllText(filename, xml);

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] response = wc.UploadData(URL, "POST", System.Text.UTF8Encoding.UTF8.GetBytes(xml));

            string respstring = System.Text.UTF8Encoding.UTF8.GetString(response);

            filename = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", ".logout.xml");
            System.IO.File.AppendAllText(filename, respstring);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(respstring);

            try
            {

                string xmlenv = doc.OuterXml;
                if (!xmlenv.Trim().StartsWith("<ENVELOPE>"))
                    xmlenv = "<ENVELOPE>" + xmlenv + "</ENVELOPE>";
                Quanto.Tally.ENVELOPE result = DeSerializeXML(xmlenv, typeof(Quanto.Tally.ENVELOPE)) as Quanto.Tally.ENVELOPE;

                if (result.Tables.Contains("DATA") && result.Tables["DATA"].Columns.Contains("LINEERROR"))
                {
                    string erromsg1 = result.Tables["DATA"].Rows[0]["LINEERROR"].ToString();
                    error = erromsg1;
                    return false;
                }
                if (!result.Tables[result.Tables.Count - 1].Columns.Contains("ERRORS"))
                    return true;
                if (result.Tables[result.Tables.Count - 1].Rows[0]["ERRORS"].ToString() == "0")
                    return true;

                string erromsg = result.Tables[result.Tables.Count - 1].Rows[0]["LINEERROR"].ToString();
                error = erromsg;
                return false;

            }
            catch (Exception exp)
            {

                if (doc.DocumentElement.ChildNodes[doc.DocumentElement.ChildNodes.Count - 1].OuterXml == "<ERRORS>0</ERRORS>")
                {
                    return true;
                }
                else
                {
                    if (doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == null)
                    {
                        if (doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "0" ||
                            doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "1") return true;

                        error = doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value;
                        return false;
                    }
                }

                return doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "1";
            }
        }
    }

    public class TallyVocher
    {
        public class Document
        {
            public string type { get; set; }
            public string id { get; set; }
            public string xml { get; set; }
            public bool success { get; internal set; }
            public string error { get; internal set; }
        }
        public List<Document> groups { get; set; }
        public List<Document> ledgers { get; set; }
        public List<Document> vouchers { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public bool success { get; internal set; }
        public string error { get; internal set; }
    }

    public class TallyResposne
    {
        public string id { get; set; }
        public string fullxml { get; set; }
        public bool issuccess { get; set; }
        public string errormessage { get; set; }
        public bool created { get; set; }
        public bool altered { get; set; }
        public bool deleted { get; set; }
    }
}
