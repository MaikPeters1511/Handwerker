/// <reference types="vitest" />
import { defineConfig } from 'vite';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  resolve: {
    mainFields: ['module'],
  },
  plugins: [angular()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['**/*.{test,spec}.ts'],
    exclude: ['node_modules/**', 'dist/**', '.idea/**', '.git/**', '.cache/**', 'tests/**'],
    pool: 'forks',
    poolOptions: {
      forks: {
        singleFork: true,
      },
    },
  },
});

