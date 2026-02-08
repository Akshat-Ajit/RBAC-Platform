import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { useToast } from '../components/ToastProvider'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { notify } = useToast()

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setLoading(true)

    const result = await login({ email, password })
    if (!result.ok) {
      setError(result.error ?? 'Login failed')
      notify(result.error ?? 'Login failed', 'error')
      setLoading(false)
      return
    }

    setLoading(false)
    notify('Welcome back', 'success')
    navigate('/dashboard/user')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Sign in</h1>
        <p>Access your enterprise workspace</p>
        <form onSubmit={handleSubmit}>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="name@company.com"
            required
          />
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="••••••••"
            required
          />
          {error && <span className="error">{error}</span>}
          <button type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Login'}
          </button>
          <span className="auth-link">
            New here? <Link to="/signup">Create an account</Link>
          </span>
        </form>
      </div>
      <div className="auth-panel">
        <h2>Role-based visibility</h2>
        <p>Admins unlock full user and role management. Managers see analytics and approvals. Users stay focused on their own work.</p>
      </div>
    </div>
  )
}
