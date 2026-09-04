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
    public partial class Customer : Form
    {
        string mobileno;
        public Customer(string mobileno)
        {
            this.mobileno = mobileno;
            InitializeComponent();
        }
        
        public string customername
        {
            get
            {
                return txtusername.Text;
            }
        }
        public string customermobileno
        {
            get
            {
                return txtpassword.Text;
            }
        }

        public bool customercredit
        {
            get
            {
                return chkcredit.Checked;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtpassword.Text) || string.IsNullOrEmpty(txtusername.Text)) return;
            InfyPOS.Processors.BillManager.CustomerManager.Instance.Add(txtpassword.Text, txtusername.Text);
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            txtusername.Text = mobileno;
            txtpassword.Focus();
        }

        private void txtpassword_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (string.IsNullOrEmpty(txtpassword.Text) || string.IsNullOrEmpty(txtusername.Text))
                {
                    txtusername.Focus();
                }
                else
                {
                    button1_Click(sender, e);
                }
            }
        }

        private void txtusername_KeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtpassword.Text) || string.IsNullOrEmpty(txtusername.Text))
            {
                txtpassword.Focus();
            }
            else
            {
                button1_Click(sender, e);
            }
        }
    }


}
