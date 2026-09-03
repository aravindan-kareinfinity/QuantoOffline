using InfyPOS.Processors;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Windows.Forms;
using static Quanto.Offline.BIProxy;
using static Quanto.Offline.BISync;

namespace Quanto.Offline
{
    public partial class BISync : Form
    {
        public BISync()
        {
            InitializeComponent();
            this.Load += BISync_Load;
        }

        string BIClientCode = "";
        private void BISync_Load(object sender, EventArgs e)
        {
            BIClientCode = System.Configuration.ConfigurationManager.AppSettings["BIClientCode"];
        }


        public class DownloadList
        {
            public bool stock { get; set; }
            public bool sales { get; set; }
            public bool purchase { get; set; }
        }
        System.ComponentModel.BackgroundWorker bgw = null;
        private void button1_Click(object sender, EventArgs e)
        {
            btnstart.Enabled = false;
            bgw = new BackgroundWorker();
            bgw.DoWork += MasterSync_DoWork;
            bgw.RunWorkerCompleted += MasterSync_RunWorkerCompleted;
            bgw.ProgressChanged += Bgw_ProgressChanged;
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;

            bgw.RunWorkerAsync(new DownloadList()
            {
                purchase = chkpurchase.Checked,
                sales = chksales.Checked,
                stock = chkstock.Checked
            });

            //InfyPOS.Processors.BillManager.Instance.Data.AutoSync = chkAutoStart.Checked;
            //InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "";
            //if (rdHour.Checked)
            //    InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Hour";
            //if (rdMinute.Checked)
            //    InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Minute";
            //if (rdDay.Checked)
            //    InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Day";
            //InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle = (int)nmEvery.Value;

            //if (chkBilling.Checked)
            //{
            //    InfyPOS.Processors.BillManager.Instance.PersistMasters();
            //    BillSync();
            //}
            //else if (chkSettlement.Checked)
            //{
            //    InfyPOS.Processors.BillManager.Instance.PersistMasters();
            //    SettlementSync();
            //}
            //else if (chkMaster.Checked)
            //{
            //    MasterSync();
            //}
            //else
            //{
            //    button1.Enabled = true;
            //    MessageBox.Show("Successfully Completed");
            //}
        }

        public bool Ready { get; set; }


        private void MasterSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnstart.Enabled = true;
            if (e.Result is Exception)
            {
                MessageBox.Show((e.Result as Exception).Message);
            }
            else
            {
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if (response != null && response.error)
                {
                    MessageBox.Show(response.errormessage);
                }
                else
                {
                    MessageBox.Show("Successfully Completed");
                }
            }
        }

        private void MasterSync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DownloadList downloadList = e.Argument as DownloadList;
                var result = BIProxy.StartDownloadMasters(System.Configuration.ConfigurationManager.AppSettings["ServerURL"],
                    BIClientCode, Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(downloadList)))).Result;
                var key = result.key;
                while (!result.hasfailed && !result.hascompleted)
                {
                    System.Threading.Thread.Sleep(10000);
                    result = BIProxy.GetDownloadedMaster(System.Configuration.ConfigurationManager.AppSettings["ServerURL"],
                        BIClientCode, key).Result;
                }

                var bg = sender as System.ComponentModel.BackgroundWorker;
                bg.ReportProgress(50, result.result);
                System.Threading.Thread.Sleep(1000);
                if (result.hascompleted)
                {
                    int index = 0;
                    foreach (var datatable in result.result)
                    {
                        index++;
                        datatable.tablename = datatable.tablename.ToLower();
                        var table = datatable.GetTable();
                        datatable.availablerows = table.Rows.Count;
                        var progress = (100 / result.result.Count) * index;
                        bg.ReportProgress(progress, result.result);
                        System.Threading.Thread.Sleep(1000);
                        datatable.status = BIProxy.Sync_Masters(BIClientCode, table,
                            "bi_" + datatable.tablename,
                            datatable,
                            bg, progress, result.result);
                        datatable.completedrows = table.Rows.Count;
                        bg.ReportProgress(50 + (index * 10), result.result);
                        System.Threading.Thread.Sleep(1000);
                    }
                    var status = BIProxy.ChangeMasterStatus(System.Configuration.ConfigurationManager.AppSettings["ServerURL"],
                        BIClientCode, result.result);
                    bg.ReportProgress(100);
                    var response = new InfyPOS.Processors.OfflineClient.WindowsOfflineResponse();
                    response.status = "Success";
                    response.completed = true;
                    e.Result = response;
                }
            }
            catch (Exception exp)
            {
                e.Result = exp;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                List<BIProxy.DBData> datalist = e.UserState as List<BIProxy.DBData>;
                var stock = datalist.Find(x => x.tablename.ToLower().EndsWith("stock"));
                if (stock != null)
                {
                    chkstock.Text = "Stock - " + stock.completedrows + "/" + stock.availablerows;
                    if (stock.availablerows == 0)
                        processstock.Value = 100;
                    else if (stock.completedrows > 0)
                        processstock.Value = Convert.ToInt32((decimal)stock.completedrows / stock.availablerows * 100);
                }
                var purchase = datalist.Find(x => x.tablename.ToLower().EndsWith("purchase"));
                if (purchase != null)
                {
                    chkpurchase.Text = "Purchase - " + purchase.completedrows + "/" + purchase.availablerows;
                    if (purchase.availablerows == 0)
                        processpurchase.Value = 100;
                    else if (purchase.completedrows > 0)
                        processpurchase.Value = Convert.ToInt32((decimal)purchase.completedrows / purchase.availablerows * 100); ;
                }
                var sales = datalist.Find(x => x.tablename.ToLower().EndsWith("sales"));
                if (sales != null)
                {
                    chksales.Text = "Sales - " + sales.completedrows + "/" + sales.availablerows;
                    if (sales.availablerows == 0)
                        processsales.Value = 100;
                    else if (sales.completedrows > 0)
                        processsales.Value = Convert.ToInt32((decimal)sales.completedrows / sales.availablerows * 100); ;
                }
            }
            if (e.ProgressPercentage >= 0 && e.ProgressPercentage <= 100)
                progressBar1.Value = e.ProgressPercentage;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            BIProxy.Create_Masters();
        }

        private void button1_Click_1(object sender, EventArgs evt)
        {
            var result = BIProxy.DownloadSchema(System.Configuration.ConfigurationManager.AppSettings["ServerURL"], BIClientCode).Result;
            if (result == null)
            {
                MessageBox.Show("Please check your client code is available in the Quanto Server, or check the internet");
                return;
            }
            var currenschema = CurrentSchema();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in result)
            {
                if (currenschema.Exists(x => x.name.ToLower() == item.name.ToLower()))
                {

                    System.Text.StringBuilder sbtable = new System.Text.StringBuilder();
                    var destinationtable = currenschema.Find(x => x.name.ToLower() == item.name.ToLower());
                    foreach (var column in item.columns)
                    {
                        if (!destinationtable.columns.Exists(e => e.column_name == column.column_name))
                        {
                            if (sbtable.Length > 0)
                                sbtable.AppendLine(",");
                            if (column.character_maximum_length > 0)
                                sbtable.Append(string.Format(" ADD {1} {2}({3}) {4}", item.name, column.column_name, column.data_type,
                                    column.character_maximum_length, string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                            else if (column.numeric_precision > 0 &&
                                column.numeric_scale > 0)
                                sbtable.Append(string.Format(" ADD {1} {2}({3},{4}) {5}", item.name, column.column_name, column.data_type,
                                                                column.numeric_precision, column.numeric_scale, string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                            else
                                sbtable.Append(string.Format(" ADD {1} {2} {3}", item.name, column.column_name, column.data_type,
                                     string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                        }
                        else
                        {
                            var destinationcolumn = destinationtable.columns.Find(e => e.column_name == column.column_name);
                            if (column.data_type != destinationcolumn.data_type ||
                                column.character_maximum_length > destinationcolumn.character_maximum_length ||
                                column.numeric_precision > destinationcolumn.numeric_precision ||
                                column.numeric_scale > destinationcolumn.numeric_scale)
                            {

                                if (column.data_type == "bigint" &&
                                    destinationcolumn.data_type == "bigserial")
                                    continue;

                                if (destinationcolumn.data_type == "bigint" &&
                                    column.data_type == "bigserial")
                                    continue;

                                if (sbtable.Length > 0)
                                    sbtable.AppendLine(",");

                                if (column.character_maximum_length > 0)
                                    sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}({3})", item.name, column.column_name, column.data_type,
                                        column.character_maximum_length));
                                else if (column.numeric_precision > destinationcolumn.numeric_precision ||
                                column.numeric_scale > destinationcolumn.numeric_scale)
                                    sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}({3},{4})", item.name, column.column_name, column.data_type,
                                        column.numeric_precision, column.numeric_scale));
                                else
                                    sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}", item.name, column.column_name,
                                        column.data_type));
                            }

                            if (column.defaultvalue != destinationcolumn.defaultvalue &&
                                !string.IsNullOrEmpty(column.defaultvalue))
                            {
                                if (sbtable.Length > 0)
                                    sbtable.AppendLine(",");

                                sbtable.Append(string.Format(" ALTER COLUMN {1} SET DEFAULT {2}", item.name,
                                    column.column_name, column.defaultvalue));
                            }
                        }
                    }

                    if (sbtable.Length > 0)
                        stringBuilder.AppendLine(string.Format("ALTER TABLE {0} {1};", item.name, sbtable.ToString()));

                }
                else
                {
                    stringBuilder.AppendLine(item.CreateTableScript());
                }
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = new NpgsqlCommand(stringBuilder.ToString(), conn);
                command.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }
            MessageBox.Show("Successfully Completed");
        }

        public List<BIProxy.DBSchemaTable> CurrentSchema()
        {
            SortedDictionary<string, DBSchemaTable> tables = new SortedDictionary<string, DBSchemaTable>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string fieldsquery = @"select column_name, data_type, character_maximum_length,
                tables.table_name,columns.column_default,numeric_precision,numeric_scale from INFORMATION_SCHEMA.TABLES tables
                inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
                where tables.table_schema = 'public' and tables.table_type='BASE TABLE'
                order by tables.table_name";
                var command = new NpgsqlCommand(fieldsquery, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DBSchemaTable.Column clmn = new DBSchemaTable.Column();
                        clmn.column_name = reader.GetString(0);
                        clmn.data_type = reader.GetString(1);
                        clmn.character_maximum_length = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        if (!string.IsNullOrEmpty(reader.IsDBNull(4) ? "" : reader.GetString(4)) &&
                            reader.GetString(4).StartsWith("nextval"))
                            clmn.data_type = "bigserial";
                        string tablename = reader.GetString(3);
                        string columndefault = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        clmn.defaultvalue = columndefault.StartsWith("nextval(") ? "" : columndefault;
                        clmn.numeric_precision = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        clmn.numeric_scale = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        if (!tables.ContainsKey(tablename))
                            tables.Add(tablename, new DBSchemaTable()
                            {
                                columns = new List<DBSchemaTable.Column>(),
                                name = tablename
                            });

                        tables[tablename].columns.Add(clmn);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }

            List<DBSchemaTable> response = new List<DBSchemaTable>();
            response.AddRange(tables.Values);
            return response;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool result = BIProxy.ClearServerStatusAsync(BIClientCode, chksales.Checked,
                chkpurchase.Checked, chkstock.Checked).Result;
            if (result)
            {
                MessageBox.Show("Server transaction cache successfully cleared");
                return;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var result = BIProxy.DownloadSchema(System.Configuration.ConfigurationManager.AppSettings["ServerURL"], BIClientCode).Result;
            if (result == null)
            {
                MessageBox.Show("Please check your client code is available in the Quanto Server, or check the internet");
                return;
            }
            var currenschema = CurrentSchema();
            foreach (var item in result)
            {
                if (currenschema.Exists(x => x.name.ToLower() == item.name.ToLower()))
                {
                    NpgsqlConnection conn = CreateConnection();
                    try
                    {
                        conn.Open();
                        var command = new NpgsqlCommand("drop table "+item.name, conn);
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception exp)
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}
