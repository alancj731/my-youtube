import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-logo',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg viewBox="0 0 28 20" aria-hidden="true">
      <rect width="28" height="20" rx="5" fill="#ff0033" />
      <path d="M11 5.5v9l7.5-4.5z" fill="#fff" />
    </svg>
    <span class="word">MyYT</span>
  `,
  styles: `
    :host { display: inline-flex; align-items: center; gap: 4px; }
    svg { width: 30px; height: 21px; }
    .word { font-family: 'Roboto Condensed', Roboto, sans-serif; font-weight: 700; font-size: 20px; letter-spacing: -0.8px; color: var(--text); }
  `,
})
export class Logo {}
