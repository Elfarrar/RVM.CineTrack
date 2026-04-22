import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.CINETRACK_BASE_URL ?? 'https://cinetrack.portfolio.rvmtech.com.br';

test.describe('API Media - busca e catalogo', () => {
  test.skip(
    process.env.CINETRACK_RUN_SMOKE !== '1',
    'Defina CINETRACK_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/media retorna lista de midias', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/media`);

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(Array.isArray(body) || body.items !== undefined).toBe(true);
  });

  test('GET /api/search busca por titulo', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/search?query=test`);

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toBeDefined();
  });

  test('GET /api/stats retorna estatisticas', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/stats`);

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toBeDefined();
  });
});
