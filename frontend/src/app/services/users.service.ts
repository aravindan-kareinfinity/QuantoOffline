import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { ActionRequest } from '../models/action-request';
import { SKIP_AUTH } from '../core/http.interceptors';
import { parseLoginResponseBody } from '../core/login-response';
import {
  SystemUser,
  SystemUserSearchReq,
  UsersLoginReq,
  UsersLoginRes,
} from '../models/users';
import { LocationContextService } from './location-context.service';
import { PosSessionService } from './pos-session.service';
import { StorageService } from './storage.service';
import { environment } from '../../environments/environment';

const API_ACCEPT = 'application/json, text/plain, */*';

const tokenContext = new HttpContext().set(SKIP_AUTH, true);

function newDeviceId(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `web-${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

function mergeLoginContext(prev: UsersLoginRes, next: UsersLoginRes): UsersLoginRes {
  const out: Record<string, unknown> = { ...prev };
  for (const [key, value] of Object.entries(next)) {
    if (value === '' || value === undefined || value === null) continue;
    out[key] = value;
  }
  return out as unknown as UsersLoginRes;
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(StorageService);
  private readonly locationContext = inject(LocationContextService);
  private readonly posSession = inject(PosSessionService);
  private readonly router = inject(Router);
  private readonly tokenUrl = `${environment.baseUrl}/token`;
  private readonly usersUrl = `${environment.baseUrl}/QuantoLiteApi/Users`;

  async login(request: UsersLoginReq): Promise<UsersLoginRes> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('username', request.username.trim())
      .set('password', request.password)
      .set('org', 'undefined')
      .set('client_id', 'InfyPOS')
      .set('client_secret', 'KareInfinity')
      .set('platform', 'WEB')
      .set('deviceid', newDeviceId())
      .set('TZ', String(new Date().getTimezoneOffset()));

    const text = await firstValueFrom(
      this.http.post(this.tokenUrl, body.toString(), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: API_ACCEPT,
        },
        context: tokenContext,
        responseType: 'text',
        withCredentials: false,
      }),
    );

    return parseLoginResponseBody(text ?? '');
  }

  async refresh(): Promise<UsersLoginRes> {
    const ctx = this.storage.value;
    const refreshToken = (ctx.refresh_token ?? '').trim();
    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }

    const body = new HttpParams()
      .set('grant_type', 'refresh_token')
      .set('refresh_token', refreshToken)
      .set('client_id', 'InfyPOS')
      .set('client_secret', 'KareInfinity')
      .set('org', ctx.OrganizationCode || 'undefined');

    const text = await firstValueFrom(
      this.http.post(this.tokenUrl, body.toString(), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: API_ACCEPT,
        },
        context: tokenContext,
        responseType: 'text',
        withCredentials: false,
      }),
    );

    const merged = mergeLoginContext(ctx, parseLoginResponseBody(text ?? ''));
    this.storage.set(merged);
    return merged;
  }

  logout(): void {
    this.storage.clear();
    this.locationContext.clear();
    this.posSession.close();
    void this.router.navigate(['/login']);
  }

  async selectUsers(req: Partial<SystemUserSearchReq> = {}): Promise<SystemUser[]> {
    return await firstValueFrom(
      this.http.post<SystemUser[]>(
        `${this.usersUrl}/Select`,
        new ActionRequest({
          id: req.id ?? 0,
          username: req.username ?? '',
        }),
      ),
    );
  }

  async getById(id: number): Promise<SystemUser | null> {
    if (!Number.isFinite(id) || id <= 0) return null;
    const rows = await this.selectUsers({ id });
    return rows.find((row) => row.id === id) ?? (rows.length === 1 ? rows[0] : null);
  }

  async save(user: SystemUser): Promise<SystemUser> {
    return await firstValueFrom(
      this.http.post<SystemUser>(`${this.usersUrl}/Save`, new ActionRequest(user)),
    );
  }

  async delete(user: SystemUser): Promise<boolean> {
    return await firstValueFrom(
      this.http.post<boolean>(`${this.usersUrl}/Delete`, new ActionRequest(user)),
    );
  }
}
