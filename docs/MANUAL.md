# RVM.CineTrack - Manual de Uso

## Visao Geral

RVM.CineTrack e uma plataforma de tracking de filmes e series com avaliacoes, reviews publicas, rankings da comunidade e alertas de lancamento. O projeto faz parte do portfolio RVM Tech e demonstra integracao com a TMDB API, painel Blazor Server interativo, background workers e arquitetura Clean Architecture.

**Stack tecnica:**

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal + Controllers |
| Frontend | Blazor Server (Interactive SSR) |
| Banco de dados | PostgreSQL (via Npgsql + EF Core) |
| API externa | TMDB API v3 |
| Autenticacao | API Key via header `X-Api-Key` |
| Logs | Serilog + Seq |
| Container | Docker (Alpine) |
| Observabilidade | Health checks, Correlation ID middleware, Rate limiting |

---

## Acesso

| Ambiente | URL |
|----------|-----|
| Producao (painel Blazor) | `https://<host>/cinetrack/` |
| Producao (API) | `https://<host>/cinetrack/api/` |
| Desenvolvimento local | `http://localhost:5090/` |
| Health check | `GET /health` (anonimo) |

Para rodar localmente, veja a secao [Como Executar Localmente](#como-executar-localmente).

---

## Funcionalidades

### Busca TMDB

- Busca multi (filmes e series) com paginacao via TMDB API.
- Listagem de trending (semanal ou diario).
- Listagem de lancamentos proximos (filmes upcoming + series on the air).
- Resultados retornam titulo, poster, nota TMDB, sinopse e data de lancamento.

### Watchlist (4 estados)

Cada titulo adicionado a lista pessoal possui um dos 4 estados:

| Estado | Descricao |
|--------|-----------|
| `WantToWatch` | Quero assistir (padrao ao adicionar) |
| `Watching` | Assistindo (registra `StartedAt` automaticamente) |
| `Watched` | Assistido (registra `FinishedAt` automaticamente) |
| `Dropped` | Abandonado |

Ao adicionar um titulo a watchlist, o sistema automaticamente sincroniza os dados do TMDB (detalhes, elenco, temporadas, provedores) para o banco local.

### Avaliacoes 1-5 Oscars

O sistema de avaliacao usa escala de 1 a 5 oscars. Reviews podem ser feitas em diferentes niveis de granularidade:

- **Filme** inteiro (`mediaId`)
- **Temporada** de serie (`seasonId`)
- **Episodio** individual (`episodeId`)

Cada usuario pode ter apenas uma review por combinacao (media/temporada/episodio). Se ja existir, a review e atualizada.

### Reviews Publicas

- Qualquer usuario pode publicar reviews com nota (1-5 oscars) e comentario opcional.
- Reviews sao visiveis para toda a comunidade.
- Deletar review e permitido.

### Rankings (Top / Worst)

- **Top Avaliados**: midias ordenadas pela maior media de notas (minimo 2 reviews via API, 1 via painel).
- **Menos Avaliados**: midias ordenadas pela menor media de notas.
- Rankings sao publicos (endpoints `AllowAnonymous`).
- Limite configuravel (padrao: 20 resultados).

### Alertas de Lancamento (7 dias antes)

O background worker `ReleaseAlertWorker` roda diariamente as 08:00 UTC e:

1. Identifica series na watchlist com status `Watching` ou `WantToWatch`.
2. Verifica temporadas com `AirDate` nos proximos 7 dias.
3. Identifica filmes na watchlist com status `WantToWatch` e `ReleaseDate` nos proximos 7 dias.
4. Cria alertas (sem duplicar) que aparecem no painel e na API.

Alertas podem ser marcados como lidos individualmente ou em lote.

### Onde Assistir (Provedores BR)

Para cada midia sincronizada, o sistema busca os provedores de streaming disponiveis no Brasil via TMDB Watch Providers. Os provedores sao categorizados em:

| Tipo | Descricao |
|------|-----------|
| `flatrate` | Assinatura (Netflix, Prime Video, etc.) |
| `rent` | Aluguel |
| `buy` | Compra |

Cada provedor inclui nome, logo e link para a pagina de watch providers do TMDB.

### Stats Pessoais

O endpoint de estatisticas (`GET /api/stats/{userId}`) retorna:

- **Filmes assistidos** e **series assistidas** (contagem)
- **Episodios assistidos** (contagem)
- **Horas totais** assistidas (soma de runtime de filmes + episodios)
- **Nota media** de todas as reviews do usuario
- **Top 5 generos** mais assistidos
- **Top 10 artistas** mais assistidos (atores presentes nos filmes/series assistidos)
- **Streak atual** (dias consecutivos assistindo episodios)

### Tracking por Episodio

Para series, o sistema permite marcar episodios individuais como assistidos:

- Marcar/desmarcar episodio individual.
- Marcar temporada inteira como assistida (batch).
- Listar episodios assistidos (filtro por temporada).

### Feed Social

O endpoint `GET /api/users/community/recent` (anonimo) retorna as reviews mais recentes da comunidade, incluindo dados do usuario (username, avatar) e da midia (titulo, poster, tipo).

---

## Painel Blazor

O painel web possui 5 paginas interativas renderizadas via Blazor Server:

### 1. Dashboard (`/`)

Pagina inicial com visao geral:
- KPIs: titulos catalogados, usuarios, reviews, filmes, series.
- Chips informativos: nota media, episodios assistidos, alertas pendentes.
- Top 5 avaliados com nota em oscars.
- Atividade recente (ultimas 5 reviews).
- Acoes rapidas (links para busca, lista, rankings).

### 2. Buscar (`/search`)

- Campo de busca com suporte a Enter.
- Resultados em grid de cards com poster, titulo, tipo (filme/serie), ano, nota TMDB e sinopse.
- Paginacao completa (anterior/proxima).

### 3. Minha Lista (`/watchlist`)

- Filtros por status: Todos, Quero assistir, Assistindo, Assistido, Abandonado.
- Contadores por status e por tipo (filmes/series).
- Grid de cards com poster, titulo, status, tipo, nota em oscars e generos.

### 4. Rankings (`/rankings`)

- Toggle entre "Mais bem avaliados" e "Menos bem avaliados".
- Lista numerada com poster, titulo, generos, contagem de reviews e nota media.

### 5. Alertas (`/alerts`)

- Contadores: total e nao lidos.
- Lista de alertas com poster, titulo, descricao, data de estreia e badge "Novo" para nao lidos.

---

## Endpoints da API

Todos os endpoints (exceto os marcados como anonimos) exigem o header `X-Api-Key` com uma chave valida.

### SearchController (`/api/search`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/search?q={query}&page={n}` | Sim | Busca multi (filmes + series) no TMDB |
| `GET` | `/api/search/trending?window={day\|week}` | Sim | Trending do TMDB |
| `GET` | `/api/search/upcoming` | Sim | Proximos lancamentos (filmes + series) |

### MediaController (`/api/media`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/media/{id}` | Sim | Detalhe de midia local (seasons, cast, providers) |
| `POST` | `/api/media/sync/movie/{tmdbId}` | Sim | Sincroniza filme do TMDB para o banco local |
| `POST` | `/api/media/sync/tv/{tmdbId}?episodes={bool}` | Sim | Sincroniza serie do TMDB (com opcao de episodios) |
| `GET` | `/api/media/{id}/seasons/{seasonNumber}/episodes` | Sim | Lista episodios de uma temporada |
| `GET` | `/api/media/{id}/providers` | Sim | Lista provedores de streaming (BR) |

### WatchListController (`/api/watchlist`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/watchlist/{userId}?status={status}` | Sim | Lista watchlist do usuario (filtro opcional) |
| `POST` | `/api/watchlist?userId={id}` | Sim | Adiciona midia a watchlist (sincroniza TMDB se necessario) |
| `PUT` | `/api/watchlist/{id}` | Sim | Atualiza status e/ou rating de um item |
| `DELETE` | `/api/watchlist/{id}` | Sim | Remove item da watchlist |

**Body do POST:**
```json
{
  "tmdbId": 123,
  "mediaType": "movie",
  "status": "WantToWatch"
}
```

**Body do PUT:**
```json
{
  "status": "Watched",
  "rating": 5
}
```

### EpisodeWatchController (`/api/episodes`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/episodes/{userId}/watched?seasonId={id}` | Sim | Lista episodios assistidos |
| `POST` | `/api/episodes/{userId}/watch/{episodeId}` | Sim | Marca episodio como assistido |
| `DELETE` | `/api/episodes/{userId}/watch/{episodeId}` | Sim | Desmarca episodio |
| `POST` | `/api/episodes/{userId}/watch-season/{seasonId}` | Sim | Marca temporada inteira como assistida |

### ReviewsController (`/api/reviews`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/reviews/media/{mediaId}` | Sim | Reviews de uma midia |
| `GET` | `/api/reviews/season/{seasonId}` | Sim | Reviews de uma temporada |
| `GET` | `/api/reviews/episode/{episodeId}` | Sim | Reviews de um episodio |
| `POST` | `/api/reviews` | Sim | Cria ou atualiza review (1-5 oscars + comentario) |
| `DELETE` | `/api/reviews/{id}` | Sim | Remove review |
| `GET` | `/api/reviews/rankings/top?limit={n}` | Nao | Top avaliados (min. 2 reviews) |
| `GET` | `/api/reviews/rankings/worst?limit={n}` | Nao | Menos avaliados (min. 2 reviews) |

**Body do POST:**
```json
{
  "userId": 1,
  "rating": 4,
  "comment": "Excelente!",
  "mediaId": 10,
  "seasonId": null,
  "episodeId": null
}
```

### AlertsController (`/api/alerts`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/alerts/{userId}?unreadOnly={bool}` | Sim | Lista alertas do usuario |
| `PUT` | `/api/alerts/{id}/read` | Sim | Marca alerta como lido |
| `PUT` | `/api/alerts/{userId}/read-all` | Sim | Marca todos os alertas como lidos |

### StatsController (`/api/stats`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/stats/{userId}` | Sim | Estatisticas pessoais completas |

**Resposta exemplo:**
```json
{
  "watchedMovies": 42,
  "watchedSeries": 15,
  "episodesWatched": 320,
  "totalHours": 285.5,
  "averageRating": 3.8,
  "currentStreak": 5,
  "topGenres": [
    { "genre": "Drama", "count": 18 }
  ],
  "topArtists": [
    { "name": "Tom Hanks", "profilePath": "/...", "count": 6 }
  ]
}
```

### UsersController (`/api/users`)

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/api/users/{id}` | Sim | Busca usuario por ID |
| `GET` | `/api/users/by-external/{externalId}` | Sim | Busca usuario por ExternalId (OIDC sub) |
| `POST` | `/api/users` | Sim | Cria usuario |
| `GET` | `/api/users/community/recent?limit={n}` | Nao | Feed social (reviews recentes) |

### Health Check

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `GET` | `/health` | Nao | Verifica saude da aplicacao e banco |

---

## Background Workers

### ReleaseAlertWorker

Worker registrado como `BackgroundService` que roda em loop continuo.

**Comportamento:**
- Aguarda 30 segundos apos a inicializacao da aplicacao.
- Executa a verificacao de lancamentos.
- Agenda a proxima execucao para 08:00 UTC do dia seguinte (ou do mesmo dia, se ainda for antes das 08:00).

**Logica de verificacao:**
1. Busca series na watchlist de todos os usuarios com status `Watching` ou `WantToWatch`.
2. Para cada serie, verifica se alguma temporada tem `AirDate` entre hoje e 7 dias a frente.
3. Busca filmes na watchlist com status `WantToWatch` e `ReleaseDate` nos proximos 7 dias.
4. Cria `ReleaseAlert` para cada combinacao usuario/midia/data que ainda nao tenha alerta.

**Logs:** registra cada alerta criado e o horario da proxima execucao.

---

## TMDB API

### Como funciona a integracao

O `TmdbClient` e um servico registrado via `HttpClient` tipado que se comunica com a TMDB API v3. A autenticacao usa Bearer token (API Read Access Token) configurado via `Tmdb:ApiKey`.

Todas as requisicoes usam `language=pt-BR` por padrao para obter titulos e sinopses em portugues.

### Cache local

O CineTrack nao faz chamadas repetidas ao TMDB para midias ja conhecidas. Quando um usuario busca ou adiciona um titulo:

1. O `MediaSyncService` verifica se o `TmdbId` ja existe no banco local.
2. Se nao existe, busca detalhes completos no TMDB e persiste no PostgreSQL.
3. Dados sincronizados: detalhes da midia, temporadas, episodios (opcional), elenco (top 15 + diretores), provedores de streaming BR.
4. O campo `LastSyncedAt` registra quando a midia foi atualizada pela ultima vez.

### Endpoints TMDB utilizados

| Endpoint TMDB | Uso no CineTrack |
|---------------|------------------|
| `GET /search/multi` | Busca multi (filmes + series) |
| `GET /movie/{id}?append_to_response=credits` | Detalhes de filme + elenco |
| `GET /tv/{id}?append_to_response=credits` | Detalhes de serie + elenco |
| `GET /tv/{id}/season/{n}` | Detalhes de temporada + episodios |
| `GET /movie/{id}/watch/providers` | Provedores de streaming (filme) |
| `GET /tv/{id}/watch/providers` | Provedores de streaming (serie) |
| `GET /trending/all/{window}` | Trending semanal/diario |
| `GET /movie/upcoming` | Filmes proximos (regiao BR) |
| `GET /tv/on_the_air` | Series no ar |
| `GET /genre/movie/list` | Lista de generos (filme) |
| `GET /genre/tv/list` | Lista de generos (serie) |
| `GET /movie/{id}/recommendations` | Recomendacoes (filme) |
| `GET /tv/{id}/recommendations` | Recomendacoes (serie) |

---

## Configuracao

### Variaveis de ambiente

| Variavel | Descricao | Exemplo |
|----------|-----------|---------|
| `ConnectionStrings__DefaultConnection` | String de conexao PostgreSQL | `Host=localhost;Port=5432;Database=cinetrack;Username=postgres;Password=postgres` |
| `Tmdb__ApiKey` | API Read Access Token do TMDB (Bearer) | `eyJhbGciOi...` |
| `ApiKeys__Keys__0__Key` | Chave de API para autenticacao | `dev-key-cinetrack-2026` |
| `ApiKeys__Keys__0__AppId` | ID da aplicacao | `cinetrack` |
| `ApiKeys__Keys__0__Name` | Nome descritivo da chave | `CineTrack Production` |
| `Seq__ServerUrl` | URL do Seq para logs centralizados | `http://rvmtech-seq:5341` |
| `App__PathBase` | Path base quando atras de reverse proxy | `/cinetrack` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execucao | `Production` |
| `ASPNETCORE_URLS` | URLs de escuta | `http://+:8080` |

### appsettings.json

Configuracoes de log via Serilog (niveis minimos: `Information` geral, `Warning` para EF Core e ASP.NET).

### Rate Limiting

A API possui rate limiting global: **60 requisicoes por minuto por IP** (janela fixa). Respostas que excedem o limite retornam `429 Too Many Requests`.

---

## Arquitetura

O projeto segue Clean Architecture com separacao em 3 camadas:

```
RVM.CineTrack/
  src/
    RVM.CineTrack.Domain/         -- Entidades e Enums (sem dependencias externas)
      Entities/
        AppUser.cs                -- Usuario local (vinculado via ExternalId/OIDC)
        Media.cs                  -- Midia cached do TMDB (filme ou serie)
        Season.cs                 -- Temporada de serie
        Episode.cs                -- Episodio de serie
        WatchListItem.cs          -- Item na watchlist com status e rating
        EpisodeWatch.cs           -- Registro de episodio assistido
        Review.cs                 -- Review publica (1-5 oscars)
        ReleaseAlert.cs           -- Alerta de lancamento
        MediaCast.cs              -- Elenco/equipe cached do TMDB
        WatchProvider.cs          -- Provedor de streaming (BR)
      Enums/
        MediaType.cs              -- Movie, TvSeries
        WatchStatus.cs            -- WantToWatch, Watching, Watched, Dropped

    RVM.CineTrack.Infrastructure/ -- Acesso a dados e integracao externa
      Data/
        CineTrackDbContext.cs     -- DbContext EF Core com 10 DbSets
      Services/
        TmdbClient.cs             -- Client HTTP tipado para TMDB API v3
        TmdbModels.cs             -- DTOs de resposta do TMDB (records)
      DependencyInjection.cs      -- Registro de servicos (Npgsql + HttpClient)

    RVM.CineTrack.API/            -- Aplicacao web (API + Blazor)
      Controllers/                -- 8 controllers REST
        SearchController.cs       -- Busca e trending via TMDB
        MediaController.cs        -- Detalhes e sincronizacao de midia
        WatchListController.cs    -- Gestao da watchlist pessoal
        EpisodeWatchController.cs -- Tracking por episodio
        ReviewsController.cs      -- Reviews + rankings
        AlertsController.cs       -- Alertas de lancamento
        StatsController.cs        -- Estatisticas pessoais
        UsersController.cs        -- Gestao de usuarios + feed social
      Components/Pages/           -- 5 paginas Blazor Server
        Home.razor                -- Dashboard
        Search.razor              -- Busca TMDB
        WatchList.razor           -- Minha lista
        Rankings.razor            -- Rankings da comunidade
        Alerts.razor              -- Alertas de lancamento
      Services/
        MediaSyncService.cs       -- Orquestracao de sync TMDB -> banco local
      Workers/
        ReleaseAlertWorker.cs     -- Background worker diario de alertas
      Auth/
        ApiKeyAuthHandler.cs      -- Handler de autenticacao por API Key
        ApiKeyAuthOptions.cs      -- Configuracao de chaves
      Health/
        DatabaseHealthCheck.cs    -- Health check do PostgreSQL
      Middleware/
        CorrelationIdMiddleware.cs -- Correlation ID para rastreabilidade
      Program.cs                  -- Ponto de entrada e configuracao

  test/                           -- Testes
  docs/                           -- Documentacao
  docker-compose.prod.yml         -- Compose para producao
  RVM.CineTrack.slnx              -- Solution file
```

### Modelo de dados

O banco PostgreSQL (`cinetrack`) contem 10 tabelas com as seguintes relacoes:

- `AppUser` 1:N `WatchListItem`, `Review`, `EpisodeWatch`, `ReleaseAlert`
- `Media` 1:N `Season`, `WatchListItem`, `Review`, `MediaCast`, `WatchProvider`
- `Season` 1:N `Episode`, `Review`
- `Episode` 1:N `EpisodeWatch`, `Review`

Indices unicos garantem integridade: usuario+midia na watchlist, usuario+episodio no tracking, TmdbId+tipo na midia, etc.

---

## Como Executar Localmente

### Pre-requisitos

- .NET 10 SDK
- PostgreSQL rodando (local ou Docker)
- Chave de API do TMDB (obter em https://www.themoviedb.org/settings/api)

### Via dotnet CLI

1. Configurar o banco PostgreSQL:
```bash
# Criar banco de dados (o EF Core cria as tabelas automaticamente via EnsureCreated)
createdb cinetrack
```

2. Configurar variaveis (editar `appsettings.json` ou usar variaveis de ambiente):
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=cinetrack;Username=postgres;Password=postgres"
export Tmdb__ApiKey="seu-tmdb-read-access-token"
```

3. Executar a aplicacao:
```bash
cd src/RVM.CineTrack.API
dotnet run
```

4. Acessar:
- Painel Blazor: `http://localhost:5090/`
- API: `http://localhost:5090/api/search?q=matrix` (com header `X-Api-Key: dev-key-cinetrack-2026`)

### Via Docker Compose (producao)

1. Criar arquivo `.env` na raiz do projeto:
```env
POSTGRES_PASSWORD=sua-senha
TMDB_API_KEY=seu-tmdb-read-access-token
API_KEY=sua-api-key-producao
```

2. Subir o container:
```bash
docker compose -f docker-compose.prod.yml up -d
```

3. A aplicacao estara disponivel em `http://localhost:5090/cinetrack/`.

**Observacao:** o compose espera que a rede Docker `rvmtech` e o container `rvmtech-postgres` ja existam (infraestrutura compartilhada do ecossistema RVM Tech).

---

## Solucao de Problemas

### API retorna 401 Unauthorized

- Verifique se o header `X-Api-Key` esta presente na requisicao.
- Verifique se a chave corresponde a uma entrada valida em `ApiKeys:Keys` no `appsettings.json` ou variaveis de ambiente.
- Endpoints marcados como `AllowAnonymous` (rankings, feed social, health) nao exigem chave.

### API retorna 429 Too Many Requests

- O rate limiting esta configurado para 60 requisicoes por minuto por IP.
- Aguarde 1 minuto antes de tentar novamente.

### API retorna 502 (TMDB unavailable)

- Verifique se a chave `Tmdb:ApiKey` esta configurada corretamente.
- A chave deve ser o **Read Access Token** (Bearer), nao a API Key v3.
- Verifique conectividade com `https://api.themoviedb.org`.

### Banco de dados nao conecta

- Verifique a string de conexao em `ConnectionStrings:DefaultConnection`.
- O banco `cinetrack` deve existir no PostgreSQL. As tabelas sao criadas automaticamente via `EnsureCreatedAsync` na inicializacao.
- Verifique o health check: `GET /health`.

### Alertas nao estao sendo gerados

- O `ReleaseAlertWorker` roda diariamente as 08:00 UTC.
- Alertas so sao criados para midias na watchlist com status `Watching` ou `WantToWatch` (series) ou `WantToWatch` (filmes).
- Alertas so sao gerados para lancamentos nos proximos 7 dias.
- Verifique os logs do Serilog para mensagens do worker.

### Posters nao carregam no painel

- Os posters sao carregados diretamente do CDN do TMDB (`https://image.tmdb.org/t/p/w300/...`).
- Verifique se o navegador tem acesso a internet.
- Se o `PosterPath` estiver nulo no banco, significa que o TMDB nao tem poster para esse titulo.

### Provedores de streaming vazios

- Provedores so sao sincronizados para o Brasil (`BR`).
- Se um titulo nao tem provedores no Brasil, a lista retorna vazia.
- Para atualizar provedores, re-sincronize a midia via `POST /api/media/sync/movie/{tmdbId}` ou `POST /api/media/sync/tv/{tmdbId}`.
