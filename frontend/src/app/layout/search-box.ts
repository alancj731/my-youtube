import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { TagService } from '../core/tag.service';
import { normalizeTag, parseTagList } from '../core/tag-utils';
import { Icon } from '../shared/icon';

/**
 * YouTube-style search pill. Picking a suggestion (or typing an exact tag name) adds a tag chip;
 * any remaining text searches titles and channels.
 */
@Component({
  selector: 'app-search-box',
  imports: [Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './search-box.html',
  styleUrl: './search-box.scss',
})
export class SearchBox {
  private readonly router = inject(Router);
  private readonly tagService = inject(TagService);

  protected readonly chips = signal<string[]>([]);
  protected readonly text = signal('');
  protected readonly focused = signal(false);
  protected readonly active = signal(-1);

  private readonly knownTags = computed(() => this.tagService.tags.value().map((t) => t.name));

  protected readonly suggestions = computed(() => {
    const query = normalizeTag(this.text());
    const chosen = new Set(this.chips());
    return this.tagService
      .tags.value()
      .filter((t) => !chosen.has(t.name) && (query === '' || t.name.includes(query)))
      .slice(0, 10);
  });

  protected readonly open = computed(() => this.focused() && this.suggestions().length > 0);

  constructor() {
    // Keep the box in sync with the URL (back/forward, sidebar clicks, tag links).
    this.syncFromUrl();
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.syncFromUrl());
  }

  private syncFromUrl(): void {
    const tree = this.router.parseUrl(this.router.url);
    const onHome = !tree.root.children['primary'];
    this.chips.set(onHome ? parseTagList(tree.queryParams['tags']) : []);
    this.text.set(onHome ? (tree.queryParams['q'] ?? '') : '');
  }

  protected onInput(value: string): void {
    this.text.set(value);
    this.active.set(-1);
  }

  protected onKeydown(event: KeyboardEvent): void {
    const count = this.suggestions().length;
    if (event.key === 'ArrowDown' && this.open()) {
      event.preventDefault();
      this.active.update((i) => (i + 1) % count);
    } else if (event.key === 'ArrowUp' && this.open()) {
      event.preventDefault();
      this.active.update((i) => (i <= 0 ? count - 1 : i - 1));
    } else if (event.key === 'Enter') {
      event.preventDefault();
      const picked = this.suggestions()[this.active()];
      if (picked) this.addChip(picked.name);
      else this.submit();
    } else if (event.key === 'Backspace' && !this.text() && this.chips().length) {
      this.chips.update((c) => c.slice(0, -1));
    } else if (event.key === 'Escape') {
      (event.target as HTMLInputElement).blur();
    }
  }

  protected addChip(name: string): void {
    this.chips.update((c) => (c.includes(name) ? c : [...c, name]));
    this.text.set('');
    this.active.set(-1);
    this.submit();
  }

  protected removeChip(name: string): void {
    this.chips.update((c) => c.filter((t) => t !== name));
    this.submit();
  }

  protected clear(): void {
    this.chips.set([]);
    this.text.set('');
    this.submit();
  }

  protected submit(): void {
    let tags = this.chips();
    let q = this.text().trim();

    // Typing a tag's exact name searches by that tag rather than by title text.
    const asTag = normalizeTag(q);
    if (asTag && this.knownTags().includes(asTag)) {
      tags = tags.includes(asTag) ? tags : [...tags, asTag];
      q = '';
      this.chips.set(tags);
      this.text.set('');
    }

    const current = this.router.parseUrl(this.router.url).queryParams;
    void this.router.navigate(['/'], {
      queryParams: {
        tags: tags.length ? tags.join(',') : null,
        q: q || null,
        match: tags.length > 1 ? (current['match'] ?? null) : null,
      },
    });
  }
}
