import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// build into wwwroot so dotnet serves the static files
export default defineConfig({
  plugins: [react()],
  root: path.resolve(__dirname, 'src'),
  build: {
    outDir: path.resolve(__dirname, '..', 'wwwroot'),
    emptyOutDir: true,
    rollupOptions: {
      input: path.resolve(__dirname, 'src', 'index.html')
    }
  }
})
