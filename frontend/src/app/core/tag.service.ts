import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthService } from './auth.service';
import { TagSummary } from './models';
import { VideoService } from './video.service';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly videos = inject(VideoService);

  /** The signed-in user's tags with video counts, most used first; reloads when the library changes. */
  readonly tags = httpResource<TagSummary[]>(
    () => {
      this.videos.changed();
      return this.auth.isAuthenticated() ? '/api/tags' : undefined;
    },
    { defaultValue: [] },
  );

  rename(id: string, name: string): Observable<TagSummary> {
    return this.http
      .patch<TagSummary>(`/api/tags/${id}`, { name })
      .pipe(tap(() => this.videos.notify()));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tags/${id}`).pipe(tap(() => this.videos.notify()));
  }
}
