/** Alle verfügbaren Dashboard-/Nav-Bereiche der App. */
export type DashboardSection =
  | 'dashboard'
  | 'procurement'
  | 'assortment'
  | 'customerCare'
  | 'users'
  | 'offers'
  | 'invoices'
  | 'orders'
  | 'products'
  | 'recipients'
  | 'company'
  | 'settings';

/** Konfiguration: Welche Rollen dürfen welche Bereiche sehen? */
export interface RoleDashboardConfig {
  /** Keycloak-Rollenname (z. B. "admin", "mitarbeiter") */
  role: string;
  /** Sichtbare Bereiche für diese Rolle */
  visibleSections: DashboardSection[];
}

/** Gespeicherte Konfiguration aller Rollen */
export interface RoleDashboardSettings {
  configs: RoleDashboardConfig[];
}

