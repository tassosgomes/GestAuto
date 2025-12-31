import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import path from "path"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    host: true,
    port: 5173,
    proxy: {
      '/dashboard': {
        target: 'http://commercial-api:8080',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/dashboard/, '/api/v1/dashboard'),
      },
      '/test-drives': {
        target: 'http://commercial-api:8080',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/test-drives/, '/api/v1/test-drives'),
      },
      '/leads': {
        target: 'http://commercial-api:8080',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/leads/, '/api/v1/leads'),
      },
      '/proposals': {
        target: 'http://commercial-api:8080',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/proposals/, '/api/v1/proposals'),
      },
      '/payment-methods': {
        target: 'http://commercial-api:8080',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/payment-methods/, '/api/v1/payment-methods'),
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './tests/setup.ts',
  },
})
