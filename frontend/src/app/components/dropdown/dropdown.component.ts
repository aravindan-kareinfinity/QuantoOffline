import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  booleanAttribute,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  contentChild,
  DestroyRef,
  inject,
  input,
  output,
  signal,
  TemplateRef,
  ViewEncapsulation,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, FormsModule, NgControl, ValidationErrors } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { merge, startWith, Subject } from 'rxjs';

type AddTagFn = (term: string) => unknown | Promise<unknown>;
type CompareWithFn = (a: unknown, b: unknown) => boolean;
type GroupValueFn = (key: string, children: unknown[]) => unknown;
type DropdownPosition = 'top' | 'right' | 'bottom' | 'left' | 'auto';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [FormsModule, NgSelectModule, NgTemplateOutlet],
  templateUrl: './dropdown.component.html',
  styleUrl: './dropdown.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ql-dropdown block w-full min-w-0',
    '[class.ql-dropdown--compact]': '!effectiveSearchable()',
    '[class.ql-dropdown--invalid]': '!!displayError()',
  },
})
export class DropdownComponent implements ControlValueAccessor {
  private static nextId = 0;

  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngControl = inject(NgControl, { optional: true, self: true });
  private readonly controlTick = signal(0);
  private errorWatchStarted = false;

  // Quanto shell
  readonly label = input('');
  readonly required = input(false, { transform: booleanAttribute });
  /** When false, invalid styling and messages stay hidden (e.g. until submit). */
  readonly showError = input(true, { transform: booleanAttribute });
  readonly hideErrorMessage = input(false, { transform: booleanAttribute });
  /** Manual override; when empty, errors are read from the bound form control. */
  readonly error = input('');
  readonly hint = input('');
  readonly inputId = input(`app-dropdown-${++DropdownComponent.nextId}`);
  /** Multi-select header: select/clear all enabled options. */
  readonly showSelectAll = input(true);
  /** Property name on each item that marks it disabled (for select-all only). */
  readonly bindDisabled = input('disabled');

  // ng-select pass-through (v20.x API)
  readonly items = input<readonly unknown[]>([]);
  readonly bindLabel = input('');
  readonly bindValue = input('');
  readonly addTag = input<boolean | AddTagFn>(false);
  readonly addTagText = input('Add item');
  readonly appearance = input('');
  readonly appendTo = input('body');
  readonly bufferAmount = input<number | undefined>(undefined);
  readonly closeOnSelect = input<boolean | undefined>(undefined);
  readonly clearAllText = input('');
  readonly clearable = input(true);
  readonly clearOnBackspace = input<boolean | undefined>(undefined);
  readonly compareWith = input<CompareWithFn | undefined>(undefined);
  readonly dropdownPosition = input<DropdownPosition | undefined>(undefined);
  readonly fixedPlaceholder = input<boolean | undefined>(undefined);
  readonly groupBy = input<string | ((value: unknown) => unknown) | undefined>(undefined);
  readonly groupValue = input<GroupValueFn | undefined>(undefined);
  readonly selectableGroup = input<boolean | undefined>(undefined);
  readonly selectableGroupAsModel = input<boolean | undefined>(undefined);
  readonly loading = input(false);
  readonly loadingText = input('Loading…');
  readonly labelForId = input('');
  readonly markFirst = input<boolean | undefined>(undefined);
  readonly maxSelectedItems = input<number | undefined>(undefined);
  readonly hideSelected = input<boolean | undefined>(undefined);
  readonly multiple = input(false);
  readonly notFoundText = input('No options found');
  readonly placeholder = input('Select');
  readonly searchable = input(true);
  readonly readonly = input(false);
  readonly searchFn = input<((term: string, item: unknown) => boolean) | undefined>(undefined);
  readonly searchWhileComposing = input<boolean | undefined>(undefined);
  readonly trackByFn = input<((item: unknown) => unknown) | undefined>(undefined);
  readonly clearSearchOnAdd = input<boolean | undefined>(undefined);
  readonly deselectOnClick = input<boolean | undefined>(undefined);
  readonly editableSearchTerm = input<boolean | undefined>(undefined);
  readonly selectOnTab = input(true);
  readonly tabFocusOnClearButton = input<boolean | undefined>(undefined);
  readonly openOnEnter = input<boolean | undefined>(undefined);
  readonly outsideClickEvent = input<'click' | 'mousedown' | undefined>(undefined);
  readonly typeahead = input<Subject<string> | undefined>(undefined);
  readonly minTermLength = input<number | undefined>(undefined);
  readonly typeToSearchText = input('');
  readonly virtualScroll = input(false);
  readonly inputAttrs = input<Record<string, string> | undefined>(undefined);
  readonly tabIndex = input<number | undefined>(undefined);
  readonly preventToggleOnRightClick = input<boolean | undefined>(undefined);
  readonly keyDownFn = input<((event: KeyboardEvent) => boolean) | undefined>(undefined);

  readonly selectionChange = output<unknown>();
  readonly searchChange = output<string>();
  readonly opened = output<void>();
  readonly closed = output<void>();
  readonly add = output<unknown>();
  readonly blur = output<void>();
  readonly clear = output<void>();
  readonly focus = output<void>();
  readonly remove = output<unknown>();
  readonly scroll = output<{ start: number; end: number }>();
  readonly scrollToEnd = output<void>();

  readonly optionTemplate = contentChild<TemplateRef<{ $implicit: unknown; index: number }>>(
    'dropdownOption',
  );
  readonly selectedTemplate = contentChild<
    TemplateRef<{ $implicit: unknown | unknown[]; values: unknown[] }>
  >('dropdownSelected');
  readonly headerTemplate = contentChild('dropdownHeader', { read: TemplateRef });
  readonly footerTemplate = contentChild('dropdownFooter', { read: TemplateRef });

  protected readonly value = signal<unknown>(null);
  protected readonly cvaDisabled = signal(false);

  private onChange: (value: unknown) => void = () => undefined;
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

  protected readonly displayError = computed(() => {
    this.controlTick();
    if (!this.showError()) return '';
    const manual = this.error().trim();
    if (manual) return manual;
    return getFormControlError(this.ngControl?.control, this.ngControl?.name ?? null);
  });

  protected readonly isDisabled = computed(() => this.readonly() || this.cvaDisabled());

  protected readonly addTagEnabled = computed(() => {
    const tag = this.addTag();
    return tag === true || typeof tag === 'function';
  });

  /** Pass addTag through to ng-select (boolean or create fn). */
  protected readonly effectiveAddTag = computed((): boolean | AddTagFn => {
    const tag = this.addTag();
    return tag === false ? false : tag;
  });

  /** Let users type a new tag name even when a value is already selected. */
  protected readonly effectiveEditableSearchTerm = computed(
    () => this.editableSearchTerm() ?? this.addTagEnabled(),
  );

  /** Avoid the selected label blocking the add-tag row for the same search text. */
  protected readonly effectiveHideSelected = computed(
    () => this.hideSelected() ?? this.addTagEnabled(),
  );

  protected readonly effectiveMarkFirst = computed(
    () => this.markFirst() ?? !this.addTagEnabled(),
  );

  protected readonly effectiveSearchable = computed(() => this.searchable());

  protected readonly effectiveClearable = computed(
    () => this.clearable() && !this.required(),
  );

  protected readonly effectiveCloseOnSelect = computed(() =>
    this.closeOnSelect() ?? !this.multiple(),
  );

  protected readonly effectiveCompareWith = computed((): CompareWithFn =>
    DropdownComponent.createCompareWith(this.bindValue(), this.compareWith()),
  );

  protected readonly effectiveGroupBy = computed(
    (): string | ((value: unknown) => unknown) => this.groupBy() ?? '',
  );

  protected readonly effectiveGroupValue = computed(
    (): GroupValueFn => this.groupValue() ?? ((key) => key),
  );

  protected readonly effectiveInputAttrs = computed(
    (): Record<string, string> => this.inputAttrs() ?? {},
  );

  protected readonly effectiveKeyDownFn = computed(
    (): ((event: KeyboardEvent) => boolean) => this.keyDownFn() ?? (() => true),
  );

  protected readonly selectedValueList = computed(() => {
    const current = this.value();
    if (this.multiple()) {
      return Array.isArray(current) ? current : current == null ? [] : [current];
    }
    return current == null ? [] : [current];
  });

  protected readonly allEnabledSelected = computed(() => {
    if (!this.multiple()) return false;
    const selected = this.selectedValueList();
    const enabled = this.enabledItems();
    return enabled.length > 0 && enabled.every((item) => selected.includes(this.itemValue(item)));
  });

  protected readonly hasEnabledItems = computed(() => this.enabledItems().length > 0);

  writeValue(value: unknown): void {
    if (this.multiple()) {
      this.value.set(Array.isArray(value) ? [...value] : value == null ? [] : [value]);
    } else {
      this.value.set(this.normalizeSingleValue(value));
    }
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (value: unknown) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
    this.cdr.markForCheck();
  }

  protected onModelChange(next: unknown): void {
    const normalized = this.multiple()
      ? Array.isArray(next)
        ? next
        : next == null
          ? []
          : [next]
      : this.normalizeSingleValue(next);

    this.value.set(normalized);
    this.onChange(normalized);
    this.selectionChange.emit(normalized);
  }

  protected onSearch(event: { term: string }): void {
    this.searchChange.emit(event.term ?? '');
  }

  protected onOpen(): void {
    this.opened.emit();
  }

  protected onClose(): void {
    this.onTouched();
    this.closed.emit();
  }

  protected onAdd(item: unknown): void {
    this.add.emit(item);
  }

  protected toggleSelectAll(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.multiple() || this.isDisabled()) return;

    const enabled = this.enabledItems();
    const next = this.allEnabledSelected()
      ? []
      : enabled.map((item) => this.itemValue(item));

    this.onModelChange(next);
  }

  protected optionContext(option: unknown, index: number): { $implicit: unknown; index: number } {
    return { $implicit: option, index };
  }

  protected selectedContext(
    item: unknown | unknown[],
    values: unknown[],
  ): { $implicit: unknown | unknown[]; values: unknown[] } {
    return { $implicit: item, values };
  }

  protected itemLabel(item: unknown): string {
    if (item == null) return '';
    const key = this.bindLabel();
    if (key && typeof item === 'object') {
      const label = (item as Record<string, unknown>)[key];
      if (label != null && label !== '') return String(label);
    }
    return String(item);
  }

  private enabledItems(): unknown[] {
    return this.items().filter((item) => !this.itemDisabled(item));
  }

  private itemValue(item: unknown): unknown {
    const key = this.bindValue();
    if (!key || item == null || typeof item !== 'object') return item;
    return (item as Record<string, unknown>)[key];
  }

  private itemDisabled(item: unknown): boolean {
    const key = this.bindDisabled();
    if (!key || item == null || typeof item !== 'object') return false;
    return !!(item as Record<string, unknown>)[key];
  }

  /** ng-select shows placeholder only when value is null/undefined. */
  private normalizeSingleValue(value: unknown): unknown {
    if (value === '' || value == null) return null;
    if (this.bindValue() === 'id') {
      const n = typeof value === 'number' ? value : Number(value);
      if (!Number.isFinite(n) || n <= 0) return null;
      return n;
    }
    return value;
  }

  private static resolveBindValue(item: unknown, bindValueKey: string): unknown {
    if (item == null || !bindValueKey || typeof item !== 'object') return item;
    return (item as Record<string, unknown>)[bindValueKey];
  }

  private static createCompareWith(
    bindValueKey: string,
    custom: CompareWithFn | undefined,
  ): CompareWithFn {
    return (a: unknown, b: unknown) => {
      const left = DropdownComponent.resolveBindValue(a, bindValueKey);
      const right = DropdownComponent.resolveBindValue(b, bindValueKey);
      if (custom) return custom(left, right);
      return left === right;
    };
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
