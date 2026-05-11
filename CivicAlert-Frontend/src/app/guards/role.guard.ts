import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const userRole = authService.userRole(); // Folosim semnalul tău din AuthService

    if (userRole && allowedRoles.includes(userRole)) {
      return true;
    }

    // Dacă nu are rolul necesar, îl trimitem la o pagină sigură (ex: Home sau Login)
    router.navigate(['/home']);
    return false;
  };
};