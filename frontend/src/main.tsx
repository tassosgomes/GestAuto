import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AppConfigProvider } from './config/AppConfigProvider'

createRoot(document.getElementById('root')!).render(
  <AppConfigProvider>
    <App />
  </AppConfigProvider>,
)
