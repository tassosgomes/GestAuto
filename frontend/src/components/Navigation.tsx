import { Link } from 'react-router-dom'
import type { Role } from '../auth/types'
import { getVisibleMenus, type AppMenu } from '../rbac/rbac'

const menuToLink: Record<AppMenu, { label: string; to: string }> = {
  COMMERCIAL: { label: 'Comercial', to: '/commercial' },
  EVALUATIONS: { label: 'Avaliações', to: '/evaluations' },
  ADMIN: { label: 'Admin', to: '/admin' },
}

export function Navigation(props: { roles: Role[] }) {
  const visible = getVisibleMenus(props.roles)

  if (visible.length === 0) {
    return <p style={{ marginTop: 12 }}>Sem permissões configuradas.</p>
  }

  return (
    <nav aria-label="Navegação">
      <ul style={{ display: 'flex', gap: 12, listStyle: 'none', padding: 0, margin: '12px 0 0 0' }}>
        {visible.map((menu) => (
          <li key={menu}>
            <Link to={menuToLink[menu].to}>{menuToLink[menu].label}</Link>
          </li>
        ))}
      </ul>
    </nav>
  )
}
