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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        public bool SkipPrinter { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if(InfyPOS.Processors.BillManager.Instance.Data == null ||
                InfyPOS.Processors.BillManager.Instance.Data.Users == null)
            {
                MessageBox.Show("Configuration Loading...Wait few minutes...");
                return;
            }

            if(!InfyPOS.Processors.BillManager.Instance.Data.Users.Exists(x=>x.username == txtusername.Text))
            {
                MessageBox.Show("Username not available");
                return;
            }

            var user = InfyPOS.Processors.BillManager.Instance.Data.Users.Find(x => x.username == txtusername.Text);

            if(WebAPI.Data.Cryptor.EncryptString(txtpassword.Text, WebAPI.Data.Cryptor.DefaultToken) != 
                user.password && txtpassword.Text != user.password)
            {
                MessageBox.Show("Password wrong,try again..");
                return;
            }
            InfyPOS.Processors.BillManager.Instance.CurrentUser = user;
            this.DialogResult = DialogResult.OK;
        }

        private void LoadMasterData()
        {
            System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += Bgw_DoWork;
            bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
            bgw.RunWorkerAsync();
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += Bgw_ProgressChanged;
        }


        private void Bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Text = "Loading configuration...";
        }

        private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = "Login";
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Ready = InfyPOS.Processors.BillManager.Instance.Initialize(e, sender as System.ComponentModel.BackgroundWorker);
        }

        public bool Ready { get; set; }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (InfyPOS.Processors.BillManager.Instance.Data == null)
            {
                LoadMasterData();
            }
        }

        private void txtpassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                button1_Click(sender, e);
                e.Handled = true;
            }
        }

        private void txtusername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                txtpassword.Focus();
                e.Handled = true;
            }
        }
    }


}
