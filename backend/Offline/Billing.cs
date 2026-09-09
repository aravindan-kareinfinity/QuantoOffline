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
    public partial class Billing : Form
    {
        public Billing()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtBarcode.Text)) return;
            var stock = InfyPOS.Processors.BillManager.Instance.FindStock(txtBarcode.Text);
            long companyid = stock != null ? stock.companyid : 0;
            decimal qty;
            decimal rate;
            decimal discount;
            int step = 1;
            if(!decimal.TryParse(txtQty.Text, out qty))
            {
                var steps = txtQty.Text.Split('*');
                decimal.TryParse(steps[0], out qty);
                if (!int.TryParse(steps[1], out step))
                    step = 1;
            }
            decimal.TryParse(txtRate.Text, out rate);
            decimal.TryParse(txtDiscount.Text, out discount);
            if (rate == 0 || qty == 0) return;
            
            InfyPOS.Processors.OfflineClient.Product product = cmbProduts.SelectedItem as InfyPOS.Processors.OfflineClient.Product;
            InfyPOS.Processors.OfflineClient.Tax tax = cmbTax.SelectedItem as InfyPOS.Processors.OfflineClient.Tax;
            decimal price = rate - discount;

            var tp = tax.FindTaxPercentage(price);
            var taxperitem = Math.Round(price / (100 + tp) * tp, 4);
            for (var i = 0; i < step; i++)
            {
                var billitem = new InfyPOS.Processors.OfflineClient.BillItems()
                {
                    barcode = txtBarcode.Text,
                    productid = product.id,
                    printingname = product.name,
                    taxid = tax.id,
                    taxpercentage = tp,
                    qty = qty,
                    price = price,
                    discount = discount,
                    receivable = price * qty,
                    taxperitem = taxperitem,
                    totaltax = Math.Round(taxperitem * qty, 4),
                    stockdiscount = discount,
                    companyid = companyid
                };
                CurrentBill.Billitems.Add(billitem);
                Add2List(billitem);
            }
            ShowTotal();

            txtQty.Text = "";
            txtRate.Text = "";
            txtDiscount.Text = "";
            txtBarcode.Text = "";
            txtfinalprice.Text = "";
            cmbProduts.SelectedItem = null;
            cmbTax.SelectedItem = null;
            txtBarcode.Focus();

            if (PromotionEnabled)
            {
                var schemediscount = CurrentBill.schemediscount;
                PromotionManager.Instance.Process(CurrentBill);
                if (schemediscount != CurrentBill.schemediscount)
                {
                    RefreshBillDisplay();
                }
            }
        }

        public decimal ParseNo(string text)
        {
            decimal no = 0;
            if (decimal.TryParse(text, out no))
                return no;
            return 0;
        }


        private void RefreshBillDisplay()
        {
            listView1.Items.Clear();
            foreach (var billitem in CurrentBill.Billitems)
            {
                Add2List(billitem);
            }
            ShowTotal();
        }

        private void OfflineBilling_Load(object sender, EventArgs e)
        {
            stringFormat = new StringFormat(StringFormatFlags.NoWrap);
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;

            ProgressPanel.Visible = false;
            txtBarcode.KeyPress += TxtBarcode_KeyPress;
            cmbProduts.KeyPress += CmbProduts_KeyPress;
            cmbTax.KeyPress += CmbTax_KeyPress;
            txtQty.KeyPress += TxtQty_KeyPress;
            txtRate.KeyPress += TxtRate_KeyPress;
            txtDiscount.KeyPress += TxtDiscount_KeyPress;
            if (InfyPOS.Processors.BillManager.Instance.Data == null)
            {
                ProgressPanel.Visible = true;
                LoadMasterData();
            }
            else
            {
                InitializeSystem();
                SetDate(DateTime.Now.Date);
                ProgressPanel.Visible = false;
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
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (Ready)
            {
                InitializeSystem();
            }

            SetDate(DateTime.Now.Date);
            ProgressPanel.Visible = false;
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Ready = InfyPOS.Processors.BillManager.Instance.Initialize(e, sender as System.ComponentModel.BackgroundWorker);
        }

        public bool Ready { get; set; }
        private void TxtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtDiscount.Text.EndsWith("%"))
                {
                    decimal rate;
                    decimal discount;
                    if (decimal.TryParse(txtRate.Text, out rate) &&
                        decimal.TryParse(txtDiscount.Text.Replace("%",""),out discount))
                    {
                        txtDiscount.Text = Math.Round(rate * discount / 100, 2).ToString("N2");
                    }
                }
                else
                {
                    button1_Click(sender, e);
                    txtBarcode.Focus();
                }
            }
        }

        private void TxtRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) txtDiscount.Focus();
        }

        private void TxtQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) txtRate.Focus();
        }

        private void CmbTax_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && cmbTax.SelectedItem != null) txtQty.Focus();
        }

        private void CmbProduts_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && cmbProduts.SelectedItem != null) cmbTax.Focus();
        }

        private void TxtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (txtBarcode.Text == "0")
                {
                    button2_Click(sender, e);
                }
                if (txtBarcode.Text == "C")
                {
                    CurrentBill.customername = "";
                    CurrentBill.customermobileno = "";
                    Customer customer = new Customer("");
                    if(customer.ShowDialog() == DialogResult.OK)
                    {
                        CurrentBill.customername = customer.customername;
                        CurrentBill.customermobileno = customer.customermobileno;
                        CurrentBill.creditbill = customer.customercredit;
                    }
                }
                else if (txtBarcode.Text.StartsWith("."))
                {
                    if (AddSMCode(txtBarcode.Text))
                        txtBarcode.Text = "";
                }
                else if (AddBarcode(txtBarcode.Text))
                {
                    txtBarcode.Text = "";
                }
                else
                {
                    if(InfyPOS.Processors.BillManager.Instance.Data.Mobile10digit &&
                        txtBarcode.Text.Length == 10)
                    {
                        var customerdata = BillManager.CustomerManager.Instance.Get(txtBarcode.Text);
                        if (customerdata == null)
                        {
                            Customer customer = new Customer(txtBarcode.Text);
                            if (customer.ShowDialog() == DialogResult.OK)
                            {
                                CurrentBill.customername = customer.customername;
                                CurrentBill.customermobileno = customer.customermobileno;
                            }
                        }
                        else
                        {
                            CurrentBill.customername = customerdata.name;
                            CurrentBill.customermobileno = customerdata.no;
                            cmbProduts.Focus();
                        }
                    }
                    else
                    {
                        cmbProduts.Focus();
                    }
                    
                }
            }
        }

        private bool AddSMCode(string text)
        {
            var smcode = text.Substring(1);
            if (InfyPOS.Processors.BillManager.Instance.Data.Employee.Exists(e => e.code == smcode))
            {
                InfyPOS.Processors.OfflineClient.BillItems bs = listView1.Items[0].Tag as InfyPOS.Processors.OfflineClient.BillItems;
                bs.salesmanid = InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(e => e.code == smcode).id;
                bs.salesmancode = smcode;
                listView1.Items[0].BackColor = Color.DimGray;
                return true;
            }
            return false;
        }

        public InfyPOS.Processors.OfflineClient.Bill CurrentBill { get; set; }
        public bool PromotionEnabled { get; set; }
        //M21858
        public bool AddBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) return false;
            txtRate.ReadOnly = false;
            var stock = InfyPOS.Processors.BillManager.Instance.FindStock(barcode);
            if (stock == null)
                return false;

            if (stock.product.iscut)
            {
                cmbProduts.SelectedItem = stock.product;
                cmbTax.SelectedItem = stock.tax;
                txtRate.Text = stock.rate.ToString();
                txtDiscount.Text = stock.discount.ToString();
                txtfinalprice.Text = "";
                txtQty.Focus();
                txtRate.ReadOnly = true;
                return false;
            }

            if (CurrentBill.Billitems.Exists(e => e.barcode == stock.barcode))
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if ((listView1.Items[i].Tag as InfyPOS.Processors.OfflineClient.BillItems).barcode == barcode)
                    {
                        listView1.Items.RemoveAt(i);
                        break;
                    }
                }
                var billitem = CurrentBill.Billitems.Find(e => e.barcode == stock.barcode);
                billitem.qty += 1;
                billitem.totaltax = Math.Round(billitem.taxperitem * billitem.qty, 4);
                billitem.discount = Math.Round(billitem.stockdiscount, 2);
                billitem.receivable = Math.Round(billitem.price * billitem.qty, 2);
                Add2List(billitem);
            }
            else
            {
                var tp = stock.tax.FindTaxPercentage(stock.rate);
                var tax = Math.Round(stock.rate / (100 + tp) * tp, 4);
                var billitem = new InfyPOS.Processors.OfflineClient.BillItems()
                {
                    barcode = stock.barcode,
                    hsncode = stock.hsncode,
                    productid = stock.productid,
                    printingname = stock.product.name,
                    sellingmode = stock.product.iscut ? 3 : 2,
                    taxid = stock.tax.id,
                    taxpercentage = tp,
                    qty = 1,
                    price = stock.rate,
                    discount = stock.discount,
                    stockdiscount = stock.discount,
                    receivable = stock.rate,
                    taxperitem = tax,
                    totaltax = tax,
                    companyid = stock.companyid,
                    mrp = stock.mrp
                };
                CurrentBill.Billitems.Add(billitem);
                Add2List(billitem);
            }
            ShowTotal();
            if (PromotionEnabled)
            {
                var schemediscount = CurrentBill.schemediscount;
                PromotionManager.Instance.Process(CurrentBill);
                if (schemediscount != CurrentBill.schemediscount)
                {
                    //CurrentBill.items.ForEach(e=>
                    //{
                    //    e.receivable = ((e.price - e.adiscount) * e.qty) + e.schemediscount;
                    //});
                    RefreshBillDisplay();
                }
            }

            return true;
        }

        public void Add2List(InfyPOS.Processors.OfflineClient.BillItems billitem)
        {
            if(listView1.Items.Count == 0)
            {
                txttotaldiscountpercentage.Text = "";
                txttotaldiscount.Text = "";
                discountpercentageapplied = false;
                discountvalueapplied = false;
                if (InfyPOS.Processors.BillManager.Instance.Data.Discount > 0)
                {
                    txttotaldiscountpercentage.Text = InfyPOS.Processors.BillManager.Instance.Data.Discount.ToString("N2");
                    discountpercentageapplied = true;
                    discountvalueapplied = false;
                    ShowTotal();
                }
            }
            var lvi = new ListViewItem(new string[] { billitem.barcode, billitem.printingname,billitem.qty.ToString("N2"),
                    billitem.salerate.ToString("N2"),billitem.rowdiscount.ToString("N2"),billitem.rate.ToString("N2"),
                (billitem.receivable-billitem.schemediscount).ToString("N2") });
            lvi.Tag = billitem;
            if (billitem.schemediscount > 0)
                lvi.BackColor = Color.Gray;
            listView1.Items.Insert(0, lvi);
        }

        public void CreateNewBill()
        {
            CurrentBill = new InfyPOS.Processors.OfflineClient.Bill() { Billitems = new List<InfyPOS.Processors.OfflineClient.BillItems>() };
            CurrentBill.billdate = billdate;
            listView1.Items.Clear();
            discountpercentageapplied = false;
            discountvalueapplied = false;
            //txttotaldiscountpercentage.Text = "";
            //txttotaldiscount.Text = "";
            //txttotalamount.Text = "";
            txtBarcode.Text = "";
            txtBarcode.Focus();

        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                InfyPOS.Processors.BillManager.Instance.Load(ofd.FileName);
                InitializeSystem();
            }
        }

        private void InitializeSystem()
        {
            cmbProduts.DataSource = InfyPOS.Processors.BillManager.Instance.Data.Products.OrderBy(x=>x.name).ToList();
            cmbTax.DataSource = InfyPOS.Processors.BillManager.Instance.Data.Tax;
            if (InfyPOS.Processors.BillManager.Instance.Data.Printer != null)
                selectPrinterToolStripMenuItem.Text = "Printer : " + InfyPOS.Processors.BillManager.Instance.Data.Printer.name;
            CreateNewBill();
        }

        DateTime billdate = DateTime.Now.Date;
        private void SetDate(DateTime date)
        {
            dateToolStripMenuItem.Text = "Bill Date : " + date.ToString("dd-MM-yyyy");
            billdate = date.Date;
            if (Quanto.MachineConfig.IsClient)
            {
                try
                {
                    InfyPOS.Processors.BillManager.Instance.LoadClientBillsFromMaster(billdate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Billing",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            if (InfyPOS.Processors.BillManager.Instance.Bills == null)
                InfyPOS.Processors.BillManager.Instance.Bills = new List<OfflineClient.Bill>();
            if (InfyPOS.Processors.BillManager.Instance.Bills.Exists(e => e.billdate.Date == billdate.Date))
            {
                UpdateBillScroller(null);
            }
            else
            {
                billScroller.Minimum = 0;
                billScroller.Maximum = 0;
                billScroller.Value = 0;
                lblLastBillNo.Text = "";
                lblAvailable.Text = "Available Bills - 0";
            }
        }
        private void dateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quanto.Offline.CurrentDate currentDate = new Quanto.Offline.CurrentDate();
            if (currentDate.ShowDialog() == DialogResult.OK)
            {
                SetDate(currentDate.Date);
            }
        }

        StringFormat stringFormat = null;

        private void cmbProduts_DrawItem(object sender, DrawItemEventArgs e)
        {

            e.DrawBackground();
            Brush myBrush = Brushes.Black;
            Font ft = (sender as ComboBox).Font;
            e.Graphics.DrawString((sender as ComboBox).Items[e.Index].ToString(), ft, myBrush, e.Bounds, stringFormat);
            e.DrawFocusRectangle();
        }



        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            InfyPOS.Processors.OfflineClient.BillItems bitem = item.Tag as InfyPOS.Processors.OfflineClient.BillItems;
            txtBarcode.Text = bitem.barcode;
            cmbProduts.SelectedItem = InfyPOS.Processors.BillManager.Instance.Data.Products.Find(ex => ex.id == bitem.productid);
            cmbTax.SelectedItem = InfyPOS.Processors.BillManager.Instance.Data.Tax.Find(ex => ex.id == bitem.taxid);
            txtQty.Text = bitem.qty.ToString();
            txtRate.Text = bitem.salerate.ToString();
            txtDiscount.Text = bitem.discount.ToString();
            txtfinalprice.Text = bitem.receivable.ToString();
            listView1.Items.RemoveAt(item.Index);
            CurrentBill.Billitems.Remove(bitem);
            CurrentBill.schemediscount -= bitem.schemediscount;
            if (PromotionEnabled)
            {
                var schemediscount = CurrentBill.schemediscount;
                PromotionManager.Instance.Process(CurrentBill);
                if (schemediscount != CurrentBill.schemediscount)
                {
                    RefreshBillDisplay();
                }
            }
            ShowTotal();
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            decimal qty;
            decimal rate;
            decimal disount;
            decimal.TryParse(txtQty.Text, out qty);
            decimal.TryParse(txtRate.Text, out rate);
            decimal.TryParse(txtDiscount.Text, out disount);
            txtfinalprice.Text = (qty * (rate - disount)).ToString("N2");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                CurrentBill.Billitems.Remove(listView1.SelectedItems[0].Tag as InfyPOS.Processors.OfflineClient.BillItems);
                listView1.Items.RemoveAt(listView1.SelectedItems[0].Index);
                ShowTotal();
            }
        }

        private void ShowTotal()
        {
            var totalamount = CurrentBill.Billitems.Sum(e => e.receivable);
            var totalqty = CurrentBill.Billitems.Sum(e => e.sellingmode == 3 ? 1 : e.qty);
            var discount = CurrentBill.Billitems.Sum(e => (e.stockdiscount*e.qty));
            txtotalqty.Text = totalqty.ToString("N2");
            txttotalitemdiscount.Text = discount.ToString("N2");
            if (discountpercentageapplied)
            {
                decimal dpercent = 0;
                decimal dvalue = 0;
                if (decimal.TryParse(txttotaldiscountpercentage.Text, out dpercent))
                {
                    if(InfyPOS.Processors.BillManager.Instance.CurrentUser.alloweddiscount<dpercent)
                    {
                        MessageBox.Show("Your allowed percentage is " + InfyPOS.Processors.BillManager.Instance.CurrentUser.alloweddiscount.ToString("N2"));
                        txttotaldiscountpercentage.Text = "";
                        return;
                    }
                    dvalue = Math.Round((totalamount+ discount) * dpercent / 100, 2);
                    txttotaldiscount.Text = dvalue.ToString("N2");
                }
                else
                {
                    txttotaldiscount.Text = "";
                }
                txttotalamount.Text = (totalamount - (dvalue+CurrentBill.schemediscount)).ToString("N2");
            }
            else if (discountvalueapplied)
            {
                decimal dpercent = 0;
                decimal dvalue = 0;
                if (decimal.TryParse(txttotaldiscount.Text, out dvalue))
                {
                    dpercent = Math.Round(dvalue / (totalamount + discount) * 100, 2);
                    if (InfyPOS.Processors.BillManager.Instance.CurrentUser.alloweddiscount < dpercent)
                    {
                        MessageBox.Show("Your allowed percentage is " + InfyPOS.Processors.BillManager.Instance.CurrentUser.alloweddiscount.ToString("N2"));
                        txttotaldiscountpercentage.Text = "";
                        txttotaldiscount.Text = "";
                        return;
                    }
                    txttotaldiscountpercentage.Text = dpercent.ToString("N2");
                }
                else
                {
                    txttotaldiscountpercentage.Text = "";
                }
                txttotalamount.Text = (totalamount - (dvalue + CurrentBill.schemediscount)).ToString("N0");
            }
            else
            {
                txttotalamount.Text = (totalamount -  CurrentBill.schemediscount).ToString("N0");
            }
        }
        private void newBillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewBill();
        }

        bool discountpercentageapplied = false;
        bool discountvalueapplied = false;
        private void txtdiscountpercentage_TextChanged(object sender, EventArgs e)
        {
            if (txttotaldiscountpercentage.Focused)
            {
                discountpercentageapplied = true;
                discountvalueapplied = false;
                ShowTotal();
            }
        }

        private void txtgrossdiscount_TextChanged(object sender, EventArgs e)
        {
            if (txttotaldiscount.Focused)
            {
                discountpercentageapplied = false;
                discountvalueapplied = true;
                ShowTotal();
            }
        }

        private List<InfyPOS.Processors.OfflineClient.Bill> currentBillList = null;
        private void UpdateBillScroller(InfyPOS.Processors.OfflineClient.Bill bill)
        {
            if (bill == null)
            {
                currentBillList = InfyPOS.Processors.BillManager.Instance.Bills.FindAll(ex => ex.billdate.Date == billdate.Date);
                billScroller.Minimum = 1;
                billScroller.Maximum = currentBillList.Count;
                billScroller.Value = billScroller.Maximum;
                lblAvailable.Text = "Available Bills - " + billScroller.Maximum;
                lblLastBillNo.Text = currentBillList.Last().billno + "(" + billScroller.Maximum.ToString() + ")";
            }
            else
            {
                if (currentBillList == null)
                    currentBillList = InfyPOS.Processors.BillManager.Instance.Bills.FindAll(ex => ex.billdate.Date == billdate.Date);
                if(!currentBillList.Contains(bill))
                    currentBillList.Add(bill);
                billScroller.Maximum = currentBillList.Count;
                billScroller.Value = billScroller.Maximum;
                lblAvailable.Text = "Available Bills - " + billScroller.Maximum;
                lblLastBillNo.Text = bill.billno + "(" + billScroller.Maximum.ToString() + ")";
            }
        }

        private bool skipprinter = false;
        private void button2_Click(object sender, EventArgs e)
        {
            if (!SelectPrinter(false))
                return;

            CurrentBill.billdate = billdate;
            CurrentBill.addiscountaspercentage = discountpercentageapplied;
            CurrentBill.addiscountasvalue = discountvalueapplied;

            CurrentBill.createdon = DateTime.Now;
            CurrentBill.counterid = InfyPOS.Processors.BillManager.Instance.Data.counterid;
            CurrentBill.createdby = InfyPOS.Processors.BillManager.Instance.CurrentUser.id;


            if (CurrentBill.billattributes == null)
                CurrentBill.billattributes = new OfflineClient.Billattributes();

            if(InfyPOS.Processors.BillManager.Instance.Data.counterid >0)
                CurrentBill.billattributes.countername = InfyPOS.Processors.BillManager.Instance.Data.Counter.Find(ex => ex.id == InfyPOS.Processors.BillManager.Instance.Data.counterid).name;
            CurrentBill.billattributes.biller =
                InfyPOS.Processors.BillManager.Instance.CurrentUser.employeeid>0 &&
                InfyPOS.Processors.BillManager.Instance.Data.Employee.Exists(x => x.id == InfyPOS.Processors.BillManager.Instance.CurrentUser.employeeid) ? 
                InfyPOS.Processors.BillManager.Instance.Data.Employee.Find(x=>x.id == InfyPOS.Processors.BillManager.Instance.CurrentUser.employeeid).name :"";

            decimal d;
            if (decimal.TryParse(txttotaldiscountpercentage.Text, out d))
                CurrentBill.addiscountpercentage = d;
            if (decimal.TryParse(txttotaldiscount.Text, out d))
                CurrentBill.additionaldiscount = d;
            if (InfyPOS.Processors.BillManager.Instance.Data.employeeid == 0 ||
                string.IsNullOrEmpty(InfyPOS.Processors.BillManager.Instance.Data.BillPrefix))
            {
                Quanto.Offline.BIllSettings settings = new Quanto.Offline.BIllSettings();
                if (settings.ShowDialog() != DialogResult.OK)
                    return;
            }

            if(CurrentBill.Billitems.Exists(ex=>ex.companyid<=0))
            {
                foreach(var bitem in CurrentBill.Billitems.FindAll(ex => ex.companyid <= 0))
                {
                    if(InfyPOS.Processors.BillManager.Instance.Data.Products.Exists(ex=>ex.id == bitem.productid))
                    {
                        var product = InfyPOS.Processors.BillManager.Instance.Data.Products.Find(ex => ex.id == bitem.productid);
                        bitem.companyid = product.cmpid;
                    }

                    if (bitem.companyid <= 0)
                        bitem.companyid = InfyPOS.Processors.BillManager.Instance.Data.Company[0].id;
                }
            }

            var billlist = CurrentBill.CreateBills(InfyPOS.Processors.BillManager.Instance.Data.Tax);
            

            try
            {
            if (InfyPOS.Processors.BillManager.Instance.Data.Autosettlement || 
                InfyPOS.Processors.BillManager.Instance.Data.Location.autosettlement)
            {
                if (!Quanto.MachineConfig.IsClient)
                    InfyPOS.Processors.BillManager.Instance.UpdateBillNo(billlist);
                Settlement settlement = new Settlement();
                settlement.Initalize(true, billlist);
                if (settlement.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    billlist.Last().billattributes = new OfflineClient.Billattributes()
                    {
                        settlement = settlement.SettlementMaster
                    };
                    billlist = InfyPOS.Processors.BillManager.Instance.SaveBill(billlist);
                }
            }
            else
            {
                billlist = InfyPOS.Processors.BillManager.Instance.SaveBill(billlist);
            }
            
            foreach (var bill in billlist)
            {
                UpdateBillScroller(bill);
                PrintBill(bill);
            }
            CreateNewBill();
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message != null && exp.Message.IndexOf("Master", StringComparison.OrdinalIgnoreCase) >= 0
                        ? exp.Message
                        : ("Unable to save bill.\n\n" + exp.Message),
                    "Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PrintBill(OfflineClient.Bill currentBill)
        {
            var printbytes = InfyPOS.Processors.BillManager.Instance.PrintBill(currentBill);
            if (printbytes != null && InfyPOS.Processors.BillManager.Instance.Data.Printer != null)
            {
                try
                {
                    Quanto.Printer.PrinterService.Instance.DirectPrint(
                        InfyPOS.Processors.BillManager.Instance.Data.Printer, printbytes);
                }catch(Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
            }
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quanto.Offline.BIllSettings settings = new Quanto.Offline.BIllSettings();
            settings.ShowDialog();
        }

        private void billScroller_Scroll(object sender, ScrollEventArgs e)
        {
            var index = e.NewValue;
            if (currentBillList != null && currentBillList.Count >= index)
            {
                if (index == 0) index = 1;
                lblLastBillNo.Text = currentBillList[index - 1].billno + "(" + index.ToString() + ")";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentBillList != null)
            {
                PrintBill(currentBillList[billScroller.Value - 1]);
            }
        }

        private bool SelectPrinter(bool show)
        {
            if (show || (!skipprinter && InfyPOS.Processors.BillManager.Instance.Data.Printer == null))
            {
                Quanto.Offline.PrinterSelection printerSelection = new Quanto.Offline.PrinterSelection();
                if (printerSelection.ShowDialog() == DialogResult.OK)
                {
                    if (!printerSelection.SkipPrinter)
                    {
                        selectPrinterToolStripMenuItem.Text = "Printer : " + InfyPOS.Processors.BillManager.Instance.Data.Printer.name;
                        skipprinter = false;
                    }
                    else
                    {
                        skipprinter = true;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private void selectPrinterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectPrinter(true);
        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cbills = InfyPOS.Processors.BillManager.Instance.Bills.ToList();
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40""><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Quanto Report</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body>");
            sb.Append(@"<table border=""1""><thead><tr>" + string.Join("", new List<string>("Date,BillNo,Barcode,Product,Price,Discount,Qty,Amount".Split(',')).ConvertAll(ex => "<td>" + ex + "</td>")) + "</tr></thead>");
            sb.Append("<Tbody>");

            foreach (var bill in cbills)
            {
                foreach (var item in bill.Billitems)
                {
                    sb.Append("<tr>");
                    sb.AppendFormat("<td>{0}</td>", bill.billdate.ToString("dd-MM-yyyy"));
                    sb.AppendFormat("<td>{0}</td>", bill.billno);
                    sb.AppendFormat("<td>{0}</td>", item.barcode);
                    sb.AppendFormat("<td>{0}</td>", item.printingname);
                    sb.AppendFormat("<td>{0}</td>", item.salerate);
                    sb.AppendFormat("<td>{0}</td>", item.stockdiscount);
                    sb.AppendFormat("<td>{0}</td>", item.qty);
                    sb.AppendFormat("<td>{0}</td>", item.receivable);
                    sb.Append("</tr>");
                }
            }
            sb.Append("</Tbody></table></body></html>");
            SaveFileDialog ofd = new SaveFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(ofd.FileName, sb.ToString());
            }
            // <html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table border="1">{table}</table></body></html>
        }


        private void pROMOTIONDISABLEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromotionEnabled)
            {
                PromotionEnabled = false;
                pROMOTIONDISABLEDToolStripMenuItem.Text = "PROMOTION - DISABLED";
                pROMOTIONDISABLEDToolStripMenuItem.ForeColor = System.Drawing.Color.Crimson;
            }
            else
            {
                PromotionEnabled = true;
                pROMOTIONDISABLEDToolStripMenuItem.Text = "PROMOTION - ENABLED";
                pROMOTIONDISABLEDToolStripMenuItem.ForeColor = System.Drawing.Color.LimeGreen;
                var schemediscount = CurrentBill.schemediscount;
                PromotionManager.Instance.Process(CurrentBill);
                if (schemediscount != CurrentBill.schemediscount)
                {
                    RefreshBillDisplay();
                }
            }
        }

        private void txttotaldiscountpercentage_KeyUp(object sender, KeyEventArgs e)
        {
            //if(e.KeyCode == Keys.Enter)
            //{
            //    button2_Click(sender, e);
            //}
        }

        private void txtDiscount_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
