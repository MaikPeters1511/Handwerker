import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, from, switchMap, catchError, of, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export interface KeycloakUserProfile {
  id?: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  emailVerified?: boolean;
  attributes?: {
    phoneNumber?: string[];
    gender?: string[];
    country?: string[];
    address?: string[];
    profileImageUrl?: string[];
    [key: string]: string[] | undefined;
  };
}

export interface ProfileFormData {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  gender?: string;
  country?: string;
  address?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  // Verwende relativen Pfad für Proxy
  // Der Proxy leitet /realms/* an http://localhost:8080 weiter
  private readonly accountApiUrl = '/realms/handwerker/account';

  /**
   * Load the current user's profile from Backend API
   * Backend holt die Daten von Keycloak mit allen Attributes
   */
  getUserProfile(): Observable<KeycloakUserProfile> {
    return this.http.get<KeycloakUserProfile>('/api/profile').pipe(
      catchError(error => {
        console.warn('Failed to load profile from backend, using token data as fallback', error);
        // Fallback: Verwende Token-Daten (ohne Attributes)
        return this.getProfileFromToken();
      })
    );
  }

  /**
   * Get profile data from the ID token as fallback
   */
  private getProfileFromToken(): Observable<KeycloakUserProfile> {
    const userProfile = this.authService.userProfile();
    if (!userProfile?.info) {
      return throwError(() => new Error('No user profile data available'));
    }

    const profile: KeycloakUserProfile = {
      firstName: userProfile.info.given_name || '',
      lastName: userProfile.info.family_name || '',
      email: userProfile.info.email || '',
      username: userProfile.info.preferred_username || '',
      emailVerified: userProfile.info.email_verified || false,
      attributes: {
        phoneNumber: [''],
        gender: [''],
        country: [''],
        address: ['']
      }
    };

    return of(profile);
  }

  /**
   * Update the current user's profile via Backend API
   * (Keycloak Account API gibt 401 zurück, daher verwenden wir unsere eigene API)
   */
  updateUserProfile(profile: Partial<KeycloakUserProfile>): Observable<KeycloakUserProfile> {
    // Verwende Backend-API statt Keycloak direkt anzusprechen
    return this.http.put<KeycloakUserProfile>('/api/profile', profile);
  }

  /**
   * Upload profile image to backend API
   */
  uploadProfileImage(imageFile: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('image', imageFile);

    return this.http.post<{ imageUrl: string }>('/api/profile/image', formData);
  }

  /**
   * Get profile image URL from backend API
   */
  getProfileImageUrl(): Observable<{ imageUrl: string | null }> {
    return this.http.get<{ imageUrl: string | null }>('/api/profile/image');
  }

  /**
   * Delete profile image from backend API
   */
  deleteProfileImage(): Observable<void> {
    return this.http.delete<void>('/api/profile/image');
  }

  /**
   * Convert Keycloak profile to form data
   */
  profileToFormData(profile: KeycloakUserProfile): ProfileFormData {
    return {
      firstName: profile.firstName || '',
      lastName: profile.lastName || '',
      email: profile.email || '',
      phoneNumber: profile.attributes?.phoneNumber?.[0] || '',
      gender: profile.attributes?.gender?.[0] || '',
      country: profile.attributes?.country?.[0] || '',
      address: profile.attributes?.address?.[0] || ''
    };
  }

  /**
   * Convert form data to Keycloak profile format
   */
  formDataToProfile(formData: ProfileFormData): Partial<KeycloakUserProfile> {
    const attributes: KeycloakUserProfile['attributes'] = {};

    // Füge nur nicht-leere Attribute hinzu
    if (formData.phoneNumber?.trim()) {
      attributes.phoneNumber = [formData.phoneNumber.trim()];
    }
    if (formData.gender?.trim()) {
      attributes.gender = [formData.gender.trim()];
    }
    if (formData.country?.trim()) {
      attributes.country = [formData.country.trim()];
    }
    if (formData.address?.trim()) {
      attributes.address = [formData.address.trim()];
    }

    return {
      firstName: formData.firstName,
      lastName: formData.lastName,
      email: formData.email,
      attributes
    };
  }
}
