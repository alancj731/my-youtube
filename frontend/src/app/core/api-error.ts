import { HttpErrorResponse } from '@angular/common/http';

/** Turns an HTTP failure (ProblemDetails from the API) into a message fit for the UI. */
export function errorMessage(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return "Can't reach the server. Is the API running?";
    if (err.status === 429) return 'Too many attempts. Please wait a minute and try again.';
    const title = err.error?.title;
    if (typeof title === 'string' && title.length > 0 && err.status < 500) return title;
    if (err.status === 502 && typeof title === 'string') return title;
  }
  return fallback;
}
