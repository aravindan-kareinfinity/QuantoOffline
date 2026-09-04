using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Quanto.Printer.PrinterService;
using static Quanto.TallyVocher;

namespace Quanto.Printer
{
    public class PrinterService
    {
        static PrinterService instance;

        public bool loginput { get; set; }

        public static PrinterService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PrinterService();
                    instance.loginput = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableInput"]) &&
                            System.Configuration.ConfigurationManager.AppSettings["EnableInput"].ToLower() == "true";
                }
                return instance;
            }
        }

        public LocationInfo GetLicense()
        {
            string keyfile = System.Reflection.Assembly.GetExecutingAssembly().Location + ".license";
            LocationInfo locationInfo = new LocationInfo();
            locationInfo.sourcekey = Quanto.WinService.License.PrivateKey;
            if (System.IO.File.Exists(keyfile))
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        locationInfo.ipaddress = ip.ToString();
                        break;
                    }
                }


                var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(System.IO.File.ReadAllText(keyfile));
                var bytes = rsa.Encrypt(System.Text.ASCIIEncoding.ASCII.GetBytes(Quanto.WinService.License.Key), false).ToArray();
                locationInfo.licensekey = Convert.ToBase64String(bytes);

                locationInfo.locationkey = locationInfo.ipaddress;
                if (!string.IsNullOrEmpty(locationInfo.locationkey))
                {
                    bytes = rsa.Encrypt(System.Text.ASCIIEncoding.ASCII.GetBytes(locationInfo.locationkey), false).ToArray();
                    locationInfo.locationkey = Convert.ToBase64String(bytes);
                }

                locationInfo.computername = Dns.GetHostName();
                bytes = rsa.Encrypt(System.Text.ASCIIEncoding.ASCII.GetBytes(locationInfo.computername), false).ToArray();
                locationInfo.computername = Convert.ToBase64String(bytes);

                locationInfo.publicip = HttpGetIPAddress();
                bytes = rsa.Encrypt(System.Text.ASCIIEncoding.ASCII.GetBytes(locationInfo.publicip), false).ToArray();
                locationInfo.publicip = Convert.ToBase64String(bytes);
            }




            return locationInfo;
        }

        public class GETJSONIP
        {
            public string ip { get; set; }
        }
        //{"ip":"106.203.49.251","about":"/about","Pro!":"http://getjsonip.com","reject-fascism":"Support the ACLU: https://action.aclu.org/secure/donate-to-aclu"}
        private string HttpGetIPAddress()
        {
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create("https://jsonip.com");
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                return Newtonsoft.Json.JsonConvert.DeserializeObject<GETJSONIP>(sr.ReadToEnd().Trim()).ip;
            }
            catch (Exception exp)
            {
                try
                {
                    System.Net.WebRequest req = System.Net.WebRequest.Create("https://api.ipify.org?format=json");
                    System.Net.WebResponse resp = req.GetResponse();
                    System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<GETJSONIP>(sr.ReadToEnd().Trim()).ip;
                }
                catch (Exception expv)
                {
                    return "";
                }
            }
        }

        public class LocationInfo
        {
            public string sourcekey { get; set; }
            public string licensekey { get; set; }
            public string computername { get; set; }
            public string ipaddress { get; set; }
            public string locationkey { get; set; }
            public string publicip { get; set; }
        }
        public class Printer
        {
            public override string ToString()
            {
                return name;
            }
            public string name { get; set; }
            public string id { get; set; }
            public string status { get; set; }
            public bool raw { get; set; }
            public string remotepath { get; internal set; }
            public bool remote { get; internal set; }
            public string remoteid { get; internal set; }

            public Printer()
            {
                raw = true;
            }
        }

        public class PrintDocument
        {
            public string id { get; set; }
            public string content { get; set; }
            public string encoded { get; set; }
        }
        List<Printer> printerlist = null;
        private static object lockobject = "LOCK";
        public bool Print(PrintDocument document)
        {
            if (string.IsNullOrEmpty(document.content)) return false;
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(document.content);

            try
            {
                lock (lockobject)
                {
                    //Logger.Current.Info("Line 1");
                    if (printerlist == null || !printerlist.Exists(e => e.id == document.id))
                    {
                        printerlist = ListOfPrinters();
                        //Logger.Current.Info("Line 2");
                    }
                    else
                    {
                        printerlist = ListOfPrinters();
                        if (!printerlist.Exists(e => e.id == document.id))
                        {
                            Logger.Current.Error(document.encoded + "\r\n" + document.content, new Exception("Can't find the printer " + document.id));
                            Newtonsoft.Json.JsonConvert.SerializeObject(printerlist);
                            return false;
                        }
                    }
                    Printer printer = printerlist.Find(e => e.id == document.id);
                    if (printer.remote)
                    {
                        PrintRemote(printer, document);
                    }
                    else
                    {
                        if (document.encoded == "BASE64")
                        {
                            string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            int index = 0;
                            foreach (var idata in mprintdata)
                            {
                                try
                                {
                                    data = Convert.FromBase64String(idata);
                                    if (!Print(index++, printer, data))
                                        return false;
                                }
                                catch (Exception exp)
                                {
                                    Logger.Current.Error(idata, exp);
                                }
                            }
                        }
                        else if (document.encoded == "UNICODEBASE64")
                        {
                            string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            int index = 0;
                            foreach (var idata in mprintdata)
                            {
                                try
                                {
                                    data = Convert.FromBase64String(idata);
                                    if (!Print(index++, printer, data))
                                        return false;
                                }
                                catch (Exception exp)
                                {
                                    Logger.Current.Error(idata, exp);
                                }
                            }
                        }
                        else
                        {
                            Print(-1, printer, data);
                        }
                    }
                }
            }
            catch (Exception exp)
            {

                Logger.Current.Error(document.encoded + "\r\n" + document.content, exp);
            }
            return true;
        }

        private void PrintRemote(Printer printer, PrintDocument document)
        {
            document.id = printer.remoteid;
            var httpClient = new HttpClient();
            var content = new StringContent(document.ToJSON(),
                Encoding.UTF8, "application/json");

            var result = Task.Run(() => httpClient.PostAsync(printer.remotepath + "/TextilePOS/Print"
                , content)).Result;

            var response = result.Content.ReadAsStringAsync().Result;
        }

        public bool DirectPrint(Printer printer, byte[] data)
        {
            return Print(0, printer, data);
        }
        private bool Print(int index,Printer printer, byte[] data)
        {

            if (loginput)
                Logger.Current.Info(System.Text.ASCIIEncoding.ASCII.GetString(data));
            //if(data.Length>2 && 
            //    data[data.Length-1] != 10 &&
            //    data[data.Length - 2] != 13 )
            //{
            //    List<byte> bytes = new List<byte>(data);
            //    bytes.Add(13);
            //    bytes.Add(10);
            //    data = bytes.ToArray();
            //}
            string printerName = string.Format(@"{0}", printer.name);
            NativeMethods.DOC_INFO_1 documentInfo;
            IntPtr printerHandle;
            documentInfo = new NativeMethods.DOC_INFO_1();
            documentInfo.pDataType = "RAW";
            documentInfo.pDocName = "QUANTO POS";
            printerHandle = new IntPtr(0);

            if (NativeMethods.OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
            {
                if (NativeMethods.StartDocPrinter(printerHandle, 1, documentInfo))
                {
                    int bytesWritten;
                    byte[] managedData;
                    IntPtr unmanagedData;
                    managedData = data;
                    unmanagedData = Marshal.AllocCoTaskMem(managedData.Length);
                    Marshal.Copy(managedData, 0, unmanagedData, managedData.Length);

                    if (NativeMethods.StartPagePrinter(printerHandle))
                    {
                        NativeMethods.WritePrinter(
                            printerHandle,
                            unmanagedData,
                            managedData.Length,
                            out bytesWritten);
                        NativeMethods.EndPagePrinter(printerHandle);
                        Marshal.FreeCoTaskMem(unmanagedData);
                        NativeMethods.EndDocPrinter(printerHandle);
                        NativeMethods.ClosePrinter(printerHandle);
                    }
                    else
                    {
                        Marshal.FreeCoTaskMem(unmanagedData);
                        NativeMethods.EndDocPrinter(printerHandle);
                        NativeMethods.ClosePrinter(printerHandle);
                        throw new Exception("Can't start page printer");
                    }


                }
                else
                {
                    throw new Exception("can't startdoc printer");
                }

            }
            else
            {
                throw new Exception("can't open printer");
            }
            return true;
        }


        public bool PrintPDF(PrintDocument document)
        {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(document.content);

            try
            {
                lock (lockobject)
                {
                    //Logger.Current.Info("Line 1");
                    if (printerlist == null || !printerlist.Exists(e => e.id == document.id))
                    {
                        printerlist = ListOfPrinters();
                        //Logger.Current.Info("Line 2");
                    }
                    else
                    {
                        printerlist = ListOfPrinters();
                        if (!printerlist.Exists(e => e.id == document.id))
                        {
                            Logger.Current.Error(document.encoded + "\r\n" + document.content, new Exception("Can't find the printer " + document.id));
                            Newtonsoft.Json.JsonConvert.SerializeObject(printerlist);
                            return false;
                        }
                    }
                    Printer printer = printerlist.Find(e => e.id == document.id);
                    if (document.encoded == "BASE64")
                    {
                        string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idata in mprintdata)
                        {
                            try
                            {
                                data = Convert.FromBase64String(idata);
                                if (loginput)
                                    Logger.Current.Info(System.Text.ASCIIEncoding.ASCII.GetString(data));
                                if (!PrintPDF(printer, Newtonsoft.Json.JsonConvert.DeserializeObject<PDFGraphics.JSONDocument>(System.Text.ASCIIEncoding.ASCII.GetString(data))))
                                    return false;
                            }
                            catch (Exception exp)
                            {
                                Logger.Current.Error(idata, exp);
                            }
                        }
                    }
                    else if (document.encoded == "UNICODEBASE64")
                    {
                        string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idata in mprintdata)
                        {
                            try
                            {
                                data = Convert.FromBase64String(idata);
                                if (loginput)
                                    Logger.Current.Info(System.Text.ASCIIEncoding.ASCII.GetString(data));
                                if (!PrintPDF(printer, Newtonsoft.Json.JsonConvert.DeserializeObject<PDFGraphics.JSONDocument>(System.Text.UnicodeEncoding.Unicode.GetString(data))))
                                    return false;
                            }
                            catch (Exception exp)
                            {
                                Logger.Current.Error(idata, exp);
                            }
                        }
                    }
                    else
                    {
                        PrintPDF(printer, Newtonsoft.Json.JsonConvert.DeserializeObject<PDFGraphics.JSONDocument>(System.Text.ASCIIEncoding.ASCII.GetString(data)));
                    }
                }
            }
            catch (Exception exp)
            {

                Logger.Current.Error(document.encoded + "\r\n" + document.content, exp);
            }
            return true;
        }

        private bool PrintPDF(Printer printer, byte[] data)
        {

            if (loginput)
                Logger.Current.Info(System.Text.ASCIIEncoding.ASCII.GetString(data));

            var document = Newtonsoft.Json.JsonConvert.DeserializeObject<PDFGraphics.JSONDocument>(System.Text.ASCIIEncoding.ASCII.GetString(data));
            ReportLibrary.JSONPrinter jSONPrinter = new ReportLibrary.JSONPrinter();
            jSONPrinter.Print(printer, document);
            return true;
        }

        private bool PrintPDF(Printer printer, PDFGraphics.JSONDocument document)
        {
            ReportLibrary.JSONPrinter jSONPrinter = new ReportLibrary.JSONPrinter();
            if (document.noofcopy <= 0) document.noofcopy = 1;
            for (int i = 0; i < document.noofcopy; i++)
            {
                jSONPrinter = new ReportLibrary.JSONPrinter();
                jSONPrinter.Print(printer, document);
            }
            return true;
        }

        DateTime? lastRemovePrinter = null;
        List<Printer> remoteprinterlist = null;
        public List<Printer> ProxyPrinters()
        {
            if (lastRemovePrinter.HasValue &&
               remoteprinterlist != null &&
               DateTime.Now.Subtract(lastRemovePrinter.Value).TotalMinutes < 60)
            {
                return remoteprinterlist;
            }

            List<Printer> lst = new List<Printer>();
            var SharedIPList = System.Configuration.ConfigurationManager.AppSettings["SharedIPList"];
            if (!string.IsNullOrEmpty(SharedIPList))
            {
                var iplist = SharedIPList.Split(',');
                foreach (var ip in iplist)
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var gresult = Task.Run(() => httpClient.GetAsync(ip + "/TextilePOS/listoflocalPrinters")).Result;
                            if (gresult.StatusCode == HttpStatusCode.OK)
                            {
                                var json = gresult.Content.ReadAsStringAsync().Result;
                                var lstnew = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Printer>>(json);
                                var iparts = ip.Split(new string[] { "//", ":" }, StringSplitOptions.RemoveEmptyEntries);
                                if (iparts.Length > 2)
                                {
                                    lstnew.ForEach(x =>
                                    {
                                        x.name = x.name + "(" + iparts[1] + ")";
                                        x.remote = true;
                                        x.remotepath = ip;
                                        x.remoteid = x.id;
                                        x.id = sha256_hash(x.name + x.id);
                                    });
                                }
                                lst.AddRange(lstnew);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.Current.Error(ip + "\r\n" + exp.Message);
                    }
                }
            }
            lastRemovePrinter = DateTime.Now;
            remoteprinterlist = lst;
            return lst;
        }


        public List<Printer> ListOfPrinters()
        {
            var printers = ListOfLocalPrinters();
            printers.AddRange(ProxyPrinters());
            return printers;
        }

        public List<Printer> ListOfLocalPrinters()
        {
            List<Printer> lst = new List<Printer>();
            try
            {

                foreach (string printname in PrinterSettings.InstalledPrinters)
                {
                    Printer printer = new Printer();
                    printer.name = printname;
                    printer.id = sha256_hash(printname);

                    lst.Add(printer);
                }
            }
            catch (Exception exception)
            {
                Logger.Current.Error(exception);
            }

            if (loginput)
                Logger.Current.Info(JsonConvert.SerializeObject(lst));

            try
            {


                string query = "SELECT Name, ServerName, ShareName, PortName, DriverName, PrinterStatus FROM Win32_Printer";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject printer in searcher.Get())
                {
                    if (printer["ShareName"] != null &&
                        !string.IsNullOrEmpty(printer["ShareName"].ToString()))
                    {
                        Printer printeritem = new Printer();
                        printeritem.name = printer["Name"].ToString();
                        printeritem.id = sha256_hash(printeritem.name);
                        lst.Add(printeritem);
                    }
                }

                //var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Shared = TRUE");
                //foreach (ManagementObject printer in searcher.Get())
                //{
                //    string name = printer["Name"]?.ToString();
                //    string shareName = printer["ShareName"]?.ToString();
                //    string serverName = printer["ServerName"]?.ToString(); // Might be null for local printers

                //    Console.WriteLine($"Printer: {name}");
                //    Console.WriteLine($"Share Name: {shareName}");

                //    // Construct UNC path
                //    if (!string.IsNullOrEmpty(shareName))
                //    {
                //        string unc = $@"\\{Environment.MachineName}\{shareName}";
                //        Printer printeritem = new Printer();
                //        printeritem.name = unc;
                //        printeritem.id = sha256_hash(printeritem.name);
                //    }
                //}

                //System.Management.ManagementScope objMS =
                //   new System.Management.ManagementScope(ManagementPath.DefaultPath);
                //objMS.Connect();



                //SelectQuery objQuery = new SelectQuery("SELECT * FROM Win32_Printer");
                //ManagementObjectSearcher objMOS = new ManagementObjectSearcher(objMS, objQuery);
                //System.Management.ManagementObjectCollection objMOC = objMOS.Get();

                //foreach (ManagementObject Printers in objMOC)
                //{
                //    //Logger.Current.Info(Printers.GetPropertyValue("DeviceID"));
                //    //Logger.Current.Info(Printers.GetPropertyValue("Name"));
                //    if (!lst.Exists(e => e.name == Printers.GetPropertyValue("DeviceID").ToString()))     // ALL NETWORK PRINTERS.
                //    {
                //        Printer printer = new Printer();
                //        printer.name = Printers.GetPropertyValue("DeviceID").ToString();
                //        printer.id = sha256_hash(printer.name);
                //    }
                //}
            }
            catch (Exception exception)
            {
                Logger.Current.Error(exception);
            }
            return lst;
        }

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        internal List<Printer> ListOfRemotePrinters(string ipaddress, string port)
        {
            List<Printer> lst = new List<Printer>();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var gresult = Task.Run(() => httpClient.GetAsync("http://" + ipaddress + ":" + port + "/TextilePOS/listoflocalPrinters")).Result;
                    if (gresult.StatusCode == HttpStatusCode.OK)
                    {
                        var json = gresult.Content.ReadAsStringAsync().Result;
                        var lstnew = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Printer>>(json);
                        lstnew.ForEach(x =>
                        {
                            x.name = x.name + "(" + ipaddress + ")";
                            x.remote = true;
                            x.remotepath = ipaddress;
                            x.remoteid = x.id;
                            x.id = sha256_hash(x.name + x.id);
                        });
                        lst.AddRange(lstnew);
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Current.Error(ipaddress + ":" + port + "\r\n" + exp.Message);
            }
            return lst;
        }
    }
}
