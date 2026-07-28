import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import Keycloak, { KeycloakInstance, KeycloakConfig, KeycloakProfile } from 'keycloak-js';

const origin = window.location.origin;
const REALM_URL = 'https://localhost:8443/realms/handwerker';
const CLIENT_ID = 'angular-client';

type UserClaims = {
  name?: string;            // Vollständiger Name
  given_name?: string;      // Vorname
  family_name?: string;     // Nachname
  preferred_username?: string; // Benutzername
  email?: string;           // E-Mail-Adresse
  email_verified?: boolean; // E-Mail verifiziert?
  realm_access?: {          // Rollen auf Realm-Ebene
    roles: string[];
  };
  resource_access?: {       // Rollen auf Client-Ebene
    [key: string]: {
      roles: string[];
    };
  };
};

type UserProfile = { info: UserClaims };

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Router via functional inject
  private router = inject(Router);

  // Internal Keycloak instance stored in a signal for reactivity
  private _kc = signal<KeycloakInstance | null>(null);
  private _authenticated = signal(false);
  private _initialized = signal(false);

  // Public signals
  userProfile = signal<UserProfile | null>(null);
  isLoggedIn = this._authenticated.asReadonly();
  /** True sobald die Keycloak-Initialisierung abgeschlossen ist (unabhängig vom Auth-Status). */
  initialized = this._initialized.asReadonly();
  userName = computed(() => {
    const profile = this.userProfile();
    return (
      profile?.info?.given_name ||
      profile?.info?.family_name ||
      profile?.info?.preferred_username ||
      profile?.info?.email ||
      'Benutzer'
    );
  });

  /** Realm-Rollen direkt aus dem Keycloak-Token (kc.realmAccess oder idTokenParsed) */
  private _roles = signal<string[]>([]);

  roles = this._roles.asReadonly();

  isAdmin = computed(() => this._roles().includes('admin'));

  constructor() {
    // Start initialization but don't block construction
    void this.init().catch(err => console.error('Keycloak init failed', err));
  }

  // --- Initialisierung ---
  private async init(): Promise<void> {
    const { baseUrl, realm } = this.parseRealmUrl(REALM_URL);

    const config: KeycloakConfig = {
      url: baseUrl,
      realm,
      clientId: CLIENT_ID
    };

    const kc = new Keycloak(config);
    this._kc.set(kc);

    // Event-Handler
    kc.onAuthSuccess = () => {
       this._authenticated.set(true);
       void this.onAuthSuccess();
    };
    kc.onAuthError = (err) => {
       this._authenticated.set(false);
       console.error('Keycloak auth error', err);
    };
    kc.onAuthRefreshSuccess = () => void this.onAuthSuccess();
    kc.onAuthRefreshError = () => {
      console.warn('Keycloak refresh failed');
      void this.logout();
    };
    kc.onAuthLogout = () => {
      this._authenticated.set(false);
      this.userProfile.set(null);
      this._roles.set([]);
    };
    kc.onTokenExpired = () => {
      // Try to refresh token silently; if fails, logout
      void kc.updateToken(5).catch(() => {
        console.warn('Token refresh on expiry failed');
        void this.logout();
      });
    };

    // Init Keycloak. check-sso tries to restore session if present without forcing login.
    try {
      const initialized = await kc.init({ onLoad: 'check-sso', pkceMethod: 'S256', checkLoginIframe: false });
      this._authenticated.set(!!kc.authenticated);

      if (initialized && kc.authenticated) {
        await this.loadUserProfile();
      } else {
        // Not authenticated - keep userProfile null
        this.userProfile.set(null);
      }
    } catch (err) {
      console.error('Keycloak init error', err);
      this._authenticated.set(false);
      this.userProfile.set(null);
    } finally {
      this._initialized.set(true);
    }
  }

  // --- Hilfsfunktionen ---
  private parseRealmUrl(realmUrl: string): { baseUrl: string; realm: string } {
    try {
      const url = new URL(realmUrl);
      // Expecting path like /realms/{realm}
      const parts = url.pathname.split('/').filter(Boolean);
      const realmIndex = parts.indexOf('realms');
      const realm = realmIndex >= 0 && parts.length > realmIndex + 1 ? parts[realmIndex + 1] : parts[parts.length - 1];
      // baseUrl should be origin (protocol + host + optional port) + potential context before /realms
      const basePath = parts.slice(0, realmIndex === -1 ? parts.length - 1 : realmIndex).join('/');
      const baseUrl = `${url.protocol}//${url.host}${basePath ? '/' + basePath : ''}`;
      return { baseUrl, realm };
    } catch (e) {
      // Fallback: try to be permissive
      console.warn('parseRealmUrl failed, falling back to defaults', e);
      return { baseUrl: 'https://localhost:8443', realm: 'handwerker' };
    }
  }

  private async onAuthSuccess(): Promise<void> {
    // When auth succeeded, try to load profile
    await this.loadUserProfile();
  }

  private async loadUserProfile(): Promise<void> {
    const kc = this._kc();
    if (!kc) return;

    // Rollen direkt aus Keycloak-JS realmAccess lesen (wird automatisch aus dem Access-Token geparst)
    const realmRoles: string[] = kc.realmAccess?.roles ?? [];

    // Fallback: Rollen aus idTokenParsed lesen falls realmAccess leer ist
    if (realmRoles.length === 0 && kc.idTokenParsed) {
      const parsed = kc.idTokenParsed as Record<string, unknown>;
      // Mapper sendet ggf. realm_access als Objekt { roles: [...] } oder als flaches Array
      const realmAccess = parsed['realm_access'] as { roles?: string[] } | undefined;
      if (realmAccess?.roles?.length) {
        this._roles.set(realmAccess.roles);
      }
    } else {
      this._roles.set(realmRoles);
    }

    // Optimierung: Daten direkt aus dem ID-Token lesen (vermeidet HTTP-Request & CORS-Probleme)
    if (kc.idTokenParsed) {
      // idTokenParsed enthält Standard OIDC Claims
      this.userProfile.set({
        info: this.toClaims(kc.idTokenParsed as unknown as KeycloakProfile)
      });
      return;
    }

    // Fallback: Nur laden, wenn kein ID-Token vorhanden (selten)
    try {
      const profile = await kc.loadUserProfile();
      this.userProfile.set({ info: this.toClaims(profile) });
    } catch (err) {
      console.error('Failed to load Keycloak profile', err);
      // Nicht null setzen, um UI nicht zu flackern, falls wir vorher Daten hatten
      if (!this.userProfile()) {
         this.userProfile.set(null);
      }
    }
  }

  private toClaims(profile: any): UserClaims {
    // KeycloakProfile uses camelCase fields. Our UI expects common OIDC claim names.
    // When parsing from idTokenParsed, they are already OIDC names.
    const firstName = profile.given_name ?? profile.firstName ?? undefined;
    const lastName = profile.family_name ?? profile.lastName ?? undefined;

    const name = (profile.name ?? [firstName, lastName].filter(Boolean).join(' ')) || undefined;

    return {
      name,
      given_name: firstName,
      family_name: lastName,
      email: profile.email ?? undefined,
      preferred_username: profile.preferred_username ?? profile.username ?? undefined,
      email_verified: profile.email_verified ?? undefined,
      realm_access: profile.realm_access ?? undefined,
      resource_access: profile.resource_access ?? undefined
    };
  }

  // --- Öffentliche API ---
  async login(): Promise<void> {
    const kc = this._kc();
    if (!kc) throw new Error('Keycloak not initialized');
    await kc.login({ redirectUri: `${origin}/` });
  }

  async logout(): Promise<void> {
    const kc = this._kc();
    if (!kc) {
      this._authenticated.set(false);
      this.userProfile.set(null);
      void this.router.navigate(['/']);
      return;
    }

    try {
      await kc.logout({ redirectUri: `${origin}/` });
    } catch (err) {
      // Even if logout fails, clear local state
      console.error('Keycloak logout failed', err);
    }

    this._authenticated.set(false);
    this.userProfile.set(null);
    void this.router.navigate(['/']);
  }

  /**
   * Checks if the user has a specific realm role.
   */
  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  /**
   * Returns a valid token string, attempting a refresh if needed.
   * If no token is available, returns null.
   */
  async getToken(minValiditySeconds = 30): Promise<string | null> {
    const kc = this._kc();
    if (!kc) return null;

    try {
      // updateToken resolves to true if token was refreshed
      // We call updateToken to ensure token validity
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore - typings between keycloak-js versions differ
      await kc.updateToken(minValiditySeconds);
      return kc.token ?? null;
    } catch (err) {
      console.warn('Failed to refresh token', err);
      return kc.token ?? null;
    }
  }
}
