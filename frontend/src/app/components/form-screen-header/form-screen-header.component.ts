import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { ButtonComponent, type AppButtonVariant } from '../button/button.component';
import { IconComponent } from '../icon';

@Component({
  selector: 'app-form-screen-header',
  standalone: true,
  imports: [ButtonComponent, IconComponent],
  templateUrl: './form-screen-header.component.html',
  styleUrl: './form-screen-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormScreenHeaderComponent {
  readonly backLabel = input.required<string>();
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly isSaving = input(false);
  readonly saveDisabled = input(false);
  readonly showActions = input(true);
  /** Primary action label (default Save). */
  readonly saveLabel = input('Save');
  /** Label while isSaving is true (default Saving…). */
  readonly saveLoadingLabel = input('Saving…');
  /** Primary action button variant (default primary). */
  readonly saveVariant = input<AppButtonVariant>('primary');

  readonly backClicked = output<void>();
  readonly cancelClicked = output<void>();
  readonly saveClicked = output<void>();
}
