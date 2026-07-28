import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthService } from './auth.service';
import {
  DashboardSection,
  RoleDashboardConfig,
  RoleDashboardSettings
} from '../interfaces/role-dashboard.interface';

const STORAGE_KEY = 'handwerker_role_dashboard_settings';

/** Alle existierenden Bereiche mit Label und Icon für die Admin-UI */
export const ALL_SECTIONS: { key: DashboardSection; labelKey: string; icon: string }[] = [
  { key: 'dashboard',    labelKey: 'nav.dashboard',     icon: 'fa-gauge' },
  { key: 'procurement',  labelKey: 'nav.procurement',   icon: 'fa-cart-shopping' },
  { key: 'assortment',   labelKey: 'nav.assortment',    icon: 'fa-boxes-stacked' },
  { key: 'customerCare', labelKey: 'nav.customerCare',  icon: 'fa-handshake' },
  { key: 'offers',       labelKey: 'nav.offers',        icon: 'fa-file-contract' },
  { key: 'orders',       labelKey: 'nav.orders',        icon: 'fa-truck' },
  { key: 'invoices',     labelKey: 'nav.invoices',      icon: 'fa-file-invoice' },
  { key: 'products',     labelKey: 'nav.products',      icon: 'fa-box' },
  { key: 'recipients',   labelKey: 'nav.customers',     icon: 'fa-users' },
  { key: 'users',        labelKey: 'nav.employees',     icon: 'fa-user-shield' },
  { key: 'company',      labelKey: 'nav.company',       icon: 'fa-building' },
  { key: 'settings',     labelKey: 'nav.settings',      icon: 'fa-gear' },
];

/** Standard-Konfiguration: admin sieht alles */
const DEFAULT_SETTINGS: RoleDashboardSettings = {
  configs: [
    {
      role: 'admin',
      visibleSections: ALL_SECTIONS.map(s => s.key)
    }
  ]
};

@Injectable({ providedIn: 'root' })
export class RoleDashboardService {
  private authService = inject(AuthService);

  private _settings = signal<RoleDashboardSettings>(this.load());

  readonly settings = this._settings.asReadonly();

  /** Bereiche, die der aktuell eingeloggte User sehen darf */
  readonly visibleSections = computed<Set<DashboardSection>>(() => {
    const roles = this.authService.roles();
    const configs = this._settings().configs;

    // Admin bekommt immer alles
    if (roles.includes('admin')) {
      return new Set(ALL_SECTIONS.map(s => s.key));
    }

    const visible = new Set<DashboardSection>();
    for (const config of configs) {
      if (roles.includes(config.role)) {
        for (const section of config.visibleSections) {
          visible.add(section);
        }
      }
    }

    // Wenn keine Rolle konfiguriert ist, nur Dashboard anzeigen
    if (visible.size === 0) {
      visible.add('dashboard');
    }

    return visible;
  });

  canSee(section: DashboardSection): boolean {
    return this.visibleSections().has(section);
  }

  /** Gibt die Konfiguration für eine bestimmte Rolle zurück (oder erstellt eine leere). */
  getConfigForRole(role: string): RoleDashboardConfig {
    return (
      this._settings().configs.find(c => c.role === role) ?? {
        role,
        visibleSections: ['dashboard']
      }
    );
  }

  /** Speichert die komplette Einstellungs-Konfiguration. */
  saveSettings(settings: RoleDashboardSettings): void {
    this._settings.set(settings);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
  }

  /** Fügt eine neue Rolle hinzu oder aktualisiert eine bestehende. */
  upsertRoleConfig(config: RoleDashboardConfig): void {
    this._settings.update(current => {
      const exists = current.configs.some(c => c.role === config.role);
      const configs = exists
        ? current.configs.map(c => (c.role === config.role ? config : c))
        : [...current.configs, config];
      const next = { configs };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      return next;
    });
  }

  /** Löscht eine Rollenkonfiguration. */
  deleteRoleConfig(role: string): void {
    this._settings.update(current => {
      const next = { configs: current.configs.filter(c => c.role !== role) };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      return next;
    });
  }

  private load(): RoleDashboardSettings {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        return JSON.parse(raw) as RoleDashboardSettings;
      }
    } catch {
      // ignore parse errors
    }
    return DEFAULT_SETTINGS;
  }
}

