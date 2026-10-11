import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// Every backend is proxied through the dev server, so the browser never hits them directly.
// These ports are hard-coded across three repos (test / test2 / test3) -- change them everywhere or nowhere.
const proxy = (target, prefix) => ({
  target,
  changeOrigin: true,
  rewrite: (path) => path.replace(new RegExp(`^${prefix}`), ''),
})

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/svc/orders': proxy('http://localhost:5101', '/svc/orders'),
      '/svc/customers': proxy('http://localhost:5102', '/svc/customers'),
      '/svc/inventory': proxy('http://localhost:8081', '/svc/inventory'),
      '/svc/reports': proxy('http://localhost:9002', '/svc/reports'),
      '/svc/notifications': proxy('http://localhost:9001', '/svc/notifications'),
    },
  },
})
