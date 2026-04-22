import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.CINETRACK_BASE_URL ?? 'https://cinetrack.portfolio.rvmtech.com.br';

test.describe('API Users e Watchlist', () => {
  test.skip(
    process.env.CINETRACK_RUN_SMOKE !== '1',
    'Defina CINETRACK_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/users retorna lista de usuarios', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/users`);

    // Pode exigir auth - aceitar 200 ou 401
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/watchlist retorna lista ou exige auth', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/watchlist`);

    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/reviews retorna lista ou exige auth', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/reviews`);

    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/alerts retorna lista ou exige auth', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/api/alerts`);

    expect([200, 401]).toContain(response.status());
  });
});
