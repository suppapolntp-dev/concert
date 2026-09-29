import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TokenService } from './token.service';

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  readonly currentUser = signal<UserProfile | null>(null);
  readonly isAuthenticated = computed(() => !!this.tokenService.token());

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return roles.some((role) => user.roles.includes(role));
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return user.permissions.includes(permission) || user.roles.includes('PlatformAdmin');
  }

  login(credentials: { email: string; password?: string }): void {
    // Skeleton implementation
    console.log('AuthService.login skeleton', credentials);
  }

  logout(): void {
    this.tokenService.clearTokens();
    this.currentUser.set(null);
    this.router.navigate(['/']);
  }
}
