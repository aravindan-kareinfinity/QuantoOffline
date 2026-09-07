using System;
using System.Linq;

namespace Quanto
{
    /// <summary>
    /// Idempotent Windows Firewall rule for the local API port (private LAN reachability).
    /// </summary>
    public static class FirewallHelper
    {
        public const string RuleName = "Quanto Client API - TCP 8099";

        public static bool EnsureApiPortAllowed(int port, out string message)
        {
            message = null;
            try
            {
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (ruleType == null || policyType == null)
                {
                    message = "Windows Firewall COM API unavailable. Manually allow inbound TCP " + port + ".";
                    Logger.Current.Error(message);
                    return false;
                }

                dynamic policy = Activator.CreateInstance(policyType);
                foreach (dynamic existing in policy.Rules)
                {
                    try
                    {
                        string name = existing.Name;
                        if (string.Equals(name, RuleName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(name, "Quanto Printing Service", StringComparison.OrdinalIgnoreCase))
                        {
                            // Refresh port if needed
                            try { existing.LocalPorts = port.ToString(); } catch { /* ignore */ }
                            try { existing.Enabled = true; } catch { /* ignore */ }
                            message = "Firewall rule already present: " + name;
                            Logger.Current.Info(message);
                            EnsureUdpDiscoveryAllowed(MachineConfig.DiscoveryUdpPort);
                            return true;
                        }
                    }
                    catch
                    {
                        // skip unreadable rules
                    }
                }

                dynamic rule = Activator.CreateInstance(ruleType);
                rule.Name = RuleName;
                rule.Description = "Allow inbound TCP for Quanto.Client local API (LAN Master/Client).";
                rule.Protocol = 6; // TCP
                rule.LocalPorts = port.ToString();
                rule.Direction = 1; // inbound
                rule.Action = 1; // allow
                rule.Enabled = true;
                rule.InterfaceTypes = "All";
                // Prefer private + domain profiles when supported (bit flags)
                try { rule.Profiles = 7; } catch { /* All profiles fallback via InterfaceTypes */ }

                policy.Rules.Add(rule);
                message = "Created firewall rule: " + RuleName + " TCP " + port;
                Logger.Current.Info(message);

                // MASTER discovery uses UDP; allow it without opening extra TCP ports.
                EnsureUdpDiscoveryAllowed(MachineConfig.DiscoveryUdpPort);

                return true;
            }
            catch (UnauthorizedAccessException exp)
            {
                message =
                    "Cannot configure Windows Firewall (administrator permission required). " +
                    "Manually allow inbound TCP port " + port + " named \"" + RuleName + "\".";
                Logger.Current.Error(message, exp);
                return false;
            }
            catch (Exception exp)
            {
                message =
                    "Firewall configuration failed. Manually allow inbound TCP port " + port +
                    " (\"" + RuleName + "\"). " + exp.Message;
                Logger.Current.Error(message, exp);
                return false;
            }
        }

        private static void EnsureUdpDiscoveryAllowed(int udpPort)
        {
            const string udpRuleName = "Quanto Client Discovery - UDP 18099";
            try
            {
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (ruleType == null || policyType == null) return;

                dynamic policy = Activator.CreateInstance(policyType);
                foreach (dynamic existing in policy.Rules)
                {
                    try
                    {
                        if (string.Equals((string)existing.Name, udpRuleName, StringComparison.OrdinalIgnoreCase))
                            return;
                    }
                    catch { /* skip */ }
                }

                dynamic rule = Activator.CreateInstance(ruleType);
                rule.Name = udpRuleName;
                rule.Description = "Allow UDP discovery for Quanto.Client MASTER on private LAN.";
                rule.Protocol = 17; // UDP
                rule.LocalPorts = udpPort.ToString();
                rule.Direction = 1;
                rule.Action = 1;
                rule.Enabled = true;
                rule.InterfaceTypes = "All";
                try { rule.Profiles = 7; } catch { /* ignore */ }
                policy.Rules.Add(rule);
                Logger.Current.Info("Created firewall rule: " + udpRuleName);
            }
            catch (Exception exp)
            {
                Logger.Current.Error(
                    "Could not create UDP discovery firewall rule. Manual: allow inbound UDP " + udpPort, exp);
            }
        }
    }
}
