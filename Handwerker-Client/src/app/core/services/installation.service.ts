import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InstallationService {
  private http = inject(HttpClient);
  private router = inject(Router);

  checkInstallationStatus(): Observable<{ isCompleted: boolean }> {
    return this.http.get<{ isCompleted: boolean }>('/api/installation/status');
  }

  navigateBasedOnStatus(): Observable<{ isCompleted: boolean }> {
    return this.checkInstallationStatus();
  }
}
