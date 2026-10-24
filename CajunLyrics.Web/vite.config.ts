import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// Proxy /api to the .NET API at https://localhost:7156, accepting the
// self-signed dev cert. Shared with preview: `vite preview` does not
// inherit server.proxy, so it needs its own entry.
const apiProxy = {
  target: 'https://localhost:7156',
  changeOrigin: true,
  secure: false,
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': apiProxy,
    },
  },
  preview: {
    proxy: {
      '/api': apiProxy,
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/test/**', 'src/main.tsx'],
    },
  },
})
