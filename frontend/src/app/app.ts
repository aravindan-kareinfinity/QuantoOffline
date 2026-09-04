import { NgClass } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ConfirmDialogComponent } from './components/confirm-dialog';
import { NotificationService } from './services/notification.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ConfirmDialogComponent, NgClass],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly notifications = inject(NotificationService);
}
