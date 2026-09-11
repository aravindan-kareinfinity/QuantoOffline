using System;
using System.Linq;
using System.Threading;

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
                    instance = new OfflineBill();
                return instance;
            }
        }
        int seconds;
        System.Timers.Timer timer = null;
        public bool running { get; set; }
        public string AutoSync_Mode { get; set; }
        public int AutoSync_Cycle { get; set; }
        private readonly object _runLock = new object();

        public static void StartIfConfigured()
        {
            Instance.Intiaize();
        }

        public void Intiaize()
        {
            Stop();
            running = false;
            seconds = 0;

            if (InfyPOS.Processors.BillManager.Instance.Data == null ||
                !InfyPOS.Processors.BillManager.Instance.Data.AutoSync)
            {
                Logger.Current.Info("Auto sync is off.");
                return;
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
                    seconds = AutoSync_Cycle * 24 * 60 * 60;
                    break;
            }

            if (seconds <= 0)
            {
                Logger.Current.Info("Auto sync not started: set Every and Hour/Minute/Day in Data Sync.");
                return;
            }

            running = true;
            timer = new System.Timers.Timer(seconds * 1000d);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(_runLock))
                return;
            try
            {
                Logger.Current.Info("Auto sync starting...");
                if (InfyPOS.Processors.BillManager.Instance.CurrentUser == null)
                {
                    Logger.Current.Info("Auto sync skipped: login required.");
                    return;
                }

                var Ready = InfyPOS.Processors.BillManager.Instance.Initialize(
                    new System.ComponentModel.DoWorkEventArgs(null),
                    sender as System.ComponentModel.BackgroundWorker);
                if (!Ready)
                {
                    Logger.Current.Info("Auto sync skipped: master data is not loaded.");
                    return;
                }

                ServiceProxy.Instance.DownloadMasterFromConfiguredSource(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                    locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                    datatype = "MASTER",
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                });

                var bills = InfyPOS.Processors.BillManager.Instance.Bills;
                if (bills == null)
                    bills = new System.Collections.Generic.List<InfyPOS.Processors.OfflineClient.Bill>();
                var sourceList = bills.ToList();
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
                    Thread.Sleep(1000);
                    var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                    if (statusresponse.completed || statusresponse.error)
                        break;
                }

                var settlements = InfyPOS.Processors.BillManager.Instance.Settlements;
                if (settlements == null)
                    settlements = new System.Collections.Generic.List<InfyPOS.Processors.OfflineClient.Settlement>();
                var sourceSettlementList = settlements.ToList();
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
                    Thread.Sleep(1000);
                    var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                    if (statusresponse.completed || statusresponse.error)
                        break;
                }

                Logger.Current.Info("Auto sync completed.");
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Auto sync failed", exp);
            }
            finally
            {
                Monitor.Exit(_runLock);
            }
        }

        public void Start()
        {
            if (!running || timer == null)
                return;
            timer.Enabled = true;
            timer.Start();
            Logger.Current.InfoFormat(
                "Auto sync started: every {0} {1}(s)",
                AutoSync_Cycle,
                AutoSync_Mode);
        }

        public void Stop()
        {
            if (timer == null)
                return;
            timer.Stop();
            timer.Enabled = false;
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            timer = null;
            running = false;
        }
    }
}
