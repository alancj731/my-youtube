/** Mirrors the backend's TagName rules so the UI can reject bad tags before a round trip. */
export const MAX_TAG_LENGTH = 30;

const ALLOWED = /^[\p{L}\p{N} _-]+$/u;

export function normalizeTag(raw: string): string {
  return raw.trim().replace(/^#+/, '').replace(/\s+/g, ' ').toLowerCase();
}

export function tagError(tag: string): string | null {
  if (!tag) return 'Tag cannot be empty.';
  if (tag.length > MAX_TAG_LENGTH) return `Tags can be at most ${MAX_TAG_LENGTH} characters.`;
  if (!ALLOWED.test(tag)) return "Tags may only contain letters, digits, spaces, '-' and '_'.";
  return null;
}

/** Splits "a, b ,c" into normalized, de-duplicated tags (commas are never valid inside a tag). */
export function parseTagList(value: string | null | undefined): string[] {
  if (!value) return [];
  return [...new Set(value.split(',').map(normalizeTag).filter((t) => t.length > 0))];
}
