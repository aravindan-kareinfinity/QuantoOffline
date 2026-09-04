using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using InfyPOS.Processors;
using Newtonsoft.Json;
using Quanto.Printer;
using Quanto.SMS;
using static Quanto.Printer.PrinterService;
using static Quanto.TallyVocher;

namespace Quanto.Data
{
    public class DataManager
    {
        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new DataManager();
                return instance;
            }
        }

        MessageQueue<TransferDocument> senderqueue;
        bool hasstopped = false;
        System.Threading.Thread senderthread;
        System.Threading.AutoResetEvent are;

        System.Collections.Concurrent.ConcurrentDictionary<long, List<String>> confirmedMessages = new System.Collections.Concurrent.ConcurrentDictionary<long, List<string>>();
        public void Start()
        {
            are = new System.Threading.AutoResetEvent(false);
            senderqueue = new MessageQueue<TransferDocument>(int.MaxValue);

            senderthread = new System.Threading.Thread(new System.Threading.ThreadStart(Sender));
            senderthread.Start();
        }

        public void Stop()
        {
            hasstopped = true;
            are.Set();
            System.Threading.Thread.Sleep(1000);
            foreach (var entity in senderqueue.GetAll())
            {
                transportInstance.Send(entity);
            }
        }

        public string SentWebData(Quanto.Data.DataManager.TransferDocument request)
        {
            try
            {
                Logger.Current.Error(JsonConvert.SerializeObject(request), new Exception("SendWebData - Start"));
                if (request.mode == "get")
                {
                    var httpClient = new HttpClient();
                    var result = Task.Run(() => httpClient.GetAsync(request.destination)).Result;
                    return result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    var httpClient = new HttpClient();
                    var content = new StringContent(request.content,
                        Encoding.UTF8, "application/json");

                    var result = Task.Run(() => httpClient.PostAsync(request.destination
                        , content)).Result;
                    return result.Content.ReadAsStringAsync().Result;
                }
            }catch(Exception err)
            {
                Logger.Current.Error("SendData - Error", err);
                return @"{""success"":false,""message"":'"+ err.Message+ "'}";
            }
        }
        public void Sender()
        {
            System.Threading.Thread.Sleep(100);
            TransferDocument entity;
            while (senderqueue.Dequeue(out entity))
            {
                transportInstance.Send(entity);
            }
        }


        internal bool Sent(TransferDocument document)
        {
            if (hasstopped || senderqueue == null)
                return false;
            return senderqueue.Enqueue(document);

        }

        Transport transportInstance = null;
        private DataManager()
        {
            transportInstance = new Transport();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        public class Transport
        {
            public void Send(TransferDocument document)
            {
                if (document.mode.ToLower() == "file")
                    Sent2File(document);
                if (document.mode.ToLower() == "tcp")
                    Sent2TCP(document);
                if (document.mode.ToLower() == "web")
                    Sent2Web(document);
                if (document.mode.ToLower() == "sql")
                    Sent2DB(document);
            
            }
            public void Sent2DB(TransferDocument document)
            {
                byte[] content = null;
                using (var ms = new System.IO.MemoryStream())
                {
                    if (document.encoded == "BASE64")
                    {
                        string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idata in mprintdata)
                        {
                            var bytes = Convert.FromBase64String(document.content);
                            ms.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(document.content);
                        ms.Write(bytes, 0, bytes.Length);
                    }
                    content = ms.ToArray();
                }

                try
                {
                    
                    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(document.destination);
                    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(System.Text.ASCIIEncoding.ASCII.GetString(content),conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception exp)
                {
                    var file = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName);
                    var appDirectory = file.Directory.FullName;
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(appDirectory, "quanto.txt"), content);
                }
            }
            public void Sent2File(TransferDocument document)
            {
                byte[] content = null;
                using (var ms = new System.IO.MemoryStream())
                {
                    if (document.encoded == "BASE64")
                    {
                        string[] mprintdata = document.content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idata in mprintdata)
                        {
                            var bytes = Convert.FromBase64String(document.content);
                            ms.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(document.content);
                        ms.Write(bytes, 0, bytes.Length);
                    }
                    content = ms.ToArray();
                }

                try
                {
                    var path = new System.IO.FileInfo(document.destination);
                    if (!path.Directory.Exists)
                        path.Directory.Create();
                    System.IO.File.WriteAllBytes(path.FullName, content);
                }
                catch (Exception exp)
                {
                    var file = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName);
                    var appDirectory = file.Directory.FullName;
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(appDirectory, "quanto.txt"), content);
                }
            }
            public void Sent2TCP(TransferDocument document)
            {

            }
            public void Sent2Web(TransferDocument document)
            {

            }
        }


        public enum SendMode
        {
            File,
            TCP,
            Web
        }
        public class TransferDocument
        {
            public string mode { get; set; }
            public string destination { get; set; }
            public string content { get; set; }
            public string encoded { get; set; }

        }

    }
}
