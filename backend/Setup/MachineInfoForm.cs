using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Quanto
{
    public class MachineInfoForm : Form
    {
        public MachineInfoForm()
        {
            Text = "Device Information";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(480, 360);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            var sb = new StringBuilder();
            sb.AppendLine("Configured: " + (MachineConfig.IsConfigured ? "Yes" : "No"));
            sb.AppendLine("Role: " + (MachineConfig.MachineRole ?? "(none)"));
            sb.AppendLine("Device ID: " + (MachineConfig.DeviceId ?? "(none)"));
            sb.AppendLine("Listen URL: " + MachineConfig.ListenUrl);
            sb.AppendLine("API Port: " + MachineConfig.ApiPort);
            sb.AppendLine("Machine: " + Environment.MachineName);
            sb.AppendLine();
            sb.AppendLine("Private IPs:");
            var ips = NetworkInfo.GetPrivateIPv4Addresses();
            if (ips.Count == 0) sb.AppendLine("  (none detected)");
            else foreach (var ip in ips) sb.AppendLine("  " + ip);

            if (MachineConfig.IsClient)
            {
                sb.AppendLine();
                sb.AppendLine("Master Device ID: " + MachineConfig.MasterDeviceId);
                sb.AppendLine("Master Name: " + MachineConfig.LastKnownMasterName);
                sb.AppendLine("Master API: " + MachineConfig.MasterApiUrl);
                sb.AppendLine("Last Known Master IP: " + MachineConfig.LastKnownMasterIp);
                sb.AppendLine("Master Connected: " + (MasterConnectionMonitor.MasterConnected ? "Yes" : "No"));
                sb.AppendLine("Current Master IP: " + (MasterConnectionMonitor.MasterIp ?? "(n/a)"));
            }

            Controls.Add(new Label
            {
                Text = "Quanto Device Information",
                Font = new Font("Segoe UI Semibold", 12F),
                Location = new Point(20, 16),
                AutoSize = true
            });

            Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 50),
                Size = new Size(440, 250),
                Text = sb.ToString(),
                Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.FixedSingle
            });

            var btn = new Button
            {
                Text = "Close",
                Location = new Point(360, 310),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };
            Controls.Add(btn);
            AcceptButton = btn;
        }
    }
}
