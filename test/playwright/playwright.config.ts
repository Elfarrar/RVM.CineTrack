import { defineConfig, devices } from '@playwright/test';

const baseURL = process.env.CINETRACK_BASE_URL ?? 'https://cinetrack.portfolio.rvmtech.com.br';

export default defineConfig({
  testDir: './tests',
  timeout: 60_000,
  outputDir: 'test-results',
  expect: {
    timeout: 10_000,
  },
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
