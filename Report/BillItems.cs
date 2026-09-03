
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billitems 
    {
        public string salesmancode { get; set; }
        public int orderid { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string barcode { get; set; }
        public long stockid { get; set; }
        public long productid { get; set; }
        public string printingname { get; set; }
        public string description
        {
            get
            {
                return string.Concat(barcode.Trim(), "/", printingname.Trim());
            }
        }
        public decimal incentive { get; set; }
        [System.ComponentModel.Description("tax = qty * itemtax")]
        public decimal tax { get; set; }

        [System.ComponentModel.Description("price-rate")]
        public decimal itemtax { get; set; }

        [System.ComponentModel.Description("price/100+tax*100")]
        public decimal rate { get; set; }

        [System.ComponentModel.Description("Price*Qty, All item")]
        public decimal receivable { get; set; }

        [System.ComponentModel.Description("Price = salerate - discount")]
        public decimal price { get; set; }

        [System.ComponentModel.Description("Discount = Price - , Per item , 200 - 20%")]
        public decimal discount { get; set; }

        [System.ComponentModel.Description("fixed at warehouse including tax , Per item - 1000")]
        public decimal salerate { get; set; }

        [System.ComponentModel.Description("qty*rate")]
        public decimal saleamount { get; set; }

        public bool isexchange { get; set; }
        public bool runtimeprice { get; set; }

        [JsonIgnore]
        public decimal? meteramount
        {
            get;set;
        }
        public decimal amount
        {
            get
            {
                if (meteramount.HasValue)
                    return meteramount.Value;
                return price * qty;
            }
        }

        [System.ComponentModel.Description("additionaldiscount = Rate % AddtionalDiscountRate, Per piece")]
        public decimal additionaldiscount { get; set; }
        public decimal schemediscount { get; set; }
        public decimal schemereward { get; set; }
        public decimal schemediscountcoupen { get; set; }
        public bool schemeeligble { get; set; }
        public long schemeid { get; set; }
        public string scheme { get; set; }
        public bool hasscheme
        {
            get
            {
                return schemeid > 0;
            }
        }
        [System.ComponentModel.Description("Tax Percentage")]
        public decimal taxpercentage { get; set; }
        [System.ComponentModel.Description("Discount Percentage")]
        public decimal discountpercentage { get; set; }
        [System.ComponentModel.Description("Additional Discount Percentage")]
        public decimal additionaldiscountpercentage { get; set; }
        public long salesmanid { get; set; }
        public string salesmanname { get; set; }
        public long taxid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long billid { get; set; }
        public long returnbillitemid { get; set; }
        public decimal qty { get; set; }
        [System.ComponentModel.Description("stock adjustment ")]
        public decimal journalqty { get; set; }
        [System.ComponentModel.Description("no of pieces")]
        public decimal piece { get; set; }

        [JsonIgnore]
        public string qtyinpiece
        {
            get;set;
        }
        [JsonIgnore]
        public string qtyinmeter
        {
            get; set;
        }
        [JsonIgnore]
        public string qtytometer
        {
            get
            {
                if (sellingmode == 3)
                    return qty.ToString("N2");
                return "";
            }
        }
        [JsonIgnore]
        public decimal gross
        {
            get
            {
                return receivable - tax;
            }

        }
		
		 [JsonIgnore]
        public decimal b2bgross
        {
            get
            {
                return (attributes != null && attributes.gross>0) ? attributes.gross : (salerate + additionaldiscount);
            }
        }

        [JsonIgnore]
        public string qtytopiece
        {
            get
            {
                if (sellingmode != 3)
                    return qty.ToString("N0");
                return "";
            }
        }

        [JsonIgnore]
        public decimal costprice
        {
            get
            {
                return salerate * qty;
            }
        }

        [JsonIgnore]
        public decimal itemtaxable
        {
            get
            {
                return rate - itemtax;
            }
        }
		
        public bool haslastbit
        {
            get
            {
                return journalqty != 0;
            }
        }
        public decimal lastbitqty
        {
            get
            {
                if(journalqty>0)
                    return journalqty - qty;
                return 0;
            }
        }
        //[{ 'id': 1, 'name': 'Piece' }, { 'id': 2, 'name': 'Pack' }, { 'id': 3, 'name': 'Cut' }];
        public int sellingmode { get; set; }
        public long salablegoodsid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public TaxSplit taxsplit { get; set; }

        public decimal schemediscountpercentage
        {
            get
            {
                if (schemediscount <= 0) return 0;
                return Math.Round(schemediscount / qty / price * 100, 2);
            }
        }
        public decimal totaldiscountpercentage
        {
            get
            {
                return discountpercentage + additionaldiscountpercentage +schemediscountpercentage;
            }
        }
        public bool hasdiscount { get { return totaldiscount != 0; } }

        public decimal totaldiscount
        {
            get
            {
                return Math.Round(discount + (additionaldiscount*qty),2);
            }
        }
        public decimal netprice
        {
            get
            {
                return Math.Round(salerate - ((discount / qty) + additionaldiscount));
            }
        }
        public decimal grossprice
        {
            get
            {
                return Math.Round(salerate - (discount/qty));
            }
        }
        public bool hasserial
        {
            get
            {
                return attributes != null && !string.IsNullOrEmpty(attributes.serialno);
            }
        }


        public string hsncode { get; set; }

    
        public BillItemAttributes attributes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); }
            set {
                if(!string.IsNullOrEmpty(value) && value != "null")
                attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillItemAttributes>(value);
            }
        }

        internal decimal getactualreceivable()
        {
            return (price * qty);
        }
    }
    public class BillitemsCreteria : Billitems
    {
        public List<long> ids { get; set; }
    }

    public class BillItemAttributes
    {
        public class FreeItems
        {
            public string name { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public decimal qty { get; set; }
            public decimal amount { get; set; }
        }
        public string serialno { get; set; }
        public decimal customerdiscount { get; set; }
        public decimal margin { get; set; }
        public decimal cost { get; set; }
        public decimal gross { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        public string designid { get; set; }
        public string sizegroup { get; set; }
        public string colourname { get; set; }
        public string groupkey { get; set; }
        public string groupname { get; set; }
        public List<itemqty> so { get; set; }
        public class itemqty
        {
            public long id { get; set; }
            public decimal qty { get; set; }
        }
        public List<FreeItems> freeitems { get; set; }
    }
    public class Billitemdetails : Billitems
    {
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public string location { get; set; }
        public string returnimageid { get; set; }
        public string stockimageid { get; set; }
    }
}
