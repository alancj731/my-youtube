/** The signed-in person, as reported by Firebase Authentication. */
export interface User {
  uid: string;
  email: string;
  displayName: string;
  photoUrl: string | null;
}

export interface Video {
  id: string;
  youTubeId: string;
  title: string;
  channelName: string | null;
  thumbnailUrl: string;
  watchUrl: string;
  tags: string[];
  createdAt: string;
}

export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasMore: boolean;
}

export interface TagSummary {
  id: string;
  name: string;
  videoCount: number;
}

export type TagMatch = 'all' | 'any';

export interface VideoFilter {
  tags: string[];
  match: TagMatch;
  q: string;
}
