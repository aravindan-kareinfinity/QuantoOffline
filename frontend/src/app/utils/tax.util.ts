import {
  RangedGstSlab,
  Tax,
  TAX_KIND_OPTIONS,
  TaxKind,
  TaxSplit,
  TaxSplitItem,
} from '../models/tax';

export function taxKindLabel(type: number): string {
  return TAX_KIND_OPTIONS.find((o) => o.value === type)?.label ?? '—';
}

function isNumericToken(value: string): boolean {
  return /^\d+(\.\d+)?$/.test(value.trim());
}

function isStoredNameNumericPlaceholder(name: string, row: Tax): boolean {
  if (!isNumericToken(name)) return false;
  const n = Number(name);
  if (!Number.isFinite(n)) return false;
  return n === row.id || n === Number(row.taxpercentage);
}

function formatTaxDefaultName(row: Pick<Tax, 'type' | 'taxpercentage' | 'taxsplit'>): string {
  const kind = taxKindLabel(row.type);
  if (row.type === TaxKind.RangedGst) {
    const slabs = row.taxsplit?.items?.length ?? 0;
    return slabs ? `${kind} (${slabs} slab${slabs === 1 ? '' : 's'})` : kind;
  }
  const rate = Number(row.taxpercentage);
  if (Number.isFinite(rate) && rate > 0) return `${kind} ${rate}%`;
  return kind;
}

export function taxDisplayName(row: Tax | null | undefined): string {
  if (!row) return '—';

  const name = row.name?.trim() ?? '';
  const code = row.code?.trim() ?? '';

  if (name && !isStoredNameNumericPlaceholder(name, row)) return name;
  if (code) return code;
  if (name) return formatTaxDefaultName(row);
  return formatTaxDefaultName(row) || '—';
}

export function round2(n: number): number {
  return Math.round(n * 100) / 100;
}

export function splitGstHalves(totalPercent: number): { cgst: number; sgst: number } {
  const half = totalPercent / 2;
  return { cgst: round2(half), sgst: round2(half) };
}

export function rateLabel(row: Tax | undefined): string {
  if (!row) return '—';
  if (row.type === TaxKind.RangedGst) {
    const n = row.taxsplit?.items?.length ?? 0;
    return n ? `${n} slab(s)` : '—';
  }
  return `${row.taxpercentage}%`;
}

export function gstHalfLabel(row: Tax | undefined, half: 'cgst' | 'sgst'): string {
  if (!row || row.type !== TaxKind.Gst) return '—';
  const r = Number(row.taxpercentage);
  if (!Number.isFinite(r) || r <= 0) return '—';
  return `${splitGstHalves(r)[half]}%`;
}

export function resolveTaxNameForSave(input: {
  name: string;
  taxType: number;
  rate: number | null;
  slabCount?: number;
}): string {
  const trimmed = input.name.trim();
  if (trimmed && !isNumericToken(trimmed)) return trimmed;

  if (input.taxType === TaxKind.RangedGst) {
    const slabs = input.slabCount ?? 0;
    return slabs
      ? `${taxKindLabel(input.taxType)} (${slabs} slab${slabs === 1 ? '' : 's'})`
      : taxKindLabel(input.taxType);
  }

  const rate = Number(input.rate);
  if (Number.isFinite(rate) && rate > 0) {
    return `${taxKindLabel(input.taxType)} ${rate}%`;
  }

  return trimmed;
}

export function resolveTaxCodeForSave(code: string, name: string): string {
  const trimmedCode = code.trim().toUpperCase();
  if (trimmedCode) return trimmedCode;
  return name.replace(/[^a-z0-9]/gi, '').toUpperCase().slice(0, 32);
}

export function taxsplitToSlabs(split: TaxSplit | null | undefined): RangedGstSlab[] {
  if (!split?.items?.length) return [];
  return split.items.map((it) => {
    const slab = new RangedGstSlab();
    slab.priceFrom = Number(it.fromamount) || 0;
    slab.priceTo = Number(it.toamount) || 0;
    slab.ratePercent =
      Number(it.taxpercentage) ||
      (Number(it.cgst) || 0) + (Number(it.sgst) || 0) ||
      0;
    return slab;
  });
}

export function slabsToTaxsplitItems(slabs: RangedGstSlab[]): TaxSplitItem[] {
  return slabs.map((s) => {
    const rate = Number(s.ratePercent) || 0;
    const item = new TaxSplitItem();
    item.fromamount = Number(s.priceFrom) || 0;
    item.toamount = Number(s.priceTo) || 0;
    item.taxpercentage = rate;
    item.cgst = round2(rate / 2);
    item.sgst = round2(rate / 2);
    item.igst = round2(rate / 2);
    item.incost = false;
    return item;
  });
}

export function flatRateTaxsplit(ratePercent: number): TaxSplit {
  const rate = Number(ratePercent) || 0;
  const half = round2(rate / 2);
  const split = new TaxSplit();
  const item = new TaxSplitItem();
  item.fromamount = 0;
  item.toamount = 0;
  item.taxpercentage = rate;
  item.cgst = half;
  item.sgst = half;
  item.igst = half;
  item.incost = false;
  split.items = [item];
  return split;
}
