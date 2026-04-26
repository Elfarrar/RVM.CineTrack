# RVM.CineTrack - Manual do Usuario

> Tracking de Filmes e Series — Guia Completo de Funcionalidades
>
> Gerado em 26/04/2026 | RVM Tech

---

## Visao Geral

O **RVM.CineTrack** e um sistema de tracking de filmes e series.
Registre o que voce assistiu, avalie com 1 a 5 oscars, monte sua watchlist e
descubra novos titulos com recomendacoes personalizadas via TMDB.

**Recursos principais:**
- **Tracking completo** — historico de filmes e series assistidos
- **Avaliacao por oscars** — escala exclusiva de 1 a 5 oscars
- **Watchlist** — organize o que quer assistir com prioridades
- **Busca via TMDB** — catalogo completo de filmes e series
- **Rankings sociais** — veja o que a comunidade recomenda
- **Alertas de lancamento** — notificacoes de novos episodios e estreias

---

## 1. Home / Dashboard

Pagina inicial do RVM.CineTrack com resumo da atividade recente: filmes assistidos, series em acompanhamento, recomendacoes personalizadas e lancamentos proximos.

**Funcionalidades:**
- Resumo: filmes assistidos, series ativas e avaliacoes pendentes
- Recomendacoes baseadas no historico de avaliacoes
- Lancamentos proximos de filmes e series monitorados
- Ultimas atividades do feed social
- Acesso rapido a busca e watchlist

> **Dicas:**
> - Quanto mais filmes e series voce avaliar, melhores ficam as recomendacoes.
> - O dashboard atualiza automaticamente quando novos lancamentos sao detectados via TMDB.

*Screenshot nao disponivel. Execute o script Playwright para gerar.*

---

## 2. Busca de Filmes e Series

Encontre qualquer filme ou serie usando a busca integrada com a API do TMDB. Veja ficha tecnica, elenco, sinopse e adicione diretamente a sua watchlist ou historico.

**Funcionalidades:**
- Busca em tempo real por titulo (filmes e series)
- Filtros: genero, ano, idioma, nota minima
- Card com poster, titulo, ano e nota media (TMDB)
- Ver detalhes: sinopse, elenco, duracao, generos
- Adicionar a Watchlist diretamente nos resultados
- Marcar como assistido com avaliacao (1-5 oscars)

> **Dicas:**
> - Use aspas para buscar titulos exatos: "Star Wars".
> - Filmes e series ja na sua watchlist ou historico sao destacados nos resultados.

*Screenshot nao disponivel. Execute o script Playwright para gerar.*

---

## 3. Watchlist (Quero Assistir)

Lista pessoal de filmes e series que voce quer assistir. Organize por prioridade, filtre por genero e acompanhe os lancamentos da sua lista.

**Funcionalidades:**
- Lista de filmes e series para assistir
- Prioridade: alta, media, baixa
- Filtros por tipo (filme/serie), genero e prioridade
- Ordenar por data de adicao, titulo ou nota TMDB
- Mover para historico ao assistir (com avaliacao)
- Remover da watchlist
- Notificacao quando um titulo da watchlist esta disponivel em streaming

> **Dicas:**
> - Use a prioridade para separar o que vai assistir esta semana do que e para depois.
> - A watchlist e privada por padrao — voce escolhe o que compartilhar no feed social.

*Screenshot nao disponivel. Execute o script Playwright para gerar.*

---

## 4. Rankings

Veja os rankings de filmes e series mais bem avaliados pela comunidade CineTrack. Compare com suas proprias avaliacoes e descubra novos titulos.

**Funcionalidades:**
- Top 50 filmes mais bem avaliados pela comunidade
- Top 50 series mais bem avaliadas
- Filtros por genero, ano e numero minimo de avaliacoes
- Sua avaliacao vs media da comunidade
- Botao para adicionar a watchlist ou marcar como assistido
- Acesso ao perfil dos principais avaliadores

> **Dicas:**
> - Rankings com menos de 5 avaliacoes sao marcados com (*) — podem mudar rapidamente.
> - Filmes sem nota propria aparecem com a nota TMDB como referencia.

*Screenshot nao disponivel. Execute o script Playwright para gerar.*

---

## 5. Alertas de Lancamentos

Gerencie alertas para ser notificado quando novos episodios, temporadas ou filmes estiverem disponiveis. Nunca mais perca um lancamento.

**Funcionalidades:**
- Listagem de alertas ativos por titulo
- Criar alerta para: novo episodio, nova temporada, estreia em cinema/streaming
- Plataformas monitoradas: Netflix, Prime, Disney+, HBO, Cinema
- Historico de notificacoes enviadas
- Ativar/desativar alerta sem excluir
- Integrado com calendario pessoal (exportar .ics)

> **Dicas:**
> - Ative alertas para series em andamento para acompanhar episodio a episodio.
> - O sistema verifica novos lancamentos via TMDB duas vezes ao dia.

*Screenshot nao disponivel. Execute o script Playwright para gerar.*

---

## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Backend** | ASP.NET Core + Blazor Server |
| **Banco de dados** | PostgreSQL 16 + EF Core |
| **API externa** | TMDB (The Movie Database) |
| **Avaliacao** | Escala 1-5 oscars (customizada) |
| **Deploy** | Docker Compose + Nginx |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
