import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { errorMessage } from '../core/api-error';
import { LayoutService } from '../core/layout.service';
import { Video } from '../core/models';
import { TagService } from '../core/tag.service';
import { VideoService } from '../core/video.service';
import { Icon } from '../shared/icon';
import { TagInput } from '../shared/tag-input';

const ID_PATTERN = /(?:v=|youtu\.be\/|shorts\/|embed\/|live\/)([\w-]{11})|^([\w-]{11})$/;

@Component({
  selector: 'app-add-video-dialog',
  imports: [Icon, TagInput],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './add-video-dialog.html',
  styleUrl: './add-video-dialog.scss',
})
export class AddVideoDialog {
  private readonly layout = inject(LayoutService);
  private readonly videos = inject(VideoService);
  private readonly router = inject(Router);
  protected readonly tagService = inject(TagService);

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  protected readonly url = signal('');
  protected readonly tags = signal<string[]>([]);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly added = signal<Video | null>(null);

  protected readonly knownTags = computed(() => this.tagService.tags.value().map((t) => t.name));

  /** Instant thumbnail preview from the pasted link, before the server confirms it. */
  protected readonly previewId = computed(() => {
    const match = this.url().trim().match(ID_PATTERN);
    return match ? (match[1] ?? match[2]) : null;
  });

  constructor() {
    effect(() => {
      const dialog = this.dialog().nativeElement;
      if (this.layout.addVideoOpen() && !dialog.open) {
        this.reset();
        dialog.showModal();
      } else if (!this.layout.addVideoOpen() && dialog.open) {
        dialog.close();
      }
    });
  }

  protected close(): void {
    this.layout.addVideoOpen.set(false);
  }

  /** Clicks on the dialog element itself (not its panel) land on the backdrop. */
  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement) this.close();
  }

  protected save(): void {
    if (!this.url().trim() || this.saving()) return;
    this.saving.set(true);
    this.error.set(null);

    this.videos.add(this.url().trim(), this.tags()).subscribe({
      next: (video) => {
        this.saving.set(false);
        this.added.set(video);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(errorMessage(err));
      },
    });
  }

  protected watch(video: Video): void {
    this.close();
    void this.router.navigate(['/watch', video.id]);
  }

  protected addAnother(): void {
    this.reset();
  }

  private reset(): void {
    this.url.set('');
    this.tags.set([]);
    this.error.set(null);
    this.added.set(null);
    this.saving.set(false);
  }
}
