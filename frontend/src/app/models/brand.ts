/** Brand master — mirrors InfyPOS `Models/Brand.cs` / `brand` table. */

export class Brand {
  id = 0;
  code = '';
  name = '';
  printingname = '';
  barcodeprefix = '';
  barcodesuffix = '';
  logoid = '';
  discountmode = 0;
  discountvalue = 0;
  preferredsupplierid = 0;
  groupid = 0;
  minmargin = 0;
  maxmargin = 0;
  isactive = true;
  version = 0;
  marginlist: { productid: number; margin: number }[] = [];
  ErrorMessage?: string;
}

export class BrandSearchReq {
  id = 0;
  code = '';
  name = '';
  inactive = false;
}

export class BrandFormModel {
  code = '';
  name = '';
  isactive = true;
}
