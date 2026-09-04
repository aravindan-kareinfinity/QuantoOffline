import type { IconName } from '../../components/icon';

export const DEFAULT_SUBMENU_ICON: IconName = 'doc';

export class NavSubItem {
  id = '';
  label = '';
  path = '';
  icon: IconName = DEFAULT_SUBMENU_ICON;
}

export class NavItem {
  id = '';
  label = '';
  /** Leaf link when there is no submenu. */
  path: string | null = null;
  icon: IconName = 'dashboard';
  submenu: NavSubItem[] = [];
}

function nav(
  id: string,
  label: string,
  path: string | null,
  icon: IconName,
  submenu: NavSubItem[] = [],
): NavItem {
  const item = new NavItem();
  item.id = id;
  item.label = label;
  item.path = path;
  item.icon = icon;
  item.submenu = submenu;
  return item;
}

function sub(id: string, label: string, path: string, icon: IconName = DEFAULT_SUBMENU_ICON): NavSubItem {
  const item = new NavSubItem();
  item.id = id;
  item.label = label;
  item.path = path;
  item.icon = icon;
  return item;
}

/** Top-level nav — login/dashboard + offline tools. */
export const NAV_ITEMS: NavItem[] = [
  nav('home', 'Home', '/dashboard', 'dashboard'),
  nav('offline', 'Offline', null, 'download', [
    sub('o-master', 'Master download', '/offline/master-download', 'download'),
  ]),
];
