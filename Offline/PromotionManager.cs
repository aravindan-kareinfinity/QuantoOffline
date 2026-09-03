using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InfyPOS.Processors;
using InfyPOS.Models;

namespace Quanto.Offline
{
    public class PromotionManager
    {
        static PromotionManager instance = new PromotionManager();
        public static PromotionManager Instance
        {
            get
            {
                return instance;
            }
        }

        List<InfyPOS.Processors.OfflineClient.Promotion> branchschems = null;
        internal void Initialize()
        {
            branchschems = InfyPOS.Processors.BillManager.Instance.Data.Promotions;
            if (branchschems == null)
                branchschems = new List<InfyPOS.Processors.OfflineClient.Promotion>();
            initialized = true;
        }
        private bool initialized = false;

        public class SchemeBenefit
        {
            public long id { get; set; }
            public decimal value { get; set; }
            public decimal fixedvalue { get; set; }
            public OfflineClient.Bill bill { get; set; }
            public DateTime validity { get; set; }
            public decimal minbillvalue { get; set; }
            public bool unlimited { get; set; }
            public decimal maxdiscountpercentage { get; set; }
            public decimal unit { get; set; }
            public long locationid { get; set; }
            public bool includediscount { get; set; }
            public long filterschemeid { get; set; }
            public bool iscombo { get; set; }
            public bool ismultiple { get; set; }
        }

        public class ItemDetail
        {
            public long id { get; set; }
            public long brandid { get; set; }
            public long productid { get; set; }
            public long colourid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long typeid { get; set; }
            public long materialid { get; set; }
            public long sleeveid { get; set; }
            public long fitid { get; set; }
            public string barcode { get; set; }
            public string designid { get; set; }
            public string agmtcode { get; set; }
            public string serialno { get; set; }
        }

        private long CurrentLocationId()
        {
            if (InfyPOS.Processors.BillManager.Instance.Data == null) return 0;
            return InfyPOS.Processors.BillManager.Instance.Data.locationid;
        }

        private bool MatchesLocation(InfyPOS.Models.Promotions scheme)
        {
            if (scheme == null) return false;
            var locationid = CurrentLocationId();
            if (scheme.locationid == 0 || scheme.locationid == -1 || scheme.locationid == locationid)
                return true;
            return scheme.schemelocations != null && scheme.schemelocations.Contains(locationid);
        }

        private bool IsEnabled(InfyPOS.Models.Promotions scheme)
        {
            return scheme != null && scheme.enabled;
        }

        private bool IsInDate(InfyPOS.Models.Promotions scheme, DateTime billdate)
        {
            return scheme.validfrom.Date <= billdate.Date && scheme.validto.Date.AddDays(1) > billdate.Date;
        }

        private bool IsImmediate(InfyPOS.Models.Promotions scheme)
        {
            return scheme.applicablemode == 0
                || scheme.applicablemode == (int)Promotions.ApplicableMode.Immediate
                || scheme.applicablemode == (int)Promotions.ApplicableMode.ImmediateAddon;
        }

        private bool IsBillItem(InfyPOS.Models.Promotions scheme)
        {
            return scheme.mode == (int)Promotions.Modes.BillItem || scheme.mode == 0;
        }

        private bool IsBillValue(InfyPOS.Models.Promotions scheme)
        {
            return scheme.mode == (int)Promotions.Modes.BillValue;
        }

        private bool MatchesAttribute(ItemDetail e, Promotions.ItemDefinition item)
        {
            if (item == null) return true;
            return (e.brandid == item.brandid || item.brandid == 0) &&
                (e.productid == item.productid || item.productid == 0) &&
                (e.colourid == item.colourid || item.colourid == 0) &&
                (e.sizeid == item.sizeid || item.sizeid == 0) &&
                (e.materialid == item.materialid || item.materialid == 0) &&
                (e.typeid == item.typeid || item.typeid == 0) &&
                (e.patternid == item.patternid || item.patternid == 0) &&
                (e.sleeveid == item.sleeveid || item.sleeveid == 0) &&
                (e.fitid == item.fitid || item.fitid == 0) &&
                (e.designid == item.designid || string.IsNullOrEmpty(item.designid)) &&
                (e.styleid == item.styleid || item.styleid == 0);
        }

        private static long ColLong(DataRow row, string name)
        {
            if (row == null || !row.Table.Columns.Contains(name) || row[name] == DBNull.Value) return 0;
            return Convert.ToInt64(row[name]);
        }

        private static string ColString(DataRow row, string name)
        {
            if (row == null || !row.Table.Columns.Contains(name) || row[name] == DBNull.Value) return "";
            return Convert.ToString(row[name]);
        }

        private List<ItemDetail> FindItemDetail(OfflineClient.Bill bill)
        {
            List<ItemDetail> retValue = new List<ItemDetail>();
            if (bill.Billitems == null) return retValue;
            var stocks = InfyPOS.Processors.BillManager.Instance.Stocks;
            foreach (var item in bill.Billitems)
            {
                var detail = new ItemDetail()
                {
                    barcode = item.barcode,
                    productid = item.productid,
                    serialno = ""
                };
                if (stocks != null && stocks.Columns.Contains("barcode") && !string.IsNullOrEmpty(item.barcode))
                {
                    var safe = item.barcode.Replace("'", "''");
                    DataRow[] rows = null;
                    try
                    {
                        rows = stocks.Select("barcode = '" + safe + "'");
                        if ((rows == null || rows.Length == 0) && stocks.Columns.Contains("serialno"))
                            rows = stocks.Select("serialno = '" + safe + "'");
                    }
                    catch
                    {
                        rows = null;
                    }
                    if (rows != null && rows.Length > 0)
                    {
                        var row = rows[0];
                        detail.productid = ColLong(row, "productid") != 0 ? ColLong(row, "productid") : detail.productid;
                        detail.brandid = ColLong(row, "brandid");
                        detail.colourid = ColLong(row, "colourid");
                        detail.patternid = ColLong(row, "patternid");
                        detail.sizeid = ColLong(row, "sizeid");
                        detail.styleid = ColLong(row, "styleid");
                        detail.typeid = ColLong(row, "typeid");
                        detail.materialid = ColLong(row, "materialid");
                        detail.sleeveid = ColLong(row, "sleeveid");
                        detail.fitid = ColLong(row, "fitid");
                        detail.designid = ColString(row, "designid");
                        detail.agmtcode = ColString(row, "agmtcode");
                        if (string.IsNullOrEmpty(detail.agmtcode))
                            detail.agmtcode = ColString(row, "itbuscode");
                        detail.serialno = ColString(row, "serialno");
                    }
                }
                retValue.Add(detail);
            }
            return retValue;
        }

        internal OfflineClient.Bill Process(OfflineClient.Bill bill)
        {
            try
            {
                if (!initialized)
                    Initialize();
                if (branchschems == null || branchschems.Count == 0) return bill;
                if (bill.billdate.Date == DateTime.MinValue)
                    bill.billdate = DateTime.Now;
                bill.removescheme();

                var schemelist = branchschems.FindAll(e => e.scheme != null
                    && IsEnabled(e.scheme)
                    && IsImmediate(e.scheme)
                    && IsBillItem(e.scheme)
                    && MatchesLocation(e.scheme)
                    && IsInDate(e.scheme, bill.billdate));
                if (schemelist.Count == 0)
                {
                    CheckBillValuePromotion(bill);
                    if (bill.Billitems != null && bill.Billitems.Sum(e => e.schemediscount) > 0)
                        bill.schemediscount = bill.Billitems.Sum(e => e.schemediscount);
                    return bill;
                }

                List<ItemDetail> itemlist = FindItemDetail(bill);
                List<OfflineClient.Bill> afterdiscount = new List<OfflineClient.Bill>();
                foreach (var item in schemelist)
                {
                    var newbill = bill.ToJSON().Json2Object<OfflineClient.Bill>();
                    ApplyEligibility(item, newbill, itemlist);
                    var eligbleItems = newbill.Billitems.FindAll(e => e.schemeeligble);
                    if (eligbleItems.Count == 0) continue;

                    afterdiscount.Add(newbill);
                    ApplyBenefits(item, newbill, eligbleItems, itemlist, bill);
                }

                if (afterdiscount.Count > 0)
                {
                    bill = PostProcess(bill, afterdiscount);
                }
                CheckBillValuePromotion(bill);
                bill.Billitems.ForEach(x =>
                {
                    if (x.schemediscount < 0)
                    {
                        x.schemediscount = 0;
                        x.schemeid = 0;
                    }
                });
                if (bill.Billitems.Sum(e => e.schemediscount) > 0)
                {
                    bill.schemediscount = bill.Billitems.Sum(e => e.schemediscount);
                }
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.Message);
            }
            return bill;
        }

        private void ApplyEligibility(OfflineClient.Promotion item, OfflineClient.Bill newbill, List<ItemDetail> itemlist)
        {
            if (item.scheme.rules != null && item.scheme.rules.Count > 0 &&
                item.scheme.rules.All(e => e.mode == Promotions.Rules.RuleMode.excludeattribute
                || e.mode == Promotions.Rules.RuleMode.excludebarcode
                || e.mode == Promotions.Rules.RuleMode.excludemarker))
            {
                newbill.Billitems.ForEach(e => e.schemeeligble = e.qty > 0);
            }

            if (item.scheme.rules != null)
            {
                foreach (var rule in item.scheme.rules.Where(e => e.mode != Promotions.Rules.RuleMode.excludediscount &&
                    e.mode != Promotions.Rules.RuleMode.excludebarcode && e.mode != Promotions.Rules.RuleMode.excludemarker &&
                    e.mode != Promotions.Rules.RuleMode.excludeattribute))
                {
                    switch (rule.mode)
                    {
                        case Promotions.Rules.RuleMode.attributes:
                            var availableItems = itemlist.FindAll(e => MatchesAttribute(e, rule.item));
                            newbill.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                            {
                                e.schemeeligble = e.qty > 0;
                            });
                            break;
                        case Promotions.Rules.RuleMode.agmtcode:
                            if (rule.item != null && rule.item.agmtcodes != null && rule.item.agmtcodes.Count > 0)
                            {
                                var availableCode = itemlist.FindAll(e => rule.item.agmtcodes.Contains(e.agmtcode));
                                newbill.Billitems.FindAll(e => availableCode.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                                {
                                    e.schemeeligble = e.qty > 0;
                                });
                            }
                            break;
                        case Promotions.Rules.RuleMode.barcodeprefix:
                            if (rule.item != null && rule.item.barcodeprefixes != null && rule.item.barcodeprefixes.Count > 0)
                            {
                                newbill.Billitems.FindAll(e =>
                                    rule.item.barcodeprefixes.Exists(x => !string.IsNullOrEmpty(e.barcode) && e.barcode.StartsWith(x))).ForEach(e =>
                                    {
                                        e.schemeeligble = e.qty > 0;
                                    });
                            }
                            break;
                        case Promotions.Rules.RuleMode.billvalue:
                            newbill.Billitems.FindAll(e => (e.receivable >= rule.ValueFrom && (rule.ValueTo == 0 || e.receivable <= rule.ValueTo))).ForEach(e =>
                            {
                                e.schemeeligble = e.qty > 0;
                            });
                            break;
                    }
                }
            }

            if (item.barcode != null && item.barcode.Count > 0)
            {
                newbill.Billitems.FindAll(e => item.barcode.Contains(e.barcode)).ForEach(e =>
                {
                    e.schemeeligble = e.qty > 0;
                });
            }

            if (item.scheme.rules != null && item.scheme.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludediscount))
            {
                var excludediscount = item.scheme.rules.Find(e => e.mode == Promotions.Rules.RuleMode.excludediscount).excludediscount;
                foreach (var bitem in newbill.Billitems.FindAll(e => e.schemeeligble))
                {
                    if (bitem.qty == 0) continue;
                    if (Math.Round((bitem.price + (bitem.discount / bitem.qty)) * excludediscount / 100, 2) < (bitem.discount / bitem.qty))
                        bitem.schemeeligble = false;
                }
            }

            if (item.scheme.rules != null && item.scheme.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
            {
                foreach (var ritem in item.scheme.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                {
                    var availableItems = itemlist.FindAll(e => MatchesAttribute(e, ritem.item));
                    newbill.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                    {
                        e.schemeeligble = false;
                    });
                }
            }

            if (item.scheme.barcode_exclude_add != null && item.scheme.barcode_exclude_add.Count > 0)
            {
                newbill.Billitems.FindAll(e => item.scheme.barcode_exclude_add.Contains(e.barcode)).ForEach(e =>
                {
                    e.schemeeligble = false;
                });
            }
        }

        private void ApplyBenefits(OfflineClient.Promotion item, OfflineClient.Bill newbill, List<OfflineClient.BillItems> eligbleItems, List<ItemDetail> itemlist, OfflineClient.Bill sourcebill)
        {
            if (item.scheme.benefit == null) return;
            foreach (var benefit in item.scheme.benefit.OrderBy(e => e.priority))
            {
                var pricerange = eligbleItems.FindAll(e => e.schemeid == 0).FindAll(e => (benefit.pricefrom == 0 && benefit.priceto == 0) || (e.price + (e.discount == 0 || e.qty == 0 ? 0 : (e.discount / e.qty)) >= benefit.pricefrom && e.price + (e.discount == 0 || e.qty == 0 ? 0 : (e.discount / e.qty)) <= benefit.priceto)).OrderBy(e => e.price).ToList();
                if (benefit.discountondiscount)
                {
                    pricerange = eligbleItems.FindAll(e => e.schemeid == 0).FindAll(e => (benefit.pricefrom == 0 && benefit.priceto == 0) || (e.price >= benefit.pricefrom && e.price <= benefit.priceto)).OrderBy(e => e.price).ToList();
                }
                if (benefit.hasdiscountrule)
                {
                    pricerange = pricerange.FindAll(x => x.discountpercentage >= benefit.mindiscountpercentage && x.discountpercentage <= benefit.maxdiscountpercentage);
                }
                if (pricerange.Sum(e => e.qty) == 0) break;

                switch (benefit.mode)
                {
                    case Promotions.Benefit.BenefitMode.DiscountInQty:
                        ApplyDiscountInQty(item, benefit, pricerange);
                        break;
                    case Promotions.Benefit.BenefitMode.DiscountItem:
                        ApplyDiscountItem(item, benefit, pricerange, eligbleItems, itemlist, sourcebill);
                        break;
                    case Promotions.Benefit.BenefitMode.Combo:
                        ApplyCombo(item, benefit, pricerange);
                        break;
                    case Promotions.Benefit.BenefitMode.FlatPrice:
                        if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                            (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                        {
                            foreach (var bitem in pricerange)
                            {
                                bitem.schemeid = item.scheme.id;
                                var newreceivable = bitem.qty * benefit.value;
                                if ((bitem.receivable - bitem.schemediscount) > newreceivable)
                                    bitem.schemediscount = (bitem.receivable - bitem.schemediscount) - newreceivable;
                            }
                        }
                        break;
                    case Promotions.Benefit.BenefitMode.DiscountInFixedValue:
                        ApplyDiscountInFixedValue(item, benefit, pricerange);
                        break;
                    case Promotions.Benefit.BenefitMode.DiscountInPercentage:
                        ApplyDiscountInPercentage(item, benefit, pricerange);
                        break;
                }
            }
        }

        private void ApplyDiscountInQty(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> pricerange)
        {
            if (benefit.similarprice)
            {
                foreach (var pricegroups in pricerange.GroupBy(e => e.price))
                {
                    ApplyDiscountInQtyGroup(item, benefit, pricegroups.ToList());
                }
            }
            else
            {
                ApplyDiscountInQtyGroup(item, benefit, pricerange);
            }
        }

        private void ApplyDiscountInQtyGroup(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> group)
        {
            if (!(benefit.qtyfrom == 0 || benefit.qtyfrom <= group.Sum(e => e.qty) &&
                (benefit.qtyto == 0 || benefit.qtyto >= group.Sum(e => e.qty)))) return;

            var freeqty = (benefit.qtyto == 0) ? ((group.Sum(e => e.qty) / benefit.qtyfrom) * benefit.value) : benefit.value;
            if (Math.Ceiling(benefit.value) == Math.Floor(benefit.value))
                freeqty = Math.Floor(freeqty);

            decimal adjustable = freeqty * (benefit.qtyfrom == 0 ? 1 : benefit.qtyfrom);
            foreach (var bitem in group)
            {
                if (bitem.qty <= freeqty)
                {
                    bitem.schemeid = item.scheme.id;
                    bitem.schemediscount = (bitem.getactualreceivable());
                    freeqty -= bitem.qty;
                }
                else
                {
                    bitem.schemeid = item.scheme.id;
                    bitem.schemediscount = Math.Round(((bitem.getactualreceivable()) / bitem.qty) * freeqty, 2);
                    freeqty = 0;
                }
                if (freeqty == 0)
                    break;
            }
            if (adjustable > 0)
            {
                foreach (var bitem in group)
                {
                    adjustable = adjustable > bitem.qty ? adjustable - bitem.qty : 0;
                    bitem.schemeid = item.scheme.id;
                    if (adjustable <= 0) break;
                }
            }
        }

        private void ApplyDiscountItem(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> pricerange, List<OfflineClient.BillItems> eligbleItems, List<ItemDetail> itemlist, OfflineClient.Bill bill)
        {
            decimal billvaluefrom = 0;
            decimal billvalueto = 0;
            if (item.scheme.rules != null && item.scheme.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.billvalue))
            {
                billvaluefrom = item.scheme.rules.Find(e => e.mode == Promotions.Rules.RuleMode.billvalue).ValueFrom;
                billvalueto = item.scheme.rules.Find(e => e.mode == Promotions.Rules.RuleMode.billvalue).ValueTo;
            }
            if ((billvaluefrom > 0 && billvaluefrom > eligbleItems.Sum(e => e.receivable)) ||
                (billvalueto > 0 && billvalueto < eligbleItems.Sum(e => e.receivable)))
                return;

            if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
            {
                var freeqty = benefit.unit;
                var totqty = pricerange.Sum(e => e.qty);
                var freeitemfixedprice = benefit.value;
                if (benefit.qtyto > 1)
                    totqty = Math.Floor(totqty / benefit.qtyto) * freeqty;
                else
                    totqty = Math.Floor(totqty / (benefit.qtyfrom <= 0 ? 1 : benefit.qtyfrom)) * freeqty;

                var matchedItems = itemlist.FindAll(e => MatchesAttribute(e, benefit.item));
                matchedItems = matchedItems.FindAll(e => !pricerange.Exists(p => p.barcode == e.barcode));
                var discountitemlist = bill.Billitems.Where(e => matchedItems.Exists(x => x.barcode == e.barcode)).ToList();
                discountitemlist.ForEach(x => x.schemeeligble = true);
                if (discountitemlist.Count > 0)
                {
                    var discountqty = discountitemlist.Sum(e => e.qty);
                    var acceptableqty = totqty <= discountqty ? totqty : discountqty;
                    discountitemlist.OrderBy(e => e.price).ToList().ForEach(e =>
                    {
                        if (acceptableqty > 0)
                        {
                            if (e.qty <= acceptableqty)
                            {
                                acceptableqty -= e.qty;
                                e.schemediscount = freeitemfixedprice > 0 ? e.receivable - freeitemfixedprice : e.receivable;
                                e.schemeid = item.scheme.id;
                                e.totaltax = 0;
                                e.taxperitem = 0;
                                e.taxpercentage = 0;
                            }
                            else if (acceptableqty > 0)
                            {
                                e.totaltax = Math.Round(e.totaltax / e.qty * acceptableqty, 4);
                                e.schemediscount = (e.receivable / e.qty * acceptableqty) - (e.discount / e.qty * acceptableqty);
                                acceptableqty = 0;
                                e.schemeid = item.scheme.id;
                            }
                        }
                    });
                }
            }
        }

        private void ApplyCombo(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> pricerange)
        {
            if (!(benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty)))) return;

            var totalValue = benefit.value;
            if (benefit.qtyto == 0)
            {
                var maxqty = Math.Floor(pricerange.Sum(e => e.qty) / benefit.qtyfrom);
                totalValue = benefit.value * maxqty;
                var removelist = new List<InfyPOS.Processors.OfflineClient.BillItems>();
                if (maxqty * benefit.qtyfrom != pricerange.Sum(e => e.qty))
                {
                    var remqty = pricerange.Sum(e => e.qty) - (maxqty * benefit.qtyfrom);
                    foreach (var pitem in pricerange.OrderByDescending(e => e.price))
                    {
                        if (remqty > pitem.qty)
                        {
                            remqty -= pitem.qty;
                            removelist.Add(pitem);
                        }
                        else
                        {
                            totalValue += (pitem.receivable / pitem.qty) * remqty;
                            remqty = 0;
                        }
                        if (remqty == 0)
                            break;
                    }
                    foreach (var pitem in removelist)
                        pricerange.Remove(pitem);
                }
            }
            var currentValue = pricerange.Sum(e => e.receivable - e.schemediscount);
            if (currentValue == 0) return;
            var discountpercent = (currentValue - totalValue) / currentValue * 100;
            if (discountpercent > 0)
            {
                if (benefit.flatdiscount)
                {
                    var maxqty = Math.Floor(pricerange.Sum(e => e.qty) / benefit.qtyfrom);
                    var finalprice = benefit.value / benefit.qtyfrom;
                    var removeqty = pricerange.Sum(e => e.qty) - (maxqty * benefit.qtyfrom);
                    foreach (var bitem in pricerange)
                    {
                        bitem.schemeid = item.scheme.id;
                        var discount = (bitem.price) - finalprice;
                        bitem.schemediscount = discount * bitem.qty;
                    }
                    if (removeqty > 0)
                    {
                        var remitems = pricerange.FindAll(x => x.qty >= removeqty &&
                            x.qty % benefit.qtyfrom != 0);
                        foreach (var ritem in remitems)
                        {
                            var discount = (ritem.price) - finalprice;
                            if (ritem.qty >= removeqty)
                            {
                                ritem.schemediscount = discount * (ritem.qty - removeqty);
                                removeqty = 0;
                            }
                            else
                            {
                                ritem.schemediscount = 0;
                                removeqty -= ritem.qty;
                            }
                            if (removeqty <= 0) break;
                        }
                    }
                }
                else
                {
                    foreach (var bitem in pricerange)
                    {
                        bitem.schemeid = item.scheme.id;
                        var receivable = Math.Round(totalValue * ((bitem.receivable - bitem.schemediscount) / currentValue * 100) / 100, 2);
                        bitem.schemediscount = (bitem.receivable - bitem.schemediscount) - receivable;
                    }
                }
            }
            if (!benefit.flatdiscount)
            {
                var newtotalvalue = pricerange.Sum(e => e.schemediscount);
                var diffvalue = totalValue - (currentValue - newtotalvalue);
                if (diffvalue != 0 && pricerange.Count > 0)
                    pricerange.Last().schemediscount += (diffvalue * -1);
            }
        }

        private void ApplyDiscountInFixedValue(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> pricerange)
        {
            if (!(benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty)))) return;

            if (benefit.mrpprice)
            {
                var fixeddiscount = benefit.value;
                foreach (var bitem in pricerange)
                {
                    bitem.schemeid = item.scheme.id;
                    var discountvalue = (bitem.discount > 0 && benefit.includediscount && bitem.qty > 0) ? (bitem.discount / bitem.qty) : 0;
                    var availablediscount = fixeddiscount - ((bitem.mrp - bitem.price) + discountvalue);
                    if (availablediscount > 0)
                        bitem.schemediscount = Math.Round(availablediscount, 2);
                }
            }
            else
            {
                var fixeddiscount = benefit.value;
                foreach (var bitem in pricerange)
                {
                    bitem.schemeid = item.scheme.id;
                    if (fixeddiscount > (benefit.includediscount ? bitem.discount : 0))
                        bitem.schemediscount = Math.Round(fixeddiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                    if (benefit.qtyfrom > 1) break;
                }
            }
        }

        private void ApplyDiscountInPercentage(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> pricerange)
        {
            if (benefit.mrpprice)
            {
                foreach (var pricegroups in pricerange.GroupBy(e => e.mrp))
                    ApplyPercentageGroup(item, benefit, pricegroups.ToList(), true);
            }
            if (benefit.similarprice)
            {
                foreach (var pricegroups in pricerange.GroupBy(e => e.price))
                    ApplyPercentageGroup(item, benefit, pricegroups.ToList(), false);
            }
            else if (!benefit.mrpprice)
            {
                ApplyPercentageGroup(item, benefit, pricerange, false);
            }
        }

        private void ApplyPercentageGroup(OfflineClient.Promotion item, Promotions.Benefit benefit, List<OfflineClient.BillItems> group, bool usemrp)
        {
            if (!(benefit.qtyfrom == 0 || benefit.qtyfrom <= group.Sum(e => e.qty) &&
                (benefit.qtyto == 0 || benefit.qtyto >= group.Sum(e => e.qty)))) return;

            if (benefit.applicableqty > 0)
            {
                var applicableqty = benefit.applicableqty;
                foreach (var bitem in group)
                {
                    if (usemrp && bitem.mrp == 0) continue;
                    var baseprice = usemrp ? bitem.mrp : bitem.price;
                    if (applicableqty >= bitem.qty)
                    {
                        applicableqty -= bitem.qty;
                        var tdiscount = Math.Round(((baseprice + (bitem.qty == 0 ? 0 : (bitem.discount / bitem.qty))) * benefit.value / 100) * bitem.qty, 2);
                        if (benefit.discountondiscount)
                            tdiscount = Math.Round((baseprice * benefit.value / 100) * bitem.qty, 2);
                        bitem.schemeid = item.scheme.id;
                        bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount && bitem.qty > 0 ? (bitem.discount / bitem.qty) : 0), 2);
                    }
                    else
                    {
                        var tdiscount = Math.Round(((baseprice + (bitem.qty == 0 ? 0 : (bitem.discount / bitem.qty))) * benefit.value / 100) * applicableqty, 2);
                        if (benefit.discountondiscount)
                            tdiscount = Math.Round((baseprice * benefit.value / 100) * applicableqty, 2);
                        bitem.schemeid = item.scheme.id;
                        bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                        applicableqty = 0;
                    }
                    if (applicableqty == 0) break;
                }
                if (benefit.qtyto > 0)
                {
                    applicableqty = benefit.qtyto;
                    foreach (var bitem in group)
                    {
                        if (usemrp && bitem.mrp == 0) continue;
                        if (applicableqty >= bitem.qty)
                        {
                            applicableqty -= bitem.qty;
                            if (benefit.includediscount && bitem.schemediscount == 0)
                                bitem.schemediscount = bitem.discount * -1;
                        }
                        else
                        {
                            if (benefit.includediscount && bitem.schemediscount == 0 && bitem.qty > 0)
                                bitem.schemediscount = (bitem.discount / bitem.qty * applicableqty) * -1;
                            applicableqty = 0;
                        }
                        bitem.schemeid = item.scheme.id;
                        if (applicableqty == 0) break;
                    }
                }
            }
            else
            {
                var disountpercentage = benefit.value;
                if (benefit.eachqtyfrom && benefit.qtyfrom > 0)
                {
                    var maxqty = Math.Floor(group.Sum(e => e.qty) / benefit.qtyfrom) * benefit.qtyfrom;
                    foreach (var bitem in group.OrderByDescending(x => x.qty))
                    {
                        if (usemrp && bitem.mrp == 0) continue;
                        decimal qty = bitem.qty <= maxqty ? bitem.qty : maxqty;
                        if (qty > 0)
                        {
                            var baseprice = usemrp ? bitem.mrp : bitem.price;
                            var tdiscount = Math.Round(((baseprice + (bitem.qty == 0 ? 0 : (bitem.discount / bitem.qty))) * disountpercentage / 100) * qty, 2);
                            if (benefit.discountondiscount)
                                tdiscount = Math.Round((baseprice * disountpercentage / 100) * qty, 2);
                            bitem.schemeid = item.scheme.id;
                            bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                            maxqty -= qty;
                        }
                    }
                }
                else
                {
                    foreach (var bitem in group)
                    {
                        if (usemrp && bitem.mrp == 0) continue;
                        if (bitem.qty > 0)
                        {
                            var baseprice = usemrp ? bitem.mrp : bitem.price;
                            var tdiscount = Math.Round(((baseprice + (bitem.discount / bitem.qty)) * disountpercentage / 100) * bitem.qty, 2);
                            if (benefit.discountondiscount)
                                tdiscount = Math.Round((baseprice * disountpercentage / 100) * bitem.qty, 2);
                            bitem.schemeid = item.scheme.id;
                            bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                        }
                    }
                }
            }
        }

        private OfflineClient.Bill PostProcess(OfflineClient.Bill source, List<OfflineClient.Bill> discountedbills)
        {
            List<SchemeBenefit> schemebenefits = new List<SchemeBenefit>();
            foreach (var item in branchschems.Where(e => e.scheme != null && MatchesLocation(e.scheme)))
            {
                var iscombo = item.scheme.benefit != null && item.scheme.benefit.Exists(e => e.mode == Promotions.Benefit.BenefitMode.Combo);
                var ismultiply = item.scheme.benefit != null && item.scheme.benefit.Exists(e => e.mode != Promotions.Benefit.BenefitMode.Combo && e.qtyfrom > 1);
                var discount = discountedbills.Sum(e => e.Billitems.Where(x => x.schemeid == item.scheme.id).Sum(y => y.schemediscount));
                if (discount != 0)
                {
                    schemebenefits.Add(new SchemeBenefit()
                    {
                        id = item.scheme.id,
                        value = discount,
                        iscombo = iscombo,
                        ismultiple = ismultiply,
                        bill = discountedbills.Where(e => e.Billitems.Exists(ex => ex.schemeid == item.scheme.id)).OrderByDescending(e => e.Billitems.Where(x => x.schemeid == item.scheme.id && x.schemediscount != 0).Sum(y => y.schemediscount)).First()
                    });
                }
            }

            int index = 0;
            source.Billitems.ForEach(e => e.orderid = index++);
            foreach (var sb in schemebenefits.OrderByDescending(e => e.value))
            {
                index = 0;
                sb.bill.Billitems.ForEach(e => e.orderid = index++);
                foreach (var bitem in sb.bill.Billitems.Where(e => e.schemeid == sb.id))
                {
                    var actualbillitem = source.Billitems.Find(e => e.orderid == bitem.orderid);
                    if (actualbillitem.schemeid == 0)
                    {
                        actualbillitem.schemediscount = bitem.schemediscount;
                        actualbillitem.schemeeligble = bitem.schemeeligble;
                        actualbillitem.schemeid = bitem.schemeid;
                    }
                    else if (!sb.iscombo && !sb.ismultiple &&
                      actualbillitem.schemediscount < bitem.schemediscount)
                    {
                        var currentscheme = schemebenefits.Find(e => e.id == actualbillitem.schemeid);
                        if (currentscheme != null && (currentscheme.ismultiple || currentscheme.iscombo)) continue;
                        actualbillitem.schemediscount = bitem.schemediscount;
                        actualbillitem.schemeeligble = bitem.schemeeligble;
                        actualbillitem.schemeid = bitem.schemeid;
                    }
                }
            }
            return source;
        }

        public decimal SchemedBillValue(InfyPOS.Models.Promotions scheme, List<OfflineClient.Bill> bills)
        {
            var promo = branchschems.Find(e => e.scheme != null && e.scheme.id == scheme.id);
            List<ItemDetail> itemlist = new List<ItemDetail>();
            bills.ForEach(b => itemlist.AddRange(FindItemDetail(b)));
            foreach (var bill in bills)
            {
                if (promo != null)
                    ApplyEligibility(promo, bill, FindItemDetail(bill));
            }
            return bills.Sum(b => b.Billitems.Sum(bi => bi.schemeeligble ? bi.receivable : 0));
        }

        public bool BillValuePromotion(OfflineClient.Bill source)
        {
            var taxsplits = new SortedDictionary<string, OfflineClient.Tax>();
            foreach (var item in source.Billitems)
            {
                if (!taxsplits.ContainsKey(item.barcode) && InfyPOS.Processors.BillManager.Instance.Data.Tax != null)
                {
                    var tax = InfyPOS.Processors.BillManager.Instance.Data.Tax.Find(ex => ex.id == item.taxid);
                    if (tax != null)
                        taxsplits.Add(item.barcode, tax);
                }
            }
            List<SchemeBenefit> schemebenefits = new List<SchemeBenefit>();
            foreach (var item in branchschems.Where(e => e.scheme != null && IsEnabled(e.scheme) && MatchesLocation(e.scheme) && IsBillValue(e.scheme) && IsInDate(e.scheme, source.billdate) && IsImmediate(e.scheme)))
            {
                var bills = new List<OfflineClient.Bill>();
                bills.Add(source.ToJSON().Json2Object<OfflineClient.Bill>());
                bool hasfilter = item.scheme.rules != null && item.scheme.rules.Exists(e => e.mode != Promotions.Rules.RuleMode.none && e.mode != Promotions.Rules.RuleMode.billvalue);
                var billamount = hasfilter ? SchemedBillValue(item.scheme, bills) : source.total;
                if (billamount == 0) continue;
                if (!hasfilter)
                {
                    bills[0].Billitems.ForEach(e => e.schemeeligble = true);
                }
                if (item.scheme.rules != null && item.scheme.rules.Exists(e => (e.mode == Promotions.Rules.RuleMode.billvalue || e.mode == Promotions.Rules.RuleMode.none) && (e.ValueFrom == 0 || e.ValueFrom <= billamount) && (e.ValueTo == 0 || e.ValueTo >= billamount)))
                {
                    foreach (var benefit in item.scheme.benefit.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.DiscountInPercentage))
                    {
                        var fixedvalue = billamount * benefit.value / 100;
                        SchemeBenefit schemeBenefit = new SchemeBenefit();
                        schemeBenefit.bill = bills[0];
                        schemeBenefit.id = item.scheme.id;
                        schemeBenefit.includediscount = benefit.includediscount;
                        schemeBenefit.value = fixedvalue / billamount * 100;
                        schemebenefits.Add(schemeBenefit);
                    }
                    foreach (var benefit in item.scheme.benefit.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.DiscountInFixedValue))
                    {
                        SchemeBenefit schemeBenefit = new SchemeBenefit();
                        schemeBenefit.bill = bills[0];
                        schemeBenefit.id = item.scheme.id;
                        schemeBenefit.includediscount = benefit.includediscount;
                        schemeBenefit.fixedvalue = benefit.value;
                        schemeBenefit.value = benefit.value / billamount * 100;
                        schemebenefits.Add(schemeBenefit);
                    }
                }
            }
            bool isincludediscount = false;
            if (schemebenefits.Count > 0)
            {
                var scheme = schemebenefits.OrderByDescending(e => e.value).First();
                var list = source.Billitems.OrderByDescending(e => e.receivable).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (!scheme.bill.Billitems.Exists(e => e.barcode == list[i].barcode && e.schemeeligble)) continue;
                    if (list[i].qty == 0) continue;

                    var price = list[i].price;
                    var currentBasePrice = price - (list[i].schemediscount / list[i].qty);
                    var newBasePrice = price - Math.Round(list[i].price * scheme.value / 100, 2);
                    if (scheme.includediscount)
                    {
                        newBasePrice = price += (list[i].discount / list[i].qty);
                        newBasePrice = newBasePrice - Math.Round(newBasePrice * scheme.value / 100, 2);
                    }
                    if (newBasePrice < currentBasePrice)
                    {
                        var schemediscount = Math.Round(price * scheme.value * list[i].qty / 100, 2);

                        if (scheme.includediscount)
                        {
                            schemediscount = Math.Round((price - (list[i].discount / list[i].qty)) * scheme.value * list[i].qty / 100, 2);
                            newBasePrice = list[i].price + (list[i].discount / list[i].qty);
                            newBasePrice = newBasePrice - (schemediscount / list[i].qty);
                            if (schemediscount < list[i].discount)
                                continue;
                        }

                        list[i].schemeid = scheme.id;
                        var baseprice = list[i].price - (list[i].schemediscount / list[i].qty);
                        if (scheme.includediscount)
                        {
                            baseprice = baseprice + (list[i].discount / list[i].qty);
                            schemediscount -= list[i].discount;
                        }

                        list[i].schemediscount = schemediscount;
                        if (taxsplits.ContainsKey(list[i].barcode))
                        {
                            var newtaxtaxpercent = taxsplits[list[i].barcode].FindTaxPercentage(baseprice);
                            if (newtaxtaxpercent != list[i].taxpercentage)
                            {
                                list[i].taxpercentage = newtaxtaxpercent;
                            }
                        }
                        list[i].schemeid = scheme.id;
                        list[i].totaltax = Math.Round(list[i].qty * (baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage), 4);
                        list[i].taxperitem = Math.Round(baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage, 4);
                        list[i].receivable = baseprice * list[i].qty;
                    }
                }
            }
            return isincludediscount;
        }

        public bool CheckBillValuePromotion(OfflineClient.Bill source)
        {
            if (branchschems != null && branchschems.Exists(e => e.scheme != null && IsBillValue(e.scheme) && MatchesLocation(e.scheme)))
            {
                var newbill = source.ToJSON().Json2Object<OfflineClient.Bill>();
                var includediscount = BillValuePromotion(newbill);
                bool calculateBillValue = false;

                for (int i = 0; i < newbill.Billitems.Count; i++)
                {
                    var bitem = newbill.Billitems[i];
                    if (bitem.schemeid > 0 && bitem.schemediscount > 0)
                    {
                        var actualbillitem = source.Billitems[i];
                        if (actualbillitem.schemediscount < bitem.schemediscount)
                        {
                            actualbillitem.schemediscount = bitem.schemediscount;
                            actualbillitem.schemeid = bitem.schemeid;
                            actualbillitem.discount = bitem.discount;
                            actualbillitem.totaltax = bitem.totaltax;
                            actualbillitem.taxperitem = bitem.taxperitem;
                            actualbillitem.price = bitem.price;
                            actualbillitem.receivable = bitem.receivable;
                            calculateBillValue = true;
                        }
                    }
                }

                if (calculateBillValue)
                {
                    source.additionaldiscount = source.Billitems.Sum(e => e.additionaldiscount * e.qty);
                    source.discount = source.Billitems.Sum(e => e.discount);
                }
                return includediscount;
            }
            return false;
        }

    }
}
