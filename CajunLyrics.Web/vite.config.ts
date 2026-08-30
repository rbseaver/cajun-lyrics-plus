import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7156',
        changeOrigin: true,
        // Accept the self-signed dev cert from the .NET API
        secure: false,
      },
    },
  },
})
