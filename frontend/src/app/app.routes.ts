import { Routes } from '@angular/router';
import { AuthGuard, RoleGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/callback',
    loadComponent: () => import('./pages/auth-callback/auth-callback.component').then(m => m.AuthCallbackComponent)
  },
  {
    path: 'ideas',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/ideas/ideas.component').then(m => m.IdeasComponent)
  },
  {
    path: 'ideas/:id',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/idea-detail/idea-detail.component').then(m => m.IdeaDetailComponent)
  },
  {
    path: 'ideas/new',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/idea-form/idea-form.component').then(m => m.IdeaFormComponent)
  },
  {
    path: 'teams',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/teams/teams.component').then(m => m.TeamsComponent)
  },
  {
    path: 'judging',
    canActivate: [AuthGuard, RoleGuard],
    data: { role: 'judge' },
    loadComponent: () => import('./pages/judging/judging.component').then(m => m.JudgingComponent)
  },
  {
    path: 'admin',
    canActivate: [AuthGuard, RoleGuard],
    data: { role: 'admin' },
    loadComponent: () => import('./pages/admin/admin.component').then(m => m.AdminComponent)
  },
  {
    path: '**',
    loadComponent: () => import('./pages/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];
