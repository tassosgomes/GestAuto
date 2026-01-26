import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AppConfigProvider } from './config/AppConfigProvider'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { loadAppConfig } from './config/loadAppConfig'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
})

const FORCE_TELEMETRY = import.meta.env.VITE_FORCE_TELEMETRY === 'true'

if (import.meta.env.PROD || FORCE_TELEMETRY) {
  import('./telemetry')
    .then(({ initTelemetry }) => loadAppConfig().then((config) => ({ initTelemetry, config })))
    .then(({ initTelemetry, config }) => {
      initTelemetry({
        serviceName: 'frontend',
        serviceVersion: config.version ?? '1.0.0',
        otlpEndpoint: config.otelEndpoint ?? 'https://otel.tasso.dev.br/v1/traces',
      })
    })
    .catch((err) => {
      console.error('Failed to load telemetry config:', err)
    })
}

createRoot(document.getElementById('root')!).render(
  <AppConfigProvider>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </AppConfigProvider>,
)
