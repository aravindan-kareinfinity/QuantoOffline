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
    public partial class Settlement : Form
    {
        public Settlement()
        {
            InitializeComponent();
        }

        bool isSingleSettlement;
        List<InfyPOS.Processors.OfflineClient.Bill> settlementbill;
        InfyPOS.Processors.OfflineClient.Settlement settlement;
        InfyPOS.Processors.OfflineClient.SettlementStatus status;
        public InfyPOS.Processors.OfflineClient.Settlement SettlementMaster
        {
            get
            {
                return settlement;
            }
        }
        public List<InfyPOS.Processors.OfflineClient.Bill> SettlementBills
        {
            get
            {
                return settlementbill;
            }
        }
        long employeeid { get; set; }
        long counterid { get; set; }
        public void Initalize(bool isSingleSettlement, List<InfyPOS.Processors.OfflineClient.Bill> bill)
        {
            this.isSingleSettlement = isSingleSettlement;
            this.settlementbill = bill;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            if (isSingleSettlement)
            {
                ProgressPanel.Visible = false;
                counterid = InfyPOS.Processors.BillManager.Instance.Data.counterid;
                employeeid = InfyPOS.Processors.BillManager.Instance.Data.employeeid;
                lblCounter.Text = InfyPOS.Processors.BillManager.Instance.Data.Counter.Find(ex => ex.id == counterid).name;
                lblCollectionby.Text = InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(ex => ex.id == employeeid).name;
                settlement = new InfyPOS.Processors.OfflineClient.Settlement();
                foreach (var bill in settlementbill)
                {
                    settlement.Billsettlement.Add(new InfyPOS.Processors.OfflineClient.Settlement.SettlementItem()
                    {
                        billno = bill.billno,
                        receivable = bill.total
                    });
                    settlement.receivable += bill.total;
                    settlementDate.Value = bill.billdate.Date;

                }
                listView1.Items.Clear();
                foreach (var item in settlement.Billsettlement)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { item.billno, item.receivable.ToString() }));
                }
                settlementDate.Enabled = false;
                status = InfyPOS.Processors.BillManager.Instance.GetSettlementStatus(settlementbill.First().billdate.Date);
                DisplayStatus(null);
                UpdateDisplay();
            }
            else
            {
                LoadMasterData();
            }
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
            if (e.ProgressPercentage == 0)
            {
                ProgressPanel.Visible = true;
            }
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (Ready)
            {
                counterid = InfyPOS.Processors.BillManager.Instance.Data.counterid;
                employeeid = InfyPOS.Processors.BillManager.Instance.Data.employeeid;
                if (counterid > 0)
                    lblCounter.Text = InfyPOS.Processors.BillManager.Instance.Data.Counter.Find(ex => ex.id == counterid).name;
                if (employeeid > 0)
                    lblCollectionby.Text = InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(ex => ex.id == employeeid).name;
                settlement = new InfyPOS.Processors.OfflineClient.Settlement();
                settlement.settlementon = DateTime.Now.Date;
                status = InfyPOS.Processors.BillManager.Instance.GetSettlementStatus(settlement.settlementon);
                DisplayStatus(null);
            }
            ProgressPanel.Visible = false;
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Ready = InfyPOS.Processors.BillManager.Instance.Initialize(e, sender as System.ComponentModel.BackgroundWorker);
        }

        public bool Ready { get; set; }
        public void DisplayStatus(InfyPOS.Processors.OfflineClient.Settlement settlement)
        {
            if (settlement != null)
            {
                status.cash_paid += settlement.paymentinfo.cash_paid;
                status.card_paid += settlement.paymentinfo.card_paid;
                status.discount_paid += settlement.paymentinfo.discount_paid;
                status.billcount += settlement.Billsettlement.Count;
            }
            stcash.Text = string.Format("Cash Received : {0}", status.cash_paid);
            stcard.Text = string.Format("Card Received : {0}", status.card_paid);
            stdiscount.Text = string.Format("Discount : {0}", status.discount_paid);
            sttotal.Text = string.Format("Total Collection : {0}", status.total);
            stbills.Text = string.Format("Bills : {0}", status.billcount);
        }

        public decimal ParseDecimal(string value)
        {
            decimal d;
            if (decimal.TryParse(value, out d))
                return d;
            return 0;
        }
        private void UpdateDisplay()
        {
            settlement.paymentinfo.cash_paid = ParseDecimal(txtCash.Text);
            settlement.paymentinfo.card_paid = ParseDecimal(txtCard.Text);
            settlement.paymentinfo.credit_paid = ParseDecimal(txtCredit.Text);
            settlement.paymentinfo.adjustment_paid = ParseDecimal(txtAdjustment.Text);
            settlement.paymentinfo.discount_paid = ParseDecimal(txtDiscount.Text);
            settlement.paymentinfo.cash_return = 0;
            if (settlement.balance >= 0)
            {
                lblrefund.Visible = false;
                lblrefundcaption.Visible = false;
                lblBalance.Text = settlement.balance.ToString();
            }
            else
            {
                lblrefund.Visible = true;
                lblrefundcaption.Visible = true;
                lblrefund.Text = Math.Abs(settlement.balance).ToString();
                if (settlement.paymentinfo.cash_paid > 0)
                {
                    settlement.paymentinfo.cash_return = Math.Abs(settlement.balance);
                }
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void txtBillNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtBillNo.Text == "" || txtBillNo.Text == "0")
                {
                    txtCard.Focus();
                    return;
                }
                var billparts = txtBillNo.Text.Split('#');
                if (billparts.Length >= 2)
                {
                    if (settlement.Billsettlement.Exists(ex => ex.billno == billparts[0]))
                    {
                        txtBillNo.Text = "";
                        return;
                    }
                    if (InfyPOS.Processors.BillManager.Instance.Settlements.Exists(x => x.Billsettlement.Exists(y => y.billno == billparts[0])))
                    {
                        if (MessageBox.Show("Already got the settlement, do you want to print again?", "Setteled", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            settlement = InfyPOS.Processors.BillManager.Instance.Settlements.Find(x => x.Billsettlement.Exists(y => y.billno == billparts[0]));
                            txtBillNo.Text = "";
                            listView1.Items.Clear();
                            foreach (var item in settlement.Billsettlement)
                            {
                                var lvi = listView1.Items.Add(new ListViewItem(new string[] { item.billno, item.receivable.ToString() }));
                                lvi.Tag = item;
                            }
                            txtCash.Text = settlement.paymentinfo.cash_paid.ToString();
                            txtCard.Text = settlement.paymentinfo.card_paid.ToString();
                            txtCredit.Text = settlement.paymentinfo.credit_paid.ToString();
                            txtAdjustment.Text = settlement.paymentinfo.adjustment_paid.ToString();
                            txtDiscount.Text = settlement.paymentinfo.discount_paid.ToString();
                            UpdateDisplay();
                        }

                        return;
                    }
                    long companyid = 0;
                    if (billparts.Length == 3)
                        companyid = (int)ParseDecimal(billparts[2]);

                    if (companyid <= 0)
                        companyid = InfyPOS.Processors.BillManager.Instance.Data.Company.First().id;

                    settlement.Billsettlement.Add(new InfyPOS.Processors.OfflineClient.Settlement.SettlementItem()
                    {
                        billno = billparts[0],
                        receivable = ParseDecimal(billparts[1]),
                        companyid = companyid
                    });
                    listView1.Items.Add(new ListViewItem(new string[] { billparts[0], billparts[1] }));
                    settlement.receivable = settlement.Billsettlement.Sum(ex => ex.receivable);
                    lblBillValue.Text = settlement.receivable.ToString("N2");
                    txtBillNo.Text = "";
                    UpdateDisplay();
                }
                else if (ParseDecimal(txtBillNo.Text) > 0)
                {
                    txtCash.Text = txtBillNo.Text;
                    txtBillNo.Text = "";
                    if (settlement.balance <= 0)
                    {
                        button1_Click_1(sender, e);
                    }
                }
            }
        }



        private void button1_Click_1(object sender, EventArgs e)
        {

            if (employeeid == 0 || counterid == 0)
            {
                BIllSettings bIllSettings = new BIllSettings(true);
                if (bIllSettings.ShowDialog() == DialogResult.Cancel) return;
            }

            if (settlement.paymentinfo.credit_paid > 0)
            {
                EmployeeSelection employeeSelection = new EmployeeSelection();
                employeeSelection.Text = "Credit Approved By";
                if (employeeSelection.ShowDialog() == DialogResult.Cancel)
                    return;

                settlement.paymentinfo.credit_approvedby = employeeSelection.employeeid;
                settlement.paymentinfo.credit_approvedbyname =
                    InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(x => x.id == employeeSelection.employeeid).name;
            }

            if (settlement.paymentinfo.discount_paid > 0)
            {
                EmployeeSelection employeeSelection = new EmployeeSelection();
                employeeSelection.Text = "Discount Approved By";
                if (employeeSelection.ShowDialog() == DialogResult.Cancel)
                    return;

                settlement.paymentinfo.discount_approvedby = employeeSelection.employeeid;
                settlement.paymentinfo.discount_approvedbyname =
                    InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(x => x.id == employeeSelection.employeeid).name;
            }


            settlement.settlementon = settlementDate.Value.Date;
            settlement.createdon = DateTime.Now;
            settlement.cashiername = lblCollectionby.Text;
            settlement.countername = lblCounter.Text;
            settlement.counterid = InfyPOS.Processors.BillManager.Instance.Data.counterid;
            settlement.createdby = InfyPOS.Processors.BillManager.Instance.CurrentUser.id;

            if (settlement.Billsettlement.Exists(ex => ex.companyid == 0))
            {
                settlement.Billsettlement.FindAll(ex => ex.companyid == 0).ForEach(x =>
                {
                    x.companyid = InfyPOS.Processors.BillManager.Instance.Data.Company.First().id;
                });
            }

            var cash_paid = settlement.paymentinfo.cash_paid;
            var card_paid = settlement.paymentinfo.card_paid;
            var adjustment_paid = settlement.paymentinfo.adjustment_paid;
            var credit_paid = settlement.paymentinfo.credit_paid;
            var discount_paid = settlement.paymentinfo.discount_paid;
            foreach (var sitem in settlement.Billsettlement.GroupBy(ex=>ex.companyid))
            {
                var tempsettlement = Newtonsoft.Json.JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.Settlement>(Newtonsoft.Json.JsonConvert.SerializeObject(settlement));
                tempsettlement.Billsettlement.Clear();
                tempsettlement.Billsettlement.AddRange(sitem);
                tempsettlement.companyid = sitem.Key;
                tempsettlement.paymentinfo.cash_return = 0;
                tempsettlement.paymentinfo.adjustment_paid = 0;
                tempsettlement.paymentinfo.credit_paid = 0;
                tempsettlement.paymentinfo.discount_paid = 0;

                if (settlement.paymentinfo.cash_paid>0 &&
                    settlement.paymentinfo.card_paid == 0)
                {
                    tempsettlement.paymentinfo.cash_paid = sitem.Sum(ex => ex.receivable)- tempsettlement.paymentinfo.extra_paid;
                }
                else if (settlement.paymentinfo.card_paid > 0 &&
                    settlement.paymentinfo.cash_paid == 0)
                {
                    tempsettlement.paymentinfo.card_paid = sitem.Sum(ex => ex.receivable) - tempsettlement.paymentinfo.extra_paid;
                }
                else
                {
                    if(sitem.Sum(ex => ex.receivable)<= cash_paid)
                    {
                        tempsettlement.paymentinfo.cash_paid = sitem.Sum(ex => ex.receivable);
                        cash_paid -= tempsettlement.paymentinfo.cash_paid;
                    }
                    else if (sitem.Sum(ex => ex.receivable) <= card_paid)
                    {
                        tempsettlement.paymentinfo.cash_paid = sitem.Sum(ex => ex.receivable);
                        card_paid -= tempsettlement.paymentinfo.card_paid;
                    }
                    else
                    {
                        var available = sitem.Sum(ex => ex.receivable);
                        if (cash_paid > 0)
                        {
                            tempsettlement.paymentinfo.cash_paid = cash_paid;
                            available -= cash_paid;
                            cash_paid = 0;
                        }
                        if (card_paid >= available)
                        {
                            tempsettlement.paymentinfo.card_paid = available;
                            card_paid -= available;
                        }
                        if(available>0 && credit_paid > 0)
                        {
                            tempsettlement.paymentinfo.credit_paid = available> credit_paid ?credit_paid: available;
                            available -= tempsettlement.paymentinfo.credit_paid;
                            credit_paid -= tempsettlement.paymentinfo.credit_paid;
                        }
                        if (available > 0 && adjustment_paid > 0)
                        {
                            tempsettlement.paymentinfo.adjustment_paid = available > adjustment_paid ? adjustment_paid : available;
                            available -= tempsettlement.paymentinfo.adjustment_paid;
                            adjustment_paid -= tempsettlement.paymentinfo.adjustment_paid;
                        }
                        if (available > 0 && discount_paid > 0)
                        {
                            tempsettlement.paymentinfo.discount_paid = available > discount_paid ? discount_paid : available;
                            available -= tempsettlement.paymentinfo.discount_paid;
                            discount_paid -= tempsettlement.paymentinfo.discount_paid;
                        }
                        if(available>0)
                        {
                            MessageBox.Show("Please enter the cash/card amount");
                            return;
                        }
                    }
                }
                InfyPOS.Processors.BillManager.Instance.SaveSettlement(tempsettlement);
                if (!isSingleSettlement)
                {
                    var printbytes = InfyPOS.Processors.BillManager.Instance.PrintSettlement(tempsettlement);
                    if (printbytes != null && InfyPOS.Processors.BillManager.Instance.Data.Printer != null)
                    {
                        try
                        {
                            Quanto.Printer.PrinterService.Instance.DirectPrint(
                                InfyPOS.Processors.BillManager.Instance.Data.Printer, printbytes);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                    }
                }
            }
            if (isSingleSettlement)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                DisplayStatus(settlement);
                settlement = new InfyPOS.Processors.OfflineClient.Settlement();
                txtBillNo.Text = "";
                lblBillValue.Text = "";
                txtCard.Text = "";
                txtCash.Text = "";
                txtCredit.Text = "";
                txtDiscount.Text = "";
                txtAdjustment.Text = "";
                listView1.Items.Clear();
                UpdateDisplay();
            }
        }

        private void lblCounter_DoubleClick(object sender, EventArgs e)
        {
            BIllSettings bIllSettings = new BIllSettings(true);
            if (bIllSettings.ShowDialog() == DialogResult.Cancel) return;
        }

        private void settlementDate_ValueChanged(object sender, EventArgs e)
        {
            status = InfyPOS.Processors.BillManager.Instance.GetSettlementStatus(settlementDate.Value.Date);
            DisplayStatus(null);
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            var cbills = InfyPOS.Processors.BillManager.Instance.Settlements.ToList();
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40""><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Quanto Report</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body>");
            sb.Append(@"<table border=""1""><thead><tr>" + string.Join("", new List<string>("Date,BillNo,Value,Cash,Card,Discount,Adjustment".Split(',')).ConvertAll(ex => "<td>" + ex + "</td>")) + "</tr></thead>");
            sb.Append("<Tbody>");

            foreach (var bill in cbills)
            {
                var settlements = bill.Billsettlement;
                if (settlements.Count > 1)
                {
                    var cash_paid = bill.paymentinfo.cash_paid - bill.paymentinfo.cash_return;
                    var card_paid = bill.paymentinfo.card_paid;
                    var discount_paid = bill.paymentinfo.discount_paid;
                    var adjustment_paid = bill.paymentinfo.adjustment_paid;
                    foreach (var item in settlements)
                    {
                        sb.Append("<tr>");
                        sb.AppendFormat("<td>{0}</td>", bill.settlementon.ToString("dd-MM-yyyy"));
                        sb.AppendFormat("<td>{0}</td>", item.billno);
                        sb.AppendFormat("<td>{0}</td>", item.receivable);
                        var receivable = item.receivable;
                        if (cash_paid > 0)
                        {
                            sb.AppendFormat("<td>{0}</td>", cash_paid >= receivable ? receivable : cash_paid);
                            if (cash_paid >= receivable)
                            {
                                cash_paid -= receivable;
                                receivable = 0;
                            }
                            else
                            {
                                receivable -= cash_paid;
                                cash_paid = 0;
                            }
                        }
                        else
                        {
                            sb.AppendFormat("<td>{0}</td>", 0);
                        }
                        if (card_paid > 0 && receivable > 0)
                        {
                            sb.AppendFormat("<td>{0}</td>", card_paid >= receivable ? receivable : card_paid);
                            if (cash_paid >= receivable)
                            {
                                cash_paid -= receivable;
                                receivable = 0;
                            }
                            else
                            {
                                receivable -= cash_paid;
                                cash_paid = 0;
                            }
                        }
                        else
                        {
                            sb.AppendFormat("<td>{0}</td>", 0);
                        }
                        if (discount_paid > 0 && receivable > 0)
                        {
                            sb.AppendFormat("<td>{0}</td>", discount_paid >= receivable ? receivable : discount_paid);
                            if (discount_paid >= receivable)
                            {
                                discount_paid -= receivable;
                                receivable = 0;
                            }
                            else
                            {
                                receivable -= discount_paid;
                                discount_paid = 0;
                            }
                        }
                        else
                        {
                            sb.AppendFormat("<td>{0}</td>", 0);
                        }
                        if (adjustment_paid > 0 && receivable > 0)
                        {
                            sb.AppendFormat("<td>{0}</td>", adjustment_paid >= receivable ? receivable : adjustment_paid);
                            if (adjustment_paid >= receivable)
                            {
                                adjustment_paid -= receivable;
                                receivable = 0;
                            }
                            else
                            {
                                receivable -= adjustment_paid;
                                discount_paid = 0;
                            }
                        }
                        else
                        {
                            sb.AppendFormat("<td>{0}</td>", 0);
                        }
                        sb.Append("</tr>");
                    }
                }
                foreach (var item in settlements)
                {
                    sb.Append("<tr>");
                    sb.AppendFormat("<td>{0}</td>", bill.settlementon.ToString("dd-MM-yyyy"));
                    sb.AppendFormat("<td>{0}</td>", item.billno);
                    sb.AppendFormat("<td>{0}</td>", item.receivable);
                    sb.AppendFormat("<td>{0}</td>", bill.paymentinfo.cash_paid - bill.paymentinfo.cash_return);
                    sb.AppendFormat("<td>{0}</td>", bill.paymentinfo.card_paid);
                    sb.AppendFormat("<td>{0}</td>", bill.paymentinfo.discount_paid);
                    sb.AppendFormat("<td>{0}</td>", bill.paymentinfo.adjustment_paid);
                    sb.Append("</tr>");
                }
            }
            sb.Append("</Tbody></table></body></html>");
            SaveFileDialog ofd = new SaveFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(ofd.FileName, sb.ToString());
            }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void txtBillNo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                if (ParseDecimal(txtBillNo.Text) > 0)
                {
                    txtCash.Text = txtBillNo.Text;
                    UpdateDisplay();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                settlement.Billsettlement.Remove(listView1.SelectedItems[0].Tag as InfyPOS.Processors.OfflineClient.Settlement.SettlementItem);
                listView1.Items.RemoveAt(listView1.SelectedItems[0].Index);
                UpdateDisplay();
            }
        }

        private void txtCard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                e.Handled = true;
                if (settlement.balance <= 0)
                {
                    button1_Click_1(sender, e);
                }
                else
                {
                    txtDiscount.Focus();
                }
            }
        }

        private void txtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (settlement.balance <= 0)
                {
                    button1_Click_1(sender, e);
                }
                else
                {
                    txtAdjustment.Focus();
                }
            }
        }

        private void txtAdjustment_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (settlement.balance <= 0)
                {
                    button1_Click_1(sender, e);
                }
                else
                {
                    txtCredit.Focus();
                }
            }
        }

        private void txtCredit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (settlement.balance <= 0)
                {
                    button1_Click_1(sender, e);
                }
                else
                {
                    txtBillNo.Focus();
                }
            }
        }
    }

}
