import { Link } from 'react-router-dom'

export default function UnauthorizedPage() {
  return (
    <div className="page">
      <h2>Access denied</h2>
      <p>You do not have permission to view this page.</p>
      <Link to="/dashboard/user">Return to dashboard</Link>
    </div>
  )
}
