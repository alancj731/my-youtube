import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { errorMessage } from '../../core/api-error';
import { TagSummary } from '../../core/models';
import { TagService } from '../../core/tag.service';
import { normalizeTag, tagError } from '../../core/tag-utils';
import { Icon } from '../../shared/icon';

@Component({
  selector: 'app-manage-tags',
  imports: [RouterLink, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './manage-tags.html',
  styleUrl: './manage-tags.scss',
})
export class ManageTags {
  protected readonly tagService = inject(TagService);

  protected readonly editingId = signal<string | null>(null);
  protected readonly deletingId = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected startEdit(tag: TagSummary): void {
    this.editingId.set(tag.id);
    this.deletingId.set(null);
    this.error.set(null);
  }

  protected rename(tag: TagSummary, raw: string): void {
    const name = normalizeTag(raw);
    if (name === tag.name) {
      this.editingId.set(null);
      return;
    }
    const problem = tagError(name);
    if (problem) {
      this.error.set(problem);
      return;
    }

    this.busy.set(true);
    this.tagService.rename(tag.id, name).subscribe({
      next: () => {
        this.busy.set(false);
        this.editingId.set(null);
        this.error.set(null);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(errorMessage(err));
      },
    });
  }

  protected remove(tag: TagSummary): void {
    this.busy.set(true);
    this.tagService.delete(tag.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.deletingId.set(null);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(errorMessage(err));
      },
    });
  }
}
