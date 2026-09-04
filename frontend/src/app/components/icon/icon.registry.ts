export type IconPath = {
  d: string;
  linecap?: 'round' | 'butt' | 'square';
  linejoin?: 'round' | 'miter' | 'bevel';
  fill?: string;
};

export type IconCircle = {
  cx: number;
  cy: number;
  r: number;
};

export type IconDefinition = {
  viewBox?: string;
  strokeWidth?: number | string;
  fill?: string;
  stroke?: string;
  readonly paths?: readonly IconPath[];
  readonly circles?: readonly IconCircle[];
};

export const ICONS = {
  'chevron-left': {
    strokeWidth: 2,
    paths: [{ d: 'M15 19l-7-7 7-7', linecap: 'round', linejoin: 'round' }],
  },
  'chevron-right': {
    strokeWidth: 2,
    paths: [{ d: 'M9 5l7 7-7 7', linecap: 'round', linejoin: 'round' }],
  },
  'chevron-down': {
    strokeWidth: 2,
    paths: [{ d: 'M19 9l-7 7-7-7', linecap: 'round', linejoin: 'round' }],
  },
  'chevrons-right': {
    strokeWidth: 2,
    paths: [
      { d: 'M6 17l5-5-5-5', linecap: 'round', linejoin: 'round' },
      { d: 'M13 17l5-5-5-5', linecap: 'round', linejoin: 'round' },
    ],
  },
  'chevrons-left': {
    strokeWidth: 2,
    paths: [
      { d: 'M18 7l-5 5 5 5', linecap: 'round', linejoin: 'round' },
      { d: 'M11 7l-5 5 5 5', linecap: 'round', linejoin: 'round' },
    ],
  },
  calendar: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25m-18 0A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75m-18 0v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  check: {
    strokeWidth: 2.5,
    paths: [{ d: 'M5 13l4 4L19 7', linecap: 'round', linejoin: 'round' }],
  },
  search: {
    strokeWidth: 2,
    circles: [{ cx: 11, cy: 11, r: 7 }],
    paths: [{ d: 'M20 20l-3-3', linecap: 'round' }],
  },
  scan: {
  strokeWidth: 2,
  paths: [
    { d: 'M4 7V5a1 1 0 0 1 1-1h2', linecap: 'round' },
    { d: 'M17 4h2a1 1 0 0 1 1 1v2', linecap: 'round' },
    { d: 'M20 17v2a1 1 0 0 1-1 1h-2', linecap: 'round' },
    { d: 'M7 20H5a1 1 0 0 1-1-1v-2', linecap: 'round' },
    { d: 'M7 12h10', linecap: 'round' },
  ],
},
  'search-outline': {
    strokeWidth: 2,
    paths: [
      {
        d: 'M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  plus: {
    strokeWidth: 2.5,
    paths: [{ d: 'M12 5v14M5 12h14', linecap: 'round' }],
  },
  download: {
    strokeWidth: 2,
    paths: [
      { d: 'M12 3v12', linecap: 'round' },
      { d: 'M8 11l4 4 4-4', linecap: 'round', linejoin: 'round' },
      { d: 'M5 19h14', linecap: 'round' },
    ],
  },
  edit: {
    strokeWidth: 2,
    paths: [
      { d: 'M12 20h9' },
      { d: 'M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5z', linecap: 'round', linejoin: 'round' },
    ],
  },
  trash: {
    strokeWidth: 2,
    paths: [
      { d: 'M3 6h18' },
      { d: 'M8 6V4h8v2' },
      { d: 'M19 6l-1 14H6L5 6' },
      { d: 'M10 11v6M14 11v6', linecap: 'round' },
    ],
  },
  history: {
    strokeWidth: 2,
    paths: [
      { d: 'M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8', linecap: 'round', linejoin: 'round' },
      { d: 'M3 3v5h5', linecap: 'round', linejoin: 'round' },
      { d: 'M12 7v5l4 2', linecap: 'round', linejoin: 'round' },
    ],
  },
  upload: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  /** Bring data in (file import) — arrow into a tray. */
  import: {
    strokeWidth: 2,
    paths: [
      { d: 'M12 3v12', linecap: 'round' },
      { d: 'M16 11l-4 4-4-4', linecap: 'round', linejoin: 'round' },
      {
        d: 'M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  menu: {
    strokeWidth: 2,
    paths: [
      { d: 'M4 6h16M4 12h16M4 18h16', linecap: 'round', linejoin: 'round' },
    ],
  },
  close: {
    strokeWidth: 2,
    paths: [{ d: 'M6 18L18 6M6 6l12 12', linecap: 'round', linejoin: 'round' }],
  },
  bell: {
    strokeWidth: 2,
    paths: [
      {
        d: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  dashboard: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'm2.25 12 8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  warehouse: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M8.25 21v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21m0 0h4.5V3.545M12.75 21h7.5V10.75M2.25 21h1.5m18 0h-18M2.25 9l4.5-1.636M18.75 3l-1.5.545m0 6.205 3 1m1.5.5-1.5-.5M6.75 7.364V3h-3v18m3-13.636 10.5-3.819',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  sales: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 0 0-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 0 0-16.536-1.84M7.5 14.25 5.106 5.272M6 20.25a1.5 1.5 0 1 1 3 0m3 0a1.5 1.5 0 1 1 3 0m-9 0h.008v.008H9v-.008ZM15 20.25a1.5 1.5 0 1 1 3 0m3 0a1.5 1.5 0 1 1 3 0m-9 0h.008v.008H15v-.008Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  bolt: {
    strokeWidth: 1.75,
    paths: [
      {
        d: 'M3.75 13.5 14.25 2.25 12 10.5h8.25L9.75 21.75 12 13.5H3.75Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  rewards: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M21 11.25v8.25a1.5 1.5 0 0 1-1.5 1.5H5.25a1.5 1.5 0 0 1-1.5-1.5v-8.25M12 4.875A2.625 2.625 0 1 0 9.375 7.5H12m0-2.625V7.5m0-2.625A2.625 2.625 0 1 1 14.625 7.5H12m0 0V21m-8.625-9.75h18c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  store: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M13.5 21v-7.5a.75.75 0 0 1 .75-.75h3a.75.75 0 0 1 .75.75V21m-4.5 0H2.36m11.14 0H18m0 0h3.64m-1.39 0V9.349M3.75 21V9.349m0 0a3.001 3.001 0 0 0 3.75-.615A2.993 2.993 0 0 0 9.75 9.75c.896 0 1.7-.393 2.25-1.016a2.993 2.993 0 0 0 2.25 1.016c.896 0 1.7-.393 2.25-1.015a3.001 3.001 0 0 0 3.75.614m-16.5 0a3.004 3.004 0 0 1-.621-4.72l1.189-1.19A1.5 1.5 0 0 1 5.378 3h13.243a1.5 1.5 0 0 1 1.06.44l1.19 1.189a3 3 0 0 1-.621 4.72M6.75 18h3.75a.75.75 0 0 0 .75-.75V13.5a.75.75 0 0 0-.75-.75H6.75a.75.75 0 0 0-.75.75v3.75c0 .414.336.75.75.75Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  master: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M6 6.878V6a2.25 2.25 0 0 1 2.25-2.25h7.5A2.25 2.25 0 0 1 18 6v.878m-12 0c.235-.083.487-.128.75-.128h10.5c.263 0 .515.045.75.128m-12 0A2.25 2.25 0 0 0 4.5 9v.878m13.5-3A2.25 2.25 0 0 1 19.5 9v.878m0 0a2.246 2.246 0 0 0-.75-.128H5.25c-.263 0-.515.045-.75.128m15 0A2.25 2.25 0 0 1 21 12v6a2.25 2.25 0 0 1-2.25 2.25H5.25A2.25 2.25 0 0 1 3 18v-6c0-.98.626-1.813 1.5-2.122',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  settings: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.28Z',
        linecap: 'round',
        linejoin: 'round',
      },
      {
        d: 'M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  doc: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  building: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M3.75 21h16.5M4.5 3h15M5.25 3v18m13.5-18v18M9 6.75h1.5m-1.5 3h1.5m-1.5 3h1.5m3-6H15m-1.5 3H15m-1.5 3H15M9 21v-3.375c0-.621.504-1.125 1.125-1.125h3.75c.621 0 1.125.504 1.125 1.125V21',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  clipboard: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 0 0 2.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 0 0-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 0 0 .75-.75 2.25 2.25 0 0 0-.1-.664m-5.8 0A2.251 2.251 0 0 1 13.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25ZM6.75 12h.008v.008H6.75V12Zm0 3h.008v.008H6.75V15Zm0 3h.008v.008H6.75V18Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  users: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  vendor: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M8.25 18.75a1.5 1.5 0 0 1-3 0m3 0a1.5 1.5 0 0 0-3 0m3 0h6m-9 0H3.375a1.125 1.125 0 0 1-1.125-1.125V14.25m17.25 0h-2.25m0 0h-2.25m2.25 0v-4.5m0 0h-5.25m5.25 0v4.5M3.375 6.75h17.25c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125H3.375a1.125 1.125 0 0 1-1.125-1.125v-9.75c0-.621.504-1.125 1.125-1.125Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  squares: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M3.75 6A2.25 2.25 0 0 1 6 3.75h2.25A2.25 2.25 0 0 1 10.5 6v2.25a2.25 2.25 0 0 1-2.25 2.25H6a2.25 2.25 0 0 1-2.25-2.25V6ZM3.75 15.75A2.25 2.25 0 0 1 6 13.5h2.25a2.25 2.25 0 0 1 2.25 2.25V18a2.25 2.25 0 0 1-2.25 2.25H6A2.25 2.25 0 0 1 3.75 18v-2.25ZM13.5 6a2.25 2.25 0 0 1 2.25-2.25H18A2.25 2.25 0 0 1 20.25 6v2.25A2.25 2.25 0 0 1 18 10.5h-2.25a2.25 2.25 0 0 1-2.25-2.25V6ZM13.5 15.75a2.25 2.25 0 0 1 2.25-2.25H18a2.25 2.25 0 0 1 2.25 2.25V18A2.25 2.25 0 0 1 18 20.25h-2.25A2.25 2.25 0 0 1 13.5 18v-2.25Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  'map-pin': {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z',
        linecap: 'round',
        linejoin: 'round',
      },
      {
        d: 'M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1 1 15 0Z',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
  counter: {
    strokeWidth: 1.5,
    paths: [
      {
        d: 'M9 17.25v1.007a3 3 0 0 1-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0 1 15 18.257V17.25m6-12V15a2.25 2.25 0 0 1-2.25 2.25H5.25A2.25 2.25 0 0 1 3 15V5.25m18 0A2.25 2.25 0 0 0 18.75 3H5.25A2.25 2.25 0 0 0 3 5.25m18 0V12a2.25 2.25 0 0 1-2.25 2.25H5.25A2.25 2.25 0 0 1 3 12V5.25',
        linecap: 'round',
        linejoin: 'round',
      },
    ],
  },
} as const;

export type IconName = keyof typeof ICONS;

export function getIconDefinition(name: IconName): IconDefinition {
  return ICONS[name];
}

/** Build an SVG element for non-Angular contexts (e.g. AG Grid cell renderers). */
export function createIconElement(name: IconName, className = 'h-4 w-4'): SVGSVGElement {
  const def = getIconDefinition(name);
  const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('viewBox', def.viewBox ?? '0 0 24 24');
  svg.setAttribute('fill', def.fill ?? 'none');
  svg.setAttribute('stroke', def.stroke ?? 'currentColor');
  svg.setAttribute('stroke-width', String(def.strokeWidth ?? 2));
  svg.setAttribute('aria-hidden', 'true');
  if (className) svg.setAttribute('class', className);

  for (const circle of def.circles ?? []) {
    const el = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    el.setAttribute('cx', String(circle.cx));
    el.setAttribute('cy', String(circle.cy));
    el.setAttribute('r', String(circle.r));
    svg.append(el);
  }

  for (const path of def.paths ?? []) {
    const el = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    const p = path as IconPath;
    el.setAttribute('d', p.d);
    if (p.linecap) el.setAttribute('stroke-linecap', p.linecap);
    if (p.linejoin) el.setAttribute('stroke-linejoin', p.linejoin);
    if (p.fill) el.setAttribute('fill', p.fill);
    svg.append(el);
  }

  return svg;
}
