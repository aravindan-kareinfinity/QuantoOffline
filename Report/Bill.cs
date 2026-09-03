using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace InfyPOS.Models
{
    public class Bill
    {
        public Bill()
        {
            Billitems = new List<Billitems>();
        }
        public long id { get; set; }
        public long counterid { get; set; }
        public string countername { get; set; }
        public long floorid { get; set; }
        public decimal additionalcharges { get; set; }
        public bool isb2b { get; set; }
        public bool isreturn { get; set; }
        public bool iscancel { get; set; }
        public string floorname { get; set; }
        public long locationid { get; set; }
        public bool isactive { get; set; }
        public bool isinterstatetransfer { get; set; }
        [JsonIgnore]
        public bool issynchronize { get; set; }
        public decimal qty { get; set; }
        public long createdby { get; set; }
        public bool hasemi { get; set; }
        public decimal customerreward { get; set; }
        public bool hasloan
        {

            get
            {
                return emidetail != null && emidetail.loanproviderid > 0;
            }
        }
        public string hypothecation
        {
            get
            {
                return hasloan ? emidetail.loanprovidername : "";
            }
        }

        public string loanreference
        {
            get
            {
                return hasloan ? emidetail.loanreference : "";
            }
        }

        public decimal emiamount
        {
            get
            {
                return hasemi ? emidetail.loanamount : 0;
            }
        }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string emidetail_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(emidetail); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    emidetail = Newtonsoft.Json.JsonConvert.DeserializeObject<EMI>(value);
            }
        }

        public List<BillGroupingItem> groupedbillitems { get; set; }
        public class BillGroupingItem
        {
            public string printingname { get; set; }
            public string colour { get; set; }
            public string design { get; set; }
            public string sizegroup { get; set; }
            public string size { get; set; }
            public int orderid { get; set; }
            public string hsncode { get; set; }
            public decimal qty { get; set; }
            public decimal rate { get; set; }
            public decimal gross { get; set; }
        }

        public EMI emidetail { get; set; }
        public class EMI
        {
            public string schemename { get; set; }
            public string loanprovidername { get; set; }
            public long schemeid { get; set; }
            public long loanproviderid { get; set; }
            public decimal advanceemi { get; set; }
            public int advancemonth { get; set; }
            public decimal billvalue { get; set; }
            public decimal rateofinterest { get; set; }
            public int tenure { get; set; }
            public decimal loanamount { get; set; }
            public decimal emicharges { get; set; }
            public decimal dbdcharge { get; set; }
            public decimal documentcharge { get; set; }
            public decimal marginmoney { get; set; }
            public decimal emicardcharge { get; set; }
            public string loanreference { get; set; }
            public string loanapprovedby { get; set; }
            public string loanreferredby { get; set; }
            public string loanreferrercontactno { get; set; }
        }
        public string biller
        {
            get
            {
                if (billattributes != null)
                    return billattributes.biller;
                return "";
            }
        }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long billmasterid { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public long companyid { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Company company { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public CompanyLocation location { get; set; }
        public BillDeliveryDetails billdeliverydetails { get; set; }
        public Customer customer { get; set; }
        public long customerid { get; set; }
        public decimal tax { get; set; }
        public decimal rounding { get; set; }
        public decimal discount { get; set; }
        public decimal receivable { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal schemediscount { get; set; }
        public decimal schemereward { get; set; }
        public decimal schemediscountcoupen { get; set; }
        public decimal saleamount
        {
            get
            {
                return receivable - tax;

            }
        }
        public string deliveryno
        {
            get
            {
                if (billattributes != null && billattributes.deliveryno != null)
                    return billattributes.deliveryno;
                return "";
            }
        }
        public bool iscredit { get { return billtype == BillTypes.Credit || billtype == BillTypes.Bulk; } }
        public bool isbill { get { return billtype != BillTypes.Cancel; } }
        public bool iscreditbill { get; set; }
        public bool isreturnbill { get { return Billitems.All(e => e.receivable < 0); } }
        public bool hascustomer { get { return customerid != 0 && !string.IsNullOrEmpty(customername); } }
        public bool hasdiscount { get { return totaldiscount != 0; } }
        public bool hasgst { get { return customerid != 0 && !string.IsNullOrEmpty(customergst); } }
        public bool hasdiscountandexchange { get { return totaldiscount != 0 && hasexchange; } }
        public bool hasexchange
        {
            get
            {
                return Billitems.Any(e => e.receivable < 0)
                    && Billitems.Any(e => e.receivable > 0);
            }
        }
        public bool isbulkreturn { get; set; }
        public bool isbulkreturngoods { get; set; }

        public bool hasrewardcoupons
        {
            get
            {
                return billattributes != null && (billattributes.couponamount != 0 || billattributes.rewardamount != 0 || billattributes.manualrewardpoint != 0 || billattributes.manualrewardamount != 0);
            }
        }

        public decimal availablereward
        {
            get
            {
                return billattributes != null && billattributes.settlement != null ? billattributes.settlement.paymentinfo.avlrewardpoint : 0;
            }
        }

        public decimal rewardpoint
        {
            get
            {
                return billattributes != null && billattributes.settlement != null ? billattributes.settlement.paymentinfo.rewardpoint : 0;
            }
        }


        public decimal adjustedcouponamount
        {
            get
            {
                return billattributes != null ? billattributes.couponamount : 0;
            }
        }

        public decimal adjustedrewardamount
        {
            get
            {
                return billattributes != null ? billattributes.rewardamount+ billattributes.manualrewardamount : 0;
            }
        }

        public decimal customerdiscount { get; set; }
        public decimal rewardunit { get; set; }
        public string returnbillno { get; set; }
        public long returnbillid { get; set; }
        public long returnreasonid { get; set; }
        public bool settled { get; set; }
        public decimal totaldiscount { get { return discount + additionaldiscount; } }
        public long salereasonid { get; set; }
        public string areaname { get; set; }

        public long areaid { get; set; }
        public long seasonid { get; set; }
        [JsonIgnore]
        public bool showfixeddisplay { get; set; }
        [JsonIgnore]
        public string fixedbilldisplay { get; set; }
        public string alternatebilldisplay
        {
            get
            {
                if (billdisplay == "DELIVERY CHALLAN") return "APPROVED BILL";
                else if (billdisplay == "(COPY) DELIVERY CHALLAN") return "(COPY) APPROVED BILL";
                else return billdisplay;
            }
        }
        public string billdisplay
        {
            get
            {
                if (DateTime.Now.Date == billdate.Date || DateTime.Now.Date == createdon.Date)
                {
                    if (showfixeddisplay)
                        return fixedbilldisplay;
                    if (billtype == BillTypes.Credit)
                    {
                        if (receivable < 0)
                            return "RETURN CREDIT BILL";
                        else
                            return "CREDIT BILL";
                    }
                    else if (billtype == BillTypes.Bulk)
                        return "DELIVERY CHALLAN";
                    else if (billtype == BillTypes.ReturnBill)
                        return "SALE RETURN";
                    return "SALES INVOICE";
                }

                if (showfixeddisplay)
                    return "(COPY) " + fixedbilldisplay;

                if (billtype == BillTypes.Credit)
                {
                    if (receivable < 0)
                        return "(COPY) RETURN CREDIT BILL";
                    else
                        return "(COPY) CREDIT BILL";
                }
                else if (billtype == BillTypes.Bulk)
                    return "(COPY) DELIVERY CHALLAN";
                else if (billtype == BillTypes.ReturnBill)
                    return "(COPY) SALE RETURN";
                return "(COPY) SALES INVOICE";
            }
        }

        public string numberinword
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(totalreceivable);
            }
        }
        public decimal totalcostprice
        {
            get
            {
                return Billitems.Sum(e => e.salerate > 0 ? e.salerate : 0);
            }
        }

        public decimal totalamount_return
        {
            get
            {
                return Billitems.Sum(e => e.receivable < 0 ? e.receivable : 0);
            }
        }
        public decimal totalamount_exchange
        {
            get
            {
                return Billitems.Sum(e => e.receivable > 0 ? e.receivable : 0);
            }
        }

        public decimal totalamount_returndiscount
        {
            get
            {
                return Billitems.Sum(e => e.discount < 0 ? e.discount : 0);
            }
        }
        public decimal totalamount_exchangediscount
        {
            get
            {
                return Billitems.Sum(e => e.discount > 0 ? e.discount : 0);
            }
        }
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
                return totalamount-discount;
            }
        }
        public BillAttribute billattributes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string billattributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(billattributes); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    billattributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(value);
            }
        }

        public decimal totalreceivable { get { return receivable + rounding + additionalcharges; } }
        public decimal totalqty { get; set; }
        public decimal totalpiece { get; set; }
        public List<Billadditionalcharges> additionalchargeslist { get; set; }
        public List<Billitems> Billitems { get; set; }
        public List<BillTax> BillTax { get; set; }
        public string customername { get; set; }
        public string customergst { get; set; }
        public string phoneno { get; set; }
        public BillTypes billtype { get; set; }

        public bool offline { get; set; }
        public bool HasPromotionApplied { get; set; }
        public bool HasPendingPromotion { get; set; }
        internal void removescheme()
        {
            receivable += schemediscount;
            schemediscount = 0;
            schemediscountcoupen = 0;
            schemereward = 0;
            Billitems.ForEach(e =>
            {
                e.receivable += e.schemediscount;
                e.schemereward = 0;
                e.schemediscountcoupen = 0;
                e.schemediscount = 0;
                e.schemeeligble = false;
                e.schemeid = 0;
                if (e.attributes != null && e.attributes.freeitems != null)
                    e.attributes.freeitems.Clear();
            });
        }

        public Discountqueue.DiscountStatus discountstatus { get; set; }
        public string printdocument { get; set; }
        public enum BillTypes
        {
            Cash = 0,
            Credit = 1,
            Card = 2,
            Redemption = 3,
            InterstateTransfers = 7,
            ReturnBill = 8,
            Cancel = 9,
            Bulk = 10,
            BulkSaleCompleted = 11,
            BulkSaleReturn = 12,
            BulkSaleBilled=13,
            BulkSaleBilledWithReturns=14
        }

        internal void AddItems(List<Billitems> list, bool computetotal)
        {
            //if (computetotal)
            //{
            //    tax = 0;
            //    discount = 0;
            //    receivable = 0;
            //    additionaldiscount = 0;
            //    total = 0;
            //}
            this.BillTax = new List<BillTax>();
            this.Billitems = new List<Billitems>();
            foreach (var item in list)
            {
                //if (item.id == 0)
                //{
                //    if (item.discount > 0)
                //        item.discountpercentage = Math.Round(item.discount / (item.discount + item.price) * 100, 0);
                //    //item.piece = item.sellingmode == 3 ? 1 : item.qty;
                //    item.saleamount = item.qty * item.rate;
                //    item.salerate = Math.Round(item.price + (item.totaldiscountpercentage/item.qty),2);
                //    item.itemtax = Math.Round(item.tax / item.qty, 4);
                //}
                //if (item.taxsplit != null)
                //    item.taxpercentage = item.taxsplit.cgsttaxpercentage + item.taxsplit.sgsttaxpercentage;
                //item.taxpercentage = item.discount / item.qty;
                this.Billitems.Add(item);
                //if (computetotal)
                //{
                //    tax += item.tax;
                //    discount += item.discount;
                //    receivable += item.receivable+item.additionaldiscount;
                //    additionaldiscount += item.additionaldiscount;
                //    total += (item.receivable);
                //}

                if (item.schemediscount != 0 && computetotal)
                {
                    var price = item.price + (item.discount / item.qty);
                    if (item.schemediscount < 0)
                    {
                        item.discount -= (item.schemediscount * -1);
                        item.schemediscount = 0;
                    }

                    item.discount += (item.schemediscount);
                    decimal baserate = price - ((item.discount / item.qty) + item.additionaldiscount);
                    if (isinterstatesale)
                    {
                        var basePrice = (baserate / (100 + (item.taxpercentage)) * 100);
                        var halfTax = Math.Round(basePrice * (item.taxpercentage / 2) / 100, 5);
                        item.itemtax = Math.Round(halfTax * 2, 5);
                    }
                    else
                    {
                        item.itemtax = (baserate / (100 + item.taxpercentage)) * item.taxpercentage;
                    }
                    item.tax = Math.Round(item.itemtax * item.qty, 5);
                    item.rate = Math.Round(baserate - item.itemtax, 0);
                    item.receivable = Math.Round(baserate * item.qty, 2);
                    item.saleamount = item.rate * item.qty;
                }

                if (this.BillTax.Exists(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage))
                {
                    var taxitem = this.BillTax.Find(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage);
                    taxitem.tax += item.tax;
                    taxitem.amount += (item.gross);
                }
                else
                {
                    BillTax bt = new Models.BillTax();
                    bt.taxid = item.taxid;
                    bt.tax = item.tax;
                    bt.amount = item.gross;
                    bt.taxpercentage = item.taxpercentage;
                    this.BillTax.Add(bt);
                }
            }
        }

        public void RecalculateTax()
        {
            this.BillTax = new List<BillTax>();
            foreach (var item in Billitems)
            {
                if (this.BillTax.Exists(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage))
                {
                    var taxitem = this.BillTax.Find(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage);
                    taxitem.tax += item.tax;
                    taxitem.amount += item.gross;
                }
                else
                {
                    BillTax bt = new Models.BillTax();
                    bt.taxid = item.taxid;
                    bt.tax = item.tax;
                    bt.amount = item.gross;
                    bt.taxpercentage = item.taxpercentage;
                    this.BillTax.Add(bt);
                }
            }
            this.taxsplit = new TaxSplit();
            this.taxsplit.totalamount = this.Billitems.Sum(e => e.rate * e.qty);
            if (this.isinterstatesale)
            {
                this.taxsplit.cgsttaxamount = 0;
                this.taxsplit.sgsttaxamount = this.tax;
                this.taxsplit.secondtax = "IGST";
                this.Billitems.ForEach(e =>
                {
                    e.taxsplit = new TaxSplit()
                    {
                        cgsttaxamount = 0,
                        cgsttaxpercentage = 0,
                        secondtax = "IGST",
                        sgsttaxamount = e.tax,
                        sgsttaxpercentage = e.taxpercentage
                    };
                });
            }
            else
            {
                this.taxsplit.cgsttaxamount = Math.Round(this.tax / 2, 5);
                this.taxsplit.sgsttaxamount = Math.Round(this.tax / 2, 5);
                this.taxsplit.secondtax = "SGST";
                this.Billitems.ForEach(e =>
                {
                    e.taxsplit = new TaxSplit()
                    {
                        cgsttaxamount = Math.Round(e.tax / 2, 5),
                        cgsttaxpercentage = e.taxpercentage / 2,
                        secondtax = "SGST",
                        sgsttaxamount = Math.Round(e.tax / 2, 5),
                        sgsttaxpercentage = e.taxpercentage / 2
                    };
                });
            }
            this.BillTax.ForEach(e =>
            {
                e.secondtax = this.taxsplit.secondtax;
            });
        }

        public string discountrequestreason { get; set; }
        public bool isinterstatesale { get; set; }
        public TaxSplit taxsplit { get; set; }
        public decimal total
        {
            get
            {
                return receivable + rounding+additionalcharges;
            }
        }
        [JsonIgnore]
        public bool DontChangeStock { get;  set; }
        [JsonIgnore]
        public bool updatestock { get;  set; }
        [JsonIgnore]
        public bool unsettledcancelled { get; set; }
        [JsonIgnore]
        public string greetingtext { get; set; }
        [JsonIgnore]
        public bool hasgreetingtext
        {
            get
            {
                return !string.IsNullOrEmpty(greetingtext);
            }
        }
        [JsonIgnore]
        public bool isfirst { get;  set; }
        [JsonIgnore]
        public bool islast { get; set; }
        [JsonIgnore]
        public Servicecenters servicecenter { get;  set; }
        public bool hasservicecenter
        {
            get
            {
                return servicecenter != null;
            }
        }
        [JsonIgnore]
        public bool backwardserialno { get;  set; }
        public string errormessage { get;  set; }
        public bool directpdf { get;  set; }
        [JsonIgnore]
        public string approvedby { get;  set; }
        [JsonIgnore]
        public bool hasapprovedby {
            get
            {
                return !string.IsNullOrEmpty(approvedby);
            }
        }
    }

    public class BillTax
    {
        public long taxid { get; set; }
        public string taxname { get; set; }
        public decimal tax { get; set; }
        public decimal taxpart
        {
            get
            {
                return Math.Round(tax / 2, 2);
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
        public decimal amount { get; set; }
        public decimal taxpercentage { get; set; }
        public string secondtax { get; set; }
    }
    public class BillCreteria : Bill
    {
        public bool enableprintdocument { get; set; }
        public string barcode { get; set; }

        public List<long> ids { get; set; }
        public bool forsettlement { get; set; }
        public bool forsalesreturn { get; set; }
        public bool isMasterBill { get; set; }
        public bool removecancelled { get; set; }
    }

    public class BillAttribute
    {
        public long rewardamount { get; set; }
        public string couponcode { get; set; }
        public long couponid { get; set; }
        public decimal discountamount { get; set; }
        public decimal couponamount { get; set; }
        public long manualrewardpoint { get; set; }
        public long manualrewardamount { get; set; }
        public long newreward { get; set; }
        public long earnedreward { get; set; }
        public long remainingreward { get; set; }
        public long spentreward { get; set; }
        public string mode { get; set; }
        public string deliveryno { get; set; }
        public Billsettlementmaster settlement { get; set; }
        public string biller { get; set; }
        public string floorname { get; set; }
        public string countername { get; set; }
        public long floorid { get; set; }
        public long counterid { get; set; }
        public bool showdiscount { get; set; }
        public long approvedby { get; set; }
        public string remarks { get; set; }
        public bool removed { get;  set; }
    }

    public class SalesReport : BaseEntity
    {

        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public long itemid { get; set; }
        public decimal buyingprice { get; set; }
        public decimal price { get; set; }
        public decimal iqty { get; set; }
        public decimal dqty { get; set; }
        public decimal oqty { get; set; }
        public decimal qty { get; set; }
        public long typeid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long sizeid { get; set; }
        public string barcode { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal amount { get; set; }
        public decimal tax { get; set; }
        public decimal total { get; set; }
        public decimal receivable { get; set; }
        public decimal discount { get; set; }
    }

    public class BillCancel : BaseEntity
    {
        public List<long> ids { get; set; }

        public long locationid { get; set; }
        public bool disablestockreverse { get; set; }
        public long counterid { get; set; }
        public bool isbulkreturn { get; set; }
        public bool isbulksale { get; set; }
    }

    public class SalesReportCreteria : SalesReport
    {
        public bool barcod { get; set; }
        public bool pattern { get; set; }
        public bool size { get; set; }
        public bool product { get; set; }
        public bool color { get; set; }
        public bool brand { get; set; }
        public bool style { get; set; }
        public bool material { get; set; }
        public bool supplier { get; set; }
        public bool invoicedate { get; set; }
        public bool netamount { get; set; }
        public bool availqty { get; set; }

    }

    public class TaxSplit
    {
        public decimal taxpercentage { get
            {
                return cgsttaxpercentage + sgsttaxpercentage;
            }
        }
        public decimal taxamount { get
            {
                return cgsttaxamount + sgsttaxamount;
            }
        }
        public string name { get; set; }
        public decimal totalamount { get; set; }
        public decimal cgsttaxpercentage { get; set; }
        public decimal cgsttaxamount { get; set; }
        public string secondtax { get; set; }
        public decimal sgsttaxpercentage { get; set; }
        public decimal sgsttaxamount { get; set; }
        [JsonIgnore]
        public decimal igsttaxamount { get; set; }
        [JsonIgnore]
        public bool isinterstatesale { get; set; }
    }

}
