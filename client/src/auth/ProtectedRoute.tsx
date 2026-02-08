import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

type ProtectedRouteProps = {
  roles?: string[]
}

export function ProtectedRoute({ roles }: ProtectedRouteProps) {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (roles && !roles.some((role) => user.roles.includes(role))) {
    return <Navigate to="/unauthorized" replace />
  }

  return <Outlet />
}
