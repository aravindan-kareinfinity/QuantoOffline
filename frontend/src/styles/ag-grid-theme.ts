import {
  type AutoSizeStrategy,
  type ColDef,
  type GridApi,
  type ICellRendererParams,
  themeQuartz,
} from 'ag-grid-community';

import { createIconElement } from '../app/components/icon';

/** Shared Quartz theme tuned to QuantoLite design tokens. */
export const qlGridTheme = themeQuartz.withParams({
  accentColor: 'var(--color-accent)',
  backgroundColor: 'var(--color-surface)',
  borderColor: 'var(--color-border)',
  borderRadius: 8,
  browserColorScheme: 'light',
  cellHorizontalPaddingScale: 0.95,
  chromeBackgroundColor: { ref: 'backgroundColor' },
  columnBorder: false,
  fontFamily: 'inherit',
  fontSize: 12,
  foregroundColor: 'var(--color-foreground)',
  headerBackgroundColor: 'var(--color-grid-header)',
  headerFontSize: 11,
  headerFontWeight: 600,
  headerTextColor: 'var(--color-muted)',
  headerColumnResizeHandleColor: 'transparent',
  rowBorder: { color: 'var(--color-border)', width: 1 },
  rowVerticalPaddingScale: 0.9,
  sidePanelBorder: false,
  spacing: 6,
  wrapperBorder: false,
  wrapperBorderRadius: 0,
});

export const qlPageSize = 20;

/** Size columns to fill the grid width on load and resize. */
export const qlAutoSizeStrategy: AutoSizeStrategy = {
  type: 'fitGridWidth',
  defaultMinWidth: 100,
};

/** Window event dispatched while shell layout animates (sidebar toggle). */
export const QL_LAYOUT_RESIZE = 'ql-layout-resize';

/** Fit visible columns to the current grid width. */
export function fitGridColumns(api: GridApi): void {
  if (!api.isDestroyed()) {
    api.sizeColumnsToFit();
  }
}

function collectResizeTargets(container: HTMLElement): HTMLElement[] {
  const targets: HTMLElement[] = [container];
  let parent = container.parentElement;
  for (let depth = 0; depth < 4 && parent; depth += 1) {
    targets.push(parent);
    parent = parent.parentElement;
  }
  return targets;
}

/**
 * Keep grid columns fitted whenever layout width changes (sidebar, window, etc.).
 * Returns a cleanup function.
 */
export function bindGridContainerResize(api: GridApi): () => void {
  let frame = 0;
  const scheduleFit = (): void => {
    cancelAnimationFrame(frame);
    // Run after AG Grid finishes its own gridSizeChanged handling.
    frame = requestAnimationFrame(() => {
      requestAnimationFrame(() => fitGridColumns(api));
    });
  };

  const onGridSizeChanged = (): void => scheduleFit();
  api.addEventListener('gridSizeChanged', onGridSizeChanged);
  api.addEventListener('firstDataRendered', onGridSizeChanged);

  const container = api.getGridElement();
  const observer = new ResizeObserver(() => scheduleFit());
  if (container instanceof HTMLElement) {
    for (const target of collectResizeTargets(container)) {
      observer.observe(target);
    }
  }

  const onLayoutResize = (): void => scheduleFit();
  const onWindowResize = (): void => scheduleFit();
  window.addEventListener(QL_LAYOUT_RESIZE, onLayoutResize);
  window.addEventListener('resize', onWindowResize);

  scheduleFit();

  return () => {
    cancelAnimationFrame(frame);
    api.removeEventListener('gridSizeChanged', onGridSizeChanged);
    api.removeEventListener('firstDataRendered', onGridSizeChanged);
    observer.disconnect();
    window.removeEventListener(QL_LAYOUT_RESIZE, onLayoutResize);
    window.removeEventListener('resize', onWindowResize);
  };
}

export const qlDefaultColDef: ColDef = {
  sortable: true,
  filter: false,
  floatingFilter: false,
  resizable: true,
  flex: 1,
  minWidth: 100,
  cellStyle: { textAlign: 'left' },
  headerClass: 'ag-left-aligned-header',
  // filterParams: {
  //   buttons: ['reset'],
  // },
};

export type GridActionContext<T> = {
  onEdit: (row: T) => void;
  onDelete: (row: T) => void;
  onHistory?: (row: T) => void;
};

export type GridEditActionContext<T> = {
  onEdit: (row: T) => void;
};

export function editActionCellRenderer<T>(
  params: ICellRendererParams<T, unknown, GridEditActionContext<T>>,
): HTMLElement {
  const row = params.data;
  const wrap = document.createElement('div');
  wrap.className = 'flex items-center gap-1';

  if (!row) return wrap;

  const editBtn = document.createElement('button');
  editBtn.type = 'button';
  editBtn.title = 'Edit';
  editBtn.setAttribute('aria-label', 'Edit');
  editBtn.className =
    'inline-flex h-7 w-7 items-center justify-center rounded text-accent hover:bg-surface hover:text-accent-hover';
  editBtn.append(createIconElement('edit', 'h-3.5 w-3.5'));
  editBtn.addEventListener('click', (event) => {
    event.stopPropagation();
    params.context?.onEdit(row);
  });

  wrap.append(editBtn);
  return wrap;
}

export function editActionColumnDef<T>(): ColDef<T> {
  return {
    headerName: 'Action',
    width: 90,
    flex: 0,
    sortable: false,
    filter: false,
    floatingFilter: false,
    suppressHeaderMenuButton: true,
    cellRenderer: editActionCellRenderer,
  };
}

export function actionCellRenderer<T>(
  params: ICellRendererParams<T, unknown, GridActionContext<T>>,
): HTMLElement {
  const row = params.data;
  const wrap = document.createElement('div');
  wrap.className = 'flex items-center gap-1';

  if (!row) return wrap;

  const editBtn = document.createElement('button');
  editBtn.type = 'button';
  editBtn.title = 'Edit';
  editBtn.setAttribute('aria-label', 'Edit');
  editBtn.className =
    'inline-flex h-7 w-7 items-center justify-center rounded text-accent hover:bg-surface hover:text-accent-hover';
  editBtn.append(createIconElement('edit', 'h-3.5 w-3.5'));
  editBtn.addEventListener('click', (event) => {
    event.stopPropagation();
    params.context?.onEdit(row);
  });

  wrap.append(editBtn);

  if (params.context?.onHistory) {
    const historyBtn = document.createElement('button');
    historyBtn.type = 'button';
    historyBtn.title = 'History';
    historyBtn.setAttribute('aria-label', 'History');
    historyBtn.className =
      'inline-flex h-7 w-7 items-center justify-center rounded text-accent hover:bg-surface hover:text-accent-hover';
    historyBtn.append(createIconElement('history', 'h-3.5 w-3.5'));
    historyBtn.addEventListener('click', (event) => {
      event.stopPropagation();
      params.context?.onHistory?.(row);
    });
    wrap.append(historyBtn);
  }

  const deleteBtn = document.createElement('button');
  deleteBtn.type = 'button';
  deleteBtn.title = 'Delete';
  deleteBtn.setAttribute('aria-label', 'Delete');
  deleteBtn.className =
    'inline-flex h-7 w-7 items-center justify-center rounded text-danger hover:bg-surface';
  deleteBtn.append(createIconElement('trash', 'h-3.5 w-3.5'));
  deleteBtn.addEventListener('click', (event) => {
    event.stopPropagation();
    params.context?.onDelete(row);
  });

  wrap.append(deleteBtn);
  return wrap;
}

export function serialNoColumnDef<T>(): ColDef<T> {
  return {
    headerName: 'S.No',
    width: 72,
    maxWidth: 88,
    flex: 0,
    suppressSizeToFit: true,
    sortable: false,
    filter: false,
    floatingFilter: false,
    suppressHeaderMenuButton: true,
    valueFormatter: (params) => String((params.node?.rowIndex ?? 0) + 1),
    cellClass: 'tabular-nums text-muted',
  };
}

export function statusColumnDef<T>(
  isActive: (row: T | undefined) => boolean,
  opts?: { headerName?: string; width?: number },
): ColDef<T> {
  return {
    headerName: opts?.headerName ?? 'Status',
    width: opts?.width ?? 110,
    flex: 0,
    suppressSizeToFit: true,
    sortable: true,
    filter: false,
    floatingFilter: false,
    valueGetter: (params) => (isActive(params.data) ? 'Active' : 'Inactive'),
    cellClass: (params) => (isActive(params.data) ? 'font-semibold text-success' : 'text-muted'),
  };
}

export function actionColumnDef<T>(opts?: { width?: number }): ColDef<T> {
  return {
    //headerName: 'Action',
    width: opts?.width ?? 90,
    flex: 0,
    sortable: false,
    filter: false,
    floatingFilter: false,
    suppressHeaderMenuButton: true,
    cellRenderer: actionCellRenderer,
  };
}
