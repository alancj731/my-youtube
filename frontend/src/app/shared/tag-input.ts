import { ChangeDetectionStrategy, Component, computed, input, model, output, signal } from '@angular/core';
import { normalizeTag, tagError } from '../core/tag-utils';
import { Icon } from './icon';

/** Chip-style tag editor with autocomplete from the user's existing tags. */
@Component({
  selector: 'app-tag-input',
  imports: [Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="field" (click)="input.focus()">
      @for (tag of tags(); track tag) {
        <span class="chip">
          #{{ tag }}
          <button type="button" class="remove" (click)="remove(tag); $event.stopPropagation()" [attr.aria-label]="'Remove ' + tag">
            <app-icon name="close" />
          </button>
        </span>
      }
      <input
        #input
        [id]="inputId()"
        [value]="text()"
        [placeholder]="tags().length ? '' : placeholder()"
        (input)="onInput(input.value)"
        (keydown)="onKeydown($event)"
        (focus)="focused.set(true)"
        (blur)="onBlur()"
        autocomplete="off"
        role="combobox"
        [attr.aria-expanded]="showSuggestions()"
      />
    </div>
    @if (showSuggestions()) {
      <ul class="suggestions" role="listbox">
        @for (s of suggestions(); track s; let i = $index) {
          <li role="option" [class.active]="i === active()" [attr.aria-selected]="i === active()" (mousedown)="$event.preventDefault(); add(s)">
            #{{ s }}
          </li>
        }
      </ul>
    }
    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `,
  styleUrl: './tag-input.scss',
})
export class TagInput {
  readonly tags = model<string[]>([]);
  readonly known = input<string[]>([]);
  readonly placeholder = input('Add tags…');
  readonly inputId = input<string | undefined>(undefined);
  readonly max = input(20);
  /** Enter pressed with nothing left to add. */
  readonly submitted = output<void>();

  protected readonly text = signal('');
  protected readonly focused = signal(false);
  protected readonly active = signal(0);
  protected readonly error = signal<string | null>(null);

  protected readonly suggestions = computed(() => {
    const query = normalizeTag(this.text());
    const chosen = new Set(this.tags());
    return this.known()
      .filter((t) => !chosen.has(t) && (query === '' || t.includes(query)))
      .sort((a, b) => Number(!a.startsWith(query)) - Number(!b.startsWith(query)))
      .slice(0, 8);
  });

  protected readonly showSuggestions = computed(
    () => this.focused() && this.text().length > 0 && this.suggestions().length > 0,
  );

  protected onInput(value: string): void {
    // Typing a comma commits the current tag, like most tag inputs.
    if (value.includes(',')) {
      value.split(',').slice(0, -1).forEach((part) => this.add(part));
      value = value.slice(value.lastIndexOf(',') + 1);
    }
    this.text.set(value);
    this.active.set(0);
    this.error.set(null);
  }

  protected onKeydown(event: KeyboardEvent): void {
    const suggestions = this.suggestions();
    switch (event.key) {
      case 'ArrowDown':
        if (this.showSuggestions()) {
          event.preventDefault();
          this.active.update((i) => (i + 1) % suggestions.length);
        }
        break;
      case 'ArrowUp':
        if (this.showSuggestions()) {
          event.preventDefault();
          this.active.update((i) => (i - 1 + suggestions.length) % suggestions.length);
        }
        break;
      case 'Enter':
      case 'Tab':
        if (this.showSuggestions()) {
          event.preventDefault();
          this.add(suggestions[this.active()]);
        } else if (this.text().trim()) {
          event.preventDefault();
          this.add(this.text());
        } else if (event.key === 'Enter') {
          event.preventDefault();
          this.submitted.emit();
        }
        break;
      case 'Backspace':
        if (!this.text() && this.tags().length) this.remove(this.tags().at(-1)!);
        break;
      case 'Escape':
        this.text.set('');
        break;
    }
  }

  protected onBlur(): void {
    this.focused.set(false);
    if (this.text().trim()) this.add(this.text());
  }

  add(raw: string): void {
    const tag = normalizeTag(raw);
    const problem = tagError(tag);
    if (problem) {
      this.error.set(problem);
      return;
    }
    if (!this.tags().includes(tag)) {
      if (this.tags().length >= this.max()) {
        this.error.set(`At most ${this.max()} tags.`);
        return;
      }
      this.tags.update((tags) => [...tags, tag]);
    }
    this.text.set('');
    this.active.set(0);
    this.error.set(null);
  }

  remove(tag: string): void {
    this.tags.update((tags) => tags.filter((t) => t !== tag));
  }
}
