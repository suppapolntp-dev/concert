import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const expectedRoles = (route.data?.['roles'] as string[]) || [];

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  if (expectedRoles.length > 0 && !authService.hasAnyRole(expectedRoles)) {
    return router.createUrlTree(['/unauthorized']);
  }

  return true;
};
