using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace Quanto
{
    /// <summary>
    /// CLIENT-only: verify MASTER via /health, retry last IP, then UDP discovery. Non-blocking.
    /// </summary>
    public static class MasterConnectionMonitor
    {
        private static Thread _thread;
        private static volatile bool _running;
        private static readonly object StateLock = new object();

        public static bool MasterConnected { get; private set; }
        public static string MasterIp { get; private set; }

        public static void Start()
        {
            if (!MachineConfig.IsClient) return;
            if (_running) return;

            _running = true;
            _thread = new Thread(MonitorLoop)
            {
                IsBackground = true,
                Name = "QuantoMasterMonitor"
            };
            _thread.Start();
            Logger.Current.Info("CLIENT master connection monitor started");
        }

        public static void Stop()
        {
            _running = false;
        }

        private static void MonitorLoop()
        {
            int delaySeconds = 5;
            while (_running)
            {
                try
                {
                    TryConnect();
                }
                catch (Exception exp)
                {
                    Logger.Current.Error("Master connection monitor error", exp);
                    SetDisconnected();
                }

                // Backoff when disconnected; stay calm when connected
                int sleep = MasterConnected ? 30 : delaySeconds;
                if (!MasterConnected)
                    delaySeconds = Math.Min(delaySeconds * 2, 120);

                for (int i = 0; i < sleep && _running; i++)
                    Thread.Sleep(1000);

                if (MasterConnected)
                    delaySeconds = 5;
            }
        }

        private static void TryConnect()
        {
            var expectedId = MachineConfig.MasterDeviceId;
            if (string.IsNullOrWhiteSpace(expectedId))
            {
                Logger.Current.Info("CLIENT: MasterDeviceId not configured; skipping master connect");
                SetDisconnected();
                return;
            }

            // 1) Last known IP
            var lastIp = MachineConfig.LastKnownMasterIp;
            if (!string.IsNullOrWhiteSpace(lastIp) && VerifyMaster(lastIp, expectedId))
            {
                SetConnected(lastIp);
                return;
            }

            // 2) MasterApiUrl host if present
            var apiUrl = MachineConfig.MasterApiUrl;
            if (!string.IsNullOrWhiteSpace(apiUrl) &&
                Uri.TryCreate(apiUrl, UriKind.Absolute, out var uri) &&
                !string.IsNullOrWhiteSpace(uri.Host) &&
                !uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                uri.Host != "127.0.0.1" &&
                uri.Host != "0.0.0.0" &&
                VerifyMaster(uri.Host, expectedId))
            {
                SetConnected(uri.Host);
                MachineConfig.SaveLastKnownMasterIp(uri.Host);
                return;
            }

            // 3) LAN discovery
            var discovered = MasterDiscovery.DiscoverMasterIp(expectedId);
            if (!string.IsNullOrWhiteSpace(discovered) && VerifyMaster(discovered, expectedId))
            {
                MachineConfig.SaveLastKnownMasterIp(discovered);
                SetConnected(discovered);
                return;
            }

            SetDisconnected();
        }

        public static bool VerifyMaster(string ip, string expectedDeviceId)
        {
            try
            {
                var url = "http://" + ip + ":" + MachineConfig.MasterPort + "/health";
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                {
                    var response = http.GetAsync(url).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        return false;
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var health = JsonConvert.DeserializeObject<HealthPayload>(json);
                    if (health == null) return false;
                    if (!string.Equals(health.machineRole, "Master", StringComparison.OrdinalIgnoreCase))
                        return false;
                    if (!string.Equals(health.deviceId, expectedDeviceId, StringComparison.OrdinalIgnoreCase))
                        return false;
                    return string.Equals(health.status, "ok", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        private static void SetConnected(string ip)
        {
            lock (StateLock)
            {
                MasterConnected = true;
                MasterIp = ip;
            }
            Logger.Current.InfoFormat("CLIENT connected to MASTER {0} at {1}", MachineConfig.MasterDeviceId, ip);
        }

        private static void SetDisconnected()
        {
            lock (StateLock)
            {
                MasterConnected = false;
            }
        }

        private class HealthPayload
        {
            public string status { get; set; }
            public string deviceId { get; set; }
            public string machineRole { get; set; }
        }
    }
}
