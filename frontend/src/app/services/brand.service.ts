import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { ActionRequest } from '../models/action-request';
import { Brand, BrandSearchReq } from '../models/brand';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/BrandService`;

  async search(req: Partial<BrandSearchReq> = {}): Promise<Brand[]> {
    const payload = Object.assign(new BrandSearchReq(), req);
    const rows = await firstValueFrom(
      this.http.post<Brand[]>(`${this.baseUrl}/Search`, new ActionRequest(payload)),
    );
    return Array.isArray(rows) ? rows : [];
  }

  async getById(id: number): Promise<Brand | null> {
    if (!Number.isFinite(id) || id <= 0) return null;
    const rows = await this.search({ id });
    return rows.find((row) => row.id === id) ?? rows[0] ?? null;
  }

  async save(brand: Brand): Promise<Brand> {
    const payload = Object.assign(new Brand(), brand);
    if (!payload.printingname?.trim()) {
      payload.printingname = payload.name?.trim() ?? '';
    }
    return await firstValueFrom(
      this.http.post<Brand>(`${this.baseUrl}/Save`, new ActionRequest(payload)),
    );
  }

  async delete(brand: Brand): Promise<boolean> {
    return await firstValueFrom(
      this.http.post<boolean>(`${this.baseUrl}/Delete`, new ActionRequest(brand)),
    );
  }
}
