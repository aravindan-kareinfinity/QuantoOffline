import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { SKIP_AUTH } from '../core/http.interceptors';
import {
  ClientMasterPackage,
  MasterDownloadFormModel,
  OfflineMasterSettings,
  WindowsOfflineRequest,
  WindowsOfflineResponse,
} from '../models/master-download';
import { StorageService } from './storage.service';

const SETTINGS_KEY = 'offlineMasterSettings';
const DEVICE_KEY = 'offlineSystemKey';
const POLL_MS = 5000;
const MAX_POLLS = 120; // ~10 minutes

const skipAuth = new HttpContext().set(SKIP_AUTH, true);

@Injectable({ providedIn: 'root' })
export class MasterDownloadService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(StorageService);

  loadSettings(): OfflineMasterSettings {
    const defaults = new OfflineMasterSettings();
    try {
      const raw = localStorage.getItem(SETTINGS_KEY);
      if (!raw) return defaults;
      const parsed = JSON.parse(raw) as Partial<OfflineMasterSettings>;
      return Object.assign(new OfflineMasterSettings(), parsed);
    } catch {
      return defaults;
    }
  }

  saveSettings(settings: OfflineMasterSettings): void {
    localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
  }

  /** Stable browser machine key (mirrors WinForms systemkey). */
  getSystemKey(): string {
    let key = localStorage.getItem(DEVICE_KEY);
    if (!key) {
      key =
        typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
          ? crypto.randomUUID()
          : `web-${Date.now()}-${Math.random().toString(36).slice(2)}`;
      localStorage.setItem(DEVICE_KEY, key);
    }
    return key;
  }

  async downloadFromForm(form: MasterDownloadFormModel): Promise<WindowsOfflineResponse> {
    const dataPath = form.dataPath.trim();
    if (!dataPath) {
      return failResponse('Data folder path is required.');
    }

    const settings = new OfflineMasterSettings();
    settings.masterSource = form.masterSource;
    settings.organizationCode = form.organizationCode.trim();
    settings.locationCode = form.locationCode.trim();
    settings.dataPath = dataPath;
    settings.lastDownloadedAt = this.loadSettings().lastDownloadedAt;

    if (form.masterSource === 'Client') {
      settings.clientUrl = form.serverUrl.trim().replace(/\/+$/, '');
      settings.serverUrl = this.loadSettings().serverUrl;
      this.saveSettings(settings);
      return this.downloadAndApplyClientMaster(settings.clientUrl, dataPath);
    }

    settings.serverUrl = form.serverUrl.trim().replace(/\/+$/, '');
    settings.clientUrl = this.loadSettings().clientUrl;
    this.saveSettings(settings);

    const request = new WindowsOfflineRequest();
    request.orgainzationcode = settings.organizationCode;
    request.locationcode = settings.locationCode;
    request.datatype = 'master';
    request.username = form.username.trim();
    request.password = form.password;
    request.zerostock = form.zeroStock;
    request.zerostockfrom = form.zeroStock
      ? toApiDate(form.zeroStockFrom || todayIso())
      : null;
    request.systemkey = this.getSystemKey();
    request.machinename =
      typeof navigator !== 'undefined' ? navigator.userAgent.slice(0, 120) : 'web';
    request.userid = Number(this.storage.value.userId ?? 0) || 0;

    return this.downloadMasterFromServer(settings.serverUrl, request, dataPath);
  }

  private async downloadMasterFromServer(
    serverUrl: string,
    request: WindowsOfflineRequest,
    dataPath: string,
  ): Promise<WindowsOfflineResponse> {
    const base = serverUrl.replace(/\/+$/, '');
    if (!base) {
      return failResponse('Server URL is required.');
    }

    let result = await firstValueFrom(
      this.http.post<WindowsOfflineResponse>(
        `${base}/SyncController/DownloadWindowsOffline`,
        request,
        { context: skipAuth },
      ),
    ).catch(() => null);

    if (!result) {
      return failResponse('Unable to start master download from Quanto server.');
    }

    let polls = 0;
    while (!result.completed && !result.error && polls < MAX_POLLS) {
      await delay(POLL_MS);
      polls += 1;
      const status = await firstValueFrom(
        this.http.get<WindowsOfflineResponse>(
          `${base}/SyncController/UploadWindowsOfflineStatus/${encodeURIComponent(result.key)}`,
          { context: skipAuth },
        ),
      ).catch(() => null);

      if (status && (status.completed || status.error)) {
        result = status;
        break;
      }
    }

    if (!result.completed && !result.error) {
      return failResponse('Master download timed out. Try again.');
    }

    if (result.completed && result.data) {
      await saveDataFile('master.data', result.data, dataPath);
      this.markDownloaded(dataPath);
      result.data = null;
    }

    return result;
  }

  private async downloadAndApplyClientMaster(
    clientUrl: string,
    dataPath: string,
  ): Promise<WindowsOfflineResponse> {
    if (!clientUrl) {
      return failResponse('Client URL is not configured.');
    }

    const packageResult = await firstValueFrom(
      this.http.get<ClientMasterPackage>(`${clientUrl}/TextilePOS/ClientMaster`, {
        context: skipAuth,
      }),
    ).catch(() => null);

    if (!packageResult) {
      return failResponse('Unable to download master from the client application.');
    }
    if (packageResult.error) {
      return failResponse(packageResult.errormessage || 'Client master download failed.');
    }

    if (packageResult.master) {
      await saveDataFile('master.data', packageResult.master, dataPath);
    }
    if (packageResult.stock) {
      await saveDataFile('stock.data', packageResult.stock, dataPath);
    }
    if (packageResult.customer) {
      await saveDataFile('customer.data', packageResult.customer, dataPath);
    }

    this.markDownloaded(dataPath);

    const ok = new WindowsOfflineResponse();
    ok.completed = true;
    return ok;
  }

  private markDownloaded(dataPath: string): void {
    const settings = this.loadSettings();
    settings.dataPath = dataPath;
    settings.lastDownloadedAt = new Date().toISOString();
    this.saveSettings(settings);
  }
}

function failResponse(message: string): WindowsOfflineResponse {
  const res = new WindowsOfflineResponse();
  res.error = true;
  res.completed = true;
  res.errormessage = message;
  return res;
}

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function todayIso(): string {
  const d = new Date();
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

function toApiDate(isoDate: string): string {
  const [y, m, d] = isoDate.split('-').map(Number);
  return new Date(y, (m || 1) - 1, d || 1).toISOString();
}

function base64ToBytes(base64: string): Uint8Array {
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}

/**
 * Save offline .data file. Prefer File System Access picker (user can choose Data folder);
 * fall back to browser download. Browsers cannot write a raw Windows path from a text field.
 */
async function saveDataFile(filename: string, base64: string, dataPath: string): Promise<void> {
  const bytes = base64ToBytes(base64);
  const copy = new Uint8Array(bytes.byteLength);
  copy.set(bytes);
  const blob = new Blob([copy], { type: 'application/octet-stream' });
  const win = window as Window & {
    showSaveFilePicker?: (options?: {
      suggestedName?: string;
      types?: { description: string; accept: Record<string, string[]> }[];
    }) => Promise<FileSystemFileHandle>;
  };

  if (typeof win.showSaveFilePicker === 'function') {
    try {
      const handle = await win.showSaveFilePicker({
        suggestedName: filename,
        types: [
          {
            description: `Save into Data folder: ${dataPath}`,
            accept: { 'application/octet-stream': ['.data'] },
          },
        ],
      });
      const writable = await handle.createWritable();
      await writable.write(blob);
      await writable.close();
      return;
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        throw new Error(`Save cancelled. Place ${filename} in ${dataPath}`);
      }
    }
  }

  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  URL.revokeObjectURL(url);
}
