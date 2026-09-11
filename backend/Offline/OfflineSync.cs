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
    public partial class OfflineSync : Form
    {
        public OfflineSync()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(InfyPOS.Processors.BillManager.Instance.CurrentUser == null)
            {
                MessageBox.Show("Please login before sync");
                return;
            }
            button1.Enabled = false;
            InfyPOS.Processors.BillManager.Instance.Data.AutoSync = chkAutoStart.Checked;
            InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "";
            if (rdHour.Checked)
                InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Hour";
            if (rdMinute.Checked)
                InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Minute";
            if (rdDay.Checked)
                InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode = "Day";
            InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle = (int)nmEvery.Value;
            InfyPOS.Processors.BillManager.Instance.PersistMasters();
            OfflineBill.StartIfConfigured();
            
            if (chkBilling.Checked)
            {
                BillSync();
            }
            else if (chkSettlement.Checked)
            {
                SettlementSync();
            }
            else if (chkMaster.Checked)
            {
                MasterSync();
            }
            else
            {
                button1.Enabled = true;
                MessageBox.Show("Successfully Completed");
            }
        }

        private void Bgw_MasterDoWork(object sender, DoWorkEventArgs e)
        {
            Ready = InfyPOS.Processors.BillManager.Instance.Initialize(e, sender as System.ComponentModel.BackgroundWorker);
        }

        private void Bgw_Master_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Ready)
            {
                lblbillpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Bills.Count.ToString();
                lblsettlementpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Settlements.Count.ToString();
                lblmasterstatus.Text = "Sync: " + InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");

                chkAutoStart.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync;
                rdHour.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Hour";
                rdMinute.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Minute";
                rdDay.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Day";
                nmEvery.Value = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle;
            }
        }


        private void BillSync()
        {
            lblbillpending.Text = "Synchronizing...";
            System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += BillSync_DoWork;
            bgw.RunWorkerCompleted += BillSync_RunWorkerCompleted;
            bgw.RunWorkerAsync(chkautobarcode.Checked);
        }
        private void BillSync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                
                var sourceList = InfyPOS.Processors.BillManager.Instance.Bills.ToList();
                foreach(var bill in sourceList)
                {
                    if(bill.Billitems.Exists(x=>x.barcode.IndexOf(' ')>=0))
                    {
                        bill.Billitems.ForEach(x => x.barcode = x.barcode.Trim());
                    }
                }

                byte[] sourceBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(sourceList));

                var result = ServiceProxy.Instance.SyncOffline(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    datatype = "BILL",
                    datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                    data = InfyPOS.Processors.BufferedRealtimeCompressionEngine.Compress(sourceBytes),
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                    organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id,
                    autobarcode = e.Argument != null && (bool)e.Argument
                });
                var response = result.Result;
                while (!response.completed && !response.error)
                {
                    System.Threading.Thread.Sleep(5000);
                    var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                    if (statusresponse.completed || statusresponse.error)
                    {
                        e.Result = statusresponse;
                        break;
                    }
                }
            }
            catch (Exception exp)
            {
                e.Result = exp;
            }
        }

        private void BillSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                MessageBox.Show((e.Result as Exception).Message);
                button1.Enabled = true;
                chkautobarcode.Visible = true;
                lblbillpending.Text = "Error-Retry";
            }
            else
            {
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if (response.error)
                {
                    
                    MessageBox.Show(response.errormessage, "Bill Synchronization");
                    button1.Enabled = true;
                    chkautobarcode.Visible = true;
                    lblbillpending.Text = "Error-Retry";
                }
                else
                {
                    lblbillpending.Text = "Completed..";
                    InfyPOS.Processors.BillManager.Instance.PersistBill(true);
                    chkBilling.Checked = false;
                    chkBilling.Enabled = true;
                    lblbillpending.Text = "";
                    if (chkSettlement.Checked)
                    {
                        SettlementSync();
                    }
                    else if (chkMaster.Checked)
                    {
                        MasterSync();
                    }
                    else
                    {
                        button1.Enabled = true;
                        MessageBox.Show("Successfully Completed");
                    }
                }
            }
        }


        private void SettlementSync()
        {
            lblsettlementpending.Text = "Synchronizing..";
            System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += SettlementSync_DoWork;
            bgw.RunWorkerCompleted += SettlementSync_RunWorkerCompleted;
            bgw.RunWorkerAsync();
        }

        private void SettlementSync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var sourceList = InfyPOS.Processors.BillManager.Instance.Settlements.ToList();
                byte[] sourceBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(sourceList));

                var result = ServiceProxy.Instance.SyncOffline(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    datatype = "SETTLEMENT",
                    datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                    data = InfyPOS.Processors.BufferedRealtimeCompressionEngine.Compress(sourceBytes),
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                    organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                });
                var response = result.Result;
                while (!response.completed && !response.error)
                {
                    System.Threading.Thread.Sleep(5000);
                    var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                    if (statusresponse.completed || statusresponse.error)
                    {
                        e.Result = statusresponse;
                        break;
                    }
                }
            }
            catch (Exception exp)
            {
                e.Result = exp;
            }
        }

        private void SettlementSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                MessageBox.Show((e.Result as Exception).Message);
                button1.Enabled = true;
            }
            else
            {
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if (response.error)
                {
                    if(response.errormessage.Contains("Already settled"))
                    {
                        var settled = response.errormessage.Split(':')[1].Split(',');
                        InfyPOS.Processors.BillManager.Instance.PersistSettlement(settled.ToList());
                        response.errormessage = response.errormessage + " Try Now.";
                    }
                    MessageBox.Show(response.errormessage, "Settlement Synchronization");
                    button1.Enabled = true;
                }
                else
                {
                    chkSettlement.Checked = false;
                    chkSettlement.Enabled = false;
                    lblsettlementpending.Text = "Completed";
                    
                    InfyPOS.Processors.BillManager.Instance.PersistSettlement(true);
                    if (chkMaster.Checked)
                    {
                        MasterSync();
                    }
                    else
                    {
                        button1.Enabled = true;
                        MessageBox.Show("Successfully Completed");
                    }
                }
            }
        }
        private void MasterSync()
        {
            //StockSyncFromClient
            var datafolder = System.Configuration.ConfigurationManager.AppSettings["StockSyncFromClient"];
            if (!string.IsNullOrEmpty(datafolder) && datafolder.ToLower() == "true")
            {
                if (MessageBox.Show("Do you want to upload the stock barcode to the primary server?", "Upload Stock", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
                    bgw.DoWork += MasterReverseSync_DoWork;
                    bgw.RunWorkerCompleted += MasterReverseSync_RunWorkerCompleted;
                    bgw.RunWorkerAsync();
                }
                else
                {
                    System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
                    bgw.DoWork += MasterSync_DoWork;
                    bgw.RunWorkerCompleted += MasterSync_RunWorkerCompleted;
                    bgw.RunWorkerAsync();
                }
            }
            else
            {
                System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += MasterSync_DoWork;
                bgw.RunWorkerCompleted += MasterSync_RunWorkerCompleted;
                bgw.RunWorkerAsync();
            }
        }

        private void MasterReverseSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                MessageBox.Show((e.Result as Exception).Message);
                button1.Enabled = true;
            }
            else
            {
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if (response.error)
                {
                    MessageBox.Show(response.errormessage);
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = true;
                    MessageBox.Show("Successfully Completed");
                }
            }
        }
        private void MasterReverseSync_DoWork(object sender, DoWorkEventArgs ea)
        {
            try
            {

                var sourceList = InfyPOS.Processors.BillManager.Instance.Stocks;
                //var pr3 = InfyPOS.Processors.BillManager.Instance.Bills.FindAll(e => e.Billitems.Exists(x => x.barcode == "PR3"));

                if (!sourceList.Columns.Contains("productname"))
                {
                    sourceList.Columns.Add("productname", typeof(string));
                }
                foreach(DataRow dr in sourceList.Rows)
                {
                    var productid = Convert.ToInt64(dr["productid"]);
                    if(InfyPOS.Processors.BillManager.Instance.Data.Products.Exists(e=>e.id == productid))
                    {
                        dr["productname"] = InfyPOS.Processors.BillManager.Instance.Data.Products.Find(e => e.id == productid).name;
                    }
                }
                var clonedtable = sourceList.Copy();

                byte[] sourceBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(sourceList));

                var result = ServiceProxy.Instance.SyncOffline(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    datatype = "STOCK",
                    datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                    data = InfyPOS.Processors.BufferedRealtimeCompressionEngine.Compress(sourceBytes),
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                    organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                });
                var response = result.Result;
                while (!response.completed && !response.error)
                {
                    System.Threading.Thread.Sleep(5000);
                    var statusresponse = ServiceProxy.Instance.GetWindowsOfflineStatus(response.key).Result;
                    if (statusresponse.completed || statusresponse.error)
                    {
                        ea.Result = statusresponse;
                        break;
                    }
                }
            }
            catch (Exception exp)
            {
                ea.Result = exp;
            }
        }


        public bool Ready { get; set; }


        private void MasterSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                MessageBox.Show((e.Result as Exception).Message);
                button1.Enabled = true;
            }
            else
            {
                InfyPOS.Processors.OfflineClient.WindowsOfflineResponse response = e.Result as InfyPOS.Processors.OfflineClient.WindowsOfflineResponse;
                if (response.error)
                {
                    MessageBox.Show(response.errormessage);
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = true;
                    InfyPOS.Processors.BillManager.Instance.Load(response.data);
                    lblmasterstatus.Text = InfyPOS.Processors.BillManager.Instance.Data != null
                        ? "Sync: " + InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt")
                        : lblmasterstatus.Text;
                    MessageBox.Show("Successfully Completed");
                }
            }
        }

        private void MasterSync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = ServiceProxy.Instance.DownloadMasterFromConfiguredSource(new InfyPOS.Processors.OfflineClient.WindowsOfflineRequest()
                {
                    organizationid = InfyPOS.Processors.BillManager.Instance.Data.organizationid,
                    locationid = InfyPOS.Processors.BillManager.Instance.Data.locationid,
                    datatype = "MASTER",
                    systemkey = InfyPOS.Processors.BillManager.Instance.SystemKey(),
                    datafrom = InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn,
                    userid = InfyPOS.Processors.BillManager.Instance.CurrentUser.id
                });
            }
            catch (Exception exp)
            {
                e.Result = exp;
            }
        }

        private void OfflineSync_Load(object sender, EventArgs e)
        {
            if (InfyPOS.Processors.BillManager.Instance.Data == null)
            {
                System.ComponentModel.BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += Bgw_MasterDoWork;
                bgw.RunWorkerCompleted += Bgw_Master_RunWorkerCompleted;
                bgw.WorkerReportsProgress = true;
                bgw.RunWorkerAsync();
            }
            else
            {
                if (InfyPOS.Processors.BillManager.Instance.Settlements == null)
                    InfyPOS.Processors.BillManager.Instance.Settlements = new List<OfflineClient.Settlement>();
                if (InfyPOS.Processors.BillManager.Instance.Bills == null)
                    InfyPOS.Processors.BillManager.Instance.Bills = new List<OfflineClient.Bill>();
                lblbillpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Bills.Count.ToString();
                lblsettlementpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Settlements.Count.ToString();
                lblmasterstatus.Text = "Sync: " + InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");

                chkAutoStart.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync;
                rdHour.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Hour";
                rdMinute.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Minute";
                rdDay.Checked = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Mode == "Day";
                nmEvery.Value = InfyPOS.Processors.BillManager.Instance.Data.AutoSync_Cycle;
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            Quanto.Offline.Login login = new Login();
            if (login.ShowDialog() == DialogResult.OK)
            {
                
                
                var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
                if (OfflineDataStore.Instance.Exists(OfflineDataFile.Offline))
                {
                    var offlinedata = BufferedRealtimeCompressionEngine.Decompress(OfflineDataStore.Instance.ReadAllBytes(OfflineDataFile.Offline));
                    offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
                }

                InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
                deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
                deletedInfo.deletedon = DateTime.Now;
                deletedInfo.Bills = InfyPOS.Processors.BillManager.Instance.Bills;
                deletedInfo.Settlements = InfyPOS.Processors.BillManager.Instance.Settlements;
                offlinelist.Add(deletedInfo);

                var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
                OfflineDataStore.Instance.WriteAllBytes(OfflineDataFile.Offline, masterdata);

                InfyPOS.Processors.BillManager.Instance.PersistBill(true);
                InfyPOS.Processors.BillManager.Instance.PersistSettlement(true);

                lblbillpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Bills.Count.ToString();
                lblsettlementpending.Text = "Pending - " + InfyPOS.Processors.BillManager.Instance.Settlements.Count.ToString();
                lblmasterstatus.Text = "Sync: " + InfyPOS.Processors.BillManager.Instance.Data.lastSyncOn.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void lblbillpending_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OfflineSyncDetail offlineSyncDetail = new OfflineSyncDetail();
            offlineSyncDetail.ShowDialog();
        }
    }


}
