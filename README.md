# RVM.CineTrack

Plataforma de tracking de filmes e series com avaliacoes em escala de 1-5 oscars, reviews publicas, rankings da comunidade, watchlist com 4 estados e alertas de lancamento. Integra com a TMDB API para busca, sincronizacao de dados e provedores de streaming.

**Projeto de portfolio** — demonstra integracao com API externa (TMDB), Blazor Server interativo, background workers e Clean Architecture.

## Funcionalidades

- **Busca TMDB:** filmes e series (paginada), trending diario/semanal, proximos lancamentos
- **Watchlist:** 4 estados (`WantToWatch`, `Watching`, `Watched`, `Dropped`) com timestamps automaticos
- **Avaliacoes 1-5 Oscars:** por filme, temporada ou episodio individual
- **Reviews publicas:** com nota e comentario, visiveis para toda a comunidade
- **Rankings:** top/worst avaliados por media de notas da comunidade
- **Alertas de lancamento:** worker diario notifica temporadas e filmes com estreia nos proximos 7 dias
- **Onde assistir:** provedores de streaming para o Brasil (flatrate, aluguel, compra) via TMDB
- **Stats pessoais:** filmes/series/episodios assistidos, horas totais, top generos, top artistas, streak
- **Tracking por episodio:** marcar/desmarcar episodios e temporadas inteiras
- **Feed social:** reviews recentes da comunidade com dados do usuario e da midia
- **Painel Blazor:** dashboard, busca, minha lista, rankings, alertas
- 67 testes xUnit

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core (Controllers) |
| Frontend | Blazor Server (Interactive SSR) |
| Banco de dados | PostgreSQL 16 (EF Core 10) |
| API externa | TMDB API v3 |
| Background worker | `ReleaseAlertWorker` (Hosted Service) |
| Sincronizacao | `MediaSyncService` |
| Autenticacao | API Key via `X-Api-Key` |
| Logs | Serilog + Seq |
| Container | Docker (Alpine) |

## Como Rodar

**Pre-requisitos:** Docker, .NET 10 SDK, chave TMDB API

```bash
# Subir via Docker Compose
docker compose -f docker-compose.prod.yml up -d

# Ou rodar diretamente (requer PostgreSQL local)
cd src/RVM.CineTrack.API
TMDB__ApiKey=<sua-chave> dotnet run
```

**Acesso local:** `http://localhost:5090`

Variaveis de ambiente essenciais:

| Variavel | Descricao |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL |
| `Tmdb__ApiKey` | Chave da TMDB API (obter em themoviedb.org) |
| `ApiKeys__Keys__0__Key` | Chave de autenticacao da API |

## Estrutura

```
RVM.CineTrack/
├── src/
│   ├── RVM.CineTrack.Domain/          # Entidades, enums, interfaces, value objects
│   ├── RVM.CineTrack.Infrastructure/  # EF Core, TMDB client, repositorios
│   └── RVM.CineTrack.API/             # Controllers, DTOs, Blazor, workers
├── test/
│   ├── RVM.CineTrack.Test/            # 67 testes xUnit
│   └── playwright/                    # Testes E2E Playwright
├── docs/
│   └── MANUAL.md                      # Manual completo de uso e API
└── docker-compose.prod.yml
```

## Painel Blazor

| Rota | Descricao |
|---|---|
| `/` | Dashboard com KPIs, top 5 avaliados, atividade recente |
| `/search` | Busca filmes e series no TMDB (paginada) |
| `/watchlist` | Minha lista com filtros por status |
| `/rankings` | Top/worst avaliados pela comunidade |
| `/alerts` | Alertas de lancamento pendentes |

Documentacao completa dos endpoints em [`docs/MANUAL.md`](docs/MANUAL.md).

## Licenca

MIT
