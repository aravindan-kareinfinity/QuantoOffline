using System;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json;

namespace Quanto
{
    /// <summary>
    /// Centralized Master/Client machine identity and LAN settings (App.config).
    /// </summary>
    public static class MachineConfig
    {
        public const int DefaultApiPort = 8099;
        public const string DefaultListenUrl = "http://0.0.0.0:8099";
        public const int DiscoveryUdpPort = 18099;

        private static readonly object Sync = new object();
        private static bool _initialized;

        public static bool IsConfigured
        {
            get
            {
                var v = App("IsConfigured");
                return !string.IsNullOrWhiteSpace(v) &&
                       (v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1");
            }
        }

        public static string MachineRole
        {
            get
            {
                var role = App("MachineRole");
                return string.IsNullOrWhiteSpace(role) ? "" : role.Trim();
            }
        }

        public static bool IsMaster =>
            MachineRole.Equals("Master", StringComparison.OrdinalIgnoreCase);

        public static bool IsClient =>
            MachineRole.Equals("Client", StringComparison.OrdinalIgnoreCase);

        public static string DeviceId => App("DeviceId")?.Trim() ?? "";

        public static string MasterDeviceId => App("MasterDeviceId")?.Trim() ?? "";

        public static string MasterApiUrl => App("MasterApiUrl")?.Trim().TrimEnd('/') ?? "";

        public static string LastKnownMasterIp => App("LastKnownMasterIp")?.Trim() ?? "";

        public static int MasterPort
        {
            get
            {
                if (int.TryParse(App("MasterPort"), out var port) && port > 0 && port < 65536)
                    return port;
                return ApiPort;
            }
        }

        public static int ApiPort
        {
            get
            {
                if (int.TryParse(App("ApiPort"), out var port) && port > 0 && port < 65536)
                    return port;
                if (Uri.TryCreate(ListenUrl, UriKind.Absolute, out var uri) && uri.Port > 0)
                    return uri.Port;
                return DefaultApiPort;
            }
        }

        public static string ListenUrl
        {
            get
            {
                var url = App("ListenUrl");
                if (string.IsNullOrWhiteSpace(url))
                    return DefaultListenUrl;
                return url.Trim();
            }
        }

        public static string AllowedOrigins => App("AllowedOrigins")?.Trim() ?? "";

        /// <summary>
        /// Called after role is known. Does not invent a role when unconfigured.
        /// </summary>
        public static void EnsureInitialized()
        {
            lock (Sync)
            {
                if (_initialized) return;

                if (string.IsNullOrWhiteSpace(App("ListenUrl")))
                    SaveSetting("ListenUrl", DefaultListenUrl);

                if (string.IsNullOrWhiteSpace(App("ApiPort")))
                    SaveSetting("ApiPort", DefaultApiPort.ToString());

                if (IsConfigured && string.IsNullOrWhiteSpace(DeviceId) && !string.IsNullOrWhiteSpace(MachineRole))
                {
                    SaveSetting("DeviceId", GenerateDeviceId(MachineRole));
                    Logger.Current.InfoFormat("Generated persistent DeviceId={0}", DeviceId);
                }

                if (IsClient && string.IsNullOrWhiteSpace(App("MasterSource")))
                    SaveSetting("MasterSource", "Client");

                _initialized = true;
                Logger.Current.InfoFormat(
                    "MachineConfig: Configured={0} Role={1} DeviceId={2} ListenUrl={3}",
                    IsConfigured, MachineRole, DeviceId, ListenUrl);
            }
        }

        public static void ResetInitializedFlag()
        {
            lock (Sync) { _initialized = false; }
        }

        public static string GenerateDeviceId(string role)
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();
            if (role != null && role.Equals("Client", StringComparison.OrdinalIgnoreCase))
                return "POS-" + suffix;
            return "MASTER-" + suffix;
        }

        public static string EnsureDeviceIdForRole(string role)
        {
            if (!string.IsNullOrWhiteSpace(DeviceId))
            {
                // Keep existing ID; optionally align prefix only when empty role change left mismatched — do not rewrite.
                return DeviceId;
            }
            var id = GenerateDeviceId(role);
            SaveSetting("DeviceId", id);
            return id;
        }

        public static void ConfigureAsMaster()
        {
            var deviceId = EnsureDeviceIdForRole("Master");
            // If existing ID was POS-* from prior client role, generate a master ID only when prefix wrong after explicit reconfigure.
            if (deviceId.StartsWith("POS-", StringComparison.OrdinalIgnoreCase))
            {
                deviceId = GenerateDeviceId("Master");
                SaveSetting("DeviceId", deviceId);
            }

            SaveSetting("MachineRole", "Master");
            SaveSetting("MasterSource", "Server");
            SaveSetting("ListenUrl", DefaultListenUrl);
            SaveSetting("ApiPort", DefaultApiPort.ToString());
            SaveSetting("MasterDeviceId", "");
            SaveSetting("MasterApiUrl", "");
            SaveSetting("LastKnownMasterIp", "");
            SaveSetting("MasterPort", "");
            SaveSetting("IsConfigured", "true");
            ResetInitializedFlag();
            EnsureInitialized();
        }

        public static void ConfigureAsClientPendingMaster()
        {
            var deviceId = EnsureDeviceIdForRole("Client");
            if (deviceId.StartsWith("MASTER-", StringComparison.OrdinalIgnoreCase))
            {
                deviceId = GenerateDeviceId("Client");
                SaveSetting("DeviceId", deviceId);
            }

            SaveSetting("MachineRole", "Client");
            SaveSetting("ListenUrl", DefaultListenUrl);
            SaveSetting("ApiPort", DefaultApiPort.ToString());
            SaveSetting("MasterSource", "Client");
            // Not fully configured until master is connected
            SaveSetting("IsConfigured", "false");
            ResetInitializedFlag();
            EnsureInitialized();
        }

        public static void SaveMasterConnection(string masterDeviceId, string machineName, string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(masterDeviceId) || string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("Master DeviceId and IP are required.");

            var apiPort = port > 0 ? port : DefaultApiPort;
            var url = "http://" + ip.Trim() + ":" + apiPort;

            SaveSetting("MachineRole", "Client");
            SaveSetting("MasterDeviceId", masterDeviceId.Trim());
            SaveSetting("MasterApiUrl", url);
            SaveSetting("LastKnownMasterIp", ip.Trim());
            SaveSetting("MasterPort", apiPort.ToString());
            SaveSetting("ClientURL", url);
            SaveSetting("MasterSource", "Client");
            SaveSetting("ListenUrl", DefaultListenUrl);
            SaveSetting("ApiPort", DefaultApiPort.ToString());
            SaveSetting("IsConfigured", "true");
            if (!string.IsNullOrWhiteSpace(machineName))
                SaveSetting("LastKnownMasterName", machineName.Trim());

            EnsureDeviceIdForRole("Client");
            ResetInitializedFlag();
            EnsureInitialized();
        }

        public static void SaveLastKnownMasterIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return;
            SaveSetting("LastKnownMasterIp", ip.Trim());
            var port = MasterPort;
            var url = "http://" + ip.Trim() + ":" + port;
            SaveSetting("MasterApiUrl", url);
            SaveSetting("ClientURL", url);
            SaveSetting("MasterSource", "Client");
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string LastKnownMasterName => App("LastKnownMasterName")?.Trim() ?? "";

        public static bool VerifyRemoteMaster(string ip, int port, string expectedDeviceId, out string machineName, out string error)
        {
            machineName = null;
            error = null;
            try
            {
                var url = "http://" + ip + ":" + port + "/health";
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(4) })
                {
                    var response = http.GetAsync(url).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        error = "MASTER /health returned " + (int)response.StatusCode;
                        return false;
                    }
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var health = JsonConvert.DeserializeObject<HealthDto>(json);
                    if (health == null)
                    {
                        error = "Invalid /health response.";
                        return false;
                    }
                    if (!string.Equals(health.machineRole, "Master", StringComparison.OrdinalIgnoreCase))
                    {
                        error = "Discovered device is not a MASTER.";
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(expectedDeviceId) &&
                        !string.Equals(health.deviceId, expectedDeviceId, StringComparison.OrdinalIgnoreCase))
                    {
                        error = "MASTER DeviceId mismatch.";
                        return false;
                    }
                    machineName = health.machineName;
                    return string.Equals(health.status, "ok", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exp)
            {
                error = exp.Message;
                return false;
            }
        }

        public static void SaveSetting(string key, string value)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] == null)
                    config.AppSettings.Settings.Add(key, value ?? "");
                else
                    config.AppSettings.Settings[key].Value = value ?? "";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Failed to persist App.config key " + key, exp);
                throw;
            }
        }

        private static string App(string key) =>
            ConfigurationManager.AppSettings[key];

        private class HealthDto
        {
            public string status { get; set; }
            public string deviceId { get; set; }
            public string machineRole { get; set; }
            public string machineName { get; set; }
        }
    }
}
