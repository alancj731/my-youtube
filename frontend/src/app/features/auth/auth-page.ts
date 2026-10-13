import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FirebaseError } from 'firebase/app';
import { AuthService } from '../../core/auth.service';
import { Logo } from '../../shared/logo';

const MESSAGES: Record<string, string | null> = {
  // The user closed the popup or clicked again: not worth an error.
  'auth/popup-closed-by-user': null,
  'auth/cancelled-popup-request': null,
  'auth/unauthorized-domain': 'This site is not an authorized domain in your Firebase project settings.',
  'auth/operation-not-allowed': 'Google sign-in is not enabled for this Firebase project.',
  'auth/network-request-failed': 'Network error. Check your connection and try again.',
  'auth/invalid-api-key': 'The Firebase config in firebase.config.ts is missing or invalid.',
};

@Component({
  selector: 'app-auth-page',
  imports: [Logo],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
})
export class AuthPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Bound from the query string by the router. */
  readonly returnUrl = input<string | undefined>();

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async signIn(): Promise<void> {
    this.busy.set(true);
    this.error.set(null);
    try {
      await this.auth.signInWithGoogle();
      const target = this.returnUrl();
      await this.router.navigateByUrl(target?.startsWith('/') && !target.startsWith('//') ? target : '/');
    } catch (err) {
      const code = err instanceof FirebaseError ? err.code : '';
      const message = code in MESSAGES ? MESSAGES[code] : "Couldn't sign in. Please try again.";
      this.error.set(message);
      if (message) console.error('Sign-in failed', err);
    } finally {
      this.busy.set(false);
    }
  }
}
