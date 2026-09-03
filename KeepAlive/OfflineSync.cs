using Quanto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public class OfflineSync
    {
        private static OfflineSync instance;
        public static OfflineSync Instance
        {
            get
            {
                if (instance == null)
                {
                    int seconds = 15;
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["OfflineSyncTimer"]))
                        int.TryParse(System.Configuration.ConfigurationManager.AppSettings["OfflineSyncTimer"],out seconds);

                    instance = new OfflineSync(seconds);
                }
                return instance;
            }
        }
        int seconds;
        System.Timers.Timer timer = null;
        string OfflineSyncurl = "";
        public OfflineSync(int seconds)
        {
            this.seconds = seconds;
            timer = new System.Timers.Timer(seconds * 1000);
            timer.Elapsed += Timer_Elapsed;
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["OfflineSyncURL"]))
                OfflineSyncurl = System.Configuration.ConfigurationManager.AppSettings["OfflineSyncURL"];
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HttpPost(OfflineSyncurl);
        }

        public class CreateRequest 
        {
            public string location { get; set; }
            public string clientid { get; set; }
            public string privatekey { get; set; }
            public DateTime fromdate { get; set; }
        }

        private string HttpPost(string URI)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(new CreateRequest());
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");


                var result = Task.Run(() => client.PostAsync(URI + "/SyncController/SyncOffline", content)).Result;

                if (result.IsSuccessStatusCode)
                {
                    string json = result.Content.ReadAsStringAsync().Result;
                    
                }
                else
                {
                    string error = result.Content.ReadAsStringAsync().Result;
                    Logger.Current.Info("Error on Posting Employee in-out", new Exception(error));
                    return error;
                }
            }

            return "";
        }

        public void Start()
        {
            Quanto.Logger.Current.Info("Keep alive service starting...");
            if (string.IsNullOrEmpty(OfflineSyncurl)) return;
            timer.Enabled = true;
            timer.Start();
            Quanto.Logger.Current.Info("Keep alive service started");
        }

        public void Stop()
        {
            if (timer.Enabled)
            {
                timer.Stop();
                timer.Enabled = false;
            }
        }
    }
}
