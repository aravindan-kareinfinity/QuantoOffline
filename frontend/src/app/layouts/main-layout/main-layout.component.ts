import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  PLATFORM_ID,
  signal,
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterOutlet } from '@angular/router';

import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { QL_LAYOUT_RESIZE } from '../../../styles/ag-grid-theme';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, SidebarComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  private static readonly sidebarStorageKey = 'quanto_sidebar_collapsed';

  private readonly platformId = inject(PLATFORM_ID);

  protected readonly sidebarCollapsed = signal(this.readCollapsed());
  protected readonly mobileSidebarOpen = signal(false);

  constructor() {
    effect(() => {
      if (!isPlatformBrowser(this.platformId)) return;
      document.body.classList.toggle('overflow-hidden', this.mobileSidebarOpen());
    });

    effect(() => {
      this.sidebarCollapsed();
      this.mobileSidebarOpen();
      if (!isPlatformBrowser(this.platformId)) return;
      this.notifyLayoutResize();
    });
  }

  /** Sidebar width animates over 300ms — keep grids fitted for the full transition. */
  private notifyLayoutResize(): void {
    const duration = 320;
    const start = performance.now();
    const tick = (): void => {
      window.dispatchEvent(new Event(QL_LAYOUT_RESIZE));
      if (performance.now() - start < duration) {
        requestAnimationFrame(tick);
      }
    };
    requestAnimationFrame(tick);
  }

  protected toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => {
      const next = !v;
      localStorage.setItem(MainLayoutComponent.sidebarStorageKey, next ? '1' : '0');
      return next;
    });
  }

  protected toggleMobileSidebar(): void {
    this.mobileSidebarOpen.update((v) => !v);
  }

  protected closeMobileSidebar(): void {
    this.mobileSidebarOpen.set(false);
  }

  private readCollapsed(): boolean {
    try {
      return localStorage.getItem(MainLayoutComponent.sidebarStorageKey) === '1';
    } catch (err) {
      console.error('Could not read sidebar state from localStorage.', err);
      return false;
    }
  }
}
