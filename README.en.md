# RVM.CineTrack

Movie and TV series tracking platform with 1-5 oscar ratings, public reviews, community rankings, a 4-state watchlist, and release alerts. Integrates with the TMDB API for search, data synchronization, and streaming provider information.

**Portfolio project** — showcases external API integration (TMDB), interactive Blazor Server, background workers, and Clean Architecture.

## Features

- **TMDB Search:** movies and series (paginated), daily/weekly trending, upcoming releases
- **Watchlist:** 4 states (`WantToWatch`, `Watching`, `Watched`, `Dropped`) with automatic timestamps
- **1-5 Oscar Ratings:** per movie, season, or individual episode
- **Public reviews:** with rating and optional comment, visible to the whole community
- **Rankings:** top/worst rated by community average
- **Release alerts:** daily worker notifies seasons and movies releasing in the next 7 days
- **Where to Watch:** streaming providers for Brazil (flatrate, rent, buy) via TMDB
- **Personal stats:** watched movies/series/episodes, total hours, top genres, top artists, streak
- **Episode tracking:** mark/unmark individual episodes and full seasons
- **Social feed:** recent community reviews with user and media data
- **Blazor panel:** dashboard, search, my list, rankings, alerts
- 67 xUnit tests

## Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core (Controllers) |
| Frontend | Blazor Server (Interactive SSR) |
| Database | PostgreSQL 16 (EF Core 10) |
| External API | TMDB API v3 |
| Background worker | `ReleaseAlertWorker` (Hosted Service) |
| Sync service | `MediaSyncService` |
| Authentication | API Key via `X-Api-Key` |
| Logging | Serilog + Seq |
| Container | Docker (Alpine) |

## Getting Started

**Requirements:** Docker, .NET 10 SDK, TMDB API key

```bash
# Start with Docker Compose
docker compose -f docker-compose.prod.yml up -d

# Or run directly (requires local PostgreSQL)
cd src/RVM.CineTrack.API
TMDB__ApiKey=<your-key> dotnet run
```

**Local access:** `http://localhost:5090`

Essential environment variables:

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Tmdb__ApiKey` | TMDB API key (get at themoviedb.org) |
| `ApiKeys__Keys__0__Key` | API authentication key |

## Structure

```
RVM.CineTrack/
├── src/
│   ├── RVM.CineTrack.Domain/          # Entities, enums, interfaces, value objects
│   ├── RVM.CineTrack.Infrastructure/  # EF Core, TMDB client, repositories
│   └── RVM.CineTrack.API/             # Controllers, DTOs, Blazor, workers
├── test/
│   ├── RVM.CineTrack.Test/            # 67 xUnit tests
│   └── playwright/                    # Playwright E2E tests
├── docs/
│   └── MANUAL.md                      # Full usage and API reference
└── docker-compose.prod.yml
```

## Blazor Panel

| Route | Description |
|---|---|
| `/` | Dashboard with KPIs, top 5 rated, recent activity |
| `/search` | Search movies and series on TMDB (paginated) |
| `/watchlist` | My list with status filters |
| `/rankings` | Top/worst rated by the community |
| `/alerts` | Pending release alerts |

Full API endpoint documentation in [`docs/MANUAL.md`](docs/MANUAL.md).

## License

MIT
