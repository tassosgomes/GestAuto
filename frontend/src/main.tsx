import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AppConfigProvider } from './config/AppConfigProvider'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <AppConfigProvider>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </AppConfigProvider>,
)
