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
            ApplyMachineBillingSettings();
            PersistMasters(edata);
        }
        public void Load(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;
            var edata = JsonConvert.DeserializeObject<OfflineClient.MasterData>(System.Text.ASCIIEncoding.ASCII.GetString(bytes));
            Data = new BillSourceData(Data, edata);
            ApplyMachineBillingSettings();
            PersistMasters(edata);
        }

        public void LoadFromClientPackage(OfflineClient.ClientMasterPackage package)
        {
            if (package == null || package.master == null || package.master.Length == 0)
                throw new InvalidOperationException("Master data is not available from the MASTER.");

            // CLIENT: memory only — never write business .data locally.
            // MASTER: may also call this; only persist when not CLIENT (blocked by OfflineDataStore anyway).
            if (!Quanto.MachineConfig.IsClient)
            {
                Store.WriteAllBytes(OfflineDataFile.Master, package.master);
            }

            var masterdata = BufferedRealtimeCompressionEngine.Decompress(package.master);
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<BillSourceData>(System.Text.ASCIIEncoding.ASCII.GetString(masterdata));
            ApplyMachineBillingSettings();

            if (package.stock != null && package.stock.Length > 0)
            {
                if (!Quanto.MachineConfig.IsClient)
                    Store.WriteAllBytes(OfflineDataFile.Stock, package.stock);
                Stocks = Processors.DataTableCustomFormatter.Deserialize(package.stock, true);
                Stocks.CaseSensitive = true;
            }

            if (package.customer != null && package.customer.Length > 0)
            {
                if (!Quanto.MachineConfig.IsClient)
                    Store.WriteAllBytes(OfflineDataFile.Customer, package.customer);
                CustomerManager.Instance.ReloadFromBytes(package.customer);
            }

            // CLIENT must not persist bills locally. Keep any session list already loaded from MASTER.
            if (Bills == null)
                Bills = new List<OfflineClient.Bill>();
            if (Settlements == null)
                Settlements = new List<OfflineClient.Settlement>();
            initialized = true;
        }

        public void RefreshClientMasterFromMaster()
        {
            if (!Quanto.MachineConfig.IsClient)
                return;

            var response = Quanto.ServiceProxy.Instance.DownloadAndApplyClientMaster();
            if (response == null || response.error)
                throw new InvalidOperationException(
                    response?.errormessage ?? "Master server unavailable");
        }

        public void PersistMasters()
        {
            if (Quanto.MachineConfig.IsClient)
                return; // printer/settings stay in memory only on CLIENT

            var masterdata = BufferedRealtimeCompressionEngine.Compress(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Data)));
            Store.WriteAllBytes(OfflineDataFile.Master, masterdata);
        }

        public const string DefaultBillPrefix = "B[CMP][CNT][DD][MM][YY]/[NO]";
        public const string DefaultSettlementPrefix = "S[CMP][CNT][DD][MM][YY]/[NO]";

        /// <summary>
        /// Each machine stores its own billing configuration in App.config.
        /// First CLIENT start copies MASTER config; later CLIENT changes stay on that machine.
        /// </summary>
        public void ApplyMachineBillingSettings()
        {
            if (Data == null)
                return;

            if (Quanto.MachineConfig.IsClient && !HasSavedBillingSettings())
            {
                SaveLocalBillingSettings();
                return;
            }

            ApplyLocalBillingSettings();
        }

        private static bool HasSavedBillingSettings()
        {
            var flag = AppSetting("BillingSettingsSaved");
            return !string.IsNullOrWhiteSpace(flag) &&
                   flag.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        public void ApplyLocalBillingSettings()
        {
            if (Data == null)
                return;

            var prefix = AppSetting("BillPrefix");
            if (prefix != null)
                Data.BillPrefix = prefix;

            var settlementPrefix = AppSetting("SettlementPrefix");
            if (settlementPrefix != null)
                Data.SettlementPrefix = settlementPrefix;

            var counterPrefix = AppSetting("CounterPrefix");
            if (counterPrefix != null)
                Data.CounterPrefix = counterPrefix;

            var companyPrefixes = AppSetting("CompanyBillPrefix");
            if (companyPrefixes != null && Data.Company != null)
            {
                try
                {
                    var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(companyPrefixes);
                    if (map != null)
                    {
                        foreach (var company in Data.Company)
                        {
                            string saved;
                            if (map.TryGetValue(company.id.ToString(), out saved))
                                company.billprefix = saved ?? "";
                        }
                    }
                }
                catch
                {
                }
            }

            long id;
            if (long.TryParse(AppSetting("BillingEmployeeId"), out id) && id > 0)
                Data.employeeid = id;
            if (long.TryParse(AppSetting("BillingCounterId"), out id) && id > 0)
                Data.counterid = id;

            var autoBarcode = AppSetting("AutoBarcode");
            if (!string.IsNullOrEmpty(autoBarcode))
                Data.AutoBarcode = autoBarcode.Equals("true", StringComparison.OrdinalIgnoreCase);
            var autoSettlement = AppSetting("Autosettlement");
            if (!string.IsNullOrEmpty(autoSettlement))
                Data.Autosettlement = autoSettlement.Equals("true", StringComparison.OrdinalIgnoreCase);
            var mobile10 = AppSetting("Mobile10digit");
            if (!string.IsNullOrEmpty(mobile10))
                Data.Mobile10digit = mobile10.Equals("true", StringComparison.OrdinalIgnoreCase);

            long discount;
            if (long.TryParse(AppSetting("BillingDiscount"), out discount))
                Data.Discount = discount;

            if (Data.Company == null)
                return;
            ApplyCompanyNumberMap("CompanyBillNo", (company, no) => company.billno = no);
            ApplyCompanyNumberMap("CompanySettlementNo", (company, no) => company.settlementno = no);
        }

        private void ApplyCompanyNumberMap(string key, Action<OfflineClient.Company, long> apply)
        {
            var json = AppSetting(key);
            if (string.IsNullOrEmpty(json) || Data.Company == null)
                return;
            try
            {
                var map = JsonConvert.DeserializeObject<Dictionary<string, long>>(json);
                if (map == null)
                    return;
                foreach (var company in Data.Company)
                {
                    long no;
                    if (map.TryGetValue(company.id.ToString(), out no))
                        apply(company, no);
                }
            }
            catch
            {
            }
        }

        public void SaveLocalBillingSettings()
        {
            if (Data == null)
                return;

            Quanto.AppConfigFile.SaveSetting("BillPrefix", Data.BillPrefix ?? "");
            Quanto.AppConfigFile.SaveSetting("SettlementPrefix", Data.SettlementPrefix ?? "");
            Quanto.AppConfigFile.SaveSetting("CounterPrefix", Data.CounterPrefix ?? "");
            if (Data.Company != null)
            {
                var map = new Dictionary<string, string>();
                foreach (var company in Data.Company)
                    map[company.id.ToString()] = company.billprefix ?? "";
                Quanto.AppConfigFile.SaveSetting("CompanyBillPrefix", JsonConvert.SerializeObject(map));
                var billNos = new Dictionary<string, long>();
                var settlementNos = new Dictionary<string, long>();
                foreach (var company in Data.Company)
                {
                    billNos[company.id.ToString()] = company.billno;
                    settlementNos[company.id.ToString()] = company.settlementno;
                }
                Quanto.AppConfigFile.SaveSetting("CompanyBillNo", JsonConvert.SerializeObject(billNos));
                Quanto.AppConfigFile.SaveSetting("CompanySettlementNo", JsonConvert.SerializeObject(settlementNos));
            }
            Quanto.AppConfigFile.SaveSetting("BillingEmployeeId", Data.employeeid.ToString());
            Quanto.AppConfigFile.SaveSetting("BillingCounterId", Data.counterid.ToString());
            Quanto.AppConfigFile.SaveSetting("AutoBarcode", Data.AutoBarcode ? "true" : "false");
            Quanto.AppConfigFile.SaveSetting("Autosettlement", Data.Autosettlement ? "true" : "false");
            Quanto.AppConfigFile.SaveSetting("Mobile10digit", Data.Mobile10digit ? "true" : "false");
            Quanto.AppConfigFile.SaveSetting("BillingDiscount", Data.Discount.ToString());
            Quanto.AppConfigFile.SaveSetting("BillingSettingsSaved", "true");
        }

        public bool NeedsBillingSettingsDialog()
        {
            ApplyMachineBillingSettings();
            if (Data == null)
                return true;
            return Data.employeeid == 0 || Data.counterid == 0;
        }

        private static string AppSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
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
            if (Quanto.MachineConfig.IsClient)
                return;

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
                var allRecords = Settlements != null
                    ? Settlements.FindAll(e => e.settlementon.Date == date.Date)
                    : new List<OfflineClient.Settlement>();
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
            if (Quanto.MachineConfig.IsClient)
                return;

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
            if (Quanto.MachineConfig.IsClient)
                return;

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
            if (Quanto.MachineConfig.IsClient)
                return;

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

        /// <summary>
        /// Load bill/settlement lists from disk if not already in memory (API receive path on MASTER).
        /// </summary>
        public void EnsureTransactionFilesLoaded()
        {
            if (Quanto.MachineConfig.IsClient)
            {
                if (Bills == null)
                    Bills = new List<OfflineClient.Bill>();
                if (Settlements == null)
                    Settlements = new List<OfflineClient.Settlement>();
                return;
            }

            if (Bills == null)
            {
                if (Store.Exists(OfflineDataFile.Bill))
                {
                    var billdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Bill));
                    Bills = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Bill>>(
                        System.Text.ASCIIEncoding.ASCII.GetString(billdata)) ?? new List<OfflineClient.Bill>();
                }
                else
                {
                    Bills = new List<OfflineClient.Bill>();
                }
            }

            if (Settlements == null)
            {
                if (Store.Exists(OfflineDataFile.Settlement))
                {
                    var data = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Settlement));
                    Settlements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Settlement>>(
                        System.Text.ASCIIEncoding.ASCII.GetString(data)) ?? new List<OfflineClient.Settlement>();
                }
                else
                {
                    Settlements = new List<OfflineClient.Settlement>();
                }
            }
        }

        public List<OfflineClient.Bill> GetBillsForDate(DateTime date)
        {
            lock (Instance)
            {
                EnsureTransactionFilesLoaded();
                if (Bills == null)
                    return new List<OfflineClient.Bill>();
                return Bills.FindAll(e => e.billdate.Date == date.Date);
            }
        }

        public void LoadClientBillsFromMaster(DateTime date)
        {
            if (!Quanto.MachineConfig.IsClient)
                return;

            var pull = Quanto.ServiceProxy.Instance.DownloadBillsFromMaster(date);
            if (pull == null || pull.error)
                throw new InvalidOperationException(
                    pull?.errormessage ?? "Master server unavailable");

            if (Bills == null)
                Bills = new List<OfflineClient.Bill>();
            Bills.RemoveAll(e => e.billdate.Date == date.Date);
            if (pull.bills != null && pull.bills.Count > 0)
                Bills.AddRange(pull.bills);
        }

        public OfflineClient.ClientUploadResult AcceptRemoteBills(List<OfflineClient.Bill> incoming, string fromDeviceId)
        {
            var result = new OfflineClient.ClientUploadResult { bills = new List<OfflineClient.Bill>() };
            if (incoming == null || incoming.Count == 0)
            {
                result.completed = true;
                return result;
            }

            lock (Instance)
            {
                try
                {
                    EnsureMasterBusinessDataLoaded();
                    EnsureTransactionFilesLoaded();

                    var stockChanged = false;
                    foreach (var draft in incoming)
                    {
                        if (string.IsNullOrWhiteSpace(draft.deviceid) && !string.IsNullOrWhiteSpace(fromDeviceId))
                            draft.deviceid = fromDeviceId;

                        var already = FindStoredBill(draft);
                        if (already != null)
                        {
                            result.bills.Add(already);
                            result.accepted++;
                            continue;
                        }

                        if (draft.Billitems != null)
                        {
                            foreach (var item in draft.Billitems)
                            {
                                if (string.IsNullOrWhiteSpace(item.barcode))
                                    continue;
                                // Match local MASTER SaveBill: unknown / AutoBarcode lines are allowed.
                                // Only deduct when a real stock row with a qty column exists.
                                if (!TryValidateAndDeductStock(item.barcode, item.qty, out var deducted, out var stockError))
                                {
                                    result.error = true;
                                    result.errormessage = stockError;
                                    result.completed = true;
                                    return result;
                                }
                                if (deducted)
                                    stockChanged = true;
                            }
                        }

                        // Keep CLIENT bill numbers when unique; bump only if that number already exists.
                        if (string.IsNullOrWhiteSpace(draft.billno) || draft.index <= 0)
                            AssignBillIndexAndNumber(draft, true);
                        else
                            EnsureUniqueBillNo(draft);
                        StampBillSaveTime(draft);
                        if (draft.locationid <= 0 && Data != null)
                            draft.locationid = Data.locationid;

                        Bills.Add(draft);
                        result.bills.Add(draft);
                        result.accepted++;
                    }

                    SyncSettlementBillNumbers(incoming);

                    PersistBill(false);
                    PersistMasters();
                    if (stockChanged && Stocks != null)
                    {
                        Store.WriteAllBytes(OfflineDataFile.Stock, Processors.DataTableCustomFormatter.Serialize(Stocks, true));
                    }

                    result.completed = true;
                    Quanto.Logger.Current.InfoFormat(
                        "MASTER created {0} bill(s) from {1}",
                        result.accepted, fromDeviceId ?? "?");
                    return result;
                }
                catch (Exception exp)
                {
                    result.error = true;
                    result.completed = true;
                    result.errormessage = exp.Message;
                    Quanto.Logger.Current.Error("AcceptRemoteBills failed", exp);
                    return result;
                }
            }
        }

        public OfflineClient.ClientUploadResult AcceptRemoteSettlements(List<OfflineClient.Settlement> incoming, string fromDeviceId)
        {
            var result = new OfflineClient.ClientUploadResult { settlements = new List<OfflineClient.Settlement>() };
            if (incoming == null || incoming.Count == 0)
            {
                result.completed = true;
                return result;
            }

            lock (Instance)
            {
                try
                {
                    EnsureMasterBusinessDataLoaded();
                    EnsureTransactionFilesLoaded();

                    foreach (var settlement in incoming)
                    {
                        if (Data.Company.Exists(e => e.id == settlement.companyid) &&
                            Data.Company.Find(e => e.id == settlement.companyid).settlementno > 0)
                        {
                            settlement.index = Data.Company.Find(e => e.id == settlement.companyid).settlementno;
                            Data.Company.Find(e => e.id == settlement.companyid).settlementno = 0;
                        }
                        else if (Settlements.Exists(e => e.settlementon.Date == settlement.settlementon.Date && e.companyid == settlement.companyid))
                        {
                            settlement.index = Settlements.FindAll(e => e.settlementon.Date == settlement.settlementon.Date && e.companyid == settlement.companyid).Max(e => e.index) + 1;
                        }
                        else
                        {
                            settlement.index = NoSequenceManager.Instance.GetNo("settlement", settlement.companyid, settlement.settlementon.Date) + 1;
                        }
                        settlement.code = GetBillNo(Data.SettlementPrefix, settlement);
                        Settlements.Add(settlement);
                        result.settlements.Add(settlement);
                        result.accepted++;
                    }

                    PersistSettlement(false);
                    PersistMasters();
                    result.completed = true;
                    return result;
                }
                catch (Exception exp)
                {
                    result.error = true;
                    result.completed = true;
                    result.errormessage = exp.Message;
                    return result;
                }
            }
        }

        /// <summary>Load master/stock from disk on MASTER for API paths (no WinForms Initialize).</summary>
        public void EnsureMasterBusinessDataLoaded()
        {
            if (Data != null && Stocks != null)
                return;

            if (!Store.Exists(OfflineDataFile.Master))
                throw new InvalidOperationException("MASTER has no master.data loaded.");

            var masterdata = BufferedRealtimeCompressionEngine.Decompress(Store.ReadAllBytes(OfflineDataFile.Master));
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<BillSourceData>(System.Text.ASCIIEncoding.ASCII.GetString(masterdata));

            if (Store.Exists(OfflineDataFile.Stock))
            {
                Stocks = Processors.DataTableCustomFormatter.Deserialize(Store.ReadAllBytes(OfflineDataFile.Stock), true);
                Stocks.CaseSensitive = true;
            }
            else
            {
                throw new InvalidOperationException("MASTER has no stock.data loaded.");
            }
        }

        /// <summary>
        /// Same rules as local MASTER billing: AutoBarcode / missing stock rows still allow the bill.
        /// When a stock row has a qty column, deduct (and fail only if insufficient).
        /// </summary>
        private bool TryValidateAndDeductStock(string barcode, decimal qty, out bool deducted, out string error)
        {
            deducted = false;
            error = null;
            if (Stocks == null)
            {
                // Legacy SaveBill never required stock for bill create.
                return true;
            }

            Stocks.CaseSensitive = true;
            var safe = (barcode ?? "").Replace("'", "''");
            var rows = Stocks.Select("barcode = '" + safe + "'");
            if (rows == null || rows.Length == 0)
                rows = Stocks.Select("serialno = '" + safe + "'");
            if (rows == null || rows.Length == 0)
            {
                // Not in stock.data (common with AutoBarcode product-code-price) — allow like SaveBill.
                return true;
            }

            var row = rows[0];
            string[] qtyCols = { "qty", "quantity", "balqty", "stockqty", "available", "salableqty", "closingqty" };
            foreach (var col in qtyCols)
            {
                if (!row.Table.Columns.Contains(col) || row[col] == DBNull.Value)
                    continue;
                var available = Convert.ToDecimal(row[col]);
                if (available < qty)
                {
                    error = "Insufficient stock on MASTER for " + barcode +
                            " (available " + available + ", requested " + qty + ").";
                    return false;
                }
                row[col] = available - qty;
                deducted = true;
                return true;
            }

            // No qty column in legacy stock table — barcode exists; allow sale without qty deduction.
            return true;
        }

        private bool initialized = false;
        public bool Initialize(System.ComponentModel.DoWorkEventArgs e, System.ComponentModel.BackgroundWorker bgw)
        {
            try
            {
                if (initialized) return true;

                // CLIENT: load business data from MASTER HTTP into memory only (no local .data).
                if (Quanto.MachineConfig.IsClient)
                {
                    bgw?.ReportProgress(10);
                    if (!Quanto.MasterConnectionMonitor.MasterConnected)
                    {
                        // one quick health probe
                        var ip = Quanto.MachineConfig.LastKnownMasterIp;
                        if (string.IsNullOrWhiteSpace(ip) ||
                            !Quanto.MasterConnectionMonitor.VerifyMaster(ip, Quanto.MachineConfig.MasterDeviceId))
                        {
                            if (e != null)
                                e.Result = new InvalidOperationException("Master server unavailable");
                            return false;
                        }
                    }

                    bgw?.ReportProgress(40);
                    var response = Quanto.ServiceProxy.Instance.DownloadAndApplyClientMaster();
                    if (response == null || response.error)
                    {
                        if (e != null)
                            e.Result = new InvalidOperationException(
                                response?.errormessage ?? "Unable to load business data from MASTER.");
                        return false;
                    }
                    bgw?.ReportProgress(100);
                    initialized = true;
                    Quanto.OfflineBill.StartIfConfigured();
                    return true;
                }

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
                    ApplyMachineBillingSettings();
                    CustomerManager.Instance.Reload();
                    Quanto.OfflineBill.StartIfConfigured();
                    return initialized;
                }
            }
            catch (Exception exp)
            {
                if (e != null)
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
            if (Quanto.MachineConfig.IsClient)
            {
                var remote = Quanto.ServiceProxy.Instance.LookupStockFromMaster(barcode);
                if (remote != null && !remote.error)
                    return remote.stock;

                Quanto.Logger.Current.Info("CLIENT barcode lookup using last memory copy: " + (remote?.errormessage ?? "MASTER unavailable"));
                return FindStockLocal(barcode);
            }

            return FindStockLocal(barcode);
        }

        public OfflineClient.Stock FindStockLocal(string barcode)
        {
            if (Stocks == null || Data == null || Data.Products == null)
                return null;

            Stocks.CaseSensitive = true;
            var safe = (barcode ?? "").Replace("'", "''");
            var rows = Stocks.Select("barcode = '" + safe + "'");
            if (rows == null || rows.Length == 0)
            {
                rows = Stocks.Select("serialno = '" + safe + "'");
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
                    this.SettlementPrefix = currentData.SettlementPrefix;
                    this.CounterPrefix = currentData.CounterPrefix;
                    this.counterid = currentData.counterid;
                    this.CounterCode = currentData.CounterCode;
                    this.LocationCode = currentData.LocationCode;
                    this.Mobile10digit = currentData.Mobile10digit;
                    this.Autosettlement = currentData.Autosettlement;
                    this.AutoBarcode = currentData.AutoBarcode;
                    this.Discount = currentData.Discount;
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
                        billprefix = row.Table.Columns.Contains("billprefix") && row["billprefix"] != DBNull.Value
                            ? Convert.ToString(row["billprefix"]) : ""
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


                if (Company != null && currentData != null && currentData.Company != null)
                {
                    foreach (var company in Company)
                    {
                        var previous = currentData.Company.Find(e => e.id == company.id);
                        if (previous == null)
                            continue;
                        company.billprefix = previous.billprefix ?? "";
                        company.billno = previous.billno;
                        company.settlementno = previous.settlementno;
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

            if (Quanto.MachineConfig.IsClient)
            {
                settlement.createdby = CurrentUser != null ? CurrentUser.id : 0;
                var push = Quanto.ServiceProxy.Instance.UploadSettlementsToMaster(
                    new List<OfflineClient.Settlement> { settlement });
                if (push == null || push.error)
                    throw new InvalidOperationException(
                        push?.errormessage ?? "Master server unavailable");
                if (push.settlements != null && push.settlements.Count > 0)
                    settlement = push.settlements[0];
                // Session UI only — not an authoritative local store
                Settlements.Add(settlement);
                return true;
            }

            var balance = settlement.balance;

            if (InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno > 0)
            {
                settlement.index = InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno;
                InfyPOS.Processors.BillManager.Instance.Data.Company.Find(e => e.id == settlement.companyid).settlementno = 0;
                InfyPOS.Processors.BillManager.Instance.PersistMasters();
            }
            else if (Settlements.Exists(e => e.settlementon.Date == settlement.settlementon.Date &&
            e.companyid == settlement.companyid))
            {
                settlement.index = Settlements.FindAll(e => e.settlementon.Date == settlement.settlementon.Date &&
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
            if (billlist == null || billlist.Count == 0)
                return;

            if (Quanto.MachineConfig.IsClient)
            {
                try
                {
                    LoadClientBillsFromMaster(billlist[0].billdate);
                }
                catch
                {
                    // Preview numbers from whatever is already in memory
                }
                foreach (var currentBill in billlist)
                    AssignBillIndexAndNumber(currentBill, false);
                return;
            }

            lock (Instance)
            {
                EnsureTransactionFilesLoaded();
                foreach (var currentBill in billlist)
                    AssignBillIndexAndNumber(currentBill, false);
            }
        }

        /// <summary>
        /// Keep the selected bill date and record the actual save time on the bill.
        /// Does not overwrite a time already set by the cashier / CLIENT.
        /// </summary>
        private static void StampBillSaveTime(OfflineClient.Bill bill)
        {
            if (bill == null)
                return;

            var now = DateTime.Now;
            if (bill.createdon == DateTime.MinValue)
                bill.createdon = now;
            if (bill.createon == DateTime.MinValue)
                bill.createon = bill.createdon;

            var datePart = bill.billdate == DateTime.MinValue ? bill.createdon.Date : bill.billdate.Date;
            if (bill.billdate.TimeOfDay == TimeSpan.Zero)
                bill.billdate = datePart.Add(bill.createdon.TimeOfDay);
        }

        /// <summary>
        /// Next bill index for this company+date on this machine only.
        /// CLIENT uses its own prefix/counter from App.config, not MASTER's.
        /// </summary>
        private void AssignBillIndexAndNumber(OfflineClient.Bill bill, bool consumeStartNumber)
        {
            ApplyLocalBillingSettings();
            if (string.IsNullOrWhiteSpace(bill.deviceid))
                bill.deviceid = Quanto.MachineConfig.DeviceId;

            var company = Data != null && Data.Company != null
                ? Data.Company.Find(e => e.id == bill.companyid)
                : null;

            if (company != null && company.billno > 0)
            {
                bill.index = company.billno;
                if (consumeStartNumber)
                {
                    company.billno = 0;
                    if (Quanto.MachineConfig.IsClient)
                        SaveLocalBillingSettings();
                    else
                        PersistMasters();
                }
            }
            else if (Bills != null && Bills.Exists(e => SameMachineBill(e) && e.billdate.Date == bill.billdate.Date && e.companyid == bill.companyid))
            {
                bill.index = Bills.FindAll(e => SameMachineBill(e) && e.billdate.Date == bill.billdate.Date && e.companyid == bill.companyid).Max(e => e.index) + 1;
            }
            else
            {
                bill.index = NoSequenceManager.Instance.GetNo("bill", bill.companyid, bill.billdate.Date) + 1;
            }
            bill.billno = GetBillNo(Data != null ? Data.BillPrefix : null, bill);
            if (string.IsNullOrWhiteSpace(bill.billno))
                bill.billno = bill.index.ToString();
            EnsureUniqueBillNo(bill);
        }

        private OfflineClient.Bill FindStoredBill(OfflineClient.Bill draft)
        {
            if (Bills == null || draft == null)
                return null;
            return Bills.Find(e =>
                e.companyid == draft.companyid &&
                e.billdate.Date == draft.billdate.Date &&
                !string.IsNullOrWhiteSpace(draft.billno) &&
                string.Equals(e.billno, draft.billno, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.deviceid ?? "", draft.deviceid ?? "", StringComparison.OrdinalIgnoreCase));
        }

        private bool BillNoTaken(OfflineClient.Bill bill)
        {
            if (Bills == null || bill == null || string.IsNullOrWhiteSpace(bill.billno))
                return false;
            return Bills.Exists(e =>
                e != bill &&
                e.companyid == bill.companyid &&
                e.billdate.Date == bill.billdate.Date &&
                string.Equals(e.billno ?? "", bill.billno, StringComparison.OrdinalIgnoreCase));
        }

        private void EnsureUniqueBillNo(OfflineClient.Bill bill)
        {
            if (bill == null)
                return;
            var guard = 0;
            while (BillNoTaken(bill) && guard++ < 10000)
            {
                bill.index++;
                bill.billno = GetBillNo(Data != null ? Data.BillPrefix : null, bill);
                if (string.IsNullOrWhiteSpace(bill.billno))
                    bill.billno = bill.index.ToString();
            }
        }

        private static bool SameMachineBill(OfflineClient.Bill bill)
        {
            var deviceId = Quanto.MachineConfig.DeviceId ?? "";
            var billDevice = bill != null ? (bill.deviceid ?? "") : "";
            if (string.IsNullOrEmpty(billDevice))
                return Quanto.MachineConfig.IsMaster;
            return string.Equals(billDevice, deviceId, StringComparison.OrdinalIgnoreCase);
        }

        internal void PreviewBillNo(OfflineClient.Bill bill)
        {
            if (bill == null)
                return;
            AssignBillIndexAndNumber(bill, false);
        }

        private static void SyncSettlementBillNumbers(List<OfflineClient.Bill> bills)
        {
            if (bills == null || bills.Count == 0)
                return;
            foreach (var bill in bills)
            {
                var items = bill.billattributes != null && bill.billattributes.settlement != null
                    ? bill.billattributes.settlement.Billsettlement
                    : null;
                if (items == null)
                    continue;
                for (var i = 0; i < items.Count && i < bills.Count; i++)
                    items[i].billno = bills[i].billno;
            }
        }

        private static void CopyMasterBillNumbers(List<OfflineClient.Bill> local, List<OfflineClient.Bill> saved)
        {
            if (local == null || saved == null || saved.Count == 0)
                return;
            if (saved.Count != local.Count)
            {
                local.Clear();
                local.AddRange(saved);
                return;
            }
            for (var i = 0; i < local.Count; i++)
            {
                local[i].index = saved[i].index;
                local[i].billno = saved[i].billno;
                local[i].createdon = saved[i].createdon;
                local[i].createon = saved[i].createon;
                local[i].billdate = saved[i].billdate;
                if (saved[i].locationid > 0)
                    local[i].locationid = saved[i].locationid;
            }
        }

        internal List<OfflineClient.Bill> SaveBill(List<OfflineClient.Bill> billlist)
        {
            // CLIENT: authoritative save only on MASTER via HTTP (no local .data).
            if (Quanto.MachineConfig.IsClient)
            {
                if (Bills == null)
                    Bills = new List<OfflineClient.Bill>();

                foreach (var currentBill in billlist)
                {
                    StampBillSaveTime(currentBill);
                    currentBill.deviceid = Quanto.MachineConfig.DeviceId;
                    if (CurrentUser != null)
                    {
                        currentBill.createdby = CurrentUser.id;
                        if (CurrentUser.employeeid > 0 && Data.Employee != null &&
                            Data.Employee.Exists(e => e.id == CurrentUser.employeeid))
                        {
                            currentBill.biller = Data.Employee.Find(e => e.id == CurrentUser.employeeid).name;
                        }
                    }
                    if (Data != null)
                    {
                        currentBill.counterid = Data.counterid;
                        currentBill.locationid = Data.locationid;
                    }
                    AssignBillIndexAndNumber(currentBill, true);
                }

                var push = Quanto.ServiceProxy.Instance.UploadBillsToMaster(billlist);
                if (push == null || push.error)
                    throw new InvalidOperationException(
                        push?.errormessage ?? "Master server unavailable");

                var saved = push.bills != null && push.bills.Count > 0 ? push.bills : billlist;
                CopyMasterBillNumbers(billlist, saved);
                foreach (var currentBill in billlist)
                {
                    if (string.IsNullOrWhiteSpace(currentBill.billno))
                        AssignBillIndexAndNumber(currentBill, false);
                }
                SyncSettlementBillNumbers(billlist);
                foreach (var currentBill in billlist)
                {
                    if (Bills.Exists(e => e.companyid == currentBill.companyid &&
                        e.billdate.Date == currentBill.billdate.Date &&
                        string.Equals(e.billno, currentBill.billno, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    Bills.Add(currentBill);
                }
                return billlist;
            }

            lock (Instance)
            {
                EnsureTransactionFilesLoaded();
                foreach (var currentBill in billlist)
                {
                    AssignBillIndexAndNumber(currentBill, true);
                    StampBillSaveTime(currentBill);
                    currentBill.deviceid = Quanto.MachineConfig.DeviceId;
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
                }
                SyncSettlementBillNumbers(billlist);
                Bills.AddRange(billlist);
                PersistBill(false);
                return billlist;
            }
        }
        public string GetBillNo(string format, OfflineClient.Settlement bill)
        {
            if (string.IsNullOrEmpty(format))
                format = "[NO]";
            format = format.Replace("[MM]", bill.settlementon.ToString("MM")).Replace("[DD]",
                bill.settlementon.ToString("dd")).Replace("[YY]", bill.settlementon.ToString("yy"));
            var cmp = "";
            if (Data != null && Data.Company != null && Data.Company.Exists(e => e.id == bill.companyid))
            {
                var company = Data.Company.Find(e => e.id == bill.companyid);
                cmp = company.billprefix ?? "";
            }
            format = format.Replace("[CMP]", cmp ?? "");
            return format.Replace("[CNT]", CounterBillCode()).Replace("[NO]", bill.index.ToString());
        }
        public string GetBillNo(string format, OfflineClient.Bill bill)
        {
            if (string.IsNullOrEmpty(format))
                format = "[NO]";
            format = format.Replace("[MM]", bill.billdate.ToString("MM")).Replace("[DD]",
                bill.billdate.ToString("dd")).Replace("[YY]", bill.billdate.ToString("yy"));
            var cmp = "";
            if (Data != null && Data.Company != null && Data.Company.Exists(e => e.id == bill.companyid))
            {
                var company = Data.Company.Find(e => e.id == bill.companyid);
                cmp = company.billprefix ?? "";
            }
            format = format.Replace("[CMP]", cmp ?? "");
            return format.Replace("[CNT]", CounterBillCode()).Replace("[NO]", bill.index.ToString());
        }

        private string CounterBillCode()
        {
            if (Data == null)
                return "";
            if (!string.IsNullOrEmpty(Data.CounterPrefix))
                return Data.CounterPrefix;
            if (!string.IsNullOrEmpty(Data.CounterCode))
                return Data.CounterCode;
            if (Data.counterid > 0 && Data.Counter != null)
            {
                var counter = Data.Counter.Find(e => e.id == Data.counterid);
                if (counter != null && !string.IsNullOrEmpty(counter.code))
                    return counter.code;
            }
            return "";
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
                if (Quanto.MachineConfig.IsClient)
                {
                    list = list ?? new List<OfflineClient.Customer>();
                    return;
                }
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
            public void ReloadFromBytes(byte[] customerPackage)
            {
                if (customerPackage == null || customerPackage.Length == 0)
                {
                    list = new List<OfflineClient.Customer>();
                    return;
                }
                var customerdata = BufferedRealtimeCompressionEngine.Decompress(customerPackage);
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Customer>>(
                    System.Text.ASCIIEncoding.ASCII.GetString(customerdata))
                    ?? new List<OfflineClient.Customer>();
            }
            public void ClearMemory()
            {
                list = new List<OfflineClient.Customer>();
            }
            public OfflineClient.Customer Get(string no)
            {
                var found = FindLocal(no);
                if (found != null)
                    return found;

                if (!Quanto.MachineConfig.IsClient)
                    return FindInBills(no);

                var remote = Quanto.ServiceProxy.Instance.LookupCustomerFromMaster(no);
                if (remote != null && !remote.error && remote.customer != null)
                {
                    Cache(remote.customer);
                    return remote.customer;
                }

                return FindInBills(no);
            }

            public OfflineClient.Customer FindLocal(string no)
            {
                if (list == null || string.IsNullOrWhiteSpace(no))
                    return null;
                var needle = NormalizeMobile(no);
                if (string.IsNullOrEmpty(needle))
                    return list.Find(e => string.Equals((e.no ?? "").Trim(), no.Trim(), StringComparison.OrdinalIgnoreCase));
                return list.Find(e => NormalizeMobile(e.no) == needle);
            }

            private OfflineClient.Customer FindInBills(string no)
            {
                var needle = NormalizeMobile(no);
                if (string.IsNullOrEmpty(needle) || BillManager.Instance.Bills == null)
                    return null;
                var bill = BillManager.Instance.Bills.FindLast(e =>
                    !string.IsNullOrWhiteSpace(e.customername) &&
                    NormalizeMobile(e.customermobileno) == needle);
                if (bill == null)
                    return null;
                var customer = new OfflineClient.Customer
                {
                    no = needle,
                    name = bill.customername
                };
                Cache(customer);
                return customer;
            }

            private void Cache(OfflineClient.Customer customer)
            {
                if (customer == null || string.IsNullOrWhiteSpace(customer.no))
                    return;
                if (FindLocal(customer.no) != null)
                    return;
                if (list == null)
                    list = new List<OfflineClient.Customer>();
                list.Add(customer);
            }

            private static string NormalizeMobile(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return "";
                var digits = new string(value.Where(char.IsDigit).ToArray());
                if (digits.Length >= 10)
                    return digits.Substring(digits.Length - 10);
                return digits;
            }
            public void Add(string no, string name)
            {
                var existing = FindLocal(no);
                if (existing != null)
                {
                    if (existing.name != name)
                    {
                        existing.name = name;
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
                if (Quanto.MachineConfig.IsClient)
                    return; // customers are MASTER-owned; memory update only for this session
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
                if (Quanto.MachineConfig.IsClient)
                    return;
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
                    return list.Find(e => e.type == type && e.companyid == companyid && e.date == date.Date).no;
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
