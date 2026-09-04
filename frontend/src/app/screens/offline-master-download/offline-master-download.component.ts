import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';

import { ButtonComponent } from '../../components/button/button.component';
import { DatePickerComponent } from '../../components/date-picker';
import { FormScreenHeaderComponent } from '../../components/form-screen-header';
import { TextInputComponent } from '../../components/text-input/text-input.component';
import { MasterDownloadFormModel, MasterSourceMode } from '../../models/master-download';
import { MasterDownloadService } from '../../services/master-download.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-offline-master-download',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    FormScreenHeaderComponent,
    TextInputComponent,
    DatePickerComponent,
    ButtonComponent,
  ],
  templateUrl: './offline-master-download.component.html',
  styleUrl: './offline-master-download.component.scss',
})
export class OfflineMasterDownloadComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly masterDownload = inject(MasterDownloadService);
  private readonly notifications = inject(NotificationService);

  protected readonly draft = new MasterDownloadFormModel();
  protected readonly isSubmitting = signal(false);
  protected readonly statusText = signal('Master Download');
  protected readonly lastDownloadedAt = signal<string | null>(null);

  ngOnInit(): void {
    const settings = this.masterDownload.loadSettings();
    this.draft.masterSource = settings.masterSource;
    this.draft.serverUrl =
      settings.masterSource === 'Client' ? settings.clientUrl : settings.serverUrl;
    this.draft.organizationCode = settings.organizationCode;
    this.draft.locationCode = settings.locationCode;
    this.draft.dataPath = settings.dataPath;
    this.draft.zeroStockFrom = todayIso();
    this.lastDownloadedAt.set(settings.lastDownloadedAt);
  }

  protected get serverLabel(): string {
    return this.draft.masterSource === 'Client' ? 'Client' : 'Server';
  }

  protected onMasterSourceChange(source: MasterSourceMode): void {
    const settings = this.masterDownload.loadSettings();
    this.draft.masterSource = source;
    this.draft.serverUrl = source === 'Client' ? settings.clientUrl : settings.serverUrl;
  }

  protected goBack(): void {
    void this.router.navigate(['/dashboard']);
  }

  protected async submit(form: NgForm): Promise<void> {
    for (const control of Object.values(form.controls)) {
      control.markAsTouched();
    }
    if (form.invalid || this.isSubmitting()) return;

    if (!this.draft.serverUrl.trim()) {
      this.notifications.error(`${this.serverLabel} URL is required.`);
      return;
    }
    if (!this.draft.dataPath.trim()) {
      this.notifications.error('Data folder path is required.');
      return;
    }

    if (this.draft.masterSource === 'Server') {
      if (!this.draft.locationCode.trim()) {
        this.notifications.error('Location code is required.');
        return;
      }
      if (!this.draft.username.trim() || !this.draft.password) {
        this.notifications.error('Username and password are required.');
        return;
      }
    }

    this.isSubmitting.set(true);
    this.statusText.set('Downloading master data...');

    try {
      const result = await this.masterDownload.downloadFromForm(this.draft);
      if (result.error) {
        this.notifications.error(result.errormessage || 'Master download failed.');
        return;
      }

      this.notifications.success(
        `Master Data Successfully loaded — save files under ${this.draft.dataPath.trim()}`,
      );
      this.lastDownloadedAt.set(
        this.masterDownload.loadSettings().lastDownloadedAt ?? new Date().toISOString(),
      );
      void this.router.navigate(['/dashboard']);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Master download failed.';
      this.notifications.error(message);
    } finally {
      this.isSubmitting.set(false);
      this.statusText.set('Master Download');
    }
  }
}

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
