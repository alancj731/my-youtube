import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Sign in · MyYT',
    loadComponent: () => import('./features/auth/auth-page').then((m) => m.AuthPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'MyYT',
        loadComponent: () => import('./features/videos/home').then((m) => m.Home),
      },
      {
        path: 'watch/:id',
        title: 'Watch · MyYT',
        loadComponent: () => import('./features/videos/watch').then((m) => m.Watch),
      },
      {
        path: 'tags',
        title: 'Manage tags · MyYT',
        loadComponent: () => import('./features/tags/manage-tags').then((m) => m.ManageTags),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
