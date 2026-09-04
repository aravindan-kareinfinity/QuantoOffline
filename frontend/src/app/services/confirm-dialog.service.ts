import { Injectable, signal } from '@angular/core';

export interface ConfirmDialogOptions {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  danger?: boolean;
}

interface ConfirmDialogRequest extends Required<ConfirmDialogOptions> {
  resolve: (confirmed: boolean) => void;
}

/** Promise-based replacement for `window.confirm` — paired with `app-confirm-dialog`. */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly state = signal<ConfirmDialogRequest | null>(null);
  readonly request = this.state.asReadonly();

  confirm(options: ConfirmDialogOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.state.set({
        title: options.title ?? 'Please confirm',
        message: options.message,
        confirmText: options.confirmText ?? 'Delete',
        cancelText: options.cancelText ?? 'Cancel',
        danger: options.danger ?? true,
        resolve,
      });
    });
  }

  respond(confirmed: boolean): void {
    this.state()?.resolve(confirmed);
    this.state.set(null);
  }
}
