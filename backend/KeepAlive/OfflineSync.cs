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

        private static string _cloudOrg;
        private static string _cloudLocation;
        private static string _cloudUser;
        private static string _cloudPassword;
        private static bool _cloudLoginRemembered;

        public static void RememberCloudLogin(string organizationCode, string locationCode, string username, string password)
        {
            _cloudOrg = organizationCode ?? "";
            _cloudLocation = locationCode ?? "";
            _cloudUser = username ?? "";
            _cloudPassword = password ?? "";
            _cloudLoginRemembered = true;

            Configform.AddOrUpdateAppSettings("OrganizationCode", _cloudOrg);
            Configform.AddOrUpdateAppSettings("Locationcode", _cloudLocation);
            Configform.AddOrUpdateAppSettings("CloudUsername", _cloudUser);
            Configform.AddOrUpdateAppSettings("CloudPassword", _cloudPassword);
        }

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
            if (MachineConfig.IsClient)
            {
                Logger.Current.InfoFormat("CLIENT pulling master/stock from MASTER (every {0} minute(s))", _minutes);
                try
                {
                    BillManager.Instance.RefreshClientMasterFromMaster();
                    Logger.Current.Info("CLIENT master refresh completed.");
                }
                catch (Exception exp)
                {
                    Logger.Current.Info("CLIENT master refresh error: " + exp.Message);
                }
                return;
            }

            if (!MachineConfig.IsMaster)
                return;

            var serverUrl = ConfigurationManager.AppSettings["ServerURL"];
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                Logger.Current.Info("Scheduled master download skipped: ServerURL is empty.");
                return;
            }

            var orgCode = _cloudLoginRemembered ? _cloudOrg : ConfigurationManager.AppSettings["OrganizationCode"];
            var locationCode = _cloudLoginRemembered ? _cloudLocation : ConfigurationManager.AppSettings["Locationcode"];
            var username = _cloudLoginRemembered ? _cloudUser : ConfigurationManager.AppSettings["CloudUsername"];
            var password = _cloudLoginRemembered ? _cloudPassword : ConfigurationManager.AppSettings["CloudPassword"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Logger.Current.Info(
                    "Scheduled master download skipped: CloudUsername or CloudPassword is empty. Open Connect Server and download once, or set those keys in App.config.");
                return;
            }

            var request = new OfflineClient.WindowsOfflineRequest
            {
                orgainzationcode = orgCode,
                locationcode = locationCode,
                datatype = "master",
                username = username,
                password = password,
                systemkey = BillManager.Instance.SystemKey(),
                userid = BillManager.Instance.CurrentUser != null ? BillManager.Instance.CurrentUser.id : 0
            };

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
            if (!MachineConfig.IsMaster && !MachineConfig.IsClient)
            {
                Logger.Current.Info("Scheduled sync not started (this computer is not MASTER or CLIENT).");
                return;
            }

            if (_timer.Enabled)
                return;

            _timer.Enabled = true;
            _timer.Start();
            if (MachineConfig.IsClient)
            {
                Logger.Current.InfoFormat(
                    "CLIENT master refresh started: every {0} minute(s) from MASTER",
                    _minutes);
            }
            else
            {
                Logger.Current.InfoFormat(
                    "Scheduled master download started: every {0} minute(s) from {1} (config {2})",
                    _minutes,
                    ConfigurationManager.AppSettings["ServerURL"],
                    AppConfigFile.Path ?? "");
            }
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(8000);
                Timer_Elapsed(null, null);
            });
        }

        public static void StartIfConfigured()
        {
            var enabled = ConfigurationManager.AppSettings["EnableOffline"];
            if (string.IsNullOrEmpty(enabled) ||
                !enabled.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Current.Info(
                    "Scheduled master download not started: EnableOffline=" +
                    (enabled ?? "(missing)") +
                    ". Set EnableOffline=true in App.config next to the EXE.");
                return;
            }
            Instance.Start();
        }

        public static void StopIfConfigured()
        {
            if (instance == null)
                return;
            instance.Stop();
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
