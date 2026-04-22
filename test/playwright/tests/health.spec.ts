import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.CINETRACK_BASE_URL ?? 'https://cinetrack.portfolio.rvmtech.com.br';

test.describe('Health checks', () => {
  test.skip(
    process.env.CINETRACK_RUN_SMOKE !== '1',
    'Defina CINETRACK_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /health retorna Healthy', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;

    const response = await request.get(`${currentBaseUrl}/health`);

    expect(response.status()).toBe(200);
    const text = await response.text();
    expect(text).toContain('Healthy');
  });
});
