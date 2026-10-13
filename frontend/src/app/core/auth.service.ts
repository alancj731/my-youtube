import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FirebaseError, initializeApp } from 'firebase/app';
import {
  GoogleAuthProvider,
  User as FirebaseUser,
  getAuth,
  onAuthStateChanged,
  signInWithPopup,
  signInWithRedirect,
  signOut,
} from 'firebase/auth';
import { firebaseConfig } from './firebase.config';
import { User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly router = inject(Router);
  private readonly auth = getAuth(initializeApp(firebaseConfig));
  private readonly firebaseUser = signal<FirebaseUser | null>(null);

  /** False until Firebase has restored (or ruled out) a previous session. */
  readonly ready = signal(false);
  readonly isAuthenticated = computed(() => this.firebaseUser() !== null);
  readonly user = computed<User | null>(() => {
    const u = this.firebaseUser();
    return u
      ? {
          uid: u.uid,
          email: u.email ?? '',
          displayName: u.displayName || (u.email ?? '').split('@')[0],
          photoUrl: u.photoURL,
        }
      : null;
  });

  constructor() {
    onAuthStateChanged(this.auth, (user) => {
      this.firebaseUser.set(user);
      this.ready.set(true);
    });
  }

  async signInWithGoogle(): Promise<void> {
    const provider = new GoogleAuthProvider();
    provider.setCustomParameters({ prompt: 'select_account' });
    try {
      await signInWithPopup(this.auth, provider);
    } catch (err) {
      // Some browsers block popups; a full-page redirect always works.
      if (err instanceof FirebaseError && err.code === 'auth/popup-blocked') {
        await signInWithRedirect(this.auth, provider);
        return;
      }
      throw err;
    }
  }

  /** A current ID token for the API; the SDK refreshes it shortly before it expires. */
  async getIdToken(): Promise<string | null> {
    return (await this.auth.currentUser?.getIdToken()) ?? null;
  }

  async logout(): Promise<void> {
    await signOut(this.auth);
    await this.router.navigateByUrl('/login');
  }
}
