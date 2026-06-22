import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [tailwindcss(), react()],
  server: {
    proxy: {
      '/auth': 'http://localhost:8080',
      '/notes': 'http://localhost:8080',
      '/note-groups': 'http://localhost:8080',
    },
  },
})
