import { normalizeTag, parseTagList, tagError } from './tag-utils';

describe('tag utils', () => {
  it('normalizes like the backend', () => {
    expect(normalizeTag('  #Lo   Fi ')).toBe('lo fi');
    expect(normalizeTag('C-Sharp_10')).toBe('c-sharp_10');
  });

  it('validates characters and length', () => {
    expect(tagError('jazz')).toBeNull();
    expect(tagError('音乐')).toBeNull();
    expect(tagError('<b>')).not.toBeNull();
    expect(tagError('x'.repeat(31))).not.toBeNull();
    expect(tagError('')).not.toBeNull();
  });

  it('parses comma separated lists', () => {
    expect(parseTagList('Jazz, piano,,jazz ')).toEqual(['jazz', 'piano']);
    expect(parseTagList(null)).toEqual([]);
  });
});
