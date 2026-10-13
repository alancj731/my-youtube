import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  readonly addVideoOpen = signal(false);

  openAddVideo(): void {
    this.addVideoOpen.set(true);
  }
}
