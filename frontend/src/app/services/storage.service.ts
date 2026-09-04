import { Injectable, signal } from '@angular/core';

import { UsersLoginRes } from '../models/users';

const USER_CONTEXT_KEY = 'usercontext';

/** Session storage for login token / claims — synced across tabs. */
@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly defaultContext = new UsersLoginRes();
  private readonly state = signal<UsersLoginRes>(this.read());

  readonly usercontext = this.state.asReadonly();

  constructor() {
    if (typeof window !== 'undefined') {
      window.addEventListener('storage', (event: StorageEvent) => {
        if (event.key === USER_CONTEXT_KEY) {
          this.state.set(this.read());
        }
      });
    }
  }

  set(data: UsersLoginRes): void {
    this.state.set(data);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(USER_CONTEXT_KEY, JSON.stringify(data));
    }
  }

  clear(): void {
    this.state.set(this.defaultContext);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(USER_CONTEXT_KEY);
    }
  }

  get value(): UsersLoginRes {
    return this.state();
  }

  /** In-memory access token from the cached login context. */
  getAccessToken(): string | null {
    return this.state().access_token?.trim() || null;
  }

  private read(): UsersLoginRes {
    if (typeof localStorage === 'undefined') return this.defaultContext;
    try {
      const raw = localStorage.getItem(USER_CONTEXT_KEY);
      if (!raw) return this.defaultContext;
      const parsed = JSON.parse(raw) as unknown;
      return parsed && typeof parsed === 'object' ? (parsed as UsersLoginRes) : this.defaultContext;
    } catch (err) {
      console.error(`Could not read localStorage key "${USER_CONTEXT_KEY}".`, err);
      return this.defaultContext;
    }
  }
}
