import { BrowserRouter, Link, Route, Routes } from 'react-router-dom'
import { AppConfigGate } from './config/AppConfigProvider'
import { useAppConfig } from './config/useAppConfig'
import { AuthProvider } from './auth/AuthProvider'
import { useAuth } from './auth/useAuth'
import { useEffect, useRef } from 'react'
import { AccessDeniedPage } from './pages/AccessDeniedPage'
import { AdminPage } from './pages/AdminPage'
import { CommercialPage } from './pages/CommercialPage'
import { EvaluationsPage } from './pages/EvaluationsPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { Navigation } from './components/Navigation'
import { canAccessMenu, type AppMenu } from './rbac/rbac'

function NotFound() {
  return (
    <div style={{ padding: 16 }}>
      <h1>Página não encontrada</h1>
      <p>
        <Link to="/">Voltar para a Home</Link>
      </p>
    </div>
  )
}

function RequireAuth(props: { children: React.ReactNode }) {
  const authState = useAuth()
  const redirectStartedRef = useRef(false)

  const shouldLogin = authState.status === 'ready' && !authState.session.isAuthenticated

  useEffect(() => {
    if (!shouldLogin) return
    if (redirectStartedRef.current) return
    redirectStartedRef.current = true
    void authState.auth.login()
  }, [authState, shouldLogin])

  if (authState.status === 'loading') {
    return <div style={{ padding: 16 }}>Inicializando autenticação…</div>
  }

  if (authState.status === 'error') {
    return (
      <div style={{ padding: 16 }}>
        <h1>Erro de autenticação</h1>
        <pre style={{ whiteSpace: 'pre-wrap' }}>{authState.error.message}</pre>
      </div>
    )
  }

  if (shouldLogin) {
    return <div style={{ padding: 16 }}>Redirecionando para login…</div>
  }

  return <>{props.children}</>
}

function AuthedChrome(props: { children: React.ReactNode }) {
  const authState = useAuth()

  if (authState.status !== 'ready' || !authState.session.isAuthenticated) {
    return <>{props.children}</>
  }

  return (
    <div>
      <div style={{ padding: 16, borderBottom: '1px solid #ddd' }}>
        <strong>GestAuto</strong>
        <span style={{ marginLeft: 12 }}>Usuário: {authState.session.username ?? '—'}</span>
        <span style={{ marginLeft: 12 }}>Roles: {authState.session.roles.join(', ') || '—'}</span>
        <button
          type="button"
          style={{ marginLeft: 12 }}
          onClick={() => {
            void authState.auth.logout()
          }}
        >
          Sair
        </button>
        <span style={{ marginLeft: 12 }}>
          <Link to="/">Home</Link>
        </span>
        <Navigation roles={authState.session.roles} />
      </div>
      {props.children}
    </div>
  )
}

function RequireMenuAccess(props: { menu: AppMenu; children: React.ReactNode }) {
  const authState = useAuth()

  if (authState.status !== 'ready' || !authState.session.isAuthenticated) {
    return <>{props.children}</>
  }

  if (!canAccessMenu(authState.session.roles, props.menu)) {
    return <AccessDeniedPage />
  }

  return <>{props.children}</>
}

function ConfiguredApp() {
  const config = useAppConfig()

  return (
    <AuthProvider config={config}>
      <BrowserRouter>
        <AuthedChrome>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/denied" element={<AccessDeniedPage />} />

            <Route
              path="/"
              element={
                <RequireAuth>
                  <HomePage />
                </RequireAuth>
              }
            />
            <Route
              path="/commercial"
              element={
                <RequireAuth>
                  <RequireMenuAccess menu="COMMERCIAL">
                    <CommercialPage />
                  </RequireMenuAccess>
                </RequireAuth>
              }
            />
            <Route
              path="/evaluations"
              element={
                <RequireAuth>
                  <RequireMenuAccess menu="EVALUATIONS">
                    <EvaluationsPage />
                  </RequireMenuAccess>
                </RequireAuth>
              }
            />
            <Route
              path="/admin"
              element={
                <RequireAuth>
                  <RequireMenuAccess menu="ADMIN">
                    <AdminPage />
                  </RequireMenuAccess>
                </RequireAuth>
              }
            />

            <Route path="*" element={<NotFound />} />
          </Routes>
        </AuthedChrome>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default function App() {
  return (
    <AppConfigGate>
      <ConfiguredApp />
    </AppConfigGate>
  )
}
