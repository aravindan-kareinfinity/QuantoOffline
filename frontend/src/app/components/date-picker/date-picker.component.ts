import {
  afterNextRender,
  booleanAttribute,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  HostListener,
  inject,
  input,
  signal,
  ViewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { merge, startWith } from 'rxjs';

import { IconComponent } from '../icon';

type CalendarCell = {
  iso: string;
  day: number;
  inMonth: boolean;
};

type PanelMode = 'years' | 'months' | 'days';

const MONTH_LABELS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
] as const;

const MONTH_SHORT_LABELS = [
  'Jan',
  'Feb',
  'Mar',
  'Apr',
  'May',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Oct',
  'Nov',
  'Dec',
] as const;

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block w-full',
  },
})
export class DatePickerComponent implements ControlValueAccessor {
  private static nextId = 0;

  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly ngControl = inject(NgControl, { optional: true, self: true });
  private readonly controlTick = signal(0);
  private errorWatchStarted = false;

  @ViewChild('triggerBtn') private triggerBtn?: ElementRef<HTMLButtonElement>;

  readonly label = input('');
  readonly placeholder = input('Select date…');
  readonly disabled = input(false);
  readonly required = input(false, { transform: booleanAttribute });
  /** When false, invalid styling and messages stay hidden (e.g. until submit). */
  readonly showError = input(true, { transform: booleanAttribute });
  readonly hideErrorMessage = input(false, { transform: booleanAttribute });
  readonly error = input('');
  readonly hint = input('');
  readonly inputId = input(`app-date-picker-${++DatePickerComponent.nextId}`);

  protected readonly value = signal('');
  protected readonly cvaDisabled = signal(false);
  protected readonly panelOpen = signal(false);
  protected readonly panelMode = signal<PanelMode>('days');
  protected readonly viewYear = signal(new Date().getFullYear());
  protected readonly viewMonth = signal(new Date().getMonth());
  protected readonly yearPageStart = signal(new Date().getFullYear());
  protected readonly panelTop = signal(0);
  protected readonly panelLeft = signal(0);

  protected readonly monthOptions = MONTH_SHORT_LABELS.map((label, index) => ({
    index,
    label,
  }));

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
    afterNextRender(() => this.setupErrorWatch());
  }

  protected readonly isDisabled = computed(() => this.disabled() || this.cvaDisabled());

  protected readonly displayError = computed(() => {
    this.controlTick();
    if (!this.showError()) return '';
    const manual = this.error().trim();
    if (manual) return manual;
    return getFormControlError(this.ngControl?.control, this.ngControl?.name ?? null);
  });

  protected readonly describedBy = computed(() => {
    const ids: string[] = [];
    if (this.displayError()) ids.push(`${this.inputId()}-error`);
    else if (this.hint()) ids.push(`${this.inputId()}-hint`);
    return ids.length ? ids.join(' ') : null;
  });

  protected readonly displayText = computed(() => {
    const iso = this.value().trim();
    if (!iso) return '';
    return formatDisplayDate(iso);
  });

  protected readonly viewMonthLabel = computed(() => MONTH_LABELS[this.viewMonth()]);

  protected readonly calendarCells = computed(() =>
    buildCalendarCells(this.viewYear(), this.viewMonth()),
  );

  protected readonly yearOptions = computed(() =>
    Array.from({ length: 12 }, (_, index) => this.yearPageStart() + index),
  );

  protected readonly yearPageEnd = computed(() => this.yearPageStart() + 11);

  writeValue(value: unknown): void {
    this.value.set(normalizeIso(value));
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

  protected togglePanel(): void {
    if (this.isDisabled()) return;
    if (this.panelOpen()) {
      this.closePanel();
      return;
    }
    this.openPanel();
  }

  protected showMonthPicker(): void {
    this.panelMode.set('months');
  }

  protected showYearPicker(): void {
    this.yearPageStart.set(Math.floor(this.viewYear() / 12) * 12);
    this.panelMode.set('years');
  }

  protected selectYear(year: number): void {
    this.viewYear.set(year);
    this.panelMode.set('months');
  }

  protected selectMonth(monthIndex: number): void {
    this.viewMonth.set(monthIndex);
    this.panelMode.set('days');
  }

  protected isViewYearSelected(year: number): boolean {
    return this.viewYear() === year;
  }

  protected isSelected(iso: string): boolean {
    return this.value() === iso;
  }

  protected isViewMonthSelected(monthIndex: number): boolean {
    return this.viewMonth() === monthIndex;
  }

  protected selectDate(iso: string): void {
    this.value.set(iso);
    this.onChange(iso);
    this.onTouched();
    this.closePanel();
  }

  protected previousYearPage(): void {
    this.yearPageStart.update((start) => start - 12);
  }

  protected nextYearPage(): void {
    this.yearPageStart.update((start) => start + 12);
  }

  protected previousYear(): void {
    this.viewYear.update((y) => y - 1);
  }

  protected nextYear(): void {
    this.viewYear.update((y) => y + 1);
  }

  protected previousMonth(): void {
    if (this.viewMonth() === 0) {
      this.viewMonth.set(11);
      this.viewYear.update((y) => y - 1);
      return;
    }
    this.viewMonth.update((m) => m - 1);
  }

  protected nextMonth(): void {
    if (this.viewMonth() === 11) {
      this.viewMonth.set(0);
      this.viewYear.update((y) => y + 1);
      return;
    }
    this.viewMonth.update((m) => m + 1);
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.panelOpen()) this.closePanel();
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (!this.panelOpen()) return;
    const target = event.target as Node | null;
    if (target && this.host.nativeElement.contains(target)) return;
    this.closePanel();
  }

  @HostListener('window:resize')
  @HostListener('window:scroll')
  protected repositionPanel(): void {
    if (this.panelOpen()) this.updatePanelPosition();
  }

  private openPanel(): void {
    const iso = this.value().trim();
    const base = iso ? parseIso(iso) : new Date();
    this.viewYear.set(base.getFullYear());
    this.viewMonth.set(base.getMonth());
    this.panelMode.set('days');
    this.panelOpen.set(true);
    this.updatePanelPosition();
    this.cdr.markForCheck();
  }

  private closePanel(): void {
    this.panelOpen.set(false);
    this.panelMode.set('days');
    this.cdr.markForCheck();
  }

  private updatePanelPosition(): void {
    const el = this.triggerBtn?.nativeElement;
    if (!el) return;
    const rect = el.getBoundingClientRect();
    this.panelTop.set(rect.bottom + 6);
    this.panelLeft.set(rect.left);
  }

  private setupErrorWatch(): void {
    if (this.errorWatchStarted) return;
    const control = this.ngControl?.control;
    if (!control) return;
    this.errorWatchStarted = true;
    merge(control.statusChanges, control.valueChanges, control.events)
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.controlTick.update((v) => v + 1));
  }
}

function normalizeIso(value: unknown): string {
  if (value == null) return '';
  const raw = String(value).trim();
  if (!raw) return '';
  if (/^\d{4}-\d{2}-\d{2}$/.test(raw)) return raw;
  const d = new Date(raw);
  if (Number.isNaN(d.getTime())) return '';
  return toIso(d);
}

function parseIso(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(y, (m || 1) - 1, d || 1);
}

function toIso(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function formatDisplayDate(iso: string): string {
  const date = parseIso(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return new Intl.DateTimeFormat('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  }).format(date);
}

function buildCalendarCells(year: number, month: number): CalendarCell[] {
  const firstOfMonth = new Date(year, month, 1);
  const startOffset = (firstOfMonth.getDay() + 6) % 7;
  const gridStart = new Date(year, month, 1 - startOffset);
  const cells: CalendarCell[] = [];

  for (let i = 0; i < 42; i += 1) {
    const date = new Date(gridStart);
    date.setDate(gridStart.getDate() + i);
    cells.push({
      iso: toIso(date),
      day: date.getDate(),
      inMonth: date.getMonth() === month,
    });
  }

  return cells;
}

function getFormControlError(
  control: import('@angular/forms').AbstractControl | null | undefined,
  _field: string | number | null,
): string {
  if (!control?.errors || !(control.touched || control.dirty)) return '';
  if (control.errors['required']) return 'This field is required.';
  return 'Invalid field.';
}
