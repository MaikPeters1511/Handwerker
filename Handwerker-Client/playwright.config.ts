import { defineConfig } from '@playwright/test';

// @ts-ignore
const baseURL = process.env.E2E_BASE_URL ?? 'http://localhost:5000';

export default defineConfig({
  testDir: './tests',
  timeout: 60_000,
  fullyParallel: false,
  retries: 0,
  reporter: 'list',
  use: {
    baseURL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  }
});
