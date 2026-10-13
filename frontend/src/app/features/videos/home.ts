import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { errorMessage } from '../../core/api-error';
import { LayoutService } from '../../core/layout.service';
import { TagMatch, Video, VideoFilter } from '../../core/models';
import { TagService } from '../../core/tag.service';
import { parseTagList } from '../../core/tag-utils';
import { VideoService } from '../../core/video.service';
import { Icon } from '../../shared/icon';
import { InViewDirective } from '../../shared/in-view.directive';
import { VideoCard } from '../../shared/video-card';

const PAGE_SIZE = 24;

@Component({
  selector: 'app-home',
  imports: [RouterLink, VideoCard, Icon, InViewDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly videoService = inject(VideoService);
  protected readonly tagService = inject(TagService);
  protected readonly layout = inject(LayoutService);

  private readonly params = toSignal(this.route.queryParamMap, { requireSync: true });

  protected readonly filter = computed<VideoFilter>(() => {
    const p = this.params();
    return {
      tags: parseTagList(p.get('tags')),
      match: p.get('match') === 'any' ? 'any' : 'all',
      q: p.get('q')?.trim() ?? '',
    };
  });

  protected readonly isFiltered = computed(() => this.filter().tags.length > 0 || !!this.filter().q);

  /** Chip bar: the most used tags, with any selected tags first. */
  protected readonly chipTags = computed(() => {
    const selected = this.filter().tags;
    const others = this.tagService
      .tags.value()
      .map((t) => t.name)
      .filter((t) => !selected.includes(t));
    return [...selected, ...others].slice(0, Math.max(20, selected.length));
  });

  protected readonly videos = signal<Video[]>([]);
  protected readonly total = signal(0);
  protected readonly hasMore = signal(false);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly skeletons = Array.from({ length: 12 }, (_, i) => i);

  private page = 1;
  private request?: Subscription;

  constructor() {
    effect(() => {
      const filter = this.filter();
      this.videoService.changed();
      untracked(() => this.load(filter, 1));
    });
  }

  protected loadMore(): void {
    if (this.hasMore() && !this.loading()) this.load(this.filter(), this.page + 1);
  }

  protected retry(): void {
    this.load(this.filter(), 1);
  }

  protected isOnlyTag(tag: string): boolean {
    const tags = this.filter().tags;
    return tags.length === 1 && tags[0] === tag;
  }

  protected chipLink(tag: string): Record<string, string | null> {
    // Like YouTube: clicking a chip shows just that tag; clicking the active chip clears it.
    return { tags: this.isOnlyTag(tag) ? null : tag, q: this.filter().q || null, match: null };
  }

  protected setMatch(match: TagMatch): void {
    void this.router.navigate([], {
      queryParams: { match: match === 'any' ? 'any' : null },
      queryParamsHandling: 'merge',
    });
  }

  private load(filter: VideoFilter, page: number): void {
    this.request?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);

    this.request = this.videoService.search(filter, page, PAGE_SIZE).subscribe({
      next: (result) => {
        this.page = result.page;
        this.videos.update((current) => (page === 1 ? result.items : [...current, ...result.items]));
        this.total.set(result.totalCount);
        this.hasMore.set(result.hasMore);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(errorMessage(err, "Couldn't load your videos."));
      },
    });
  }
}
