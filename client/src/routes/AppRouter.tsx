import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../auth/ProtectedRoute'
import AdminDashboard from '../pages/AdminDashboard.tsx'
import LoginPage from '../pages/LoginPage.tsx'
import SignUpPage from '../pages/SignUpPage.tsx'
import ManagerDashboard from '../pages/ManagerDashboard.tsx'
import RolesPage from '../pages/RolesPage.tsx'
import PermissionsPage from '../pages/PermissionsPage.tsx'
import UnauthorizedPage from '../pages/UnauthorizedPage.tsx'
import UserDashboard from '../pages/UserDashboard.tsx'
import UsersPage from '../pages/UsersPage.tsx'
import AppLayout from '../components/AppLayout'

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/signup" element={<SignUpPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<UserDashboard />} />
          <Route path="/dashboard/user" element={<UserDashboard />} />
          <Route element={<ProtectedRoute roles={["Admin"]} />}>
            <Route path="/dashboard/admin" element={<AdminDashboard />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/permissions" element={<PermissionsPage />} />
          </Route>
          <Route element={<ProtectedRoute roles={["Manager"]} />}>
            <Route path="/dashboard/manager" element={<ManagerDashboard />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<LoginPage />} />
    </Routes>
  )
}
