import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';

import { getIconDefinition, type IconName } from './icon.registry';

export type IconSize = 'xs' | 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-icon',
  standalone: true,
  templateUrl: './icon.component.html',
  styleUrl: './icon.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inline-flex shrink-0 items-center justify-center',
  },
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input<IconSize>('sm');
  /** Extra classes for the SVG (color, positioning, etc.). */
  readonly svgClass = input<string>('');

  protected readonly definition = computed(() => getIconDefinition(this.name()));

  protected readonly svgClasses = computed(() => {
    const sizes: Record<IconSize, string> = {
      xs: 'h-3 w-3',
      sm: 'h-4 w-4',
      md: 'h-5 w-5',
      lg: 'h-6 w-6',
    };
    return [sizes[this.size()], this.svgClass()].filter(Boolean).join(' ');
  });
}
