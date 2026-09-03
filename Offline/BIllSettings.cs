using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InfyPOS.Processors;

namespace Quanto.Offline
{
    public partial class BIllSettings : Form
    {
        public BIllSettings()
        {
            InitializeComponent();
        }

        bool issettlement;
        public BIllSettings(bool issettlement)
        {
            this.issettlement = issettlement;
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (cmbBiller.SelectedItem != null)
            {
                InfyPOS.Processors.BillManager.Instance.Data.employeeid = (cmbBiller.SelectedItem as InfyPOS.Processors.OfflineClient.Employee).id;
            }
            if (cmbCounter.SelectedItem != null)
            {
                InfyPOS.Processors.BillManager.Instance.Data.counterid = (cmbCounter.SelectedItem as InfyPOS.Processors.OfflineClient.Counter).id;
            }

            InfyPOS.Processors.BillManager.Instance.Data.BillPrefix = txtBillPrefix.Text;
            InfyPOS.Processors.BillManager.Instance.Data.SettlementPrefix = txtSettlementPrefix.Text;
            InfyPOS.Processors.BillManager.Instance.Data.CounterPrefix = txtCounterCode.Text;
            InfyPOS.Processors.BillManager.Instance.Data.AutoBarcode = chkAutoBarcode.Checked;
            InfyPOS.Processors.BillManager.Instance.Data.Autosettlement = chkSingleWindowSettlement.Checked;
            InfyPOS.Processors.BillManager.Instance.Data.Mobile10digit = chk10Digit.Checked;
            InfyPOS.Processors.BillManager.Instance.Data.Discount = ParseNo(txtDiscount.Text);

            foreach (var t in companyPrefixList)
            {
                t.company.billprefix = t.Prefix.Text;
                t.company.billno = ParseNo(t.BillNo.Text);
                t.company.settlementno = ParseNo(t.SettlementNo.Text);
            }
            InfyPOS.Processors.BillManager.Instance.PersistMasters();
            this.DialogResult = DialogResult.OK;
        }

        public long ParseNo(string text)
        {
            long no = 0;
            if (long.TryParse(text, out no))
                return no;
            return 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        List<GridItem> companyPrefixList = new List<GridItem>();
        private void Settings_Load(object sender, EventArgs e)
        {
            //txtCompanyCode.Enabled = InfyPOS.Processors.BillManager.Instance.Data.MultiCompany;
            cmbCounter.DataSource = InfyPOS.Processors.BillManager.Instance.Data.Counter;
            cmbBiller.DataSource = InfyPOS.Processors.BillManager.Instance.Data.Employee;
            var autonumberlist = InfyPOS.Processors.BillManager.Instance.Data.AutoNumber.GroupBy(ex => new { ex.floor, ex.floorid }).ToList().
                ConvertAll(x => new InfyPOS.Processors.OfflineClient.AutoNumber()
                {
                    floor = x.Key.floor,
                    floorid = x.Key.floorid
                });

            int currenty = 40;
            foreach (var item in InfyPOS.Processors.BillManager.Instance.Data.Company)
            {
                GridItem gridItem = new GridItem() { company = item };
                System.Windows.Forms.Label label = new Label();
                label.Text = item.name;
                label.Size = new Size(200, 20);
                label.Location = new Point(lblcompany.Left, currenty);
                panel1.Controls.Add(label);
                label.Visible = true;
                System.Windows.Forms.TextBox txtbox = new TextBox();
                txtbox.Size = new Size(lblbillno.Left - lblprefix.Left, 20);
                txtbox.Location = new Point(lblprefix.Left, currenty);
                panel1.Controls.Add(txtbox);
                txtbox.Visible = true;
                txtbox.Tag = item;
                gridItem.Prefix = txtbox;
                txtbox = new TextBox();
                txtbox.Size = new Size(lblsetno.Left - lblbillno.Left, 20);
                txtbox.Location = new Point(lblbillno.Left, currenty);
                panel1.Controls.Add(txtbox);
                txtbox.Visible = true;
                txtbox.Enabled = false;
                gridItem.BillNo = txtbox;
                txtbox = new TextBox();
                txtbox.Size = new Size(this.panel1.Width - lblsetno.Left, 20);
                txtbox.Location = new Point(lblsetno.Left, currenty);
                panel1.Controls.Add(txtbox);
                txtbox.Visible = true;
                txtbox.Enabled = false;
                gridItem.SettlementNo = txtbox;
                currenty += 30;
                if (string.IsNullOrEmpty(item.billprefix))
                {
                    if (autonumberlist.Exists(ex => ex.companyid == item.id))
                    {
                        txtbox.Text = autonumberlist.Find(ex => ex.companyid == item.id).prefix;
                    }
                }
                else
                {
                    gridItem.Prefix.Text = item.billprefix;
                }
                gridItem.Prefix.Text = item.billprefix;
                gridItem.BillNo.Text = item.billno.ToString();
                gridItem.SettlementNo.Text = item.settlementno.ToString();
                if (item.billno > 0 || item.settlementno > 0)
                    NoReset.Checked = true;
                gridItem.BillNo.Enabled = NoReset.Checked;
                gridItem.SettlementNo.Enabled = NoReset.Checked;
                companyPrefixList.Add(gridItem);
            }

            if (InfyPOS.Processors.BillManager.Instance.Data.employeeid > 0)
            {
                cmbBiller.SelectedItem = InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(ex => ex.id == InfyPOS.Processors.BillManager.Instance.Data.employeeid);
            }
            if (InfyPOS.Processors.BillManager.Instance.Data.counterid > 0)
            {
                cmbCounter.SelectedItem = InfyPOS.Processors.BillManager.Instance.Data.Counter.Find(ex => ex.id == InfyPOS.Processors.BillManager.Instance.Data.counterid);
            }

            //txtLocationCode.Text = InfyPOS.Processors.BillManager.Instance.Data.LocationCode;
            txtBillPrefix.Text = InfyPOS.Processors.BillManager.Instance.Data.BillPrefix;
            txtSettlementPrefix.Text = InfyPOS.Processors.BillManager.Instance.Data.SettlementPrefix;
            txtCounterCode.Text = InfyPOS.Processors.BillManager.Instance.Data.CounterPrefix;
            chkAutoBarcode.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoBarcode;
            chkSingleWindowSettlement.Checked = InfyPOS.Processors.BillManager.Instance.Data.Autosettlement;
            chk10Digit.Checked = InfyPOS.Processors.BillManager.Instance.Data.Mobile10digit ;
            txtDiscount.Text = InfyPOS.Processors.BillManager.Instance.Data.Discount.ToString("N2"); 
        }

        public class GridItem
        {
            public TextBox Prefix { get; set; }
            public TextBox BillNo { get; set; }
            public TextBox SettlementNo { get; set; }
            public OfflineClient.Company company { get; internal set; }
        }
        private void cmbCounter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCounter.SelectedItem == null)
            {
                txtCounterCode.Text = (cmbBiller.SelectedItem as InfyPOS.Processors.OfflineClient.Counter).code;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            txtBillPrefix.Text = "B[CMP][CNT][DD][MM][YY]/[NO]";
        }

        private void label3_Click(object sender, EventArgs e)
        {
            txtSettlementPrefix.Text = "S[CMP][CNT][DD][MM][YY]/[NO]";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            foreach (var t in companyPrefixList)
            {
                t.BillNo.Text = "";
                t.SettlementNo.Text = "";
                t.BillNo.Enabled = NoReset.Checked;
                t.SettlementNo.Enabled = NoReset.Checked;
            }

        }
    }


}
