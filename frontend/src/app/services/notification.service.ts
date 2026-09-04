import { Injectable, signal } from '@angular/core';

export type NotificationKind = 'error' | 'warning' | 'info' | 'success';

export class AppNotification {
  id = 0;
  kind: NotificationKind = 'info';
  message = '';
}

/** Lightweight global toasts — no third-party dependency. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private seq = 0;
  private readonly items = signal<AppNotification[]>([]);
  readonly notifications = this.items.asReadonly();

  error(message: string): void {
    this.push('error', message);
  }

  warning(message: string): void {
    this.push('warning', message);
  }

  info(message: string): void {
    this.push('info', message);
  }

  success(message: string): void {
    this.push('success', message);
  }

  dismiss(id: number): void {
    this.items.update((list) => list.filter((n) => n.id !== id));
  }

  private push(kind: NotificationKind, message: string): void {
    const n = new AppNotification();
    n.id = ++this.seq;
    n.kind = kind;
    n.message = message;
    this.items.update((list) => [...list, n]);
    window.setTimeout(() => this.dismiss(n.id), 4500);
  }
}
