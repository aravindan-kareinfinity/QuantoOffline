import {
  ReferenceList,
  ReferenceListAdditionalInfo,
  type ReferenceListTypeCode,
} from '../models/reference-list';
import type { ReferenceListService } from '../services/reference-list.service';

export function deriveReferenceCode(
  name: string,
  existingCode?: string,
  fallback = 'REF',
): string {
  const kept = (existingCode ?? '').trim();
  if (kept) return kept;
  const slug = name
    .trim()
    .toUpperCase()
    .replace(/[^A-Z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 24);
  return slug || fallback;
}

export function refInfo(ref: ReferenceList): ReferenceListAdditionalInfo {
  const raw = ref.additionalinfo;
  if (raw == null) return {};
  if (typeof raw === 'string') {
    try {
      const parsed = JSON.parse(raw) as unknown;
      return typeof parsed === 'object' && parsed !== null
        ? (parsed as ReferenceListAdditionalInfo)
        : {};
    } catch {
      return {};
    }
  }
  return raw as ReferenceListAdditionalInfo;
}

export function defaultReferenceListIdByName(
  items: ReferenceList[],
  name = 'india',
): number | null {
  const wanted = name.trim().toLowerCase();
  const match = items.find((row) => row.name?.trim().toLowerCase() === wanted);
  return match?.id ?? null;
}

export async function referenceName(
  service: ReferenceListService,
  type: ReferenceListTypeCode | string,
  id: number | null | undefined,
): Promise<string> {
  if (!id) return '';
  const rows = await service.getByType(type);
  return rows.find((row) => row.id === id)?.name?.trim() ?? '';
}

export async function ensureReferenceDropdownSelection(
  service: ReferenceListService,
  type: ReferenceListTypeCode | string,
  items: ReferenceList[],
  id: number | null,
  fallbackName: string,
): Promise<{ items: ReferenceList[]; resolvedId: number | null }> {
  if (id) {
    if (items.some((x) => x.id === id)) {
      return { items, resolvedId: id };
    }

    const row = await service.getById(type as ReferenceListTypeCode, id);

    if (row) {
      items = [...items, row].sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
    }

    return { items, resolvedId: id };
  }

  const match = items.find(
    (x) => x.name?.trim().toLowerCase() === fallbackName.trim().toLowerCase(),
  );

  return {
    items,
    resolvedId: match?.id ?? null,
  };
}
