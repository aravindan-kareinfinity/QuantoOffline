using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public class KeepAlive
    {
        private static KeepAlive instance;
        public static KeepAlive Instance
        {
            get
            {
                if (instance == null)
                {
                    int seconds = 15;
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["KeepAliveTimer"]))
                        int.TryParse(System.Configuration.ConfigurationManager.AppSettings["KeepAliveTimer"],out seconds);

                    instance = new KeepAlive(seconds);
                }
                return instance;
            }
        }
        int seconds;
        System.Timers.Timer timer = null;
        string keepaliveurl = "";
        public KeepAlive(int seconds)
        {
            this.seconds = seconds;
            timer = new System.Timers.Timer(seconds * 1000);
            timer.Elapsed += Timer_Elapsed;
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["KeepAliveURL"]))
                keepaliveurl = System.Configuration.ConfigurationManager.AppSettings["KeepAliveURL"];
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string result = HttpGet(keepaliveurl);
                Quanto.Logger.Current.Info("Keep Alive Response : "+result);
            }
            catch(Exception exp)
            {
                Quanto.Logger.Current.Error("Keep Alive Exception", exp);
            }
        }

        private string HttpGet(string URI)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(keepaliveurl)) return;
            Quanto.Logger.Current.Info("Keep alive service starting..." + keepaliveurl);
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
