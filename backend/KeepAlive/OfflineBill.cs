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
    public class OfflineBill
    {
        private static OfflineBill instance;
        public static OfflineBill Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OfflineBill();
                    instance.Intiaize();
                }
                return instance;
            }
        }
        int seconds;
        System.Timers.Timer timer = null;
        public bool running { get; set; }
        public string AutoSync_Mode { get; set; }
        public int AutoSync_Cycle { get; set; }
        public OfflineBill()
        {
            
        }

        public void Intiaize()
        {
            if (InfyPOS.Processors.BillManager.Instance.Data != null &&
                InfyPOS.Processors.BillManager.Instance.Data.AutoSync)
            {
                if (AutoSync_Mode != InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode ||
                AutoSync_Cycle != InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle)
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    AutoSync_Mode = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode;
                    AutoSync_Cycle = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle;
                    switch (AutoSync_Mode)
                    {
                        case "Hour":
                            seconds = AutoSync_Cycle * 60 * 60;
                            break;
                        case "Minute":
                            seconds = AutoSync_Cycle * 60;
                            break;
                        case "Day":
                            seconds = AutoSync_Cycle * 60 * 60 * 60;
                            break;
                    }
                    if (seconds > 0)
                    {
                        running = true;
                        timer = new System.Timers.Timer(seconds * 1000);
                        timer.Elapsed += Timer_Elapsed;
                    }
                }
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                }
            }
        }

        System.Threading.AutoResetEvent autoreset = new System.Threading.AutoResetEvent(false);
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!autoreset.WaitOne(100)) return;
            try
            {

                var Ready = InfyPOS.Processors.BillManager.Instance.Initialize(new System.ComponentModel.DoWorkEventArgs(null), sender as System.ComponentModel.BackgroundWorker);
                if (Ready)
                {


                    ServiceProxy.Instance.DownloadMasterFromConfiguredSource(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                    {
                        organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                        locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                        datatype = "MASTER",
                        systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                        datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                        userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                    });

                    var sourceList = InfyPOS.Processors.BillManager.Instance.Bills.ToList();
                    byte[] sourceBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(sourceList));

                    var result = ServiceProxy.Instance.SyncOffline(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                    {
                        datatype = "BILL",
                        datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                        data = InfyPOS.Processors.BufferedRealtimeCompressionEngine.Compress(sourceBytes),
                        systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                        locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                        organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                        userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                    });

                    var response = result.Result;
                    while (!response.completed && !response.error)
                    {
                        System.Threading.Thread.Sleep(1000);
                        var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                        if (statusresponse.completed || statusresponse.error)
                        {
                            break;
                        }
                    }


                    var sourceSettlementList = InfyPOS.Processors.BillManager.Instance.Settlements.ToList();
                    sourceBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(sourceSettlementList));

                    result = ServiceProxy.Instance.SyncOffline(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                    {
                        datatype = "SETTLEMENT",
                        datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                        data = InfyPOS.Processors.BufferedRealtimeCompressionEngine.Compress(sourceBytes),
                        systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                        locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                        organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                        userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                    });
                    response = result.Result;
                    while (!response.completed && !response.error)
                    {
                        System.Threading.Thread.Sleep(1000);
                        var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                        if (statusresponse.completed || statusresponse.error)
                        {
                            break;
                        }
                    }
                }
            }catch(Exception exp)
            {
                Quanto.Logger.Current.Error(exp);
            }
            finally
            {
                autoreset.Set();
            }
        }




        public void Start()
        {
            if (running)
            {
                Quanto.Logger.Current.Info("Keep alive service starting...");
                timer.Enabled = true;
                timer.Start();
                Quanto.Logger.Current.Info("Keep alive service started");
            }
        }

        public void Stop()
        {
            if (running && timer.Enabled)
            {
                timer.Stop();
                timer.Enabled = false;
            }
        }
    }
}
