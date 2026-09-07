using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Quanto
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("/health")]
        [HttpGet("/api/machine/info")]
        public IActionResult Get()
        {
            var ips = NetworkInfo.GetPrivateIPv4Addresses();
            var payload = new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["deviceId"] = MachineConfig.DeviceId,
                ["machineRole"] = MachineConfig.MachineRole,
                ["machineName"] = Environment.MachineName,
                ["ipAddresses"] = ips,
                ["port"] = MachineConfig.ApiPort
            };

            if (MachineConfig.IsClient)
            {
                payload["masterDeviceId"] = MachineConfig.MasterDeviceId;
                payload["masterConnected"] = MasterConnectionMonitor.MasterConnected;
                payload["masterIp"] = MasterConnectionMonitor.MasterIp
                    ?? (string.IsNullOrWhiteSpace(MachineConfig.LastKnownMasterIp)
                        ? null
                        : MachineConfig.LastKnownMasterIp);
            }

            return Ok(payload);
        }
    }
}
