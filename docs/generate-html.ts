/**
 * RVM.CineTrack — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone
 * com descritivos de cada funcionalidade.
 *
 * Uso:
 *   npx tsx docs/generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'home',
    title: '1. Home / Dashboard',
    description:
      'Pagina inicial do RVM.CineTrack com resumo da atividade recente: filmes assistidos, ' +
      'series em acompanhamento, recomendacoes personalizadas e lancamentos proximos.',
    screenshot: '01-home',
    features: [
      'Resumo: filmes assistidos, series ativas e avaliacoes pendentes',
      'Recomendacoes baseadas no historico de avaliacoes',
      'Lancamentos proximos de filmes e series monitorados',
      'Ultimas atividades do feed social',
      'Acesso rapido a busca e watchlist',
    ],
    tips: [
      'Quanto mais filmes e series voce avaliar, melhores ficam as recomendacoes.',
      'O dashboard atualiza automaticamente quando novos lancamentos sao detectados via TMDB.',
    ],
  },
  {
    id: 'search',
    title: '2. Busca de Filmes e Series',
    description:
      'Encontre qualquer filme ou serie usando a busca integrada com a API do TMDB. ' +
      'Veja ficha tecnica, elenco, sinopse e adicione diretamente a sua watchlist ou historico.',
    screenshot: '02-search',
    features: [
      'Busca em tempo real por titulo (filmes e series)',
      'Filtros: genero, ano, idioma, nota minima',
      'Card com poster, titulo, ano e nota media (TMDB)',
      'Ver detalhes: sinopse, elenco, duracao, generos',
      'Adicionar a Watchlist diretamente nos resultados',
      'Marcar como assistido com avaliacao (1-5 oscars)',
    ],
    tips: [
      'Use aspas para buscar titulos exatos: "Star Wars".',
      'Filmes e series ja na sua watchlist ou historico sao destacados nos resultados.',
    ],
  },
  {
    id: 'watchlist',
    title: '3. Watchlist (Quero Assistir)',
    description:
      'Lista pessoal de filmes e series que voce quer assistir. Organize por prioridade, ' +
      'filtre por genero e acompanhe os lancamentos da sua lista.',
    screenshot: '03-watchlist',
    features: [
      'Lista de filmes e series para assistir',
      'Prioridade: alta, media, baixa',
      'Filtros por tipo (filme/serie), genero e prioridade',
      'Ordenar por data de adicao, titulo ou nota TMDB',
      'Mover para historico ao assistir (com avaliacao)',
      'Remover da watchlist',
      'Notificacao quando um titulo da watchlist esta disponivel em streaming',
    ],
    tips: [
      'Use a prioridade para separar o que vai assistir esta semana do que e para depois.',
      'A watchlist e privada por padrao — voce escolhe o que compartilhar no feed social.',
    ],
  },
  {
    id: 'rankings',
    title: '4. Rankings',
    description:
      'Veja os rankings de filmes e series mais bem avaliados pela comunidade CineTrack. ' +
      'Compare com suas proprias avaliacoes e descubra novos titulos.',
    screenshot: '04-rankings',
    features: [
      'Top 50 filmes mais bem avaliados pela comunidade',
      'Top 50 series mais bem avaliadas',
      'Filtros por genero, ano e numero minimo de avaliacoes',
      'Sua avaliacao vs media da comunidade',
      'Botao para adicionar a watchlist ou marcar como assistido',
      'Acesso ao perfil dos principais avaliadores',
    ],
    tips: [
      'Rankings com menos de 5 avaliacoes sao marcados com (*) — podem mudar rapidamente.',
      'Filmes sem nota propria aparecem com a nota TMDB como referencia.',
    ],
  },
  {
    id: 'alerts',
    title: '5. Alertas de Lancamentos',
    description:
      'Gerencie alertas para ser notificado quando novos episodios, temporadas ou ' +
      'filmes estiverem disponiveis. Nunca mais perca um lancamento.',
    screenshot: '05-alerts',
    features: [
      'Listagem de alertas ativos por titulo',
      'Criar alerta para: novo episodio, nova temporada, estreia em cinema/streaming',
      'Plataformas monitoradas: Netflix, Prime, Disney+, HBO, Cinema',
      'Historico de notificacoes enviadas',
      'Ativar/desativar alerta sem excluir',
      'Integrado com calendario pessoal (exportar .ics)',
    ],
    tips: [
      'Ative alertas para series em andamento para acompanhar episodio a episodio.',
      'O sistema verifica novos lancamentos via TMDB duas vezes ao dia.',
    ],
  },
];

// ---------------------------------------------------------------------------
// Gerar HTML
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.CineTrack - Manual do Usuario</title>
  <style>
    :root {
      --primary: #dc2626;
      --surface: #ffffff;
      --bg: #f4f6fa;
      --text: #1e293b;
      --text-muted: #64748b;
      --border: #e2e8f0;
      --sidebar-bg: #1c0a0a;
      --accent: #dc2626;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header {
      background: var(--sidebar-bg);
      color: white;
      padding: 3rem 1.5rem;
      text-align: center;
    }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #fca5a5; font-size: 1rem; }
    header .version { color: #f87171; font-size: 0.85rem; margin-top: 0.5rem; }
    nav {
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      padding: 1rem 1.5rem;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a {
      display: inline-block;
      padding: 0.35rem 0.75rem;
      border-radius: 0.5rem;
      font-size: 0.85rem;
      color: var(--text);
      text-decoration: none;
      background: var(--bg);
      transition: background 0.2s;
    }
    nav a:hover { background: var(--primary); color: white; }
    section {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 1rem;
      padding: 2rem;
      margin-bottom: 2rem;
    }
    section h2 {
      font-size: 1.5rem;
      color: var(--primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--border);
    }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips {
      background: var(--bg);
      border-radius: 0.75rem;
      padding: 1rem 1.25rem;
      margin-bottom: 1.25rem;
    }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #fff1f2; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots {
      display: flex;
      gap: 1.5rem;
      margin-top: 1rem;
      align-items: flex-start;
    }
    .screenshot-group {
      position: relative;
      flex: 1;
      border: 1px solid var(--border);
      border-radius: 0.75rem;
      overflow: hidden;
    }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      background: var(--sidebar-bg);
      color: white;
      font-size: 0.7rem;
      padding: 0.2rem 0.5rem;
      border-radius: 0.35rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .no-screenshot {
      background: var(--bg);
      padding: 2rem;
      border-radius: 0.75rem;
      text-align: center;
      color: var(--text-muted);
    }
    footer {
      text-align: center;
      padding: 2rem 1rem;
      color: var(--text-muted);
      font-size: 0.85rem;
    }
    @media (max-width: 768px) {
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 100%; flex: 1; }
      section { padding: 1.25rem; }
    }
    @media print {
      nav { display: none; }
      section { break-inside: avoid; page-break-inside: avoid; }
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 250px; }
    }
  </style>
</head>
<body>
  <header>
    <h1>RVM.CineTrack - Manual do Usuario</h1>
    <p>Tracking de Filmes e Series — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.CineTrack</strong> e um sistema de tracking de filmes e series.
        Registre o que voce assistiu, avalie com 1 a 5 oscars, monte sua watchlist e
        descubra novos titulos com recomendacoes personalizadas via TMDB.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Tracking completo</strong> — historico de filmes e series assistidos</li>
          <li><strong>Avaliacao por oscars</strong> — escala exclusiva de 1 a 5 oscars</li>
          <li><strong>Watchlist</strong> — organize o que quer assistir com prioridades</li>
          <li><strong>Busca via TMDB</strong> — catalogo completo de filmes e series</li>
          <li><strong>Rankings sociais</strong> — veja o que a comunidade recomenda</li>
          <li><strong>Alertas de lancamento</strong> — notificacoes de novos episodios e estreias</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Tracking de Filmes e Series</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

// ---------------------------------------------------------------------------
// Gerar Markdown
// ---------------------------------------------------------------------------
function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.CineTrack - Manual do Usuario

> Tracking de Filmes e Series — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

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

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(
      path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`),
    );

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) {
      md += `- ${f}\n`;
    }
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) {
        md += `> - ${t}\n`;
      }
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n`;
      md += `|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Backend** | ASP.NET Core + Blazor Server |
| **Banco de dados** | PostgreSQL 16 + EF Core |
| **API externa** | TMDB (The Movie Database) |
| **Avaliacao** | Escala 1-5 oscars (customizada) |
| **Deploy** | Docker Compose + Nginx |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------
const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
