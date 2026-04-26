/**
 * RVM.CineTrack — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema,
 * captura screenshots em diferentes estados e viewports, e gera as imagens
 * para o manual do usuario.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const BASE_URL = process.env.CINETRACK_BASE_URL ?? 'https://cinetrack.portfolio.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

// Garantir que o diretorio de screenshots existe
if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
}

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({
    path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`),
    fullPage,
  });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({
    path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`),
    fullPage,
  });
  await page.setViewportSize({ width: 1280, height: 800 });
}

// ---------------------------------------------------------------------------
// Telas principais
// ---------------------------------------------------------------------------
test.describe('RVM.CineTrack — Telas Principais', () => {
  test('1. Home / Dashboard', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-home');
  });

  test('2. Busca de Filmes e Series', async ({ page }) => {
    await page.goto(`${BASE_URL}/search`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-search');
  });

  test('3. Watchlist', async ({ page }) => {
    await page.goto(`${BASE_URL}/watchlist`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-watchlist');
  });

  test('4. Rankings', async ({ page }) => {
    await page.goto(`${BASE_URL}/rankings`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-rankings');
  });

  test('5. Alertas de Lancamentos', async ({ page }) => {
    await page.goto(`${BASE_URL}/alerts`);
    await page.waitForLoadState('networkidle');
    await capture(page, '05-alerts');
  });
});
