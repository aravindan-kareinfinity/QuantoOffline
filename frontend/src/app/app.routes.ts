import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./screens/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./screens/dashboard-hub/dashboard-hub.component').then(
            (m) => m.DashboardHubComponent,
          ),
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./screens/dashboard/dashboard.component').then((m) => m.DashboardComponent),
          },
        ],
      },
      {
        path: 'offline/master-download',
        loadComponent: () =>
          import('./screens/offline-master-download/offline-master-download.component').then(
            (m) => m.OfflineMasterDownloadComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
