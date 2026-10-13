# MyYT frontend

Angular 22 app. See the [root README](../README.md) for setup.

- `npm start`: dev server on http://localhost:4200, proxying `/api` to `http://localhost:5080` (`proxy.conf.json`)
- `npx ng test --watch=false`: unit tests (Vitest)
- `npx ng build`: production build in `dist/frontend`

Layout: `core/` (auth, API services, models), `layout/` (shell, search box, add-video dialog), `features/` (auth, videos, tags), `shared/` (video card, tag input, icons).
