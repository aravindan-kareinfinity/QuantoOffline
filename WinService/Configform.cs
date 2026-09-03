using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Quanto
{
    public partial class Configform : Form
    {
        public Configform()
        {
            InitializeComponent();
        }

        bool hascertificateinstalled = false;
        private void btnstart_Click(object sender, EventArgs e)
        {
            AddOrUpdateAppSettings("CompanyCode", textBox1.Text);
            AddOrUpdateAppSettings("ServerURL", textBox2.Text);
            AddOrUpdateAppSettings("LocationCode", textBox3.Text);
            AddOrUpdateAppSettings("PrintOn80Port", checkBox1.Checked ? "true" : "false");
            AddOrUpdateAppSettings("PrintOnSSLPort", checkBox2.Checked ? "true" : "false");
            if (checkBox2.Checked && !hascertificateinstalled)
            {
                InstallCertificate();
            }
            this.DialogResult = DialogResult.OK;
        }

        private void InstallCertificate()
        {
            string rootfolder = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName).DirectoryName;
            System.IO.File.WriteAllBytes(Path.Combine(rootfolder, "makecert.exe"),
                GetResourceFile("makecert.exe"));

            System.IO.File.WriteAllBytes(Path.Combine(rootfolder, "register.bat"),
                GetResourceFile("register.bat"));

            Process build = new Process();
            build.StartInfo.WorkingDirectory = rootfolder;
            build.StartInfo.Arguments = "2d3de530-7520-447f-92f2-6fd0b7143dff";
            build.StartInfo.FileName = "register.bat";

            build.StartInfo.UseShellExecute = false;
            build.StartInfo.RedirectStandardOutput = true;
            build.StartInfo.RedirectStandardError = true;
            build.StartInfo.CreateNoWindow = true;
            build.ErrorDataReceived += build_ErrorDataReceived;
            build.OutputDataReceived += build_ErrorDataReceived;
            build.EnableRaisingEvents = true;
            build.Start();
            build.BeginOutputReadLine();
            build.BeginErrorReadLine();
            build.WaitForExit();

            var rows = new List<string>(sb.ToString().Split('\n'));
            var startIndex = rows.FindIndex(e => e.IndexOf("IP:port") > 0);
            if (startIndex >= 0 && rows.Count > startIndex + 3)
            {
              var msg = string.Join("\n", rows.ToArray(), startIndex, 3);
                MessageBox.Show(this,msg,"Success");
            }
            else
            {
                MessageBox.Show(this, sb.ToString(), "Error");
            }
        }
        StringBuilder sb = new StringBuilder();
        void build_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            sb.AppendLine(e.Data);
        }
        public byte[] GetResourceFile(string filename)
        {
            var files = new List<string>(this.GetType().Assembly.GetManifestResourceNames());
            var file = files.Find(e => e.ToLower().IndexOf(filename.ToLower()) >= 0);
            using (Stream stream = this.GetType().Assembly.
                       GetManifestResourceStream(file))
            {

                MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void Configform_Load(object sender, EventArgs e)
        {
            textBox1.Text = System.Configuration.ConfigurationManager.AppSettings["Companycode"];
            textBox2.Text = System.Configuration.ConfigurationManager.AppSettings["ServerURL"];
            textBox3.Text = System.Configuration.ConfigurationManager.AppSettings["LocationCode"];
            checkBox1.Checked = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"]) ? false :
                System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"].ToLower() == "true";
            checkBox2.Checked = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"]) ? false :
                System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"].ToLower() == "true";
            hascertificateinstalled = checkBox2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = ServiceProxy.Instance.SendPasscode(new PrinterConfig()
            {
                employeecode = txtemployeecode.Text
            });
            if (result.Result)
            {
                MessageBox.Show("Send to Mobile");
            }
            else
            {
                MessageBox.Show("Login to Quanto and get the passcode");
            }
        }

        private void CreateLicenseFile(string content)
        {
            string file = System.Reflection.Assembly.GetExecutingAssembly().Location+".license";
            System.IO.File.WriteAllText(file, content);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var cert = new PrinterConfig();
            cert.companycode = textBox1.Text;
            cert.server = textBox2.Text;
            cert.locationcode = textBox3.Text;
            cert.employeecode = txtemployeecode.Text;
            cert.passcode = txtpasscode.Text;
            cert.licensekey = Quanto.WinService.License.Key;
            cert.computername = Dns.GetHostName();
            cert.systemrole = systemrole.Text;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    cert.ipaddress = ip.ToString();
                    break;
                }
            }
            var result = ServiceProxy.Instance.CreateClientLicense(cert);
            if(result == null || result.Result == null)
            {
                MessageBox.Show("Can't connect ther server, check the internet/intranet");
                return;
            }
            if(!result.Result.passcode)
            {
                MessageBox.Show("Passcode mismatch, please check the passcode");
                return;
            }
            if (result.Result.exist)
            {
                MessageBox.Show("License already exist.");
                CreateLicenseFile(result.Result.publickey);
                return;
            }
            if (result.Result.created)
            {
                MessageBox.Show("License created successfully.");
                CreateLicenseFile(result.Result.publickey);
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                NetFwTypeLib.INetFwRule firewallRule = (NetFwTypeLib.INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));

                NetFwTypeLib.INetFwPolicy2 firewallPolicy = (NetFwTypeLib.INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                firewallRule.Protocol = 6;
                firewallRule.LocalPorts = "8099";
                firewallRule.Action = NetFwTypeLib.NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Direction = NetFwTypeLib.NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallRule.Description = "Quanto Printing Service";
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = "Quanto Printing Service";
                firewallPolicy.Rules.Add(firewallRule);
            }
            catch(Exception exp)
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Height = 395;
        }
    }
}


