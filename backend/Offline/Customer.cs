using System;
using InfyPOS.Processors;
using System.Windows.Forms;

namespace Quanto.Offline
{
    public partial class Customer : Form
    {
        string mobileno;
        string namePrefill;
        bool ignoreNextEnter;

        public Customer(string mobileno)
            : this(mobileno, "")
        {
        }

        public Customer(string mobileno, string name)
        {
            this.mobileno = mobileno;
            this.namePrefill = name;
            InitializeComponent();
            AcceptButton = null;
            CancelButton = button4;
            txtusername.TabIndex = 0;
            txtpassword.TabIndex = 1;
            chkcredit.TabIndex = 2;
            button1.TabIndex = 3;
            button4.TabIndex = 4;
        }

        public string customername
        {
            get { return txtusername.Text; }
        }

        public string customermobileno
        {
            get { return txtpassword.Text; }
        }

        public bool customercredit
        {
            get { return chkcredit.Checked; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtpassword.Text) || string.IsNullOrWhiteSpace(txtusername.Text))
                return;
            InfyPOS.Processors.BillManager.CustomerManager.Instance.Add(txtpassword.Text.Trim(), txtusername.Text.Trim());
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            txtpassword.Text = mobileno ?? "";
            txtusername.Text = namePrefill ?? "";
            ignoreNextEnter = true;
            TryFillNameFromMobile();
            if (string.IsNullOrWhiteSpace(txtusername.Text) && !string.IsNullOrWhiteSpace(txtpassword.Text))
                txtusername.Focus();
            else if (string.IsNullOrWhiteSpace(txtpassword.Text))
                txtpassword.Focus();
            else
                txtusername.Focus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            TryFillNameFromMobile();
            if (string.IsNullOrWhiteSpace(txtusername.Text))
            {
                txtpassword.Focus();
                return;
            }
            txtusername.Focus();
            txtusername.SelectAll();
        }

        private bool ConsumeEnter(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return false;
            if (ignoreNextEnter)
            {
                ignoreNextEnter = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }
            return false;
        }

        private void TryFillNameFromMobile()
        {
            var mobile = (txtpassword.Text ?? "").Trim();
            if (string.IsNullOrEmpty(mobile))
                return;
            var existing = BillManager.CustomerManager.Instance.Get(mobile);
            if (existing == null || string.IsNullOrWhiteSpace(existing.name))
                return;
            if (string.IsNullOrWhiteSpace(txtusername.Text) || txtusername.Text == namePrefill)
                txtusername.Text = existing.name;
        }

        private void txtpassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (ConsumeEnter(e))
                return;
            if (e.KeyCode != Keys.Enter)
            {
                TryFillNameFromMobile();
                return;
            }

            TryFillNameFromMobile();
            if (string.IsNullOrWhiteSpace(txtusername.Text))
            {
                txtusername.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtpassword.Text))
                return;
            button1_Click(sender, e);
        }

        private void txtusername_KeyUp(object sender, KeyEventArgs e)
        {
            if (ConsumeEnter(e))
                return;
            if (e.KeyCode != Keys.Enter)
                return;

            if (string.IsNullOrWhiteSpace(txtusername.Text))
                return;

            if (string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                txtpassword.Focus();
                return;
            }

            button1_Click(sender, e);
        }
    }
}
