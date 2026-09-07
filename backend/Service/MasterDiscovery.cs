using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Quanto
{
    /// <summary>
    /// Lightweight UDP LAN discovery for MASTER (no mDNS dependency).
    /// </summary>
    public static class MasterDiscovery
    {
        public const string DiscoverRequest = "DISCOVER_QUANTO_MASTER";

        private static UdpClient _listener;
        private static Thread _thread;
        private static volatile bool _running;

        public class DiscoveryResponse
        {
            public string deviceId { get; set; }
            public string machineRole { get; set; }
            public string machineName { get; set; }
            public int port { get; set; }
        }

        public class FoundMaster
        {
            public string DeviceId { get; set; }
            public string MachineName { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }

            public override string ToString()
            {
                return string.Format("{0}  |  {1}  |  {2}:{3}",
                    MachineName ?? "(unknown)",
                    DeviceId,
                    IpAddress,
                    Port);
            }
        }

        public static void StartResponder()
        {
            if (!MachineConfig.IsMaster) return;
            if (_running) return;

            _running = true;
            _thread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "QuantoMasterDiscovery"
            };
            _thread.Start();
            Logger.Current.InfoFormat("MASTER UDP discovery listening on port {0}", MachineConfig.DiscoveryUdpPort);
        }

        public static void StopResponder()
        {
            _running = false;
            try { _listener?.Close(); } catch { /* ignore */ }
            _listener = null;
        }

        private static void ListenLoop()
        {
            try
            {
                _listener = new UdpClient(MachineConfig.DiscoveryUdpPort);
                _listener.EnableBroadcast = true;
                var endpoint = new IPEndPoint(IPAddress.Any, 0);

                while (_running)
                {
                    try
                    {
                        var data = _listener.Receive(ref endpoint);
                        var text = Encoding.UTF8.GetString(data ?? Array.Empty<byte>()).Trim();
                        if (!text.StartsWith(DiscoverRequest, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var payload = JsonConvert.SerializeObject(new DiscoveryResponse
                        {
                            deviceId = MachineConfig.DeviceId,
                            machineRole = "Master",
                            machineName = Environment.MachineName,
                            port = MachineConfig.ApiPort
                        });
                        var bytes = Encoding.UTF8.GetBytes(payload);
                        _listener.Send(bytes, bytes.Length, endpoint);
                    }
                    catch (SocketException)
                    {
                        if (!_running) break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception exp)
                    {
                        if (_running)
                            Logger.Current.Error("MASTER discovery responder error", exp);
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Failed to start MASTER UDP discovery listener", exp);
            }
        }

        /// <summary>
        /// Discover all MASTER responses on the LAN (unique by DeviceId).
        /// </summary>
        public static List<FoundMaster> DiscoverAllMasters(int timeoutMs = 3500)
        {
            var found = new Dictionary<string, FoundMaster>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var client = new UdpClient())
                {
                    client.EnableBroadcast = true;
                    client.Client.ReceiveTimeout = 400;
                    var request = Encoding.UTF8.GetBytes(DiscoverRequest);
                    SendBroadcasts(client, request);

                    var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                    while (DateTime.UtcNow < deadline)
                    {
                        try
                        {
                            var remote = new IPEndPoint(IPAddress.Any, 0);
                            var data = client.Receive(ref remote);
                            var json = Encoding.UTF8.GetString(data);
                            var resp = JsonConvert.DeserializeObject<DiscoveryResponse>(json);
                            if (resp == null) continue;
                            if (!string.Equals(resp.machineRole, "Master", StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (string.IsNullOrWhiteSpace(resp.deviceId))
                                continue;
                            if (IPAddress.IsLoopback(remote.Address))
                                continue;

                            var ip = remote.Address.ToString();
                            var port = resp.port > 0 ? resp.port : MachineConfig.DefaultApiPort;
                            found[resp.deviceId] = new FoundMaster
                            {
                                DeviceId = resp.deviceId,
                                MachineName = resp.machineName,
                                IpAddress = ip,
                                Port = port
                            };
                        }
                        catch (SocketException)
                        {
                            // receive timeout slice
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Current.Error("DiscoverAllMasters failed", exp);
            }
            return found.Values.OrderBy(m => m.MachineName).ThenBy(m => m.DeviceId).ToList();
        }

        public static string DiscoverMasterIp(string expectedMasterDeviceId, int timeoutMs = 2500)
        {
            if (string.IsNullOrWhiteSpace(expectedMasterDeviceId))
                return null;

            var all = DiscoverAllMasters(timeoutMs);
            var match = all.FirstOrDefault(m =>
                string.Equals(m.DeviceId, expectedMasterDeviceId, StringComparison.OrdinalIgnoreCase));
            return match?.IpAddress;
        }

        private static void SendBroadcasts(UdpClient client, byte[] request)
        {
            var broadcast = new IPEndPoint(IPAddress.Broadcast, MachineConfig.DiscoveryUdpPort);
            client.Send(request, request.Length, broadcast);

            foreach (var localIp in NetworkInfo.GetPrivateIPv4Addresses())
            {
                try
                {
                    if (!IPAddress.TryParse(localIp, out var addr)) continue;
                    var bytes = addr.GetAddressBytes();
                    bytes[3] = 255;
                    var subnetBroadcast = new IPAddress(bytes);
                    client.Send(request, request.Length, new IPEndPoint(subnetBroadcast, MachineConfig.DiscoveryUdpPort));
                }
                catch { /* ignore */ }
            }
        }
    }
}
