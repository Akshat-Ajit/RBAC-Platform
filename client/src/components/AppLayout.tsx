import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="brand">
          <span>ERBMS</span>
          <small>Enterprise RBAC</small>
        </div>
        <nav>
          <NavLink to="/dashboard/user">Dashboard</NavLink>
          {user?.roles.includes('Admin') && (
            <>
              <NavLink to="/users">Users</NavLink>
              <NavLink to="/roles">Roles</NavLink>
              <NavLink to="/permissions">Permissions</NavLink>
            </>
          )}
        </nav>
      </aside>
      <main className="app-main">
        <header className="app-header">
          <div>
            <h1>Welcome back</h1>
            <p>{user?.fullName ?? user?.email}</p>
          </div>
          <button className="secondary" onClick={() => void logout()}>
            Sign out
          </button>
        </header>
        <section className="app-content">
          <Outlet />
        </section>
      </main>
    </div>
  )
}
