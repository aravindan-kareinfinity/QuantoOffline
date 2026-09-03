using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InfyPOS.Models
{
    public class Promotions
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public string name { get; set; }
        public int mode { get; set; }
        public int applicablemode { get; set; }
        public bool enabled { get; set; } = true;
        public List<long> schemelocations { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string schemelocations_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(schemelocations); } set { schemelocations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(value); } }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string rules_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(rules); } set { rules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Rules>>(value); } }
        public List<Rules> rules { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string benefit_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(benefit); } set { benefit = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Benefit>>(value); } }
        public List<Benefit> benefit { get; set; }
        public class Rules
        {
            public enum RuleMode
            {
                none = 0,
                attributes = 1,
                invoiceitem = 2,
                barcode = 3,
                marker = 4,
                excludebarcode = 5,
                excludemarker = 6,
                excludediscount = 7,
                barcodefile = 8,
                excludeattribute = 9,
                billvalue = 10,
                agmtcode = 11,
                barcodeprefix = 12,
            }
            public RuleMode mode { get; set; }
            public decimal ValueFrom { get; set; }
            public decimal ValueTo { get; set; }
            public ItemDefinition item { get; set; }
            public decimal excludediscount { get; set; }
            public List<long> invoiceitemid { get; set; }
            public DateTime invoicefrom { get; set; }
            public DateTime invoiceto { get; set; }
        }

        public class ItemDefinition
        {
            public string designid { get; set; }
            public string marker { get; set; }
            public List<string> agmtcodes { get; set; }
            public List<string> barcodeprefixes { get; set; }
            public long markerid { get; set; }
            public long productid { get; set; }
            public long brandid { get; set; }
            public long typeid { get; set; }
            public long patternid { get; set; }
            public long materialid { get; set; }
            public long styleid { get; set; }
            public long colourid { get; set; }
            public long fitid { get; set; }
            public long sleeveid { get; set; }
            public long sizeid { get; set; }
        }

        public List<string> coupon_remove { get; set; }
        public List<string> coupon_add { get; set; }
        public List<string> barcode_remove { get; set; }
        public List<string> barcode_add { get; set; }

        public List<string> barcode_exclude_add { get; set; }
        public List<string> barcode_exclude_remove { get; set; }

        public List<long> excludedpromotionids { get; set; }
        public int barcode_count { get; set; }
        public int excludebarcode_count { get; set; }

        public class Benefit
        {
            public bool discountondiscount { get; set; }
            public bool includediscount { get; set; }
            public long locationid { get; set; }
            public enum BenefitMode
            {
                DiscountInPercentage = 1,
                DiscountInFixedValue = 2,
                RewardPoints = 3,
                DiscountInQty = 4,
                Combo = 5,
                DiscountItem = 6,
                Coupen = 7,
                Free = 8,
                FlatPrice = 9,
                GiftMarker = 10,
                CoupenRange = 11
            }
            public BenefitMode mode { get; set; }
            public decimal value { get; set; }
            public bool otp { get; set; }
            public bool ex_scheme { get; set; }
            public bool ex_coupen { get; set; }
            public decimal max { get; set; }
            public decimal min { get; set; }
            public decimal unit { get; set; }
            public bool mergecoupon { get; set; }
            public decimal qtyfrom { get; set; }
            public decimal qtyto { get; set; }
            public decimal applicableqty { get; set; }
            public decimal pricefrom { get; set; }
            public decimal priceto { get; set; }
            public int priority { get; set; }
            public int validitydays { get; set; }
            public bool validitytomorrow { get; set; }
            public DateTime validity { get; set; }
            public ItemDefinition item { get; set; }
            public bool eachqtyfrom { get; set; }
            public bool hasdiscountrule { get; set; }
            public decimal mindiscountpercentage { get; set; }
            public decimal maxdiscountpercentage { get; set; }
            public bool unlimited { get; set; }
            public bool flatdiscount { get; set; }
            public bool similarprice { get; set; }
            public bool mrpprice { get; set; }
            public long filterschemeid { get; set; }
            public string uancode { get; set; }
            public string product { get; set; }
            public decimal amountaspercentage { get; set; }
        }


        public enum ApplicableMode
        {
            Immediate = 1,
            NextBill = 2,
            NextDay = 3,
            Settlement = 4,
            ImmediateAddon = 5,
            Simulation = 9
        }
        public enum Modes
        {
            BillValue = 1,
            BillItem = 2
        }
    }


    public class PromotionBarcode
    {
        public long id { get; set; }
        public bool include { get; set; }
    }

}
