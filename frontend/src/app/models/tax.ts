/**
 * Tax master — mirrors InfyPOS `Models/tax.cs` / `TaxService/*`.
 */
export class TaxSplitItem {
  fromamount = 0;
  toamount = 0;
  cgst = 0;
  sgst = 0;
  igst = 0;
  incost = false;
  sourceid?: number;
  taxpercentage?: number;
}

export class TaxSplit {
  items: TaxSplitItem[] = [];
  avoidrangefrom = 0;
  avoidrangeto = 0;
}

export class Tax {
  id = 0;
  name = '';
  code = '';
  type = 0;
  taxpercentage = 0;
  issalestax = false;
  ispurchasetax = false;
  isactive = true;
  hascess = false;
  cesspercentage = 0;
  taxorder = 0;
  disable = false;
  taxlist = '';
  taxsplit = new TaxSplit();
  createdby = 0;
  modifiedby = 0;
  version = 0;
  /** Omit empty strings — Web API DateTime binding fails on `""` and leaves `request` null. */
  createdon?: string;
  modifiedon?: string;
  ErrorMessage?: string;
}

export class TaxSearchReq {
  id = 0;
  name = '';
  code = '';
  type?: number;
  taxpercentage?: number;
  ids: number[] = [];
}

export class TaxKind {
  static readonly Gst = 1;
  static readonly RangedGst = 2;
  static readonly Igst = 3;
  static readonly Vat = 4;
  static readonly Tds = 5;
  static readonly Tcs = 6;
  static readonly Cess = 7;
}

export const TAX_KIND_OPTIONS: { label: string; value: number }[] = [
  { label: 'GST', value: TaxKind.Gst },
  { label: 'Ranged GST', value: TaxKind.RangedGst },
  { label: 'IGST', value: TaxKind.Igst },
  { label: 'VAT', value: TaxKind.Vat },
  { label: 'TDS', value: TaxKind.Tds },
  { label: 'TCS', value: TaxKind.Tcs },
  { label: 'CESS', value: TaxKind.Cess },
];

export class RangedGstSlab {
  priceFrom = 0;
  priceTo = 0;
  ratePercent = 0;
}

export class TaxDraft {
  code = '';
  name = '';
  taxType: number | null = null;
  rate: number | null = null;
  slabs: RangedGstSlab[] = [];
}
