import { Link } from 'react-router-dom'
import { useAppConfig } from '../config/useAppConfig'
import { useAuth } from '../auth/useAuth'
import { Navigation } from '../components/Navigation'

export function HomePage() {
  const cfg = useAppConfig()
  const authState = useAuth()

  const roles = authState.status === 'ready' ? authState.session.roles : []

  return (
    <div style={{ padding: 16 }}>
      <h1>GestAuto</h1>
      <p>SPA (MVP) — páginas placeholder.</p>

      <h2>Navegação</h2>
      <p>
        <Link to="/">Home</Link>
      </p>
      <Navigation roles={roles} />

      <h2>Config runtime</h2>
      <pre style={{ whiteSpace: 'pre-wrap' }}>{JSON.stringify(cfg, null, 2)}</pre>
    </div>
  )
}
