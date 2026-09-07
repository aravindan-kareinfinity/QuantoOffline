using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Quanto
{
    public static class NetworkInfo
    {
        /// <summary>
        /// Active private IPv4 addresses (excludes loopback and non-private).
        /// </summary>
        public static List<string> GetPrivateIPv4Addresses()
        {
            var result = new List<string>();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    var props = ni.GetIPProperties();
                    foreach (var ua in props.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;
                        var ip = ua.Address.ToString();
                        if (IPAddress.IsLoopback(ua.Address))
                            continue;
                        if (!IsPrivateIPv4(ua.Address))
                            continue;
                        if (!result.Contains(ip))
                            result.Add(ip);
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Failed to enumerate private IPv4 addresses", exp);
            }
            return result;
        }

        public static bool IsPrivateIPv4(IPAddress address)
        {
            if (address == null || address.AddressFamily != AddressFamily.InterNetwork)
                return false;
            var bytes = address.GetAddressBytes();
            // 10.0.0.0/8
            if (bytes[0] == 10) return true;
            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168) return true;
            // 169.254.0.0/16 link-local (optional for ad-hoc LAN)
            if (bytes[0] == 169 && bytes[1] == 254) return true;
            return false;
        }
    }
}
