import { NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  HostBinding,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { IconComponent } from '../../components/icon';
import { NAV_ITEMS, type NavItem } from './nav.data';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [NgClass, RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    id: 'app-sidebar',
    class:
      'home-sidebar relative z-40 flex h-full shrink-0 flex-col transition-[width] duration-300 ease-out',
    style: 'width: var(--ql-shell-sidebar-width)',
  },
})
export class SidebarComponent {
  private readonly router = inject(Router);

  readonly collapsed = input(false);
  readonly mobileOpen = input(false);
  readonly collapseToggle = output<void>();
  readonly navigate = output<void>();

  protected readonly items = NAV_ITEMS;
  protected readonly expandedIds = signal<Set<string>>(new Set(['masters']));

  @HostBinding('style.width')
  get sidebarWidth(): string {
    return this.collapsed()
      ? 'var(--ql-shell-sidebar-width-collapsed)'
      : 'var(--ql-shell-sidebar-width)';
  }

  @HostBinding('class.home-sidebar--open')
  get openClass(): boolean {
    return this.mobileOpen();
  }

  constructor() {
    this.syncExpandedFromUrl(this.router.url);
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe((e) => this.syncExpandedFromUrl(e.urlAfterRedirects));
  }

  protected hasSubmenu(item: NavItem): boolean {
    return (item.submenu?.length ?? 0) > 0;
  }

  protected isExpanded(id: string): boolean {
    return this.expandedIds().has(id);
  }

  protected isSectionActive(item: NavItem): boolean {
    const url = this.router.url;
    return (item.submenu ?? []).some((s) => url === s.path || url.startsWith(s.path + '/'));
  }

  protected toggleSection(id: string, event?: Event): void {
    event?.stopPropagation();
    this.expandedIds.update((set) => {
      const next = new Set(set);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  protected onNavClick(): void {
    this.navigate.emit();
  }

  protected toggleCollapse(): void {
    this.collapseToggle.emit();
  }

  protected navItemClasses(): Record<string, boolean> {
    return {
      'ql-nav-item--collapsed': this.collapsed(),
      'ql-nav-item--expanded': !this.collapsed(),
    };
  }

  private syncExpandedFromUrl(url: string): void {
    for (const item of this.items) {
      if (!this.hasSubmenu(item)) continue;
      const active = (item.submenu ?? []).some(
        (s) => url === s.path || url.startsWith(s.path + '/'),
      );
      if (active) {
        this.expandedIds.update((set) => new Set(set).add(item.id));
      }
    }
  }
}
