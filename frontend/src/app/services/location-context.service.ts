import { inject, Injectable, signal } from '@angular/core';

import { LocationContext } from '../models/location-context';
import {
  REFERENCE_LIST_GENERAL_TYPES,
  REFERENCE_LIST_READ_SEGMENTS,
} from '../models/reference-list';
import { ReferenceListService } from './reference-list.service';

const LOCATION_CONTEXT_KEY = 'locationcontext';

@Injectable({ providedIn: 'root' })
export class LocationContextService {
  private readonly referenceListService = inject(ReferenceListService);
  private readonly state = signal<LocationContext>(this.read());

  readonly context = this.state.asReadonly();

  constructor() {
    if (typeof window !== 'undefined') {
      window.addEventListener('storage', (event: StorageEvent) => {
        if (event.key === LOCATION_CONTEXT_KEY) {
          this.state.set(this.read());
        }
      });
    }
  }

  get value(): LocationContext {
    return this.state();
  }

  setCompany(id: number | null, name: string): void {
    const next = { ...this.state() };
    next.companyid = id;
    next.companyName = name.trim();
    next.locationid = null;
    next.locationName = '';
    next.counterid = null;
    next.counterName = '';
    this.persist(next);
  }

  setLocation(id: number | null, name: string): void {
    const next = { ...this.state() };
    next.locationid = id;
    next.locationName = name.trim();
    next.counterid = null;
    next.counterName = '';
    this.persist(next);
  }

  setCounter(id: number | null, name: string): void {
    const next = { ...this.state() };
    next.counterid = id;
    next.counterName = name.trim();
    this.persist(next);
  }

  patch(ctx: Partial<LocationContext>): void {
    this.persist({ ...this.state(), ...ctx });
  }

  clear(): void {
    this.persist(new LocationContext());
  }

  /** Fill display names from API when ids exist but labels were not saved yet. */
  async ensureDisplayNames(): Promise<void> {
    const ctx = this.value;
    const needCompany = ctx.companyid != null && ctx.companyid > 0 && !ctx.companyName.trim();
    const needLocation = ctx.locationid != null && ctx.locationid > 0 && !ctx.locationName.trim();
    const needCounter = ctx.counterid != null && ctx.counterid > 0 && !ctx.counterName.trim();
    if (!needCompany && !needLocation && !needCounter) return;

    const patch: Partial<LocationContext> = {};

    if (needCompany) {
      const companies = await this.referenceListService.readSegment<{ id: number; name: string }>(
        REFERENCE_LIST_READ_SEGMENTS.COMPANY,
      );
      const company = companies.find((row) => row.id === ctx.companyid);
      if (company?.name) patch.companyName = company.name.trim();
    }

    if (needLocation) {
      const locations = await this.referenceListService.readSegment<{ id: number; name: string }>(
        REFERENCE_LIST_READ_SEGMENTS.RETAILLOCATION,
      );
      const location = locations.find((row) => row.id === ctx.locationid);
      if (location?.name) patch.locationName = location.name.trim();
    }

    if (needCounter) {
      const counters = await this.referenceListService.search(REFERENCE_LIST_GENERAL_TYPES.COUNTER);
      const counter = counters.find((row) => row.id === ctx.counterid);
      if (counter) {
        patch.counterName = counter.name?.trim() || counter.code?.trim() || String(counter.id);
      }
    }

    if (Object.keys(patch).length > 0) {
      this.patch(patch);
    }
  }

  private persist(ctx: LocationContext): void {
    this.state.set(ctx);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(LOCATION_CONTEXT_KEY, JSON.stringify(ctx));
    }
  }

  private read(): LocationContext {
    if (typeof localStorage === 'undefined') return new LocationContext();
    try {
      const raw = localStorage.getItem(LOCATION_CONTEXT_KEY);
      if (!raw) return new LocationContext();
      const parsed = JSON.parse(raw) as unknown;
      if (!parsed || typeof parsed !== 'object') return new LocationContext();
      const src = parsed as Record<string, unknown>;
      const ctx = new LocationContext();
      ctx.companyid = typeof src['companyid'] === 'number' ? src['companyid'] : null;
      ctx.companyName = String(src['companyName'] ?? '');
      ctx.locationid = typeof src['locationid'] === 'number' ? src['locationid'] : null;
      ctx.locationName = String(src['locationName'] ?? '');
      ctx.counterid = typeof src['counterid'] === 'number' ? src['counterid'] : null;
      ctx.counterName = String(src['counterName'] ?? '');
      return ctx;
    } catch {
      return new LocationContext();
    }
  }
}
