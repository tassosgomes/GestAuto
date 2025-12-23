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
import { canAccessMenu, type AppMenu } from './rbac/rbac'
import { DesignSystemPage } from './pages/DesignSystemPage'
import AppLayout from './components/layout/AppLayout'

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
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/denied" element={<AccessDeniedPage />} />

          {/* Rotas Protegidas com AppLayout */}
          <Route
            element={
              <RequireAuth>
                <AppLayout />
              </RequireAuth>
            }
          >
            <Route path="/" element={<HomePage />} />
            <Route
              path="/commercial"
              element={
                <RequireMenuAccess menu="COMMERCIAL">
                  <CommercialPage />
                </RequireMenuAccess>
              }
            />
            <Route
              path="/evaluations"
              element={
                <RequireMenuAccess menu="EVALUATIONS">
                  <EvaluationsPage />
                </RequireMenuAccess>
              }
            />
            <Route
              path="/admin"
              element={
                <RequireMenuAccess menu="ADMIN">
                  <AdminPage />
                </RequireMenuAccess>
              }
            />
            <Route path="/design-system" element={<DesignSystemPage />} />
          </Route>

          <Route path="*" element={<NotFound />} />
        </Routes>
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
