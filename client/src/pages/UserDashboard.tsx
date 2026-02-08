import { useAuth } from '../auth/useAuth'

export default function UserDashboard() {
  const { user } = useAuth()

  return (
    <div className="page">
      <h2>User Dashboard</h2>
      <p>Welcome {user?.fullName ?? user?.email}. Your access is ready.</p>
      <div className="stat-grid">
        <div>
          <span>Active roles</span>
          <strong>{user?.roles.length ?? 0}</strong>
        </div>
        <div>
          <span>Sessions</span>
          <strong>1</strong>
        </div>
        <div>
          <span>Last login</span>
          <strong>Just now</strong>
        </div>
      </div>
    </div>
  )
}
