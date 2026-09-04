import { inject, Injectable, signal } from '@angular/core';

import { PosSessionState } from '../models/pos-session-state';

const POS_SESSION_KEY = 'possession';

@Injectable({ providedIn: 'root' })
export class PosSessionService {
  private readonly state = signal<PosSessionState>(this.read());

  readonly session = this.state.asReadonly();

  open(
    openedAt: string,
    openingBalance: number,
    locationid: number,
    counterid: number,
    cashId = 0,
  ): void {
    const next = new PosSessionState();
    next.isOpen = true;
    next.openedAt = openedAt;
    next.openingBalance = openingBalance;
    next.locationid = locationid;
    next.counterid = counterid;
    next.cashId = cashId;
    this.persist(next);
  }

  close(): void {
    this.persist(new PosSessionState());
  }

  isActiveFor(locationid: number, counterid: number): boolean {
    const session = this.state();
    return (
      session.isOpen &&
      !!session.openedAt &&
      session.locationid === locationid &&
      session.counterid === counterid &&
      locationid > 0 &&
      counterid > 0
    );
  }

  private persist(next: PosSessionState): void {
    this.state.set(next);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(POS_SESSION_KEY, JSON.stringify(next));
    }
  }

  private read(): PosSessionState {
    if (typeof localStorage === 'undefined') return new PosSessionState();
    try {
      const raw = localStorage.getItem(POS_SESSION_KEY);
      if (!raw) return new PosSessionState();
      return Object.assign(new PosSessionState(), JSON.parse(raw));
    } catch {
      return new PosSessionState();
    }
  }
}
