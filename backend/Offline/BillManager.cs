using Newtonsoft.Json;
using Quanto.Printer;
using ReportLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace InfyPOS.Processors
{
    public class BillManager
    {
        public static string datadirectory => OfflineDataStore.Instance.DataDirectory;

        private static OfflineDataStore Store => OfflineDataStore.Instance;
        private static BillManager manager = new BillManager();
        public static BillManager Instance
        {
            get
            {
                return manager;
            }
        }
        public List<OfflineClient.Settlement> Settlements { get; set; }
        public List<OfflineClient.Bill> Bills { get; set; }
        public BillSourceData Data { get; set; }
        public System.Data.DataTable Stocks { get; set; }
        public OfflineClient.User CurrentUser { get; set; }
        public void Load(string filename)
        {
            var edata = JsonConvert.DeserializeObject<OfflineClient.MasterData>(System.IO.File.ReadAllText(filename));
            Data = new BillSourceData(Data, edata);
            PersistMasters(edata);
        }
        public void Load(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;
            var edata = JsonConvert.DeserializeObject<OfflineClient.MasterData>(System.Text.ASCIIEncoding.ASCII.GetString(bytes));
            Data = new BillSourceData(Data, edata);
            PersistMasters(edata);
        }

        public void LoadFromClientPackage(OfflineClient.ClientMasterPackage package)
        {
            if (package == null || package.master == null || package.master.Length == 0)
                throw new InvalidOperationException("Master data is not available from the client.");

            Store.WriteAllBytes(OfflineDataFile.Master, package.master);
            var masterdata = BufferedRealtimeCompressionEngine.Decompress(package.master);
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<BillSourceData>(System.Text.ASCIIEncoding.ASCII.GetString(masterdata));

            if (package.stock != null && package.stock.Length > 0)
            {
                Store.WriteAllBytes(OfflineDataFile.Stock, package.stock);
                Stocks = Processors.DataTableCustomFormatter.Deserialize(package.stock, true);
                Stocks.CaseSensitive = true;
            }

            if (package.customer != null && package.customer.Length > 0)
            {
                Store.WriteAllBytes(OfflineDataFile.Customer, package.customer);
                CustomerManager.Instance.Reload();
            }

            if (Bills == null)
                Bills = new List<OfflineClient.Bill>();
            if (Settlements == null)
                Settlements = new List<OfflineClient.Settlement>();
            initialized = true;
        }
        public void PersistMasters()
        {

            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Data)));
            Store.WriteAllBytes(OfflineDataFile.Master, masterdata);
        }

        public string SystemKey()
        {

            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            string ipkey = "";
            foreach (ManagementObject mo in moc)
            {
                if (ipkey == "")
                {
                    foreach (var item in mo.Properties)
                    {
                        if (item.Name.ToLower() == "processorid" && item.Value != null)
                        {
                            ipkey = item.Value.ToString();
                            break;
                        }
                    }
                }
            }
            return ipkey;
        }
        public byte[] PrintBill(OfflineClient.Bill bill)
        {
            if (Data.printconfig.Count == 0 || !Data.printconfig.Exists(e => e.key == "SALERECEIPT")) return null;
            OfflineClient.PrintCofig print = null;
            if (Data.printconfig.Exists(e => e.key == "SALERECEIPT" && e.companyid == bill.companyid && e.locationid == bill.locationid))
            {
                print = Data.printconfig.Find(e => e.key == "SALERECEIPT" && e.companyid == bill.companyid && e.locationid == bill.locationid);
            }
            else if (Data.printconfig.Exists(e => e.key == "SALERECEIPT" && e.locationid == bill.locationid))
            {
                print = Data.printconfig.Find(e => e.key == "SALERECEIPT" && e.locationid == bill.locationid);
            }
            else if (Data.printconfig.Exists(e => e.key == "SALERECEIPT" && e.companyid == bill.companyid))
            {
                print = Data.printconfig.Find(e => e.key == "SALERECEIPT" && e.companyid == bill.companyid);
            }
            else
            {
                print = Data.printconfig.Find(e => e.key == "SALERECEIPT");
            }
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(0, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(0, print.format);
            ChangeBillBarcode(labreport.format);
            var additional = new SortedDictionary<string, object>();
            foreach (var item in Data.imageDatas)
            {
                if (!additional.ContainsKey(item.key))
                    additional.Add(item.key, item.data);
            }

            byte[] document = null;
            if (labreport.format.PageSetting.fixedbilltypes == null || labreport.format.PageSetting.fixedbilltypes.Length == 0)
            {
                document = labreport.format.RenderBytes(bill, bill.Billitems, additional, true);
                if (InfyPOS.Processors.BillManager.Instance.Data.noofcopy > 1)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                        bw.Write(document);
                        int c = 1;
                        while (c < InfyPOS.Processors.BillManager.Instance.Data.noofcopy)
                        {
                            bw.Write(document);
                            c++;
                        }
                        document = ms.ToArray();
                    }
                }
            }
            else
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                    foreach (var btype in labreport.format.PageSetting.fixedbilltypes)
                    {
                        bill.showfixeddisplay = true;
                        bill.fixedbilldisplay = btype;
                        var tdocument = labreport.format.RenderBytes(bill, bill.Billitems, additional, true);
                        bw.Write(tdocument);
                    }
                    document = ms.ToArray();
                }
            }
            return document;
        }

        private void ChangeBillBarcode(DosFormat format)
        {
            foreach (var item in format.PageFooter.Items)
            {
                foreach (var condent in item.Items)
                {
                    if (condent.Type == DosFormat.CotnentType.Barcode)
                    {
                        condent.Type = DosFormat.CotnentType.OnlyBarcode;
                        item.Items.Add(new DosFormat.Content()
                        {
                            Type = DosFormat.CotnentType.Field,
                            Align = "center",
                            Name = "billno",
                            Length = condent.Length,
                            Style = "large"
                        });
                        condent.Name = "billnoandamount";
                        return;
                    }
                }
            }
        }

        public byte[] PrintSettlement(OfflineClient.Settlement settlement)
        {
            if (Data.printconfig.Count == 0 || !Data.printconfig.Exists(e => e.key == "SETTLEMENT")) return null;
            OfflineClient.PrintCofig print = null;
            if (Data.printconfig.Exists(e => e.key == "SETTLEMENT" && e.companyid == settlement.companyid && e.locationid == settlement.locationid))
            {
                print = Data.printconfig.Find(e => e.key == "SETTLEMENT" && e.companyid == settlement.companyid && e.locationid == settlement.locationid);
            }
            else if (Data.printconfig.Exists(e => e.key == "SETTLEMENT" && e.locationid == settlement.locationid))
            {
                print = Data.printconfig.Find(e => e.key == "SETTLEMENT" && e.locationid == settlement.locationid);
            }
            else if (Data.printconfig.Exists(e => e.key == "SETTLEMENT" && e.companyid == settlement.companyid))
            {
                print = Data.printconfig.Find(e => e.key == "SETTLEMENT" && e.companyid == settlement.companyid);
            }
            else
            {
                print = Data.printconfig.Find(e => e.key == "SETTLEMENT");
            }
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(0, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(0, print.format);
            var additional = new SortedDictionary<string, object>();
            foreach (var item in Data.imageDatas)
            {
                if (!additional.ContainsKey(item.key))
                    additional.Add(item.key, item.data);
            }

            byte[] document = labreport.format.RenderBytes(settlement, settlement.Billsettlement, additional, true);
            if (InfyPOS.Processors.BillManager.Instance.Data.noofcopy > 1)
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                    bw.Write(document);
                    int c = 1;
                    while (c < InfyPOS.Processors.BillManager.Instance.Data.noofcopy)
                    {
                        bw.Write(document);
                        c++;
                    }
                    document = ms.ToArray();
                }
            }

            return document;
        }

        public void PersistBill(bool clearcurrent)
        {
            if (clearcurrent)
            {
                var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
                if (Store.Exists(OfflineDataFile.Offline))
                {
                    var offlinedata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Offline));
                    offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
                }

                InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
                deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
                deletedInfo.deletedon = DateTime.Now;
                deletedInfo.Bills = InfyPOS.Processors.BillManager.Instance.Bills;
                deletedInfo.Settlements = new List<OfflineClient.Settlement>();
                offlinelist.Add(deletedInfo);

                var offlinebytes = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
                Store.WriteAllBytes(OfflineDataFile.Offline, offlinebytes);
                NoSequenceManager.Instance.Update(Bills);
                Bills.Clear();
            }

            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Bills)));
            Store.WriteAllBytes(OfflineDataFile.Bill, masterdata);
        }

        public OfflineClient.SettlementStatus GetSettlementStatus(DateTime date)
        {
            var allRecords = Settlements != null ? Settlements.FindAll(e => e.settlementon == date) : new List<OfflineClient.Settlement>();
            return new OfflineClient.SettlementStatus()
            {
                adjustment_paid = allRecords.Sum(e => e.paymentinfo.adjustment_paid),
                card_paid = allRecords.Sum(e => e.paymentinfo.card_paid),
                cash_paid = allRecords.Sum(e => e.paymentinfo.cash_paid),
                cash_return = allRecords.Sum(e => e.paymentinfo.cash_return),
                credit_paid = allRecords.Sum(e => e.paymentinfo.credit_paid),
                discount_paid = allRecords.Sum(e => e.paymentinfo.discount_paid),
                billcount = allRecords.Sum(e => e.Billsettlement.Count)
            };
        }
        internal void PersistSettlement(List<string> settled)
        {
            var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
            if (Store.Exists(OfflineDataFile.Offline))
            {
                var offlinedata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Offline));
                offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
            }

            var settledsettlements = InfyPOS.Processors.BillManager.Instance.Settlements.FindAll(x => settled.Exists(y => y.Trim() == x.code.Trim()));

            InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
            deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
            deletedInfo.deletedon = DateTime.Now;
            deletedInfo.Bills = new List<OfflineClient.Bill>();
            deletedInfo.Settlements = settledsettlements;
            offlinelist.Add(deletedInfo);

            var offlinebytes = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
            Store.WriteAllBytes(OfflineDataFile.Offline, offlinebytes);
            NoSequenceManager.Instance.Update(Settlements);
            Settlements.RemoveAll(x => settled.Exists(y => y.Trim() == x.code.Trim()));
            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Settlements)));
            Store.WriteAllBytes(OfflineDataFile.Settlement, masterdata);
        }
        public void PersistSettlement(bool clearcurrent)
        {
            if (clearcurrent)
            {
                var offlinelist = new List<InfyPOS.Processors.OfflineClient.DeletedInfo>();
                if (Store.Exists(OfflineDataFile.Offline))
                {
                    var offlinedata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Offline));
                    offlinelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Processors.OfflineClient.DeletedInfo>>(System.Text.ASCIIEncoding.ASCII.GetString(offlinedata));
                }

                InfyPOS.Processors.OfflineClient.DeletedInfo deletedInfo = new InfyPOS.Processors.OfflineClient.DeletedInfo();
                deletedInfo.username = InfyPOS.Processors.BillManager.Instance.CurrentUser.username;
                deletedInfo.deletedon = DateTime.Now;
                deletedInfo.Bills = new List<OfflineClient.Bill>();
                deletedInfo.Settlements = InfyPOS.Processors.BillManager.Instance.Settlements;
                offlinelist.Add(deletedInfo);

                var offlinebytes = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(offlinelist)));
                Store.WriteAllBytes(OfflineDataFile.Offline, offlinebytes);


                NoSequenceManager.Instance.Update(Settlements);
                Settlements.Clear();
            }
            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Settlements)));
            Store.WriteAllBytes(OfflineDataFile.Settlement, masterdata);
        }
        public void PersistMasters(OfflineClient.MasterData source)
        {
            Stocks = Processors.DataTableCustomFormatter.Deserialize(source.Stock, true);
            //Stocks.PrimaryKey = new System.Data.DataColumn[] { Stocks.Columns["barcode"] };

            List<long> cmpcodes = new List<long>();
            long cmp1 = (long)Stocks.Rows[0]["companyid"];
            if (BillManager.Instance.Stocks.Select("companyid <> " + cmp1.ToString()).Length > 0)
            {
                this.Data.MultiCompany = true;
            }


            Store.WriteAllBytes(OfflineDataFile.Stock, source.Stock);
            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Data)));
            Store.WriteAllBytes(OfflineDataFile.Master, masterdata);
        }

        private bool initialized = false;
        public bool Initialize(System.ComponentModel.DoWorkEventArgs e, System.ComponentModel.BackgroundWorker bgw)
        {
            try
            {
                if (initialized) return true;



                if (Store.Exists(OfflineDataFile.Master))
                {
                    bgw.ReportProgress(5);
                    var masterdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Master));
                    Data = Newtonsoft.Json.JsonConvert.DeserializeObject<BillSourceData>(System.Text.ASCIIEncoding.ASCII.GetString(masterdata));
                    bgw.ReportProgress(25);
                    if (Store.Exists(OfflineDataFile.Stock))
                    {
                        bgw.ReportProgress(30);
                        Stocks = Processors.DataTableCustomFormatter.Deserialize(Store.ReadAllBytes(OfflineDataFile.Stock), true);
                        Stocks.CaseSensitive = true;
                        //Stocks.PrimaryKey = new System.Data.DataColumn[] { Stocks.Columns["barcode"] };
                        bgw.ReportProgress(50);
                        if (Store.Exists(OfflineDataFile.Bill))
                        {
                            bgw.ReportProgress(55);
                            var billdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Bill));
                            Bills = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Bill>>(System.Text.ASCIIEncoding.ASCII.GetString(billdata));
                            bgw.ReportProgress(75);
                        }
                        else
                        {
                            Bills = new List<OfflineClient.Bill>();
                            bgw.ReportProgress(75);
                        }
                        if (Store.Exists(OfflineDataFile.Settlement))
                        {
                            bgw.ReportProgress(80);
                            var billdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Settlement));
                            Settlements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Settlement>>(System.Text.ASCIIEncoding.ASCII.GetString(billdata));
                            bgw.ReportProgress(95);
                        }
                        else
                        {
                            Settlements = new List<OfflineClient.Settlement>();
                            bgw.ReportProgress(95);
                        }
                    }
                    initialized = true;
                    return initialized;
                }
            }
            catch (Exception exp)
            {
                e.Result = exp;
            }
            return false;
        }

        public OfflineClient.Stock AutoStock(string barcode)
        {
            var parts = barcode.Split(new char[] { '_', '-' });
            if (parts.Length != 2) return null;

            var stock = new OfflineClient.Stock();
            stock.barcode = barcode;
            stock.serailno = "";
            stock.companyid = InfyPOS.Processors.BillManager.Instance.Data.Company.First().id;
            stock.price = decimal.Parse(parts[1]);
            if (Data.Products.Exists(e => e.code == parts[0]))
            {
                var product = Data.Products.Find(e => e.code == parts[0]);
                if (Data.Tax.Exists(e => e.id == product.salestaxid))
                    stock.tax = Data.Tax.Find(e => e.id == product.salestaxid);
                else
                    stock.tax = Data.Tax.First();

                stock.companyid = product.cmpid;
                stock.product = product;
                stock.productid = product.id;
                return stock;
            }
            else
            {
                return null;
            }
        }
        public OfflineClient.Stock FindStock(string barcode)
        {

            Stocks.CaseSensitive = true;
            var rows = Stocks.Select("barcode = '" + barcode + "'");
            if (rows == null || rows.Length == 0)
            {
                rows = Stocks.Select("serialno = '" + barcode + "'");
                if (rows == null || rows.Length == 0)
                {
                    if (barcode.IndexOf('-') > 0 && InfyPOS.Processors.BillManager.Instance.Data.AutoBarcode)
                        return AutoStock(barcode);
                    return null;
                }
            }
            var row = rows[0];
            var stock = new OfflineClient.Stock();
            stock.barcode = row["barcode"] != DBNull.Value ? row["barcode"].ToString() : "";
            stock.serailno = row["serialno"] != DBNull.Value ? row["serialno"].ToString() : "";
            stock.hsncode = row["hsncode"] != DBNull.Value ? row["hsncode"].ToString() : "";
            stock.productid = row["productid"] != DBNull.Value ? Convert.ToInt64(row["productid"]) : 0;
            stock.companyid = row["companyid"] != DBNull.Value ? Convert.ToInt64(row["companyid"]) : 0;
            stock.price = row["price"] != DBNull.Value ? Convert.ToDecimal(row["price"]) : 0;
            stock.discount = row["discount"] != DBNull.Value ? Convert.ToDecimal(row["discount"]) : 0;
            if (row.Table.Columns.Contains("mrp") && row["mrp"] != DBNull.Value)
                stock.mrp = Convert.ToDecimal(row["mrp"]);
            if (Data.Products.Exists(e => e.id == stock.productid))
            {
                stock.product = Data.Products.Find(e => e.id == stock.productid);
                if (Data.Tax.Exists(e => e.id == stock.product.salestaxid))
                    stock.tax = Data.Tax.Find(e => e.id == stock.product.salestaxid);
                else
                    stock.tax = Data.Tax.First();
            }
            else
            {
                return null;
            }
            return stock;
            ///salablegoods.companyid,salablegoods.barcode,productid,salablegoods.price,salablegoods.discount
        }
        public class BillSourceData
        {
            public BillSourceData()
            {

            }
            public BillSourceData(BillSourceData currentData, OfflineClient.MasterData source)
            {
                if (currentData != null)
                {
                    this.employeeid = currentData.employeeid;
                    this.BillPrefix = currentData.BillPrefix;
                    this.counterid = currentData.counterid;
                    this.CounterCode = currentData.CounterCode;
                    this.LocationCode = currentData.LocationCode;
                }
                this.organizationid = source.organizationid;
                this.AutoBarcode = source.autobarcode;
                this.imageDatas = source.imageDatas;
                this.printconfig = source.PrintConfig;
                this.lastSyncOn = source.lastSyncOn;
                this.Promotions = source.Promotions;
                var taxtable = Processors.DataTableCustomFormatter.Deserialize(source.Tax, true);
                Tax = new List<OfflineClient.Tax>();
                foreach (System.Data.DataRow row in taxtable.Rows)
                {
                    Tax.Add(new OfflineClient.Tax()
                    {
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                        name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : "",
                        taxpercentage = row["taxpercentage"] != DBNull.Value ? Convert.ToDecimal(row["taxpercentage"]) : 0,
                        taxsplit_json = row["taxsplit"] != DBNull.Value ? Convert.ToString(row["taxsplit"]) : "null",
                    });
                }

                var producttable = Processors.DataTableCustomFormatter.Deserialize(source.Products, true);
                Products = new List<OfflineClient.Product>();
                foreach (System.Data.DataRow row in producttable.Rows)
                {
                    Products.Add(new OfflineClient.Product()
                    {
                        code = row["code"] != DBNull.Value ? Convert.ToString(row["code"]) : "",
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                        cmpid = row["cmpid"] != DBNull.Value ? Convert.ToInt64(row["cmpid"]) : 0,
                        name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : "",
                        salestaxid = row["salestaxid"] != DBNull.Value ? Convert.ToInt64(row["salestaxid"]) : 0,
                        iscut = row["mode"].ToString() == "1"
                    });
                }

                var employeetable = Processors.DataTableCustomFormatter.Deserialize(source.Employee, true);
                Employee = new List<OfflineClient.Employee>();
                foreach (System.Data.DataRow row in employeetable.Rows)
                {
                    Employee.Add(new OfflineClient.Employee()
                    {
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                        code = row["employeecode"] != DBNull.Value ? Convert.ToString(row["employeecode"]) : "",
                        name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : ""
                    });
                }


                var locationtable = Processors.DataTableCustomFormatter.Deserialize(source.Location, true);
                Location = new OfflineClient.Location();
                Location.id = locationtable.Rows[0]["id"] != DBNull.Value ? Convert.ToInt64(locationtable.Rows[0]["id"]) : 0;
                Location.address = locationtable.Rows[0]["address"] != DBNull.Value ? Convert.ToString(locationtable.Rows[0]["address"]) : "";
                Location.code = locationtable.Rows[0]["code"] != DBNull.Value ? Convert.ToString(locationtable.Rows[0]["code"]) : "";
                Location.name = locationtable.Rows[0]["name"] != DBNull.Value ? Convert.ToString(locationtable.Rows[0]["name"]) : "";
                Location.autosettlement = locationtable.Rows[0]["autosettlement"] != DBNull.Value ? Convert.ToBoolean(locationtable.Rows[0]["autosettlement"]) : false;
                Location.iswarehouse = locationtable.Rows[0]["iswarehouse"] != DBNull.Value ? Convert.ToBoolean(locationtable.Rows[0]["iswarehouse"]) : false;
                locationid = Location.id;

                var companytable = Processors.DataTableCustomFormatter.Deserialize(source.Company, true);
                Company = new List<OfflineClient.Company>();
                foreach (System.Data.DataRow row in companytable.Rows)
                {
                    Company.Add(new OfflineClient.Company()
                    {
                        code = row["code"] != DBNull.Value ? Convert.ToString(row["code"]) : "",
                        name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : "",
                        logo = row["logo"] != DBNull.Value ? Convert.ToString(row["logo"]) : "",
                        address = row["address"] != DBNull.Value ? Convert.ToString(row["address"]) : "",
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                    });
                }


                var countertable = Processors.DataTableCustomFormatter.Deserialize(source.Counter, true);
                Counter = new List<OfflineClient.Counter>();
                foreach (System.Data.DataRow row in countertable.Rows)
                {
                    Counter.Add(new OfflineClient.Counter()
                    {
                        code = row["code"] != DBNull.Value ? Convert.ToString(row["code"]) : "",
                        name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : "",
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                        floorid = row["floorid"] != DBNull.Value ? Convert.ToInt64(row["floorid"]) : 0,
                        locationid = row["locationid"] != DBNull.Value ? Convert.ToInt64(row["locationid"]) : 0,
                    });
                }

                var usertable = Processors.DataTableCustomFormatter.Deserialize(source.Users, true);
                Users = new List<OfflineClient.User>();
                foreach (System.Data.DataRow row in usertable.Rows)
                {
                    Users.Add(new OfflineClient.User()
                    {
                        username = row["username"] != DBNull.Value ? Convert.ToString(row["username"]) : "",
                        password = row["password"] != DBNull.Value ? Convert.ToString(row["password"]) : "",
                        id = row["id"] != DBNull.Value ? Convert.ToInt64(row["id"]) : 0,
                        employeeid = row["employeeid"] != DBNull.Value ? Convert.ToInt64(row["employeeid"]) : 0,
                        alloweddiscount = row["alloweddiscount"] != DBNull.Value ? Convert.ToDecimal(row["alloweddiscount"]) : 0,
                        permissions_json = row["permissions"] != DBNull.Value ? Convert.ToString(row["permissions"]) : "null",
                    });
                }

                var autonumbertable = Processors.DataTableCustomFormatter.Deserialize(source.AutoNumber, true);
                AutoNumber = new List<OfflineClient.AutoNumber>();
                if (autonumbertable != null)
                {
                    foreach (System.Data.DataRow row in autonumbertable.Rows)
                    {
                        AutoNumber.Add(new OfflineClient.AutoNumber()
                        {
                            company = row["company"] != DBNull.Value ? Convert.ToString(row["company"]) : "",
                            floor = row["floor"] != DBNull.Value ? Convert.ToString(row["floor"]) : "",
                            prefix = row["prefix"] != DBNull.Value ? Convert.ToString(row["prefix"]) : "",
                            companyid = row["companyid"] != DBNull.Value ? Convert.ToInt64(row["companyid"]) : 0,
                            floorid = row["floorid"] != DBNull.Value ? Convert.ToInt64(row["floorid"]) : 0,
                            locationid = row["locationid"] != DBNull.Value ? Convert.ToInt64(row["locationid"]) : 0,
                        });
                    }
                }


            }
            public long organizationid { get; set; }
            public List<OfflineClient.ImageData> imageDatas { get; set; }
            public long locationid { get; set; }
            public DateTime lastSyncOn { get; set; }
            public List<OfflineClient.Promotion> Promotions { get; set; }
            public List<OfflineClient.Tax> Tax { get; set; }
            public List<OfflineClient.Employee> Employee { get; set; }
            public List<OfflineClient.Product> Products { get; set; }
            public OfflineClient.Location Location { get; set; }
            public List<OfflineClient.Company> Company { get; set; }
            public List<OfflineClient.Counter> Counter { get; set; }

            public List<OfflineClient.AutoNumber> AutoNumber { get; set; }
            public List<OfflineClient.PrintCofig> printconfig { get; set; }
            public PrinterService.Printer Printer { get; set; }
            public int noofcopy { get; set; }
            public long counterid { get; set; }
            public long floorid { get; set; }
            public long employeeid { get; set; }
            public string CounterPrefix { get; set; }
            public string BillPrefix { get; set; }
            public string SettlementPrefix { get; set; }
            public string CompanyCode { get; set; }
            public string LocationCode { get; set; }
            public string CounterCode { get; set; }
            public List<OfflineClient.User> Users { get; set; }
            public bool MultiCompany { get; set; }
            public bool AutoBarcode { get; set; }
            public bool Autosettlement { get; set; }
            public bool Mobile10digit { get; set; }
            public bool AutoSync { get; set; }
            public string AutoSync_Mode { get; set; }
            public int AutoSync_Cycle { get; set; }
            public long Discount { get; set; }
        }

        internal bool SaveSettlement(OfflineClient.Settlement settlement)
        {
            if (Settlements == null)
                Settlements = new List<OfflineClient.Settlement>();

            var balance = settlement.balance;

            if (InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno > 0)
            {
                settlement.index = InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno;
                InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno = 0;
                InfyPOS.Processors.BillManager.Instance.PersistMasters();
            }
            else if (Settlements.Exists(e => e.settlementon == settlement.settlementon &&
            e.companyid == settlement.companyid))
            {
                settlement.index = Settlements.FindAll(e => e.settlementon == settlement.settlementon &&
                e.companyid == settlement.companyid).Max(e => e.index) + 1;
            }
            else
            {
                settlement.index = NoSequenceManager.Instance.GetNo("settlement", settlement.companyid, settlement.settlementon.Date) + 1;
            }
            settlement.createdby = CurrentUser.id;
            settlement.code = GetBillNo(Data.SettlementPrefix, settlement);
            Settlements.Add(settlement);
            PersistSettlement(false);

            return true;
        }

        internal void UpdateBillNo(List<OfflineClient.Bill> billlist)
        {
            foreach (var currentBill in billlist)
            {
                if (InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == currentBill.companyid).billno > 0)
                {
                    currentBill.index = InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == currentBill.companyid).billno;
                }
                else if (Bills.Exists(e => e.billdate == currentBill.billdate && e.companyid == currentBill.companyid))
                {
                    currentBill.index = Bills.FindAll(e => e.billdate == currentBill.billdate && e.companyid == currentBill.companyid).Max(e => e.index) + 1;
                }
                else
                {
                    currentBill.index = NoSequenceManager.Instance.GetNo("bill", currentBill.companyid, currentBill.billdate.Date) + 1;
                }
                currentBill.billno = GetBillNo(Data.BillPrefix, currentBill);
            }
        }
        internal List<OfflineClient.Bill> SaveBill(List<OfflineClient.Bill> billlist)
        {
            if (Bills == null)
                Bills = new List<OfflineClient.Bill>();

            foreach (var currentBill in billlist)
            {
                if (InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == currentBill.companyid).billno > 0)
                {
                    currentBill.index = InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == currentBill.companyid).billno;
                    InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == currentBill.companyid).billno = 0;
                    InfyPOS.Processors.BillManager.Instance.PersistMasters();
                }
                else if (Bills.Exists(e => e.billdate == currentBill.billdate && e.companyid == currentBill.companyid))
                {
                    currentBill.index = Bills.FindAll(e => e.billdate == currentBill.billdate && e.companyid == currentBill.companyid).Max(e => e.index) + 1;
                }
                else
                {
                    currentBill.index = NoSequenceManager.Instance.GetNo("bill", currentBill.companyid, currentBill.billdate.Date) + 1;
                }
                currentBill.createdon = DateTime.Now;
                currentBill.createdby = CurrentUser.id;
                if (CurrentUser.employeeid > 0)
                {
                    if (Data.Employee.Exists(e => e.id == CurrentUser.employeeid))
                    {
                        currentBill.biller = Data.Employee.Find(e => e.id == CurrentUser.employeeid).name;
                    }
                }
                currentBill.counterid = Data.counterid;
                currentBill.locationid = Data.locationid;
                currentBill.billno = GetBillNo(Data.BillPrefix, currentBill);
            }
            Bills.AddRange(billlist);
            PersistBill(false);
            return billlist;
        }
        public string GetBillNo(string format, OfflineClient.Settlement bill)
        {
            format = format.Replace("[MM]", bill.settlementon.ToString("MM")).Replace("[DD]",
                bill.settlementon.ToString("dd")).Replace("[YY]", bill.settlementon.ToString("yy"));
            format = format.Replace("[CMP]", Data.Company.Find(e => e.id == bill.companyid).billprefix);
            return format.Replace("[CNT]", Data.CounterPrefix).Replace("[NO]", bill.index.ToString("N0"));
        }
        public string GetBillNo(string format, OfflineClient.Bill bill)
        {
            format = format.Replace("[MM]", bill.billdate.ToString("MM")).Replace("[DD]",
                bill.billdate.ToString("dd")).Replace("[YY]", bill.billdate.ToString("yy"));
            format = format.Replace("[CMP]", Data.Company.Find(e => e.id == bill.companyid).billprefix);
            return format.Replace("[CNT]", Data.CounterPrefix).Replace("[NO]", bill.index.ToString("N0"));
        }



        public class CustomerManager
        {
            static CustomerManager instance = null;
            List<OfflineClient.Customer> list = new List<OfflineClient.Customer>();
            public static CustomerManager Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new CustomerManager();
                        instance.Inialize();
                    }
                    return instance;
                }
            }
            public void Inialize()
            {
                if (Store.Exists(OfflineDataFile.Customer))
                {
                    var customerdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Customer));
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Customer>>(System.Text.ASCIIEncoding.ASCII.GetString(customerdata));
                }
                else
                {
                    list = new List<OfflineClient.Customer>();
                }
            }
            public void Reload()
            {
                Inialize();
            }
            public OfflineClient.Customer Get(string no)
            {
                if (list.Exists(e => e.no == no))
                {
                    return list.Find(e => e.no == no);
                }
                return null;
            }
            public void Add(string no, string name)
            {
                if (list.Exists(e => e.no == no))
                {
                    var customer = list.Find(e => e.no == no);
                    if (customer.name != name)
                    {
                        customer.name = name;
                        Persist();
                    }
                }
                else
                {
                    list.Add(new OfflineClient.Customer()
                    {
                        no = no,
                        name = name
                    });
                    Persist();
                }
            }
            public void Persist()
            {
                var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(list)));
                Store.WriteAllBytes(OfflineDataFile.Customer, masterdata);
            }
        }
        public class NoSequenceManager
        {
            List<NoSequence> list = new List<NoSequence>();
            public class NoSequence
            {
                public long companyid { get; set; }
                public string type { get; set; }
                public DateTime date { get; set; }
                public long no { get; set; }
            }
            static NoSequenceManager instance = null;
            public static NoSequenceManager Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new NoSequenceManager();
                        instance.Inialize();
                    }
                    return instance;
                }
            }
            public void Inialize()
            {


                if (Store.Exists(OfflineDataFile.Sequence))
                {
                    var sequencedata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Sequence));
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NoSequence>>(System.Text.ASCIIEncoding.ASCII.GetString(sequencedata));
                }
                else
                {
                    list = new List<NoSequence>();
                }
            }
            public void Persist()
            {


                var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(list)));
                Store.WriteAllBytes(OfflineDataFile.Sequence, masterdata);
            }
            public void Update(string type, long companyid, DateTime date, long no)
            {
                if (list.Exists(e => e.type == type && e.companyid == companyid && e.date == date.Date))
                {
                    var lastno = list.Find(e => e.type == type && e.companyid == companyid && e.date == date.Date).no;
                    if (lastno < no)
                        list.Find(e => e.type == type && e.companyid == companyid && e.date == date.Date).no = no;
                }
                else
                {
                    list.Add(new NoSequence()
                    {
                        type = type,
                        date = date,
                        no = no,
                        companyid = companyid
                    });
                }

            }
            public long GetNo(string type, long companyid, DateTime date)
            {
                if (list.Exists(e => e.type == type && e.companyid == companyid && e.date == date.Date))
                {
                    return list.Find(e => e.type == type && e.date == date.Date).no;
                }
                return 0;
            }

            internal void Update(List<OfflineClient.Bill> bills)
            {
                foreach (var bill in bills.GroupBy(e => new { e.billdate.Date, e.companyid }))
                {
                    Update("bill", bill.Key.companyid, bill.Key.Date, bill.Max(e => e.index));
                }
                Persist();
            }

            internal void Update(List<OfflineClient.Settlement> settlements)
            {
                foreach (var settlement in settlements.GroupBy(e => new { e.settlementon.Date, e.companyid }))
                {
                    Update("settlement", settlement.Key.companyid, settlement.Key.Date, settlement.Max(e => e.index));
                }
                Persist();
            }
        }
    }


    public static class JsonHelpers
    {
        public static T CreateFromJsonStream<T>(this Stream stream)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
            {
            });
            serializer.NullValueHandling = NullValueHandling.Ignore;
            T data;
            using (StreamReader streamReader = new StreamReader(stream))
            {
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }

        public static T CreateFromJsonString<T>(this String json)
        {
            T data;
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(json)))
            {
                data = CreateFromJsonStream<T>(stream);
            }
            return data;
        }

        public static T CreateFromJsonFile<T>(this String fileName)
        {
            T data;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                data = CreateFromJsonStream<T>(fileStream);
            }
            return data;
        }
    }
}
