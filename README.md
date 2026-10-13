# MyYT

A personal, YouTube-style library: sign in, save YouTube links, tag them, filter and search by tag, and watch in the app.

- **frontend/**: Angular 22 (standalone components, signals, zoneless) with `@angular/youtube-player`
- **backend/**: .NET 10 minimal API in Clean Architecture layers, storing data in MySQL through EF Core (Oracle `MySql.EntityFrameworkCore`)

```
backend/src/
  MyYt.Domain          entities and value objects (Video, Tag, User, YouTubeVideoId, TagName), Result/Error
  MyYt.Application     use-case handlers, DTOs, validation, repository/service interfaces
  MyYt.Infrastructure  Persistence/ (EF Core DbContext, data entities, configurations, migrations, repositories),
                       Auth/ (JWT, PBKDF2), YouTube/ (oEmbed)
  MyYt.Api             HTTP endpoints, JWT bearer auth, ProblemDetails, OpenAPI
backend/tests/         xUnit tests for Domain and Application
```

## Setup

### 1. MySQL connection

Copy `backend/.env.example` to `backend/.env` (gitignored) and fill in your server:

```bash
MYSQL_HOST=your-host
MYSQL_USER=your-user
MYSQL_PASSWORD=your-password
# optional: MYSQL_PORT=3306, MYSQL_DATABASE=myyt
```

The API loads the nearest `.env` at startup. Real environment variables take precedence, and a full
`ConnectionStrings__Default` overrides the `MYSQL_*` values entirely. It also needs a JWT signing key:

```bash
cd backend/src/MyYt.Api
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"   # any random string of 32+ bytes
```

On startup the API applies pending EF Core migrations, creating the database if the user is allowed to.
Set `Database__MigrateOnStartup=false` to manage migrations yourself instead. MySQL 8.0 or newer is expected (utf8mb4 default charset).

To develop against a throwaway local server:

```bash
podman run --rm -p 13306:3306 -e MYSQL_ROOT_PASSWORD=dev -e MYSQL_DATABASE=myyt mysql:8.4
ConnectionStrings__Default="Server=127.0.0.1;Port=13306;Database=myyt;User=root;Password=dev" \
  dotnet run --project backend/src/MyYt.Api
```

### Migrations

```bash
dotnet tool restore   # installs dotnet-ef from dotnet-tools.json
dotnet ef migrations add <Name> -p backend/src/MyYt.Infrastructure -s backend/src/MyYt.Api -o Persistence/Migrations
dotnet ef migrations script -p backend/src/MyYt.Infrastructure -s backend/src/MyYt.Api   # review the SQL
```

Two quirks of the Oracle MySQL provider to keep in mind when changing the model:
- It drops column collations from generated DDL, so `InitialCreate` sets `tags.name` to `utf8mb4_bin` with an explicit `ALTER TABLE`.
- Don't use its table charset extension (`ForMySQLHasCharset`): the generated migration code calls an internal method and won't compile.

### 2. Run

```bash
# API on http://localhost:5080 (Scalar API docs at /scalar in Development)
dotnet run --project backend/src/MyYt.Api

# UI on http://localhost:4200 (proxies /api to the API)
cd frontend && npm install && npm start
```

### Tests

```bash
dotnet test backend/MyYt.slnx
cd frontend && npx ng test --watch=false
```

## API

| Method | Route | |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | returns a JWT |
| GET | `/api/auth/me` | |
| GET | `/api/videos?tags=a,b&match=all\|any&q=text&page=1&pageSize=24` | search your videos |
| GET / DELETE | `/api/videos/{id}` | |
| POST | `/api/videos` `{ url, tags[] }` | title and channel come from YouTube oEmbed |
| PUT | `/api/videos/{id}/tags` `{ tags[] }` | replaces the video's tags |
| GET | `/api/tags` | your tags with video counts |
| PATCH / DELETE | `/api/tags/{id}` | rename / remove a tag |

Tags are per user, normalized to lower case, compared exactly (binary collation), and removed automatically once no video uses them.
