using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace InfyPOS.Processors
{
    public class OfflineClient
    {
        public class ImageData
        {
            public string key { get; set; }
            public byte[] data { get; set; }
        }
        public class MasterData
        {
            public bool autobarcode { get; set; }
            public long organizationid { get; set; }
            public List<ImageData> imageDatas { get; set; }
            public long locationid { get; set; }
            public DateTime lastSyncOn { get; set; }
            public byte[] Tax { get; set; }
            public byte[] Employee { get; set; }
            public byte[] Products { get; set; }
            public byte[] Location { get; set; }
            public byte[] Company { get; set; }
            public byte[] Stock { get; set; }
            public string LocationName { get; set; }
            public byte[] Counter { get; set; }
            public byte[] Users { get; set; }
            public List<PrintCofig> PrintConfig { get; set; }
            public List<Promotion> Promotions { get; set; }
            public byte[] AutoNumber { get;  set; }
        }

        public class Promotion
        {
            public Models.Promotions scheme { get; set; }
            public List<string> barcode { get; set; }
        }

        public class WindowsOfflineRequest
        {
            public string systemkey { get; set; }
            public string locationcode { get; set; }
            public bool autobarcode { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public DateTime datafrom { get; set; }
            public bool zerostock { get; set; }
            public DateTime zerostockfrom { get; set; }
            public DateTime datato { get; set; }

            public string datatype { get; set; }

            public byte[] data { get; set; }

            public DateTime uploadon { get; set; }

            public string machinename { get; set; }

            public string localipaddress { get; set; }

            public string internetipaddress { get; set; }

            public long organizationid { get; set; }
            public long userid { get; set; }

            public string orgainzationcode { get; set; }
            public long locationid { get; set; }

        }

        public class ClientMasterPackage
        {
            public bool error { get; set; }
            public string errormessage { get; set; }
            public bool completed { get; set; }
            public DateTime lastSyncOn { get; set; }
            public byte[] master { get; set; }
            public byte[] stock { get; set; }
            public byte[] customer { get; set; }
        }

        /// <summary>CLIENT → MASTER bill write-back over HTTP (not shared files).</summary>
        public class ClientBillUpload
        {
            public string deviceId { get; set; }
            public string machineName { get; set; }
            public List<Bill> bills { get; set; }
        }

        public class ClientSettlementUpload
        {
            public string deviceId { get; set; }
            public string machineName { get; set; }
            public List<Settlement> settlements { get; set; }
        }

        public class ClientUploadResult
        {
            public bool error { get; set; }
            public string errormessage { get; set; }
            public bool completed { get; set; }
            public int accepted { get; set; }
            public int skipped { get; set; }
            /// <summary>Bills as saved on MASTER (authoritative bill numbers).</summary>
            public List<Bill> bills { get; set; }
            public List<Settlement> settlements { get; set; }
        }

        public class ClientStockResult
        {
            public bool error { get; set; }
            public string errormessage { get; set; }
            public bool found { get; set; }
            public Stock stock { get; set; }
        }

        public class ClientCustomerResult
        {
            public bool error { get; set; }
            public string errormessage { get; set; }
            public bool found { get; set; }
            public Customer customer { get; set; }
        }

        public class WindowsOfflineResponse
        {
            DateTime createdon { get; set; }
            public WindowsOfflineResponse()
            {
                createdon = DateTime.Now;
                lockobject = new object();
            }
            public bool hasexpired()
            {
                return DateTime.Now.Subtract(createdon).TotalMinutes > 60;
            }
            public string key { get; set; }
            public string status { get; set; }
            public int progress { get; set; }
            public bool error { get; set; }
            public string errormessage { get; set; }
            public bool completed { get; set; }
            public int noofrecords { get; set; }
            public byte[] data { get; set; }
            public DateTime completedon { get; set; }
            [JsonIgnore]
            public object lockobject { get; set; }
        }

        public class PrintCofig
        {
            public string key { get; set; }
            public long locationid { get; set; }
            public long companyid { get; set; }
            public string format { get; set; }
            public string company { get; set; }
            public override string ToString()
            {
                return key+" - "+company;
            }
        }

        public class Bill
        {
            public List<Bill> CreateBills(List<Tax> taxList)
            {
                List<Bill> bills = new List<Bill>();
                foreach (var grp in Billitems.GroupBy(e => e.companyid))
                {
                    var bill = new Bill() { Billitems = new List<BillItems>() };
                    bill.billdate = this.billdate;
                    bill.createdon = this.createdon;
                    bill.createon = this.createon != DateTime.MinValue ? this.createon : this.createdon;
                    bill.deviceid = this.deviceid;
                    bill.companyid = grp.Key;
                    bill.customername = this.customername;
                    bill.customermobileno = this.customermobileno;
                    bill.creditbill = this.creditbill;
                    bill.billattributes = this.billattributes;
                    bill.counterid = this.counterid;
                    bill.createdby = this.createdby;
                    bill.locationid = this.locationid;
                    if (this.addiscountpercentage > 0 || grp.Sum(e => e.schemediscount) > 0)
                    {
                        foreach (var item in grp)
                        {
                            item.additionaldiscountpercentage = this.addiscountpercentage;
                            item.additionaldiscount = Math.Round(item.salerate * this.addiscountpercentage / 100, 2);
                            var baseprice = item.price - item.additionaldiscount;
                            if (item.schemediscount > 0)
                                baseprice -= (item.schemediscount / item.qty);
                            var tp = taxList.Find(e => e.id == item.taxid).FindTaxPercentage(baseprice);
                            var taxperitem = Math.Round((baseprice) / (100 + tp) * tp, 4);

                            item.taxperitem = taxperitem;
                            item.totaltax = Math.Round(taxperitem * item.qty, 4);
                            item.receivable = baseprice * item.qty;
                        }
                    }
                    bill.Billitems.AddRange(grp.ToList());
                    bill.gross = grp.Sum(e => e.receivable);
                    bill.discount = grp.Sum(e => e.stockdiscount * e.qty);
                    bill.additionaldiscount = grp.Sum(e => e.additionaldiscount * e.qty);
                    bill.schemediscount = grp.Sum(e => e.schemediscount );
                    bill.tax = grp.Sum(e => e.totaltax);
                    bill.total = Math.Round(bill.gross);
                    bill.rounding = bill.total - bill.gross;
                    bills.Add(bill.Printable());
                }
                return bills;
            }
            public Bill Printable()
            {
                BillTax = new List<TaxItem>();
                foreach (var item in Billitems.GroupBy(e => e.taxpercentage))
                {
                    BillTax.Add(new TaxItem()
                    {
                        amount = item.Sum(e => e.gross),
                        tax = item.Sum(e => e.totaltax),
                        secondtax = "SGST",
                        taxpercentage = item.Key
                    });
                }
                for (var i = 0; i < Billitems.Count; i++)
                {
                    Billitems[i].orderid = i + 1;
                }
                return this;
            }
            public DateTime billdate { get; set; }
            public DateTime createon { get; set; }
            public long counterid { get; set; }
            public long index { get; set; }
            public long createdby { get; set; }
            public string billno { get; set; }
            /// <summary>Machine that created the bill (MASTER/CLIENT DeviceId). Used for per-machine numbering.</summary>
            public string deviceid { get; set; }
            public long organizationid { get; set; }
            public long locationid { get; set; }
            public decimal gross { get; set; }
            public decimal discount { get; set; }
            public decimal total { get; set; }

            public decimal rounding { get; private set; }
            public long companyid { get; set; }
            public List<BillItems> Billitems { get; set; }
            [JsonIgnore]
            public bool addiscountaspercentage { get; set; }
            [JsonIgnore]
            public bool addiscountasvalue { get; set; }
            [JsonIgnore]
            public decimal addiscountpercentage { get; set; }
            [JsonIgnore]
            public decimal additionaldiscount { get; set; }
            
            [JsonIgnore]
            public bool hasdiscount
            {
                get
                {
                    return totaldiscount > 0;
                }
            }
            [JsonIgnore]
            public string billnoandamount
            {
                get
                {
                    return string.Format("{0}#{1}#{2}", billno, total.ToString("N0"),companyid);
                }
            }
            [JsonIgnore]
            public bool isfirst { get; set; }
            [JsonIgnore]
            public bool hascustomer { get; set; }
            [JsonIgnore]
            public bool hasgst { get; set; }
            [JsonIgnore]
            public decimal totalpiece
            {
                get
                {
                    return Billitems.Sum(e => e.sellingmode == 3 ? 1 : e.qty);
                }
            }
            [JsonIgnore]
            public decimal totalqty
            {
                get
                {
                    return Billitems.Sum(e => e.qty);
                }
            }
            public DateTime createdon { get; set; }
            [JsonIgnore]
            public decimal totaldiscount
            {
                get
                {
                    return discount + additionaldiscount + schemediscount;
                }
            }
            [JsonIgnore]
            public List<TaxItem> BillTax { get; set; }
            public TaxSplit taxsplit
            {
                get
                {
                    var totaltax = (Billitems == null || Billitems.Count == 0) ? 0 : Billitems.Sum(e => e.totaltax);
                    var totalreceivable = (Billitems == null || Billitems.Count == 0) ? 0 : Billitems.Sum(e => e.receivable);
                    return new TaxSplit()
                    {
                        totalamount = totalreceivable-totaltax,
                        cgsttaxamount = totaltax / 2,
                        sgsttaxamount = totaltax / 2
                    };
                }
            }

            [JsonIgnore]
            public string billdisplay
            {
                get
                {
                    if (showfixeddisplay)
                        return fixedbilldisplay;
                    return "(COPY) SALES INVOICE";
                }
            }
            [JsonIgnore]
            public string numberinword
            {
                get
                {
                    return WebAPI.Data.Numberconvertor.rupees(totalamount);
                }
            }

            [JsonIgnore]
            public decimal totalcostprice
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.salerate > 0 ? e.salerate : 0);
                }
            }
            [JsonIgnore]
            public decimal totalamount_return
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.receivable < 0 ? e.receivable : 0);
                }
            }
            [JsonIgnore]
            public decimal totalamount_exchange
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.receivable > 0 ? e.receivable : 0);
                }
            }
            [JsonIgnore]
            public decimal receivable
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.receivable > 0 ? e.receivable : 0);
                }
            }
            [JsonIgnore]
            public decimal totalamount_returndiscount
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.discount < 0 ? e.discount * e.qty : 0);
                }
            }
            [JsonIgnore]
            public decimal totalamount_exchangediscount
            {
                get
                {
                    if (Billitems == null) return 0;
                    return Billitems.Sum(e => e.discount > 0 ? e.discount*e.qty : 0);
                }
            }
            [JsonIgnore]
            public decimal totalamount
            {
                get
                {
                    return receivable + totaldiscount;
                }
            }
            public decimal grossamount
            {
                get
                {
                    return totalamount - discount;
                }
            }

            public decimal schemediscount { get; set; }
            public decimal tax { get; set; }
            [JsonIgnore]
            public bool showfixeddisplay { get; set; }
            [JsonIgnore]
            public string fixedbilldisplay { get; set; }
            [JsonIgnore]
            public string biller { get; set; }

            public class TaxSplit
            {
                public decimal totalamount { get; set; }
                public decimal cgsttaxamount { get; set; }
                public decimal sgsttaxamount { get; set; }
            }

            public class TaxItem
            {
                public string secondtax { get; set; }
                public decimal taxpercentage { get; set; }
                public decimal amount { get; set; }
                public decimal tax { get; set; }
                public decimal taxpart
                {
                    get
                    {
                        return tax / 2;
                    }
                }
                public decimal cgsttaxamount
                {
                    get
                    {
                        if (secondtax == "SGST")
                            return taxpart;
                        return 0;
                    }
                }
                public decimal sgsttaxamount
                {
                    get
                    {
                        if (secondtax == "SGST")
                            return taxpart;
                        return tax;
                    }
                }

                public decimal cgsttax
                {
                    get
                    {
                        if (secondtax == "SGST")
                            return taxpart;
                        return 0;
                    }
                }
                public decimal sgsttax
                {
                    get
                    {
                        if (secondtax == "SGST")
                            return taxpart;
                        return 0;
                    }
                }

                public decimal igsttax
                {
                    get
                    {
                        if (secondtax == "SGST")
                            return 0;
                        return tax;
                    }
                }
            }
            [JsonIgnore]
            public Billattributes billattributes { get;set; }
            public string customername { get;  set; }
            public string customermobileno { get; set; }
            public bool creditbill { get;  set; }

            internal void removescheme()
            {
                schemediscount = 0;
                if (Billitems == null) return;
                Billitems.ForEach(e =>
                {
                    e.receivable += e.schemediscount;
                    e.schemediscount = 0;
                    e.schemeeligble = false;
                    e.schemeid = 0;
                });
            }
        }

        public class Billattributes
        {
            public Settlement settlement { get; set; }
            public string biller { get; set; }
            public string countername { get; set; }
        }

        public class BillItems
        {
            public string hsncode { get; set; }
            public long companyid { get; set; }
            public string barcode { get; set; }
            public long productid { get; set; }
            public long taxid { get; set; }
            public string printingname { get; set; }
            public decimal qty { get; set; }
            public decimal price { get; set; }
            [JsonIgnore]
            public decimal salerate
            {
                get
                {
                    return price + stockdiscount;
                }
            }
            public decimal rate
            {
                get
                {
                    return price - (additionaldiscount + (schemediscount > 0 && qty > 0 ? (schemediscount / qty) : 0));
                }
            }
            [JsonIgnore]
            public bool hasdiscount { get { return totaldiscount != 0; } }
            public decimal additionaldiscount { get; set; }
            public decimal discount { get; set; }
            [JsonIgnore]
            public decimal stockdiscount { get; set; }
            public decimal rowdiscount
            {
                get
                {
                    return (discount>0?(discount):0) + additionaldiscount + (schemediscount>0 && qty>0 ? (schemediscount / qty) : 0);
                }
            }
            public decimal receivable { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal taxperitem { get; set; }
            public decimal totaltax { get; set; }
            public long salesmanid { get; set; }
            public string salesmancode { get; set; }
            public int sellingmode { get; set; }
            [JsonIgnore]
            public decimal gross
            {
                get
                {
                    return receivable - totaltax;
                }
            }
            [JsonIgnore]
            public int orderid { get; set; }
            [JsonIgnore]
            public string qtyinmeter
            {
                get
                {
                    return sellingmode == 3 ? qty.ToString("N2") : "";
                }
            }
            [JsonIgnore]
            public string qtyinpiece
            {
                get
                {
                    return sellingmode == 3 ? "1" : qty.ToString("N2");
                }
            }
            [JsonIgnore]
            public decimal totaldiscountpercentage
            {
                get
                {
                    return discountpercentage + additionaldiscountpercentage + schemediscountpercentage;
                }
            }
            [JsonIgnore]
            public decimal schemediscountpercentage
            {
                get
                {
                    if (schemediscount <= 0) return 0;
                    return Math.Round(schemediscount / qty / price * 100, 2);
                }
            }

            public decimal discountpercentage
            {
                get
                {
                    return Math.Round((discount / price) * 100, 2);
                }
            }
            public bool schemeeligble { get; set; }
            public long schemeid { get; set; }
            public decimal schemediscount { get; set; }
            public decimal mrp { get; set; }
            public decimal additionaldiscountpercentage { get; set; }
            [JsonIgnore]
            public decimal totaldiscount
            {
                get
                {

                    return (discount + additionaldiscount) * qty;
                }
            }

            internal decimal getactualreceivable()
            {
                return (price * qty);
            }
        }

        public class SettlementStatus
        {
            public decimal cash_paid { get; set; }
            public decimal card_paid { get; set; }
            public decimal discount_paid { get; set; }
            public decimal credit_paid { get; set; }
            public decimal adjustment_paid { get; set; }
            public decimal voucher_paid { get; set; }
            public decimal cash_return { get; set; }
            public decimal total
            {
                get
                {
                    return cash_paid + card_paid + credit_paid - cash_return;
                }
            }
            public long billcount { get; set; }
        }
        public class Settlement
        {
            public Settlement()
            {
                Billsettlement = new List<InfyPOS.Processors.OfflineClient.Settlement.SettlementItem>();
                paymentinfo = new InfyPOS.Processors.OfflineClient.Settlement.Paymentinfo();
            }
            public long locationid { get; set; }
            public long companyid { get; set; }
            public long createdby { get; set; }
            public long index { get; set; }
            public string cashiername { get; set; }
            public string countername { get; set; }
            public long cahserid { get; set; }
            public long counterid { get; set; }
            public DateTime settlementon { get; set; }
            public DateTime createdon { get; set; }
            public string deviceid { get; set; }
            public string code { get; set; }
            public decimal receivable { get; set; }
            public string reason { get; set; }
            public decimal balance
            {
                get
                {
                    return receivable - 
                        (paymentinfo.cash_paid + paymentinfo.credit_paid + paymentinfo.discount_paid +
                        paymentinfo.voucher_paid + paymentinfo.adjustment_paid + paymentinfo.card_paid);
                }
            }


            [JsonIgnore]
            public string billnos
            {
                get
                {
                    return string.Join(",",Billsettlement.ConvertAll(e=>e.billno));
                }
                set
                {
                    
                }
            }
            [JsonIgnore]
            public decimal cash
            {
                get
                {
                    return paymentinfo.cash_paid;
                }
                set
                {
                    paymentinfo.cash_paid = value;
                }
            }
            [JsonIgnore]
            public decimal card
            {
                get
                {
                    return paymentinfo.card_paid;
                }
                set
                {
                    paymentinfo.card_paid = value;
                }
            }
            [JsonIgnore]
            public decimal credit
            {
                get
                {
                    return paymentinfo.credit_paid;
                }
                set
                {

                }
            }
            [JsonIgnore]
            public decimal discount
            {
                get
                {
                    return paymentinfo.discount_paid;
                }
                set
                {

                }
            }
            [JsonIgnore]
            public decimal other
            {
                get
                {
                    return paymentinfo.voucher_paid + paymentinfo.adjustment_paid;
                }
                set
                {

                }
            }
            public Paymentinfo paymentinfo { get; set; }
            public class Paymentinfo
            {
                public bool HasCash
                {
                    get { return cash_paid > 0; }
                }
                public bool HasCard
                {
                    get { return card_paid > 0; }
                }

                public bool HasDiscount
                {
                    get { return discount_paid > 0; }
                }
                public bool HasCredit
                {
                    get { return credit_paid > 0; }
                }
                public bool HasAdjustment
                {
                    get { return adjustment_paid > 0; }
                }
                public bool HasVoucher
                {
                    get { return voucher_paid > 0; }
                }
                public decimal cash_paid { get; set; }
                public decimal card_paid { get; set; }
                public decimal discount_paid { get; set; }
                public decimal credit_paid { get; set; }
                public decimal adjustment_paid { get; set; }
                public decimal voucher_paid { get; set; }
                public decimal cash_return { get; set; }
                public string discount_approvedbyname { get; set; }
                public long discount_approvedby { get; set; }
                public string credit_approvedbyname { get; set; }
                public long credit_approvedby { get; set; }

                public decimal extra_paid
                {
                    get
                    {
                        return credit_paid + adjustment_paid + discount_paid+voucher_paid;
                    }
                }

            }
            public List<SettlementItem> Billsettlement { get; set; }
            public class SettlementItem
            {
                public long companyid { get; set; }
                public string billno { get; set; }
                public decimal receivable { get; set; }
            }
        }

        public class AutoNumber
        {
            public long floorid { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
            public string floor { get; set; }
            public string company { get; set; }
            public string prefix { get; set; }
            public string display
            {
                get
                {
                    return floorid > 0 ? floor : company;
                }
            }
        }

        public class Stock
        {
            public string hsncode { get; set; }
            public string serailno { get; set; }
            public string barcode { get; set; }
            public long productid { get; set; }
            public long companyid { get; set; }
            [JsonIgnore]
            public string productname { get; set; }
            public Product product { get; set; }
            public Tax tax { get; set; }
            public decimal price { get; set; }
            public decimal discount { get; set; }
            public decimal mrp { get; set; }
            [JsonIgnore]
            public decimal rate
            {
                get
                {
                    return price - discount;
                }
            }
        }

        public class Tax
        {
            public override string ToString()
            {
                return name;
            }
            public long id { get; set; }
            public string name { get; set; }
            public decimal taxpercentage { get; set; }
            [JsonIgnore]
            [JsonProperty(Required = Required.Default)]
            public string taxsplit_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(taxsplit); } set { taxsplit = Newtonsoft.Json.JsonConvert.DeserializeObject<TaxSplit>(value); } }
            public TaxSplit taxsplit { get; set; }

            public decimal FindTaxPercentage(decimal amount)
            {
                if (taxsplit != null && taxsplit.items != null && taxsplit.items.Count > 0)
                    return taxsplit.Find(amount).taxpercent;

                return taxpercentage;
            }
            public class TaxSplit
            {
                public TaxSplit()
                {
                    items = new List<Item>();
                }
                public List<Item> items { get; set; }
                public Item Find(decimal amount)
                {
                    return Find(false, amount);
                }
                public Item Find(bool incost, decimal amount)
                {
                    if (items.Count == 1) return items[0];
                    int index = items.FindIndex(e => e.incost == incost && e.fromamount <= amount && e.toamount >= amount);
                    if (index >= 0)
                        return items[index];
                    index = items.FindIndex(e => e.fromamount <= amount && e.toamount >= amount);
                    if (index >= 0)
                        return items[index];
                    if (items.Count > 0) return items[0];
                    return new Item();
                }
                public class Item
                {
                    public bool incost { get; set; }
                    public long sourceid { get; set; }
                    public decimal fromamount { get; set; }
                    public decimal toamount { get; set; }
                    public decimal cgst { get; set; }
                    public decimal sgst { get; set; }
                    public decimal igst { get; set; }
                    public decimal taxpercent
                    {
                        get
                        {
                            return cgst + cgst;
                        }
                    }
                }
            }
        }

        public class Employee
        {
            public override string ToString()
            {
                return name;
            }
            public string name { get; set; }
            public string code { get; set; }
            public long id { get; set; }
        }
        public class Product
        {
            public override string ToString()
            {
                return name;
            }
            public bool iscut { get; set; }
            public string name { get; set; }
            public long id { get; set; }
            public long salestaxid { get; set; }
            public string code { get; set; }
            public long cmpid { get; set; }
        }

        public class User
        {
            public string username { get; set; }
            public string password { get; set; }
            public long id { get; set; }
            public string employeename { get; set; }
            public long employeeid { get; set; }
            public decimal alloweddiscount { get; set; }
            [JsonIgnore]
            public string permissions_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(permissions); } set { permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Permission>>(value); } }
            public List<Permission> permissions { get; set; }
            public class Permission
            {
                public string code { get; set; }
                public string HasPermission { get; set; }
            }
        }

        public class Counter
        {
            public override string ToString()
            {
                return name;
            }
            public string name { get; set; }
            public long id { get; set; }
            public string code { get; set; }
            public long floorid { get; set; }
            public long locationid { get; set; }
        }

        public class Location
        {
            public override string ToString()
            {
                return name;
            }
            public string name { get; set; }
            public long id { get; set; }
            public string code { get; set; }
            public string address { get; set; }
            public bool autosettlement { get; set; }
            public bool iswarehouse { get; set; }
        }

        public class DeletedInfo
        {
            public string username { get; set; }
            public DateTime deletedon { get; set; }
            public int settlementcount { get; set; }
            public int billcount { get; set; }
            public List<OfflineClient.Settlement> Settlements { get; set; }
            public List<OfflineClient.Bill> Bills { get; set; }
        }
        public class Company
        {
            public override string ToString()
            {
                return name;
            }
            public string name { get; set; }
            public long id { get; set; }
            public string code { get; set; }
            public string billprefix { get; set; }
            public string logo { get; set; }
            public string address { get; set; }
            public long billno { get; set; }
            public long settlementno { get; set; }
        }

        public class Customer
        {
            public string name { get; set; }
            public long id { get; set; }
            public string no { get; set; }
            public string mobileno { get { return no; } set { if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(no)) no = value; } }
            public string mobile { get { return no; } set { if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(no)) no = value; } }
            public string phone { get { return no; } set { if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(no)) no = value; } }
        }
    }
}