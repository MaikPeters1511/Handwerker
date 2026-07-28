import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ToastService {
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  success(message: string) {
    this.successMessage.set(message);

    setTimeout(() => this.successMessage.set(null), 3000);
  }

  error(message: string) {
    this.errorMessage.set(message);

    setTimeout(() => this.errorMessage.set(null), 4000);
  }

  clear() {
    this.successMessage.set(null);
    this.errorMessage.set(null);
  }
}
