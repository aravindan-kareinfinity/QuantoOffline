using InfyPOS.Processors;
using System;
using System.Configuration;
using System.Threading;

namespace Quanto
{
    /// <summary>
    /// MASTER only: download cloud master/stock every OfflineSyncTimer minutes (default 15).
    /// Started when EnableOffline=true.
    /// </summary>
    public class OfflineSync
    {
        private static OfflineSync instance;
        public static OfflineSync Instance
        {
            get
            {
                if (instance == null)
                    instance = new OfflineSync();
                return instance;
            }
        }

        private readonly int _minutes;
        private readonly System.Timers.Timer _timer;
        private readonly object _runLock = new object();

        public OfflineSync()
        {
            _minutes = 15;
            var raw = ConfigurationManager.AppSettings["OfflineSyncTimer"];
            if (!string.IsNullOrEmpty(raw))
            {
                int parsed;
                if (int.TryParse(raw, out parsed) && parsed > 0)
                    _minutes = parsed;
            }

            _timer = new System.Timers.Timer(_minutes * 60 * 1000d);
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(_runLock))
                return;
            try
            {
                DownloadMasterFromCloud();
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Scheduled master download failed", exp);
            }
            finally
            {
                Monitor.Exit(_runLock);
            }
        }

        private void DownloadMasterFromCloud()
        {
            if (!MachineConfig.IsMaster)
                return;

            var serverUrl = ConfigurationManager.AppSettings["ServerURL"];
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                Logger.Current.Info("Scheduled master download skipped: ServerURL is empty.");
                return;
            }

            var request = new OfflineClient.WindowsOfflineRequest
            {
                orgainzationcode = ConfigurationManager.AppSettings["OrganizationCode"],
                locationcode = ConfigurationManager.AppSettings["Locationcode"],
                datatype = "master",
                systemkey = BillManager.Instance.SystemKey(),
                userid = BillManager.Instance.CurrentUser != null ? BillManager.Instance.CurrentUser.id : 0
            };

            if (BillManager.Instance.Data != null)
            {
                request.organizationid = BillManager.Instance.Data.organizationid;
                request.locationid = BillManager.Instance.Data.locationid;
                request.datafrom = BillManager.Instance.Data.lastSyncOn;
            }

            Logger.Current.InfoFormat("Scheduled master download starting (every {0} minute(s))", _minutes);
            var result = ServiceProxy.Instance.DownloadMasterFromCloud(request);
            if (result == null)
            {
                Logger.Current.Info("Scheduled master download returned no response.");
                return;
            }
            if (result.error)
            {
                Logger.Current.Info("Scheduled master download error: " + result.errormessage);
                return;
            }
            Logger.Current.Info("Scheduled master download completed.");
        }

        public void Start()
        {
            if (!MachineConfig.IsMaster)
            {
                Logger.Current.Info("Scheduled master download not started (this computer is not MASTER).");
                return;
            }

            _timer.Enabled = true;
            _timer.Start();
            Logger.Current.InfoFormat(
                "Scheduled master download started: every {0} minute(s) from {1}",
                _minutes,
                ConfigurationManager.AppSettings["ServerURL"]);
        }

        public void Stop()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Enabled = false;
            }
        }
    }
}
