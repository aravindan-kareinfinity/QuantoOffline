import { NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  HostBinding,
  HostListener,
  inject,
  input,
  OnInit,
  output,
  signal,
  ViewChild,
} from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { ButtonComponent } from '../../components/button/button.component';
import { DropdownComponent } from '../../components/dropdown';
import { IconComponent } from '../../components/icon';
import {
  REFERENCE_LIST_GENERAL_TYPES,
  REFERENCE_LIST_READ_SEGMENTS,
  ReferenceList,
} from '../../models/reference-list';
import { EmployeeService } from '../../services/employee.service';
import { LocationContextService } from '../../services/location-context.service';
import { ReferenceListService } from '../../services/reference-list.service';
import { StorageService } from '../../services/storage.service';
import { UsersService } from '../../services/users.service';
import { refInfo } from '../../utils/reference-list.util';

type HeaderCompanyOption = { id: number; name: string };

class HeaderContextDraft {
  companyid: number | null = null;
  locationid: number | null = null;
  counterid: number | null = null;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [NgClass, RouterLink, IconComponent, DropdownComponent, FormsModule, ButtonComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class:
      'home-header sticky top-0 z-50 block h-14 min-h-14 w-full shrink-0',
    style: 'background-color: var(--ql-shell-chrome-bg)',
    role: 'banner',
  },
})
export class HeaderComponent implements OnInit {
  @ViewChild('contextForm') protected contextForm?: NgForm;

  private readonly users = inject(UsersService);
  private readonly employeeService = inject(EmployeeService);
  private readonly referenceListService = inject(ReferenceListService);
  private readonly locationContext = inject(LocationContextService);
  protected readonly storage = inject(StorageService);

  readonly sidebarCollapsed = input(false);
  readonly mobileSidebarOpen = input(false);
  readonly mobileMenuToggle = output<void>();

  protected readonly profileOpen = signal(false);
  protected readonly searchFocused = signal(false);
  protected readonly contextDialogOpen = signal(false);
  protected readonly isLoadingContext = signal(false);
  protected readonly isSavingContext = signal(false);

  protected readonly companyItems = signal<HeaderCompanyOption[]>([]);
  protected readonly locationItems = signal<ReferenceList[]>([]);
  protected readonly counterItems = signal<ReferenceList[]>([]);

  protected dialogDraft = new HeaderContextDraft();

  private allLocations: ReferenceList[] = [];
  private allCounters: ReferenceList[] = [];
  private lookupsLoaded = false;

  @HostBinding('class.home-header--sidebar-collapsed')
  get collapsedClass(): boolean {
    return this.sidebarCollapsed();
  }

  ngOnInit(): void {
    void this.initContext();
  }

  protected contextCompanyLabel(): string {
    return this.locationContext.context().companyName?.trim() || 'Not selected';
  }

  protected contextLocationLabel(): string {
    return this.locationContext.context().locationName?.trim() || 'Not selected';
  }

  protected contextCounterLabel(): string {
    return this.locationContext.context().counterName?.trim() || 'Not selected';
  }

  protected openContextDialog(): void {
    this.profileOpen.set(false);
    this.patchDialogDraft();
    this.contextDialogOpen.set(true);
    void this.loadContextLookups();
  }

  protected closeContextDialog(): void {
    this.contextDialogOpen.set(false);
  }

  protected onDialogCompanyChange(id: number | null): void {
    this.dialogDraft.companyid = id;
    this.dialogDraft.locationid = null;
    this.dialogDraft.counterid = null;
    this.refreshDialogLocationItems();
    this.refreshDialogCounterItems();
  }

  protected onDialogLocationChange(id: number | null): void {
    this.dialogDraft.locationid = id;
    this.dialogDraft.counterid = null;
    this.refreshDialogCounterItems();
  }

  protected onDialogCounterChange(id: number | null): void {
    this.dialogDraft.counterid = id;
  }

  protected async saveContext(form: NgForm): Promise<void> {
    for (const control of Object.values(form.controls)) {
      control.markAsTouched();
    }
    if (form.invalid) return;

    const company = this.companyItems().find((c) => c.id === this.dialogDraft.companyid);
    const location = this.locationItems().find((l) => l.id === this.dialogDraft.locationid);
    const counter = this.counterItems().find((c) => c.id === this.dialogDraft.counterid);

    this.isSavingContext.set(true);
    try {
      this.locationContext.patch({
        companyid: this.dialogDraft.companyid,
        companyName: company?.name?.trim() ?? '',
        locationid: this.dialogDraft.locationid,
        locationName: location?.name?.trim() ?? '',
        counterid: this.dialogDraft.counterid,
        counterName: counter?.name?.trim() || counter?.code?.trim() || '',
      });
      this.closeContextDialog();
    } finally {
      this.isSavingContext.set(false);
    }
  }

  protected displayName(): string {
    const u = this.storage.usercontext();
    return u.Title || u.userName || u.username || 'My Store';
  }

  protected displayRole(): string {
    return this.storage.usercontext().Designation || 'Admin';
  }

  protected initials(): string {
    const name = this.displayName().trim();
    const parts = name.split(/\s+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.slice(0, 2).toUpperCase() || 'QL';
  }

  protected onMobileMenuClick(): void {
    this.mobileMenuToggle.emit();
  }

  protected toggleProfile(): void {
    this.profileOpen.update((v) => !v);
  }

  protected logout(): void {
    this.profileOpen.set(false);
    this.users.logout();
  }

  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;
    if (!target?.closest('.home-header-profile')) {
      this.profileOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.contextDialogOpen()) {
      this.closeContextDialog();
    }
  }

  private patchDialogDraft(): void {
    const ctx = this.locationContext.value;
    this.dialogDraft = {
      companyid: ctx.companyid,
      locationid: ctx.locationid,
      counterid: ctx.counterid,
    };
  }

  private async initContext(): Promise<void> {
    const stored = this.locationContext.value;
    const hasStored =
      stored.companyid != null || stored.locationid != null || stored.counterid != null;
    if (hasStored) {
      await this.locationContext.ensureDisplayNames();
      return;
    }

    const ctx = this.storage.value;
    const employeeId = Number(ctx.employeeId) || 0;
    let companyid: number | null = null;
    let locationid: number | null = null;
    let companyName = ctx.CompanyName?.trim() ?? '';
    let locationName = '';

    if (employeeId > 0) {
      const employee = await this.employeeService.getById(employeeId);
      if (employee) {
        companyid = employee.companyid || null;
        locationid = employee.locationid || null;
      }
    }

    this.locationContext.patch({
      companyid,
      companyName,
      locationid,
      locationName,
      counterid: null,
      counterName: '',
    });

    await this.locationContext.ensureDisplayNames();
  }

  private async loadContextLookups(): Promise<void> {
    if (this.lookupsLoaded) {
      this.refreshDialogLocationItems();
      this.refreshDialogCounterItems();
      return;
    }

    this.isLoadingContext.set(true);
    try {
      const [companies, locations, counters] = await Promise.all([
        this.referenceListService.readSegment<HeaderCompanyOption>(
          REFERENCE_LIST_READ_SEGMENTS.COMPANY,
        ),
        this.referenceListService.readSegment<ReferenceList>(
          REFERENCE_LIST_READ_SEGMENTS.RETAILLOCATION,
        ),
        this.referenceListService.search(REFERENCE_LIST_GENERAL_TYPES.COUNTER),
      ]);

      this.companyItems.set(companies.filter((c) => c.id > 0));
      this.allLocations = locations.filter((l) => l.id > 0);
      this.allCounters = counters.filter((c) => c.id > 0);
      this.lookupsLoaded = true;

      this.refreshDialogLocationItems();
      this.refreshDialogCounterItems();
      this.validateStoredSelection();
    } finally {
      this.isLoadingContext.set(false);
    }
  }

  private validateStoredSelection(): void {
    const ctx = this.locationContext.value;
    let companyid = ctx.companyid;
    let companyName = ctx.companyName;

    if (companyid != null && !this.companyItems().some((c) => c.id === companyid)) {
      companyid = null;
      companyName = '';
    }

    if (companyid !== ctx.companyid || companyName !== ctx.companyName) {
      this.locationContext.setCompany(companyid, companyName);
    }

    const locationsForCompany = this.filterLocationsForCompany(companyid);
    let locationid = ctx.locationid;
    let locationName = ctx.locationName;

    if (locationid != null && !locationsForCompany.some((l) => l.id === locationid)) {
      locationid = null;
      locationName = '';
    }

    if (locationid !== ctx.locationid || locationName !== ctx.locationName) {
      this.locationContext.setLocation(locationid, locationName);
    }

    const countersForLocation = this.filterCountersForLocation(locationid);
    let counterid = this.locationContext.value.counterid;
    let counterName = this.locationContext.value.counterName;

    if (counterid != null && !countersForLocation.some((c) => c.id === counterid)) {
      counterid = null;
      counterName = '';
    }

    if (counterid !== this.locationContext.value.counterid) {
      this.locationContext.setCounter(counterid, counterName);
    }
  }

  private refreshDialogLocationItems(): void {
    this.locationItems.set(this.filterLocationsForCompany(this.dialogDraft.companyid));
  }

  private refreshDialogCounterItems(): void {
    this.counterItems.set(this.filterCountersForLocation(this.dialogDraft.locationid));
  }

  private filterLocationsForCompany(companyid: number | null): ReferenceList[] {
    if (companyid == null) {
      return [...this.allLocations];
    }

    return this.allLocations.filter((location) => {
      const info = refInfo(location);
      const parent = info.parentid ?? location.productgroup ?? null;
      return parent == null || parent === companyid;
    });
  }

  private filterCountersForLocation(locationid: number | null): ReferenceList[] {
    if (locationid == null) {
      return [];
    }

    return this.allCounters.filter(
      (counter) => Number(refInfo(counter).locationid) === locationid,
    );
  }
}
