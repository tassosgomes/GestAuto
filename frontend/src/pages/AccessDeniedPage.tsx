import { Link } from 'react-router-dom'

export function AccessDeniedPage() {
  return (
    <div style={{ padding: 16 }}>
      <h1>Acesso negado</h1>
      <p>Você não tem permissão para acessar esta rota.</p>
      <p>
        <Link to="/">Voltar para a Home</Link>
      </p>
    </div>
  )
}
