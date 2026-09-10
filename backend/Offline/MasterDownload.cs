using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quanto.Offline
{
    public partial class MasterDownload : Form
    {
        public MasterDownload()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (!Quanto.MachineConfig.IsMaster)
            {
                MessageBox.Show(
                    "Connect Server (cloud sync) is only available on the MASTER computer.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (txtServer.Text != System.Configuration.ConfigurationManager.AppSettings["ServerURL"])
            {
                Quanto.Configform.AddOrUpdateAppSettings("ServerURL", txtServer.Text);
            }
            Quanto.OfflineSync.RememberCloudLogin(
                txtOrganizationCode.Text,
                txtLocationCode.Text,
                txtusername.Text,
                txtpassword.Text);
            LoadMasterData();
        }

        public class Info
        {
            public string organizationcode { get; set; }
            public string locationcode { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public bool zerostock { get;  set; }
            public DateTime zerostockfrom { get;  set; }
        }
        private void LoadMasterData()
        {
            this.Text = "Downloading master data...";
            button1.Enabled = false;
            button4.Enabled = false;
            System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += Bgw_DoWork;
            bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
            bgw.RunWorkerAsync(new Info()
            {
                organizationcode = txtOrganizationCode.Text,
                locationcode = txtLocationCode.Text,
                username = txtusername.Text,
                password=txtpassword.Text,
                zerostock = chkZeroStock.Checked,
                zerostockfrom = ZeroStockFrom.Value.Date
            });
        }

        
        private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button4.Enabled = true;
            if (e.Result is Exception)
            {
                this.Text = "Master Download";
                MessageBox.Show((e.Result as Exception).Message);
            }
            else
            {
                this.Text = "Master Download";
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if(response.error)
                {
                    MessageBox.Show(response.errormessage);
                }
                else
                {
                    MessageBox.Show("Master Data Successfully loaded");
                    this.Close();
                }
            }
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Info argument = e.Argument as Info;
            try
            {
                e.Result = ServiceProxy.Instance.DownloadMasterFromCloud(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    orgainzationcode = argument.organizationcode,
                    locationcode = argument.locationcode,
                    datatype = "master",
                    password = argument.password,
                    username = argument.username,
                    zerostock = argument.zerostock,
                    zerostockfrom =argument.zerostockfrom,
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser == null ? 0 : InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                });
            }catch(Exception exp)
            {
                e.Result = exp;
            }
        }

        public bool Ready { get; set; }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (!Quanto.MachineConfig.IsMaster)
            {
                MessageBox.Show(
                    "Connect Server (cloud sync) is only available on the MASTER computer.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            txtServer.Text = System.Configuration.ConfigurationManager.AppSettings["ServerURL"];
            label4.Text = "Cloud Server";
            txtOrganizationCode.Text = System.Configuration.ConfigurationManager.AppSettings["OrganizationCode"];
            txtLocationCode.Text = System.Configuration.ConfigurationManager.AppSettings["Locationcode"];
            txtusername.Text = System.Configuration.ConfigurationManager.AppSettings["CloudUsername"];
            txtpassword.Text = System.Configuration.ConfigurationManager.AppSettings["CloudPassword"];
            if (string.IsNullOrWhiteSpace(txtOrganizationCode.Text))
            {
                checkBox1.Checked = true;
                txtOrganizationCode.Enabled = true;
            }
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            txtOrganizationCode.Enabled = checkBox1.Checked;
        }
    }


}
