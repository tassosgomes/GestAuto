import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function LoginPage() {
  const authState = useAuth()
  const [clicked, setClicked] = useState(false)

  useEffect(() => {
    if (authState.status !== 'ready') return
    if (!authState.session.isAuthenticated) return
    // Já autenticado: volta para home.
    window.location.href = '/'
  }, [authState])

  if (authState.status === 'loading') {
    return <div style={{ padding: 16 }}>Inicializando autenticação…</div>
  }

  if (authState.status === 'error') {
    return (
      <div style={{ padding: 16 }}>
        <h1>Erro de autenticação</h1>
        <pre style={{ whiteSpace: 'pre-wrap' }}>{authState.error.message}</pre>
        <p>
          <Link to="/">Voltar</Link>
        </p>
      </div>
    )
  }

  return (
    <div style={{ padding: 16 }}>
      <h1>Login</h1>
      <p>Autentique-se via Keycloak.</p>
      <button
        type="button"
        onClick={() => {
          setClicked(true)
          void authState.auth.login()
        }}
        disabled={clicked}
      >
        Entrar
      </button>
    </div>
  )
}
