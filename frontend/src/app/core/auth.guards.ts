import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { CanActivateFn, Router } from '@angular/router';
import { filter, map, take } from 'rxjs';
import { AuthService } from './auth.service';

/** Waits for Firebase to restore the session so a reload doesn't bounce through /login. */
function whenReady<T>(decide: (auth: AuthService) => T) {
  const auth = inject(AuthService);
  return toObservable(auth.ready).pipe(
    filter(Boolean),
    take(1),
    map(() => decide(auth)),
  );
}

export const authGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);
  return whenReady(
    (auth) =>
      auth.isAuthenticated() ||
      router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }),
  );
};

export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);
  return whenReady((auth) => !auth.isAuthenticated() || router.createUrlTree(['/']));
};
