import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { checkEmailAvailable } from '../auth/authService'
import { useAuth } from '../auth/useAuth'
import { useToast } from '../components/ToastProvider'

export default function SignUpPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const [checkingEmail, setCheckingEmail] = useState(false)
  const [emailAvailable, setEmailAvailable] = useState<boolean | null>(null)
  const { notify } = useToast()

  const handleEmailBlur = async () => {
    if (!email) {
      return
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setFieldErrors((current) => ({ ...current, email: 'Enter a valid email address.' }))
      setEmailAvailable(null)
      return
    }

    setCheckingEmail(true)
    try {
      const available = await checkEmailAvailable(email)
      setEmailAvailable(available)
      if (!available) {
        setFieldErrors((current) => ({ ...current, email: 'Email already in use.' }))
      }
    } catch {
      setFieldErrors((current) => ({ ...current, email: 'Unable to verify email.' }))
    } finally {
      setCheckingEmail(false)
    }
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setFieldErrors({})

    if (!fullName.trim()) {
      setFieldErrors({ fullName: 'Full name is required.' })
      return
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setFieldErrors({ email: 'Enter a valid email address.' })
      return
    }

    if (emailAvailable === false) {
      setFieldErrors({ email: 'Email already in use.' })
      return
    }

    const passwordRules = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/
    if (!passwordRules.test(password)) {
      setFieldErrors({ password: 'Min 8 chars, upper, lower, number required.' })
      return
    }

    if (password !== confirmPassword) {
      setFieldErrors({ confirmPassword: 'Passwords do not match.' })
      return
    }

    setLoading(true)
    const result = await register({ fullName, email, password })
    if (!result.ok) {
      setError(result.error ?? 'Registration failed')
      if (result.error?.toLowerCase().includes('email')) {
        setFieldErrors({ email: result.error })
      }
      notify(result.error ?? 'Registration failed', 'error')
      setLoading(false)
      return
    }

    setLoading(false)
    notify(result.message ?? 'Account created. Await admin approval.', 'success')
    navigate('/login')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Create account</h1>
        <p>Set up your enterprise workspace access.</p>
        <form onSubmit={handleSubmit}>
          <label htmlFor="fullName">Full name</label>
          <input
            id="fullName"
            value={fullName}
            onChange={(event) => setFullName(event.target.value)}
            placeholder="Jane Cooper"
            required
          />
          {fieldErrors.fullName && <span className="field-error">{fieldErrors.fullName}</span>}
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(event) => {
              setEmail(event.target.value)
              setEmailAvailable(null)
              setFieldErrors((current) => ({ ...current, email: '' }))
            }}
            onBlur={() => void handleEmailBlur()}
            placeholder="name@company.com"
            required
          />
          {checkingEmail && <span className="field-error">Checking email...</span>}
          {fieldErrors.email && <span className="field-error">{fieldErrors.email}</span>}
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="••••••••"
            required
          />
          {fieldErrors.password && <span className="field-error">{fieldErrors.password}</span>}
          <label htmlFor="confirmPassword">Confirm password</label>
          <input
            id="confirmPassword"
            type="password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="••••••••"
            required
          />
          {fieldErrors.confirmPassword && <span className="field-error">{fieldErrors.confirmPassword}</span>}
          {error && <span className="error">{error}</span>}
          <button type="submit" disabled={loading}>
            {loading ? 'Creating...' : 'Sign up'}
          </button>
          <span className="auth-link">
            Already have an account? <Link to="/login">Sign in</Link>
          </span>
        </form>
      </div>
      <div className="auth-panel">
        <h2>Enterprise-ready onboarding</h2>
        <p>New users land with the default User role, and admins can elevate access in seconds.</p>
      </div>
    </div>
  )
}
