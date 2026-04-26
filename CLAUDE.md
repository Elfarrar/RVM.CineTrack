# RVM.CineTrack

## Visao Geral

RVM.CineTrack e uma plataforma de tracking de filmes e series com avaliacao em escala de 1-5 oscars, reviews publicas, rankings da comunidade, watchlist com 4 estados e alertas de lancamento. Integra com a TMDB API v3 para busca, sincronizacao de dados e provedores de streaming. Projeto de portfolio — sem dados reais de clientes.

O `MediaSyncService` e responsavel por sincronizar dados do TMDB (detalhes, elenco, temporadas, episodios, provedores) para o banco local quando um titulo e adicionado a watchlist. O `ReleaseAlertWorker` roda diariamente as 08:00 UTC verificando lancamentos nos proximos 7 dias.

## Stack

- **Runtime:** .NET 10
- **API:** ASP.NET Core Controllers
- **Frontend:** Blazor Server (Interactive SSR)
- **Banco:** PostgreSQL 16 via EF Core 10
- **API externa:** TMDB API v3 (`TmdbClient`)
- **Background workers:** `ReleaseAlertWorker`, `MediaSyncService`
- **Auth:** API Key via `X-Api-Key`
- **Logs:** Serilog + Seq opcional
- **Container:** Docker Alpine (multi-stage)
- **Observabilidade:** Health checks, Correlation ID middleware, Rate limiting

## Estrutura

```
src/
  RVM.CineTrack.Domain/          # Entidades (Media, WatchListItem, Review, Alert), Enums, Interfaces
  RVM.CineTrack.Infrastructure/  # EF Core DbContext, TmdbClient, Repositorios, Migrations
  RVM.CineTrack.API/             # Controllers, DTOs, Blazor components, Workers, Health
test/
  RVM.CineTrack.Test/            # 67 testes xUnit
  playwright/                    # Testes E2E Playwright (local ao repo)
docs/
  MANUAL.md                      # Referencia completa de endpoints e fluxos
```

## Entidades Principais

| Entidade | Responsabilidade |
|---|---|
| `Media` | Filme ou serie sincronizado do TMDB |
| `Season` / `Episode` | Temporadas e episodios de series |
| `WatchListItem` | Item na lista pessoal (estado + rating) |
| `Review` | Avaliacao (1-5 oscars) por media/temporada/episodio |
| `ReleaseAlert` | Alerta de lancamento (criado pelo worker) |
| `EpisodeWatch` | Registro de episodio assistido |
| `WatchProvider` | Provedor de streaming (flatrate/rent/buy) |

## Convencoes

- **Auth:** todos os endpoints exigem `X-Api-Key` exceto `/health`, `/api/rankings` e `/api/users/community/recent` (anonimos).
- **Watchlist:** ao adicionar titulo, o `MediaSyncService` sincroniza automaticamente dados TMDB para o banco local.
- **Rating:** escala 1-5 (oscars). Apenas uma review por (userId, mediaId/seasonId/episodeId) — atualizacao in-place.
- **Worker:** `ReleaseAlertWorker` usa `IHostedService` com `PeriodicTimer`, executa diariamente as 08:00 UTC, nao duplica alertas.
- **TMDB Client:** `TmdbClient` encapsula toda comunicacao com a TMDB API. Configurar `Tmdb__ApiKey` no ambiente.
- **Migrations EF Core:** rodar `dotnet ef migrations add <nome>` na camada Infrastructure.

## Como Rodar

```bash
# Docker Compose (prod)
docker compose -f docker-compose.prod.yml up -d
# Acesso: http://localhost:5090

# Direto (requer PostgreSQL local + chave TMDB)
cd src/RVM.CineTrack.API
dotnet run
```

Variaveis essenciais:

| Variavel | Descricao |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL |
| `Tmdb__ApiKey` | Chave TMDB API (themoviedb.org) |
| `ApiKeys__Keys__0__Key` | Chave de autenticacao da API |
| `Seq__ServerUrl` | URL Seq (opcional) |

## Testes

```bash
dotnet test
```

67 testes xUnit. Ver `TESTING.md` para detalhes.

## Decisoes de Arquitetura

- **Clean Architecture** com 3 camadas: Domain, Infrastructure, API (sem Application separada — projeto menor).
- **TMDB como fonte de verdade:** dados de midia nunca sao editados manualmente, sempre sincronizados do TMDB.
- **Sincronizacao lazy:** dados TMDB sao puxados para o banco apenas quando o usuario adiciona o titulo a watchlist (nao em background antecipadamente).
- **Alpine Docker:** imagem menor para portfolio (sem necessidade de SDK completo em prod).
- **Reviews por nivel:** permite avaliar filme, temporada e episodio de forma independente — mais granular que a maioria dos trackers.
