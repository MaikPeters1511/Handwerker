import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { TranslatePipe } from '../../../shared';
import { RoleDashboardService, ALL_SECTIONS, UserService } from '../../../core/services';
import { DashboardSection, RoleDashboardConfig } from '../../../core/interfaces/role-dashboard.interface';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-role-dashboard-settings',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './role-dashboard-settings.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleDashboardSettingsComponent {
  private roleDashboardService = inject(RoleDashboardService);
  private userService = inject(UserService);

  protected readonly allSections = ALL_SECTIONS;

  /** Alle konfigurierten Rollen-Configs */
  protected readonly configs = computed(() => this.roleDashboardService.settings().configs);

  /** Alle verfügbaren Rollen aus Keycloak */
  protected readonly availableRoles = toSignal(this.userService.getRoles(), {
    initialValue: [],
  });

  /** Rollen, die noch nicht konfiguriert sind */
  protected readonly unconfiguredRoles = computed(() => {
    const configured = new Set(this.configs().map(c => c.role));
    return this.availableRoles().filter(r => !configured.has(r.name));
  });

  /** Aktuell ausgewählte Rolle zum Bearbeiten */
  protected selectedRole = signal<string | null>(null);

  /** Draft der aktuellen Bearbeitung */
  protected draftSections = signal<Set<DashboardSection>>(new Set());

  /** Neue Rolle hinzufügen */
  protected newRoleControl = new FormControl<string>('', { nonNullable: true });

  /** Zeigt ob es ungespeicherte Änderungen gibt */
  protected isDirty = signal(false);

  /** Erfolgs- / Fehlerstatus */
  protected saveSuccess = signal(false);

  protected selectRole(role: string): void {
    this.selectedRole.set(role);
    const config = this.roleDashboardService.getConfigForRole(role);
    this.draftSections.set(new Set(config.visibleSections));
    this.isDirty.set(false);
    this.saveSuccess.set(false);
  }

  protected toggleSection(section: DashboardSection): void {
    this.draftSections.update(current => {
      const next = new Set(current);
      if (next.has(section)) {
        next.delete(section);
      } else {
        next.add(section);
      }
      return next;
    });
    this.isDirty.set(true);
    this.saveSuccess.set(false);
  }

  protected isSectionEnabled(section: DashboardSection): boolean {
    return this.draftSections().has(section);
  }

  protected saveRoleConfig(): void {
    const role = this.selectedRole();
    if (!role) return;

    const config: RoleDashboardConfig = {
      role,
      visibleSections: Array.from(this.draftSections()),
    };
    this.roleDashboardService.upsertRoleConfig(config);
    this.isDirty.set(false);
    this.saveSuccess.set(true);
  }

  protected addRole(): void {
    const role = this.newRoleControl.value.trim();
    if (!role) return;

    const config: RoleDashboardConfig = { role, visibleSections: ['dashboard'] };
    this.roleDashboardService.upsertRoleConfig(config);
    this.newRoleControl.reset();
    this.selectRole(role);
  }

  protected addRoleFromSelect(roleName: string): void {
    if (!roleName) return;
    const config: RoleDashboardConfig = { role: roleName, visibleSections: ['dashboard'] };
    this.roleDashboardService.upsertRoleConfig(config);
    this.selectRole(roleName);
  }

  protected deleteRole(role: string, event: Event): void {
    event.stopPropagation();
    this.roleDashboardService.deleteRoleConfig(role);
    if (this.selectedRole() === role) {
      this.selectedRole.set(null);
    }
  }

  protected selectAll(): void {
    this.draftSections.set(new Set(ALL_SECTIONS.map(s => s.key)));
    this.isDirty.set(true);
  }

  protected selectNone(): void {
    this.draftSections.set(new Set(['dashboard'] as DashboardSection[]));
    this.isDirty.set(true);
  }
}



