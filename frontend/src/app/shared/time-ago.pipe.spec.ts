import { timeAgo } from './time-ago.pipe';

describe('timeAgo', () => {
  const now = new Date('2026-10-12T12:00:00Z').getTime();

  it('formats past dates relative to now', () => {
    expect(timeAgo('2026-10-12T11:59:30Z', now)).toBe('just now');
    expect(timeAgo('2026-10-12T09:00:00Z', now)).toBe('3 hours ago');
    expect(timeAgo('2026-10-11T12:00:00Z', now)).toBe('yesterday');
    expect(timeAgo('2025-10-01T12:00:00Z', now)).toBe('last year');
  });
});
