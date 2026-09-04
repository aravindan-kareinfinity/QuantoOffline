import {
  afterNextRender,
  booleanAttribute,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NgControl, ValidationErrors } from '@angular/forms';
import { merge, startWith } from 'rxjs';

export type AppTextInputType =
  | 'text'
  | 'email'
  | 'password'
  | 'tel'
  | 'number'
  | 'url'
  | 'search'
  | 'date';

export type TextInputEnterEvent = {
  event: Event;
  value: string;
};

export type AppTextInputSize = 'md' | 'sm';

@Component({
  selector: 'app-text-input',
  standalone: true,
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block w-full',
  },
})
export class TextInputComponent implements ControlValueAccessor {
  private static nextId = 0;

  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngControl = inject(NgControl, { optional: true, self: true });
  private readonly controlTick = signal(0);
  private errorWatchStarted = false;

  readonly label = input('');
  readonly placeholder = input('');
  readonly type = input<AppTextInputType>('text');
  /** `sm` renders a compact box for inline grid editing. */
  readonly size = input<AppTextInputSize>('md');
  readonly disabled = input(false);
  readonly required = input(false, { transform: booleanAttribute });
  /** Manual override; when empty, errors are read from the bound form control. */
  readonly error = input('');
  readonly hint = input('');
  readonly min = input<number | null>(null);
  readonly max = input<number | null>(null);
  readonly autocomplete = input<string | null>(null);
  readonly inputId = input(`app-text-input-${++TextInputComponent.nextId}`);
  readonly enterPressed = output<TextInputEnterEvent>();
  readonly blurred = output<void>();

  protected readonly value = signal('');
  protected readonly cvaDisabled = signal(false);
  protected readonly showPassword = signal(false);
  protected readonly focused = signal(false);

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }

    afterNextRender(() => this.setupErrorWatch());
  }

  private setupErrorWatch(): void {
    if (this.errorWatchStarted) return;
    const control = this.ngControl?.control;
    if (!control) return;
    this.errorWatchStarted = true;
    watchFormControlErrors(control, this.destroyRef, this.controlTick);
  }

  protected readonly isDisabled = computed(() => this.disabled() || this.cvaDisabled());

  protected readonly displayError = computed(() => {
    this.controlTick();
    const manual = this.error().trim();
    if (manual) return manual;
    return getFormControlError(this.ngControl?.control, this.ngControl?.name ?? null);
  });

  protected readonly isPassword = computed(() => this.type() === 'password');

  protected readonly inputType = computed(() => {
    if (!this.isPassword()) return this.type();
    return this.showPassword() ? 'text' : 'password';
  });

  protected readonly describedBy = computed(() => {
    const ids: string[] = [];
    if (this.displayError()) ids.push(`${this.inputId()}-error`);
    else if (this.hint()) ids.push(`${this.inputId()}-hint`);
    return ids.length ? ids.join(' ') : null;
  });

  writeValue(value: unknown): void {
    this.value.set(value == null ? '' : String(value));
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
    this.cdr.markForCheck();
  }

  protected onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let next = input.value;
    if (this.type() === 'number' && next !== '') {
      const parsed = Number(next);
      if (!Number.isNaN(parsed)) {
        const min = this.min();
        const max = this.max();
        let clamped = parsed;
        if (min != null && clamped < min) clamped = min;
        if (max != null && clamped > max) clamped = max;
        if (clamped !== parsed) {
          next = String(clamped);
          input.value = next;
        }
      }
    }
    this.value.set(next);
    this.onChange(next);
  }

  protected onBlur(): void {
    this.focused.set(false);
    this.onTouched();
    this.blurred.emit();
  }

  protected onEnter(event: Event): void {
    this.enterPressed.emit({ event, value: this.value() });
  }

  protected togglePassword(): void {
    this.showPassword.update((v) => !v);
  }
}

type FormControlErrorMessages = Record<string, string>;

interface FormControlErrorOptions {
  messages?: FormControlErrorMessages;
  field?: string | number | null;
  firstOnly?: boolean;
  whenDirty?: boolean;
}

type ErrorValue = ValidationErrors[string];
type BuiltinMessageResolver = string | ((value: ErrorValue) => string);

const BUILTIN_ERROR_MESSAGES: Record<string, BuiltinMessageResolver> = {
  required: 'This field is required.',
  email: 'Invalid email format.',
  phone: 'Enter a valid phone number.',
  pattern: 'Invalid format.',
  format: 'Invalid format.',
  minlength: (value) =>
    `Minimum length is ${(value as { requiredLength: number }).requiredLength} characters.`,
  maxlength: (value) =>
    `Maximum length is ${(value as { requiredLength: number }).requiredLength} characters.`,
  min: (value) => `Minimum value is ${(value as { min: number }).min}.`,
  max: (value) => `Maximum value is ${(value as { max: number }).max}.`,
};

const FIELD_ERROR_MESSAGES: Record<string, FormControlErrorMessages> = {
  gstno: { format: 'Enter a valid GSTIN', pattern: 'Enter a valid GSTIN' },
  gstNumber: { format: 'Enter a valid GSTIN', pattern: 'Enter a valid GSTIN' },
  gstNo: { format: 'Invalid GSTIN', pattern: 'Invalid GSTIN' },
  pincode: { format: 'PIN code must be numeric', pattern: 'PIN code must be numeric' },
  pan: { format: 'Enter a valid PAN', pattern: 'Enter a valid PAN' },
  bankAccountNumber: { format: 'Account number must be numeric', pattern: 'Account number must be numeric' },
  ifscCode: { format: 'Enter a valid IFSC code', pattern: 'Enter a valid IFSC code' },
};

function normalizeErrorOptions(
  fieldOrOptions?: FormControlErrorOptions | string | number | null,
): FormControlErrorOptions {
  if (fieldOrOptions == null || typeof fieldOrOptions === 'object') {
    return fieldOrOptions ?? {};
  }
  return { field: fieldOrOptions };
}

function resolveBuiltinMessage(key: string, errorValue: ErrorValue): string {
  const resolver = BUILTIN_ERROR_MESSAGES[key];
  if (typeof resolver === 'function') return resolver(errorValue);
  if (resolver) return resolver;
  return 'Invalid field.';
}

function messageForErrorKey(
  key: string,
  errors: ValidationErrors,
  options: FormControlErrorOptions,
): string {
  const custom = options.messages?.[key];
  if (custom) return custom;

  const field = options.field != null ? String(options.field) : '';
  const fieldMessage = field ? FIELD_ERROR_MESSAGES[field]?.[key] : undefined;
  if (fieldMessage) return fieldMessage;

  return resolveBuiltinMessage(key, errors[key]);
}

function shouldShowFormControlErrors(
  control: import('@angular/forms').AbstractControl | null | undefined,
  whenDirty = true,
): boolean {
  if (!control?.errors) return false;
  return control.touched || (whenDirty && control.dirty);
}

function getFormControlErrors(
  control: import('@angular/forms').AbstractControl | null | undefined,
  fieldOrOptions?: FormControlErrorOptions | string | number | null,
): string[] {
  const options = normalizeErrorOptions(fieldOrOptions);
  const whenDirty = options.whenDirty !== false;

  if (!shouldShowFormControlErrors(control, whenDirty) || !control?.errors) {
    return [];
  }

  const messages = Object.keys(control.errors).map((key) =>
    messageForErrorKey(key, control.errors!, options),
  );

  return options.firstOnly === false ? messages : messages.slice(0, 1);
}

function getFormControlError(
  control: import('@angular/forms').AbstractControl | null | undefined,
  fieldOrOptions?: FormControlErrorOptions | string | number | null,
): string {
  return getFormControlErrors(control, fieldOrOptions)[0] ?? '';
}

function watchFormControlErrors(
  control: import('@angular/forms').AbstractControl | null | undefined,
  destroyRef: DestroyRef,
  tick: import('@angular/core').WritableSignal<number>,
): void {
  if (!control) return;

  merge(control.statusChanges, control.valueChanges, control.events)
    .pipe(startWith(null), takeUntilDestroyed(destroyRef))
    .subscribe(() => tick.update((value) => value + 1));
}
