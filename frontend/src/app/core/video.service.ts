import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Page, Video, VideoFilter } from './models';

@Injectable({ providedIn: 'root' })
export class VideoService {
  private readonly http = inject(HttpClient);

  /** Bumped after every change to the library so lists and tag counts can refresh. */
  readonly changed = signal(0);

  search(filter: VideoFilter, page = 1, pageSize = 24): Observable<Page<Video>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filter.tags.length) params = params.set('tags', filter.tags.join(','));
    if (filter.match === 'any') params = params.set('match', 'any');
    if (filter.q) params = params.set('q', filter.q);
    return this.http.get<Page<Video>>('/api/videos', { params });
  }

  get(id: string): Observable<Video> {
    return this.http.get<Video>(`/api/videos/${id}`);
  }

  add(url: string, tags: string[]): Observable<Video> {
    return this.http.post<Video>('/api/videos', { url, tags }).pipe(tap(() => this.notify()));
  }

  updateTags(id: string, tags: string[]): Observable<Video> {
    return this.http.put<Video>(`/api/videos/${id}/tags`, { tags }).pipe(tap(() => this.notify()));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/videos/${id}`).pipe(tap(() => this.notify()));
  }

  notify(): void {
    this.changed.update((n) => n + 1);
  }
}
