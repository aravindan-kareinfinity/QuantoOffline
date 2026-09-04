import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { ButtonComponent } from '../button/button.component';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';

/** Single instance mounted in `app.html`; opened via `ConfirmDialogService.confirm()`. */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [ButtonComponent],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  protected readonly dialog = inject(ConfirmDialogService);

  protected respond(confirmed: boolean): void {
    this.dialog.respond(confirmed);
  }
}
