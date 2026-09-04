import {
  HttpContextToken,
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../environments/environment';
import { StorageService } from '../services/storage.service';
import { NotificationService } from '../services/notification.service';

/** Skip Bearer token (login / refresh / public endpoints). */
export const SKIP_AUTH = new HttpContextToken<boolean>(() => false);

function isAuthEndpoint(url: string): boolean {
  return url.toLowerCase().includes('/token');
}

function messageFromBody(err: HttpErrorResponse): string | null {
  let body: unknown = err.error;
  if (!body) return null;

  if (typeof body === 'string' && body.trim()) {
    const text = body.trim();
    try {
      body = JSON.parse(text) as unknown;
    } catch {
      return text;
    }
  }

  if (typeof body === 'object' && body) {
    const record = body as Record<string, unknown>;
    for (const key of ['error_description', 'ErrorMessage', 'message', 'Message', 'title']) {
      const value = record[key];
      if (typeof value === 'string' && value.trim()) return value.trim();
    }
  }
  return null;
}

function prepareRequest(req: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  const setHeaders: Record<string, string> = {};

  if (!req.context.get(SKIP_AUTH) && !isAuthEndpoint(req.url) && token) {
    setHeaders['Authorization'] = `Bearer ${token}`;
  }
  if (!req.headers.has('Accept')) {
    setHeaders['Accept'] = 'application/json';
  }
  if (
    req.body &&
    typeof req.body === 'object' &&
    !(req.body instanceof FormData) &&
    !(req.body instanceof URLSearchParams) &&
    !req.headers.has('Content-Type')
  ) {
    setHeaders['Content-Type'] = 'application/json';
  }
  if (!req.headers.has('X-App-Version')) {
    setHeaders['X-App-Version'] = environment.appVersion;
  }

  return Object.keys(setHeaders).length ? req.clone({ setHeaders }) : req;
}

function handleHttpError(
  err: HttpErrorResponse,
  req: HttpRequest<unknown>,
  notifications: NotificationService,
  storage: StorageService,
  router: Router,
): void {
  const isTokenCall = isAuthEndpoint(req.url);

  if (err.status === 0) {
    notifications.error('Please check your internet connection.');
    return;
  }

  if (err.status === 401 && !isTokenCall) {
    storage.clear();
    notifications.error(
      messageFromBody(err) ?? 'Your session has expired. Please sign in again.',
    );
    void router.navigate(['/login']);
    return;
  }

  if (err.status === 403) {
    notifications.error(messageFromBody(err) ?? 'Access denied.');
    return;
  }

  if (err.status >= 500) {
    notifications.error(messageFromBody(err) ?? 'Something went wrong. Please try again.');
    return;
  }

  if (err.status >= 400) {
    const msg = messageFromBody(err);
    if (msg) {
      notifications[isTokenCall ? 'error' : 'warning'](msg);
    } else if (isTokenCall) {
      notifications.error('Login failed.');
    }
  }
}

export const appInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  const router = inject(Router);
  const storage = inject(StorageService);

  return next(prepareRequest(req, storage.getAccessToken())).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        handleHttpError(err, req, notifications, storage, router);
      }
      return throwError(() => err);
    }),
  );
};
