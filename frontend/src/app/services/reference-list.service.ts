import { HttpClient } from '@angular/common/http';
import { inject, Injectable, WritableSignal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { ActionRequest } from '../models/action-request';
import { Brand } from '../models/brand';
import { environment } from '../../environments/environment';
import {
  REFERENCE_LIST_GENERAL_TYPES,
  REFERENCE_LIST_HR_TYPES,
  REFERENCE_LIST_MGR_TYPES,
  REFERENCE_LIST_POS_TYPES,
  REFERENCE_LIST_PRODUCT_TYPES,
  ReferenceList,
  ReferenceListSearchReq,
  type ReferenceListTypeCode,
} from '../models/reference-list';
import { BrandService } from './brand.service';
import { NotificationService } from './notification.service';
import { Tax } from '../models/tax';

/**
 * ONE service for every backend master under `ReferencelistService`
 * (locations, counters, product attributes, HR lists, etc).
 *
 * `type` picks which master you're talking to — see the catalog in
 * `models/reference-list.ts` for every valid value, e.g.
 * `REFERENCE_LIST_GENERAL_TYPES.LOCATION`, `REFERENCE_LIST_HR_TYPES.FLOOR`.
 *
 * ┌─ Quick start ─────────────────────────────────────────────────────────
 * │ // 1. Fill a dropdown (result is cached — cheap to call often)
 * │ const states = await this.referenceList.getByType(REFERENCE_LIST_GENERAL_TYPES.STATE);
 * │
 * │ // 2. Load rows for a grid (always hits the server, supports filters)
 * │ const rows = await this.referenceList.search(REFERENCE_LIST_GENERAL_TYPES.LOCATION, {
 * │   inactive: this.includeInactive(),
 * │ });
 * │
 * │ // 3. Save (create or update) — set `type` + `code` + `name` on the row
 * │ const row = Object.assign(new ReferenceList(), existingRow ?? {});
 * │ row.type = REFERENCE_LIST_GENERAL_TYPES.LOCATION;
 * │ row.code = 'HQ';
 * │ row.name = 'Head Office';
 * │ await this.referenceList.save(row);
 * │
 * │ // 4. Delete a row (it already has `.type` on it from step 2)
 * │ await this.referenceList.delete(row);
 * └───────────────────────────────────────────────────────────────────────
 */
@Injectable({ providedIn: 'root' })
export class ReferenceListService {
  private readonly http = inject(HttpClient);
  private readonly notifications = inject(NotificationService);
  private readonly brandService = inject(BrandService);
  private readonly baseUrl = `${environment.baseUrl}/ReferencelistService`;

  /** Per-type cache used only by {@link getByType}. Cleared automatically on save/delete. */
  private readonly cache = new Map<string, ReferenceList[] | Tax[]>();

  /**
   * Get all rows for a master type, from cache when possible.
   *
   * Use this for **dropdown options** and other lists that rarely change —
   * it's safe to call from multiple components without re-hitting the server.
   * For a master screen's own grid, use {@link search} instead so edits show
   * up immediately.
   */
  /**
   * Read-only lookup segments served by `GET ReferencelistService/{segment}`
   * (products, transport, brand list, etc.).
   */
  async readSegment<T extends { id: number; name: string }>(
    segment: ReferenceListTypeCode,
  ): Promise<T[]> {
    const key = this.normalizeType(segment);
    const rows = await firstValueFrom(this.http.get<T[]>(`${this.baseUrl}/${key}`));
    return Array.isArray(rows) ? rows : [];
  }

  async getByType(type: ReferenceListTypeCode): Promise<ReferenceList[] | Tax[]> {
    const key = this.normalizeType(type);
    const cached = this.cache.get(key);
    if (cached) return cached;

    // Brand lives in the `brand` table (BrandService / GET ReferencelistService/BRAND),
    // not in `referencelist`. Other product attrs use POST /search.
    const rows =
      key === REFERENCE_LIST_PRODUCT_TYPES.BRAND
        ? await this.loadBrandsAsReferenceList()
        : this.isReferenceListMasterType(key)
          ? await this.search(key)
          : await this.searchByType(key);
    this.cache.set(key, rows);
    return rows;
  }
  async searchByType(type: ReferenceListTypeCode): Promise<ReferenceList[] | Tax[]> {
    type = this.normalizeType(type);

    const rows = await firstValueFrom(
      this.http.get<ReferenceList[] | Tax[]>(`${this.baseUrl}/${type}`),
    );
    return Array.isArray(rows) ? rows : [];
  }

  /**
   * Search rows for a master type — always calls the server (no caching).
   * Use this to populate a master screen's grid.
   *
   * ```ts
   * const rows = await this.referenceList.search(
   *   REFERENCE_LIST_GENERAL_TYPES.COUNTER,
   *   { inactive: false },
   * );
   * ```
   */
  async search(
    type: ReferenceListTypeCode,
    options: { id?: number; inactive?: boolean; code?: string; name?: string } = {},
  ): Promise<ReferenceList[]> {
    const body = Object.assign(new ReferenceListSearchReq(), {
      id: options.id ?? 0,
      type: this.normalizeType(type),
      inactive: options.inactive === true,
      code: options.code ?? '',
      name: options.name ?? '',
    });

    const rows = await firstValueFrom(
      this.http.post<ReferenceList[]>(`${this.baseUrl}/search`, new ActionRequest(body)),
    );
    return Array.isArray(rows) ? rows : [];
  }

  /**
   * Create or update a row. `item.type` must already be set (e.g.
   * `REFERENCE_LIST_GENERAL_TYPES.LOCATION`) — an `id` of `0` creates a new
   * row, any other `id` updates the existing one.
   *
   * Clears the whole {@link getByType} cache afterwards, so the next call
   * picks up the change.
   */
  async save(item: ReferenceList): Promise<ReferenceList> {
    const payload = Object.assign(new ReferenceList(), item);
    payload.type = this.normalizeType(payload.type);

    const saved = await firstValueFrom(
      this.http.post<ReferenceList>(`${this.baseUrl}/Save`, new ActionRequest(payload)),
    );
    this.invalidate();
    return saved;
  }

  /**
   * Create or update a row and return both the saved row and the refreshed list
   * for the same master type.
   */
  async saveAndRefresh(
    type: ReferenceListTypeCode,
    name: string,
  ): Promise<{ saved: ReferenceList | null; items: ReferenceList[] }> {
    const trimmedName = name?.trim();
    if (!trimmedName) return { saved: null, items: [] };

    const normalized = this.normalizeType(type);
    if (normalized === REFERENCE_LIST_PRODUCT_TYPES.BRAND) {
      const brand = Object.assign(new Brand(), {
        code: this.codeFromReferenceListName(trimmedName),
        name: trimmedName,
        printingname: trimmedName,
        isactive: true,
      });
      const savedBrand = await this.brandService.save(brand);
      this.invalidate(REFERENCE_LIST_PRODUCT_TYPES.BRAND);
      if (savedBrand?.ErrorMessage) {
        return { saved: Object.assign(new ReferenceList(), { ErrorMessage: savedBrand.ErrorMessage }), items: [] };
      }
      const items = await this.loadBrandsAsReferenceList();
      this.cache.set(REFERENCE_LIST_PRODUCT_TYPES.BRAND, items);
      const saved = items.find((r) => r.id === savedBrand.id) ?? this.brandToReferenceList(savedBrand);
      return { saved, items };
    }

    const payload = Object.assign(new ReferenceList(), {
      type: normalized,
      name: trimmedName,
      code: this.codeFromReferenceListName(trimmedName),
      isactive: true,
    });

    const saved = await this.save(payload);
    const refreshed = await this.getByType(saved.type ?? payload.type);
    return { saved, items: refreshed as ReferenceList[] };
  }

  /** Delete a row. `item.type` must already be set. Also clears the cache. */
  async delete(item: ReferenceList): Promise<boolean> {
    const payload = Object.assign(new ReferenceList(), item);
    payload.type = this.normalizeType(payload.type);

    const ok = await firstValueFrom(
      this.http.post<boolean>(`${this.baseUrl}/Delete`, new ActionRequest(payload)),
    );
    this.invalidate();
    return ok === true;
  }

  /**
   * Manually clear the {@link getByType} cache. Rarely needed — `save` and
   * `delete` already do this for you. Pass a `type` to clear just that one,
   * or nothing to clear everything.
   */
  invalidate(type?: string): void {
    if (type) {
      this.cache.delete(this.normalizeType(type));
      return;
    }
    this.cache.clear();
  }

  async loadItems(type: ReferenceListTypeCode, includeInactive = false): Promise<ReferenceList[]> {
    try {
      return await this.search(type, { inactive: includeInactive });
    } catch (err) {
      console.error(`Could not load reference list "${type}".`, err);
      return [];
    }
  }

  async getById(type: ReferenceListTypeCode, id: number): Promise<ReferenceList | null> {
    if (!Number.isFinite(id) || id <= 0) return null;

    const rows = await this.search(type, { id, inactive: true });
    const match = rows.find((row) => {
      const rowId = typeof row.id === 'number' ? row.id : Number(row.id);
      return Number.isFinite(rowId) && rowId === id;
    });
    return match ?? (rows.length === 1 ? rows[0] : null);
  }
  createAddTagFn(deps: {
    type: ReferenceListTypeCode;
    items: WritableSignal<ReferenceList[]>;
    allowAdd?: boolean;
    onAdded?: (row: ReferenceList) => void;
  }): ((term: string) => Promise<unknown>) | false {
    if (deps.allowAdd === false || !this.isReferenceListMasterType(String(deps.type))) {
      return false;
    }

    return async (term: string) => {
      const name = term.trim();
      if (!name) {
        throw new Error('Name is required.');
      }

      const type = this.normalizeType(String(deps.type));

      if (type === REFERENCE_LIST_PRODUCT_TYPES.BRAND) {
        const brand = Object.assign(new Brand(), {
          code: this.codeFromReferenceListName(name),
          name,
          printingname: name,
          isactive: true,
        });
        const savedBrand = await this.brandService.save(brand);
        if (savedBrand?.ErrorMessage) {
          this.notifications.error(savedBrand.ErrorMessage);
          throw new Error(savedBrand.ErrorMessage);
        }
        const saved = this.brandToReferenceList(savedBrand);
        this.invalidate(REFERENCE_LIST_PRODUCT_TYPES.BRAND);
        deps.items.update((list) =>
          [...list, saved].sort(
            (a, b) =>
              (a.sortingindex ?? 0) - (b.sortingindex ?? 0) ||
              (a.name ?? '').localeCompare(b.name ?? ''),
          ),
        );
        deps.onAdded?.(saved);
        this.notifications.success(`Added "${name}".`);
        return saved;
      }

      const row = Object.assign(new ReferenceList(), {
        type,
        code: this.codeFromReferenceListName(name),
        name,
        isactive: true,
      });

      const saved = await this.save(row);
      if (saved?.ErrorMessage) {
        this.notifications.error(saved.ErrorMessage);
        throw new Error(saved.ErrorMessage);
      }

      deps.items.update((list) =>
        [...list, saved].sort(
          (a, b) =>
            (a.sortingindex ?? 0) - (b.sortingindex ?? 0) ||
            (a.name ?? '').localeCompare(b.name ?? ''),
        ),
      );
      deps.onAdded?.(saved);
      this.notifications.success(`Added "${name}".`);
      return saved;
    };
  }

  private async loadBrandsAsReferenceList(): Promise<ReferenceList[]> {
    const brands = await this.brandService.search({});
    return brands
      .map((b) => this.brandToReferenceList(b))
      .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
  }

  private brandToReferenceList(brand: Brand): ReferenceList {
    const row = new ReferenceList();
    row.id = brand.id;
    row.type = REFERENCE_LIST_PRODUCT_TYPES.BRAND;
    row.code = brand.code ?? '';
    row.name = brand.name ?? '';
    row.isactive = brand.isactive !== false;
    row.version = brand.version ?? 0;
    return row;
  }

  private isReferenceListMasterType(type: string): boolean {
    const key = this.normalizeType(type);
    return (
      Object.values<string>(REFERENCE_LIST_PRODUCT_TYPES).includes(key) ||
      Object.values<string>(REFERENCE_LIST_GENERAL_TYPES).includes(key) ||
      Object.values<string>(REFERENCE_LIST_HR_TYPES).includes(key) ||
      Object.values<string>(REFERENCE_LIST_MGR_TYPES).includes(key) ||
      Object.values<string>(REFERENCE_LIST_POS_TYPES).includes(key) ||
      /^[A-Z]+_PG_\d+$/.test(key)
    );
  }

  private codeFromReferenceListName(name: string): string {
    const compact = name
      .trim()
      .toUpperCase()
      .replace(/[^A-Z0-9]+/g, '');
    return (compact || 'X').padEnd(2, 'X').slice(0, 20);
  }

  private normalizeType(type: string): string {
    return type.trim().toUpperCase();
  }

  async getAgainstParentId(
    type: ReferenceListTypeCode,
    parentid: number,
    options: { id?: number; inactive?: boolean; code?: string; name?: string } = {},
  ): Promise<ReferenceList[]> {
    const body = Object.assign(new ReferenceListSearchReq(), {
      id: options.id ?? 0,
      type: this.normalizeType(type),
      parentid,
      inactive: options.inactive === true,
      code: options.code ?? '',
      name: options.name ?? '',
    });

    const rows = await firstValueFrom(
      this.http.post<ReferenceList[]>(`${environment.baseUrl}/QuantoLiteApi/ReferenceList/Select`, new ActionRequest(body)),
    );
    return Array.isArray(rows) ? rows : [];
  } 
}
