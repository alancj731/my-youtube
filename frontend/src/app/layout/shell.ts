import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { LayoutService } from '../core/layout.service';
import { TagService } from '../core/tag.service';
import { parseTagList } from '../core/tag-utils';
import { Icon } from '../shared/icon';
import { Logo } from '../shared/logo';
import { AddVideoDialog } from './add-video-dialog';
import { SearchBox } from './search-box';

const WIDE_QUERY = '(min-width: 1100px)';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon, Logo, SearchBox, AddVideoDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
  host: { '(document:keydown.escape)': 'closeOverlays()' },
})
export class Shell {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly layout = inject(LayoutService);
  protected readonly tagService = inject(TagService);

  private readonly media = matchMedia(WIDE_QUERY);
  protected readonly wide = signal(this.media.matches);
  /** Wide screens: full vs. mini sidebar. Narrow screens: drawer open or closed. */
  protected readonly navExpanded = signal(this.media.matches);
  protected readonly menuOpen = signal(false);

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => this.router.url),
    ),
    { initialValue: this.router.url },
  );

  protected readonly selectedTags = computed(() => {
    const tree = this.router.parseUrl(this.url());
    return tree.root.children['primary'] ? [] : parseTagList(tree.queryParams['tags']);
  });

  protected readonly isHome = computed(() => {
    const tree = this.router.parseUrl(this.url());
    return !tree.root.children['primary'] && this.selectedTags().length === 0;
  });

  protected readonly initial = computed(() =>
    (this.auth.user()?.displayName ?? '?').trim().charAt(0).toUpperCase(),
  );

  constructor() {
    const onChange = (e: MediaQueryListEvent) => {
      this.wide.set(e.matches);
      this.navExpanded.set(e.matches);
    };
    this.media.addEventListener('change', onChange);
    inject(DestroyRef).onDestroy(() => this.media.removeEventListener('change', onChange));

    // Close the mobile drawer after navigating.
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        if (!this.wide()) this.navExpanded.set(false);
        this.menuOpen.set(false);
      });
  }

  protected signOut(): void {
    void this.auth.logout();
  }

  protected toggleNav(): void {
    this.navExpanded.update((v) => !v);
  }

  protected closeOverlays(): void {
    this.menuOpen.set(false);
    if (!this.wide()) this.navExpanded.set(false);
  }

  protected isTagSelected(name: string): boolean {
    const selected = this.selectedTags();
    return selected.length === 1 && selected[0] === name;
  }
}
