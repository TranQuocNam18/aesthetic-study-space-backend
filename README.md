# Aesthetic Study Space — Backend API

Production-ready ASP.NET Core 8 Web API for the **Aesthetic Study Space** study-with-me platform. Built with Clean Architecture for startup MVP velocity and future scale.

## Solution structure

```
AestheticStudySpace.sln
src/
├── AestheticStudySpace.Api/           # Controllers, middleware, SignalR, Swagger
├── AestheticStudySpace.Application/   # Services, DTOs, interfaces
├── AestheticStudySpace.Domain/        # Entities, enums, domain exceptions
└── AestheticStudySpace.Infrastructure/# EF Core, repositories, JWT, seed data
```

## Tech stack

| Area | Choice |
|------|--------|
| Runtime | .NET 8 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQL Server (default), PostgreSQL-ready |
| Auth | JWT + BCrypt password hashing |
| Docs | Swagger / OpenAPI + XML comments |
| Realtime (scaffold) | SignalR `PresenceHub` |

## Core design: unified Asset system

Audio and visual layers share one **`Asset`** entity with `AssetType` (`Audio` | `Visual`) and `Category` (Rain, Cafe, Lofi, Pet, etc.). Rooms reference assets through **`RoomAssetMapping`**. User workspace state is stored as flexible JSON in **`UserRoomConfig.JsonConfig`**.

Media files are **not** stored in this repo — only external URLs (Cloudinary, AWS S3, etc.).

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or Docker) **or** PostgreSQL
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## Local setup

### 1. Clone and restore

```bash
cd d:\C#\EXE
dotnet restore
dotnet build
```

### 2. Configure SQL Server

Edit `src/AestheticStudySpace.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AestheticStudySpace;Trusted_Connection=True;TrustServerCertificate=True;"
},
"Database": {
  "Provider": "SqlServer"
},
"Jwt": {
  "Secret": "your-local-secret-at-least-32-characters-long",
  "Issuer": "AestheticStudySpace",
  "Audience": "AestheticStudySpace.Client",
  "AccessTokenMinutes": 60
}
```

> **Important:** Use a strong `Jwt:Secret` (32+ characters). In production, set via environment variables — never commit secrets.

### 3. Apply migrations & seed

Migrations run automatically on startup via `SeedData.InitializeAsync`. To apply manually:

```bash
dotnet ef database update \
  --project src/AestheticStudySpace.Infrastructure/AestheticStudySpace.Infrastructure.csproj \
  --startup-project src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj
```

### 4. Run the API

```bash
dotnet run --project src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj
```

- Swagger UI: `https://localhost:7xxx/swagger` (see `launchSettings.json`)
- Health check: `GET /health`

### Seed credentials

| Field | Value |
|-------|-------|
| Email | `admin@aestheticstudy.space` |
| Password | `Admin@12345` |
| Role | Admin |

Seed includes **2 rooms** (1 premium), **5 assets** (1 premium), and room–asset mappings.

---

## EF Core migrations

**Add migration:**

```bash
dotnet ef migrations add <MigrationName> \
  --project src/AestheticStudySpace.Infrastructure/AestheticStudySpace.Infrastructure.csproj \
  --startup-project src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj \
  --output-dir Persistence/Migrations
```

**Update database:**

```bash
dotnet ef database update \
  --project src/AestheticStudySpace.Infrastructure/AestheticStudySpace.Infrastructure.csproj \
  --startup-project src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj
```

**Remove last migration:**

```bash
dotnet ef migrations remove \
  --project src/AestheticStudySpace.Infrastructure/AestheticStudySpace.Infrastructure.csproj \
  --startup-project src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj
```

---

## PostgreSQL (future migration)

1. Set provider and connection string:

```json
"Database": { "Provider": "PostgreSQL" },
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=aesthetic_study_space;Username=postgres;Password=yourpassword"
}
```

2. On Render or locally, set environment variables:

```
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...
```

3. Generate a **new** migration set for PostgreSQL (provider-specific types differ from SQL Server), or use a fresh database:

```bash
dotnet ef migrations add InitialPostgres ...
dotnet ef database update ...
```

> Enums are stored as strings for provider portability. Avoid SQL Server–specific column types in configurations.

---

## API overview

### Authentication

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/register` | Anonymous |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/refresh-token` | Anonymous |

### Rooms & assets

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/rooms` | Anonymous |
| GET | `/api/rooms/{id}` | Anonymous |
| GET | `/api/assets?type=Audio&category=Rain` | Anonymous |

### Workspace

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/workspace/me` | JWT |
| POST | `/api/workspace/save` | JWT |

### Productivity

| Method | Route | Auth |
|--------|-------|------|
| GET/POST/PUT/DELETE | `/api/todos` | JWT |
| POST | `/api/pomodoro/start` | JWT |
| POST | `/api/pomodoro/end` | JWT |
| GET | `/api/pomodoro/history?page=1&pageSize=20` | JWT |

### Admin (role: Admin)

| Method | Route |
|--------|-------|
| POST/PUT/DELETE | `/api/admin/rooms` |
| POST/PUT/DELETE | `/api/admin/assets` |

### Response envelope

```json
{
  "success": true,
  "data": { },
  "message": "Optional message"
}
```

### Example room detail

```json
{
  "success": true,
  "data": {
    "id": "22222222-2222-2222-2222-222222222201",
    "name": "Cozy Attic",
    "backgroundUrl": "https://res.cloudinary.com/...",
    "isPremium": false,
    "assets": [
      {
        "id": "33333333-3333-3333-3333-333333333301",
        "name": "Gentle Rain",
        "type": "Audio",
        "category": "Rain",
        "url": "https://res.cloudinary.com/...",
        "defaultVolume": 70,
        "defaultPositionX": 0,
        "defaultPositionY": 0,
        "defaultLayerIndex": 0
      }
    ]
  }
}
```

### Workspace JsonConfig example

```json
{
  "theme": "night",
  "assets": [
    { "assetId": "33333333-3333-3333-3333-333333333301", "enabled": true, "volume": 70 },
    {
      "assetId": "33333333-3333-3333-3333-333333333304",
      "enabled": true,
      "position": { "x": 120, "y": 330 }
    }
  ]
}
```

---

## Render deployment

### Option A: Docker (recommended)

1. Push repo to GitHub.
2. Create a **Web Service** on Render → **Docker** → point to `Dockerfile`.
3. Set environment variables:

| Variable | Example |
|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Your SQL Server / Postgres URL |
| `Database__Provider` | `SqlServer` or `PostgreSQL` |
| `Jwt__Secret` | Strong 32+ char secret |
| `Jwt__Issuer` | `AestheticStudySpace` |
| `Jwt__Audience` | `AestheticStudySpace.Client` |
| `Cors__AllowedOrigins__0` | `https://your-app.vercel.app` |
| `Swagger__Enabled` | `false` (production) |

4. Health check path: `/health`
5. Port: Render sets `PORT`; map via `ASPNETCORE_URLS=http://+:$PORT` if needed.

`render.yaml` is included as a starting blueprint.

### Option B: Native .NET

Build command: `dotnet publish src/AestheticStudySpace.Api -c Release -o ./publish`  
Start command: `dotnet AestheticStudySpace.Api.dll`

---

## Environment variables reference

| Key | Description |
|-----|-------------|
| `ConnectionStrings__DefaultConnection` | Database connection string |
| `Database__Provider` | `SqlServer` (default) or `PostgreSQL` |
| `Jwt__Secret` | Signing key for access tokens |
| `Jwt__Issuer` / `Jwt__Audience` | JWT validation |
| `Jwt__AccessTokenMinutes` | Token lifetime |
| `Cors__AllowedOrigins__0` | Vercel frontend origin |
| `Swagger__Enabled` | Enable Swagger in non-dev |

---

## Frontend integration (React + Vite + Vercel)

### CORS

Add your Vercel URL to configuration:

```json
"Cors": {
  "AllowedOrigins": ["https://your-app.vercel.app", "http://localhost:5173"]
}
```

Or on Render: `Cors__AllowedOrigins__0=https://your-app.vercel.app`

### API client example

```typescript
const API_URL = import.meta.env.VITE_API_URL;

async function login(email: string, password: string) {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  const json = await res.json();
  if (!json.success) throw new Error(json.message);
  localStorage.setItem('accessToken', json.data.accessToken);
  localStorage.setItem('refreshToken', json.data.refreshToken);
  return json.data;
}

async function getRooms() {
  const res = await fetch(`${API_URL}/api/rooms`);
  const json = await res.json();
  return json.data;
}

async function saveWorkspace(roomId: string, config: object, token: string) {
  const res = await fetch(`${API_URL}/api/workspace/save`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ roomId, jsonConfig: JSON.stringify(config) }),
  });
  return (await res.json()).data;
}
```

### Vercel env

```
VITE_API_URL=https://your-api.onrender.com
```

### SignalR (future)

Connect to `/hubs/presence` with JWT via query: `?access_token={token}` (configured in `JwtBearerEvents`).

---

## Security notes

- Passwords hashed with **BCrypt** (work factor 12)
- **JWT** access + refresh token rotation
- **Role-based** authorization (`User`, `Admin`)
- **Rate limiting** (100 req/min per policy `api`)
- **Global exception handling** — no stack traces leaked in production
- Input validation in application services

---

## Asset hosting

Store media on **Cloudinary** or **AWS S3**. Admin APIs accept URL strings only — the backend never stores binary media.

---

## License

Proprietary — Aesthetic Study Space MVP.
