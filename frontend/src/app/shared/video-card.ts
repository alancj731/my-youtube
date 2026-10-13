import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Video } from '../core/models';
import { TimeAgoPipe } from './time-ago.pipe';

@Component({
  selector: 'app-video-card',
  imports: [RouterLink, TimeAgoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let v = video();
    <a class="thumb" [routerLink]="['/watch', v.id]" [attr.aria-label]="v.title">
      <img [src]="v.thumbnailUrl" alt="" loading="lazy" />
    </a>
    <div class="details" [class.compact]="compact()">
      @if (!compact()) {
        <div class="channel-avatar" aria-hidden="true">{{ initial() }}</div>
      }
      <div class="text">
        <a class="title" [routerLink]="['/watch', v.id]" [title]="v.title">{{ v.title }}</a>
        <div class="sub">{{ v.channelName ?? 'Unknown channel' }}</div>
        <div class="sub">Added {{ v.createdAt | timeAgo }}</div>
        @if (v.tags.length) {
          <div class="tags">
            @for (tag of v.tags; track tag) {
              <a class="tag" routerLink="/" [queryParams]="{ tags: tag }">#{{ tag }}</a>
            }
          </div>
        }
      </div>
    </div>
  `,
  styleUrl: './video-card.scss',
  host: { '[class.compact]': 'compact()' },
})
export class VideoCard {
  readonly video = input.required<Video>();
  /** Horizontal layout used in the "up next" column. */
  readonly compact = input(false);

  protected readonly initial = computed(() =>
    (this.video().channelName ?? '?').trim().charAt(0).toUpperCase(),
  );
}
