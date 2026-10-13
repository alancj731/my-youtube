import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { YouTubePlayer } from '@angular/youtube-player';
import { Subscription } from 'rxjs';
import { errorMessage } from '../../core/api-error';
import { Video } from '../../core/models';
import { TagService } from '../../core/tag.service';
import { VideoService } from '../../core/video.service';
import { Icon } from '../../shared/icon';
import { TagInput } from '../../shared/tag-input';
import { TimeAgoPipe } from '../../shared/time-ago.pipe';
import { VideoCard } from '../../shared/video-card';

@Component({
  selector: 'app-watch',
  imports: [YouTubePlayer, RouterLink, Icon, TagInput, TimeAgoPipe, VideoCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './watch.html',
  styleUrl: './watch.scss',
})
export class Watch {
  private readonly videoService = inject(VideoService);
  private readonly tagService = inject(TagService);
  private readonly router = inject(Router);

  /** Route parameter, bound by the router. */
  readonly id = input.required<string>();

  protected readonly video = signal<Video | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly related = signal<Video[]>([]);
  protected readonly tags = signal<string[]>([]);
  protected readonly saveState = signal<'idle' | 'saving' | 'saved' | 'error'>('idle');
  protected readonly saveError = signal<string | null>(null);
  protected readonly confirmingDelete = signal(false);
  protected readonly deleting = signal(false);

  protected readonly knownTags = computed(() => this.tagService.tags.value().map((t) => t.name));

  private readonly playerHost = viewChild<ElementRef<HTMLElement>>('playerHost');
  protected readonly playerWidth = signal(640);
  protected readonly playerHeight = computed(() => Math.round((this.playerWidth() * 9) / 16));
  protected readonly playerVars: YT.PlayerVars = { autoplay: 1, rel: 0, playsinline: 1 };

  private requests = new Subscription();
  private saveRequest?: Subscription;

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => this.load(id));
    });

    // Size the iframe to its column (the player takes explicit pixel dimensions).
    const observer = new ResizeObserver(([entry]) => this.playerWidth.set(Math.floor(entry.contentRect.width)));
    effect(() => {
      const host = this.playerHost()?.nativeElement;
      observer.disconnect();
      if (host) observer.observe(host);
    });
    inject(DestroyRef).onDestroy(() => {
      observer.disconnect();
      this.requests.unsubscribe();
    });
  }

  protected onTagsChange(tags: string[]): void {
    const video = this.video();
    if (!video) return;
    this.tags.set(tags);
    this.saveState.set('saving');
    this.saveError.set(null);

    this.saveRequest?.unsubscribe();
    this.saveRequest = this.videoService.updateTags(video.id, tags).subscribe({
      next: (updated) => {
        this.video.set(updated);
        this.saveState.set('saved');
      },
      error: (err) => {
        this.saveState.set('error');
        this.saveError.set(errorMessage(err, "Couldn't save tags."));
      },
    });
  }

  protected delete(): void {
    const video = this.video();
    if (!video) return;
    this.deleting.set(true);
    this.videoService.delete(video.id).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: (err) => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
        this.saveError.set(errorMessage(err, "Couldn't delete the video."));
      },
    });
  }

  private load(id: string): void {
    this.requests.unsubscribe();
    this.requests = new Subscription();
    this.video.set(null);
    this.related.set([]);
    this.loadError.set(null);
    this.saveState.set('idle');
    this.saveError.set(null);
    this.confirmingDelete.set(false);
    window.scrollTo({ top: 0 });

    this.requests.add(
      this.videoService.get(id).subscribe({
        next: (video) => {
          this.video.set(video);
          this.tags.set(video.tags);
          this.loadRelated(video);
        },
        error: (err) => this.loadError.set(errorMessage(err, "Couldn't load this video.")),
      }),
    );
  }

  /** "Up next": videos sharing any tag, falling back to the latest saved videos. */
  private loadRelated(video: Video): void {
    this.requests.add(
      this.videoService
        .search({ tags: video.tags, match: 'any', q: '' }, 1, 21)
        .subscribe((page) => this.related.set(page.items.filter((v) => v.id !== video.id).slice(0, 20))),
    );
  }
}
