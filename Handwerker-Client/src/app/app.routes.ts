import { Routes } from '@angular/router';
import { Dashboard } from './features/dashboard/dashboard';
import { Profile } from './features/profile/profile';
import { Products } from './features/products/products';
import { Settings } from './features/settings/settings';
import { NotificationsPage } from './features/notifications/notifications';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    { path: 'dashboard', component: Dashboard },
    { path: 'profile', component: Profile },
    { path: 'product', component: Products },
    { path: 'provider', loadComponent: () => import('./features/providers/providers').then(m => m.Providers) },
    { path: 'recipients', loadComponent: () => import('./features/recipients/recipients').then(m => m.Recipients) },
    { path: 'company', loadComponent: () => import('./features/company/company').then(m => m.CompanyPage) },
    { path: 'services', loadComponent: () => import('./features/services/services').then(m => m.Services) },
    { path: 'wages', loadComponent: () => import('./features/wages/wages').then(m => m.Wages) },
    { path: 'users', loadComponent: () => import('./features/users/users').then(m => m.Users), canActivate: [authGuard, adminGuard] },
    { path: 'settings/role-dashboard', loadComponent: () => import('./features/settings/role-dashboard-settings/role-dashboard-settings').then(m => m.RoleDashboardSettingsComponent), canActivate: [authGuard, adminGuard] },
    { path: 'offers', loadComponent: () => import('./features/offers/offers').then(m => m.Offers) },
    { path: 'offers/new', loadComponent: () => import('./features/offers/offer-detail/offer-detail').then(m => m.OfferDetailComponent) },
    { path: 'offers/:id', loadComponent: () => import('./features/offers/offer-detail/offer-detail').then(m => m.OfferDetailComponent) },
    { path: 'invoices', loadComponent: () => import('./features/invoices/invoices').then(m => m.Invoices) },
    { path: 'invoices/new', loadComponent: () => import('./features/invoices/invoice-detail/invoice-detail').then(m => m.InvoiceDetail) },
    { path: 'invoices/:id', loadComponent: () => import('./features/invoices/invoice-detail/invoice-detail').then(m => m.InvoiceDetail) },
    { path: 'orders', loadComponent: () => import('./features/orders/orders').then(m => m.Orders) },
    { path: 'orders/new', loadComponent: () => import('./features/orders/order-detail/order-detail').then(m => m.OrderDetail) },
    { path: 'orders/:id', loadComponent: () => import('./features/orders/order-detail/order-detail').then(m => m.OrderDetail) },
    { path: 'settings', component: Settings },
    { path: 'notifications', component: NotificationsPage }
];
