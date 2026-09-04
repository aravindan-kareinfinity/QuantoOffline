import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';

export type AppButtonType = 'button' | 'submit' | 'reset';
export type AppButtonVariant =
  | 'primary'
  | 'secondary'
  | 'outline'
  | 'outline-danger'
  | 'danger'
  | 'success'
  | 'warning'
  | 'link'
  | 'ghost';
export type AppButtonSize = 'sm' | 'md' | 'lg';
export type AppButtonIconPosition = 'left' | 'right';

@Component({
  selector: 'app-button',
  standalone: true,
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inline-flex',
    '[class.w-full]': 'fullWidth()',
  },
})
export class ButtonComponent {
  readonly type = input<AppButtonType>('button');
  readonly variant = input<AppButtonVariant>('primary');
  readonly size = input<AppButtonSize>('md');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly fullWidth = input(false);
  readonly iconPosition = input<AppButtonIconPosition>('left');
  /** Square icon button — pair with `ariaLabel` and projected `[buttonIcon]`. */
  readonly iconOnly = input(false);
  /** Optional accessible name when the button has no visible text. */
  readonly ariaLabel = input<string | null>(null);

  readonly clicked = output<MouseEvent>();

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());

  protected readonly classes = computed(() => {
    const base =
      'inline-flex items-center justify-center gap-2 rounded-lg font-semibold transition-colors ' +
      'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 ' +
      'disabled:pointer-events-none disabled:opacity-50';

    const iconOnly = this.iconOnly();
    const sizes: Record<AppButtonSize, string> = iconOnly
      ? {
          sm: 'h-8 w-8 p-0 text-xs',
          md: 'h-9 w-9 p-0 text-xs',
          lg: 'h-11 w-11 p-0 text-sm',
        }
      : {
          sm: 'h-8 px-3 text-xs',
          md: 'h-9 px-3.5 text-xs',
          lg: 'h-11 px-4 text-sm',
        };

    const variants: Record<AppButtonVariant, string> = {
      primary:
        'bg-primary text-primary-foreground hover:bg-primary-hover focus-visible:ring-primary',
      secondary:
        'border border-border bg-surface text-foreground hover:bg-background focus-visible:ring-muted',
      outline:
        'border border-border bg-transparent text-foreground hover:bg-background focus-visible:ring-muted',
      'outline-danger':
        'border border-danger bg-transparent text-danger hover:bg-danger/10 focus-visible:ring-danger',
      danger:
        'bg-danger text-danger-foreground hover:bg-danger-hover focus-visible:ring-danger',
      success:
        'bg-success text-success-foreground hover:bg-success-hover focus-visible:ring-success',
      warning:
        'bg-warning text-warning-foreground hover:bg-warning-hover focus-visible:ring-warning',
      link:
        'h-auto rounded-none bg-transparent px-0 !text-ui text-foreground underline-offset-4 hover:underline focus-visible:ring-muted',
      ghost: 'bg-transparent text-foreground hover:bg-background focus-visible:ring-muted',
    };

    const width = this.fullWidth() ? 'w-full' : '';

    return [base, sizes[this.size()], variants[this.variant()], width].filter(Boolean).join(' ');
  });

  protected onClick(event: MouseEvent): void {
    if (this.isDisabled()) {
      event.preventDefault();
      event.stopPropagation();
      return;
    }
    this.clicked.emit(event);
  }
}
