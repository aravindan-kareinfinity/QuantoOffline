using InfyPOS.Processors;
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
    public partial class OfflineSyncDetail : Form
    {
        public OfflineSyncDetail()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        bool isBill = false;
        bool isSettled = false;
        private void showbillrecords(object sender, EventArgs e)
        {
            var bils = InfyPOS.Processors.BillManager.Instance.Bills;
            if (isOlddata)
            {
                isOlddata = false;
                foreach (DataGridViewRow sr in dataGridView1.SelectedRows)
                {
                   var itm = sr.DataBoundItem as InfyPOS.Processors.OfflineClient.DeletedInfo;
                   bils.AddRange(itm.Bills);
                }
            }
            isBill = true;
            isSettled = false;
            dataGridView1.DataSource = null;
            
            dataGridView1.MultiSelect = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnCount = 8;
            dataGridView1.Columns[0].Name = "Bill Date";
            dataGridView1.Columns[0].DataPropertyName = "billdate";
            dataGridView1.Columns[1].Name = "Bill No";
            dataGridView1.Columns[1].DataPropertyName = "billno";
            dataGridView1.Columns[2].Name = "Total Piece";
            dataGridView1.Columns[2].DataPropertyName = "totalpiece";
            dataGridView1.Columns[3].Name = "Total Qty";
            dataGridView1.Columns[3].DataPropertyName = "totalqty";
            dataGridView1.Columns[4].Name = "Gross";
            dataGridView1.Columns[4].DataPropertyName = "gross";
            dataGridView1.Columns[5].Name = "Additional Discount";
            dataGridView1.Columns[5].DataPropertyName = "additionaldiscount";
            dataGridView1.Columns[6].Name = "Discount";
            dataGridView1.Columns[6].DataPropertyName = "totaldiscount";
            dataGridView1.Columns[7].Name = "Totl";
            dataGridView1.Columns[7].DataPropertyName = "total";
            dataGridView1.DataSource = bils;
        }

        private void showsettlement(object sender, EventArgs e)
        {
            var settlement = InfyPOS.Processors.BillManager.Instance.Settlements;
            if (isOlddata)
            {
                isOlddata = false;
                foreach (DataGridViewRow sr in dataGridView1.SelectedRows)
                {
                    var itm = sr.DataBoundItem as InfyPOS.Processors.OfflineClient.DeletedInfo;
                    settlement.AddRange(itm.Settlements);
                }
            }

            isBill = false;
            isOlddata = false;
            isSettled = true;
            
            dataGridView1.DataSource = null;
            dataGridView1.MultiSelect = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnCount = 9;
            dataGridView1.Columns[0].Name = "Settlement On";
            dataGridView1.Columns[0].DataPropertyName = "settlementon";
            dataGridView1.Columns[1].Name = "Code";
            dataGridView1.Columns[1].DataPropertyName = "code";
            dataGridView1.Columns[2].Name = "Bill Nos";
            dataGridView1.Columns[2].DataPropertyName = "billnos";
            dataGridView1.Columns[3].Name = "Amount";
            dataGridView1.Columns[3].DataPropertyName = "receivable";
            dataGridView1.Columns[4].Name = "Cash";
            dataGridView1.Columns[4].DataPropertyName = "cash";
            dataGridView1.Columns[5].Name = "Card";
            dataGridView1.Columns[5].DataPropertyName = "card";
            dataGridView1.Columns[6].Name = "credit";
            dataGridView1.Columns[6].DataPropertyName = "credit";
            dataGridView1.Columns[7].Name = "discount";
            dataGridView1.Columns[7].DataPropertyName = "discount";
            dataGridView1.Columns[8].Name = "other";
            dataGridView1.Columns[8].DataPropertyName = "other";
            dataGridView1.DataSource = settlement;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (InfyPOS.Processors.BillManager.Instance.CurrentUser == null)
            {
                Quanto.Offline.Login login = new Login();
                if (login.ShowDialog() != DialogResult.OK) return;
            }
            if (isSettled)
            {

                List<OfflineClient.Settlement> lst = new List<OfflineClient.Settlement>();
                foreach (DataGridViewRow sr in dataGridView1.SelectedRows)
                {
                    lst.Add(sr.DataBoundItem as OfflineClient.Settlement);
                }
                dataGridView1.DataSource = null;
                InfyPOS.Processors.BillManager.Instance.Settlements.RemoveAll(ex => lst.Exists(x => x.code == ex.code));


                var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
                if (System.IO.File.Exists(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")))
                {
                    var offlinedata = BufferedRealtimeCompressionEngine.Decompress(System.IO.File.ReadAllBytes(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")));
                    offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
                }

                InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
                deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
                deletedInfo.deletedon = DateTime.Now;
                deletedInfo.Bills = new List<OfflineClient.Bill>(); ;
                deletedInfo.Settlements = lst;
                offlinelist.Add(deletedInfo);

                var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(BillManager.datadirectory, "offline.data"), masterdata);
                InfyPOS.Processors.BillManager.Instance.PersistSettlement(false);
                showsettlement(sender, e);
            }
            else
            {
                List<OfflineClient.Bill> lst = new List<OfflineClient.Bill>();
                foreach (DataGridViewRow sr in dataGridView1.SelectedRows)
                {
                    lst.Add(sr.DataBoundItem as OfflineClient.Bill);
                }
                dataGridView1.DataSource = null;
                InfyPOS.Processors.BillManager.Instance.Bills.RemoveAll(ex => lst.Exists(x => x.billno == ex.billno));


                var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
                if (System.IO.File.Exists(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")))
                {
                    var offlinedata = BufferedRealtimeCompressionEngine.Decompress(System.IO.File.ReadAllBytes(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")));
                    offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
                }

                InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
                deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
                deletedInfo.deletedon = DateTime.Now;
                deletedInfo.Bills = lst;
                deletedInfo.Settlements = new List<OfflineClient.Settlement>();
                offlinelist.Add(deletedInfo);

                var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(BillManager.datadirectory, "offline.data"), masterdata);
                InfyPOS.Processors.BillManager.Instance.PersistBill(false);
                showbillrecords(sender, e);
            }
        }

        private void button4_Click_1(object sender, EventArgs ea)
        {
            List<OfflineClient.Stock> stocklist = new List<OfflineClient.Stock>();
            foreach (DataRow row in InfyPOS.Processors.BillManager.Instance.Stocks.Rows)
            {
                var stock = new OfflineClient.Stock();
                stock.barcode = row["barcode"] != DBNull.Value ? row["barcode"].ToString() : "";
                stock.serailno = row["serialno"] != DBNull.Value ? row["serialno"].ToString() : "";
                stock.hsncode = row["hsncode"] != DBNull.Value ? row["hsncode"].ToString() : "";
                stock.productid = row["productid"] != DBNull.Value ? Convert.ToInt64(row["productid"]) : 0;
                stock.companyid = row["companyid"] != DBNull.Value ? Convert.ToInt64(row["companyid"]) : 0;
                stock.price = row["price"] != DBNull.Value ? Convert.ToDecimal(row["price"]) : 0;
                stock.discount = row["discount"] != DBNull.Value ? Convert.ToDecimal(row["discount"]) : 0;
                if (InfyPOS.Processors.BillManager.Instance.Data.Products.Exists(e => e.id == stock.productid))
                {
                    stock.product = InfyPOS.Processors.BillManager.Instance.Data.Products.Find(e => e.id == stock.productid);
                    stock.productname = stock.product.name;
                    if (InfyPOS.Processors.BillManager.Instance.Data.Tax.Exists(e => e.id == stock.product.salestaxid))
                        stock.tax = InfyPOS.Processors.BillManager.Instance.Data.Tax.Find(e => e.id == stock.product.salestaxid);
                    else
                        stock.tax = InfyPOS.Processors.BillManager.Instance.Data.Tax.First();
                    stocklist.Add(stock);
                }
            }

            isBill = false;
            isOlddata = false;
            isSettled = false;
            dataGridView1.DataSource = null;
            dataGridView1.MultiSelect = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "barcode";
            dataGridView1.Columns[0].DataPropertyName = "barcode";
            dataGridView1.Columns[1].Name = "productname";
            dataGridView1.Columns[1].DataPropertyName = "productname";
            dataGridView1.Columns[2].Name = "price";
            dataGridView1.Columns[2].DataPropertyName = "price";
            dataGridView1.Columns[3].Name = "discount";
            dataGridView1.Columns[3].DataPropertyName = "discount";
            dataGridView1.DataSource = stocklist;
        }

        bool isOlddata = false;
        private void button5_Click(object sender, EventArgs e)
        {

            var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
            if (System.IO.File.Exists(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")))
            {
                var offlinedata = BufferedRealtimeCompressionEngine.Decompress(System.IO.File.ReadAllBytes(System.IO.Path.Combine(BillManager.datadirectory, "offline.data")));
                offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
            }

            isOlddata = true;
            isBill = false;
            isSettled = false;
            offlinelist = offlinelist.OrderBy(x => x.deletedon).ToList();
            offlinelist.ForEach(x =>
            {
                x.billcount = x.Bills.Count;
                x.settlementcount = x.Settlements.Count;
            });
            dataGridView1.DataSource = null;
            dataGridView1.MultiSelect = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "username";
            dataGridView1.Columns[0].DataPropertyName = "username";
            dataGridView1.Columns[1].Name = "deletedon";
            dataGridView1.Columns[1].DataPropertyName = "deletedon";
            dataGridView1.Columns[2].Name = "settlementcount";
            dataGridView1.Columns[2].DataPropertyName = "settlementcount";
            dataGridView1.Columns[3].Name = "billcount";
            dataGridView1.Columns[3].DataPropertyName = "billcount";
            dataGridView1.DataSource = offlinelist;
        }
    }


}
