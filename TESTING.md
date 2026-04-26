# TESTING — RVM.CineTrack

## Testes Unitarios (xUnit)

**Cobertura:** 67 testes cobrindo services, workers, controllers e integracao TMDB.

```bash
# Rodar todos os testes
dotnet test

# Com verbosidade detalhada
dotnet test --logger "console;verbosity=detailed"

# Filtrar por categoria
dotnet test --filter "Category=Service"
dotnet test --filter "Category=Worker"
dotnet test --filter "Category=Controller"
```

**Projeto de testes:** `test/RVM.CineTrack.Test/`

### O que e testado

| Area | Exemplos |
|---|---|
| `MediaSyncService` | Sincronizacao de filmes, series, temporadas, episodios, provedores |
| `ReleaseAlertWorker` | Criacao de alertas, deduplicacao, filtro por data |
| `TmdbClient` | Parsing de respostas TMDB (mock HTTP) |
| `WatchListController` | CRUD watchlist, atualizacao de estado |
| `ReviewsController` | Criacao/atualizacao/delecao de reviews |
| `RankingsController` | Top/worst avaliados |

## Testes E2E (Playwright)

Os testes E2E ficam em dois lugares:

1. **Local ao repo** (`test/playwright/`) — testes diretos contra a instancia rodando
2. **RVM.E2E** — suite centralizada para o portfolio

```bash
# No repo RVM.E2E
npx playwright test cinetrack.spec.ts

# Local (requer app rodando em http://localhost:5090)
cd test/playwright
npx playwright test
```

Os testes E2E cobrem o painel Blazor: dashboard, busca, watchlist, rankings e alertas.

## Ambiente de Testes

Os testes unitarios usam mocks para TMDB (nao fazem chamadas reais a API) e banco in-memory ou SQLite para repositorios. Nenhuma dependencia externa necessaria para `dotnet test`.

Para testes de integracao com banco real:

```bash
docker compose -f docker-compose.prod.yml up -d postgres
dotnet test --filter "Category=Integration"
```

**Nota:** a chave TMDB real NAO e necessaria para testes unitarios — use os mocks providos.
