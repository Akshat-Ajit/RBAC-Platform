import { useEffect, useState } from 'react'
import {
  approveUser,
  assignRole,
  cleanupIdentityByEmail,
  createUser,
  deleteUser,
  fetchUsers,
  removeRole,
  updateUser,
  type UserDto,
} from '../api/users'
import { fetchRoles, type RoleDto } from '../api/roles'
import { useToast } from '../components/ToastProvider'

export default function UsersPage() {
  const [items, setItems] = useState<UserDto[]>([])
  const [roles, setRoles] = useState<RoleDto[]>([])
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [editId, setEditId] = useState('')
  const [assignUserId, setAssignUserId] = useState('')
  const [assignRoleName, setAssignRoleName] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [assigning, setAssigning] = useState(false)
  const [approving, setApproving] = useState(false)
  const [cleaning, setCleaning] = useState(false)
  const [cleanupEmail, setCleanupEmail] = useState('')
  const [removingRoleKey, setRemovingRoleKey] = useState('')
  const [userSearch, setUserSearch] = useState('')
  const [roleSearch, setRoleSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'active'>('all')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const { notify } = useToast()

  const loadUsers = async () => {
    setLoading(true)
    setError('')
    try {
      const data = await fetchUsers()
      setItems(data)
    } catch {
      setError('Failed to load users')
    } finally {
      setLoading(false)
    }
  }

  const loadRoles = async () => {
    try {
      const data = await fetchRoles()
      setRoles(data)
    } catch {
      setError('Failed to load roles')
    }
  }

  useEffect(() => {
    void loadUsers()
    void loadRoles()
  }, [])

  const handleCreate = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})

    const passwordRules = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/
    if (!passwordRules.test(password)) {
      setFieldErrors({ password: 'Min 8 chars, upper, lower, number required.' })
      return
    }

    const emailExists = items.some((user) => user.email.toLowerCase() === email.toLowerCase())
    if (emailExists) {
      setFieldErrors({ email: 'Email is already in use.' })
      return
    }

    setSaving(true)
    try {
      await createUser({ fullName, email, password })
      setFullName('')
      setEmail('')
      setPassword('')
      await loadUsers()
      setMessage('User created')
      notify('User created', 'success')
    } catch {
      setError('Failed to create user')
      notify('Failed to create user', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleUpdate = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})
    if (!editId) {
      return
    }

    const emailExists = items.some((user) => user.id !== editId && user.email.toLowerCase() === email.toLowerCase())
    if (emailExists) {
      setFieldErrors({ email: 'Email is already in use.' })
      return
    }

    setSaving(true)
    try {
      await updateUser(editId, { fullName, email })
      setEditId('')
      setFullName('')
      setEmail('')
      await loadUsers()
      setMessage('User updated')
      notify('User updated', 'success')
    } catch {
      setError('Failed to update user')
      notify('Failed to update user', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    setError('')
    setSaving(true)
    try {
      await deleteUser(id)
      await loadUsers()
      notify('User deleted', 'success')
    } catch {
      setError('Failed to delete user')
      notify('Failed to delete user', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleAssignRole = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})
    if (!assignUserId || !assignRoleName) {
      setFieldErrors({ assign: 'Select a user and role.' })
      return
    }

    setAssigning(true)
    try {
      await assignRole({ userId: assignUserId, roleName: assignRoleName })
      setAssignUserId('')
      setAssignRoleName('')
      await loadUsers()
      setMessage('Role assigned')
      notify('Role assigned', 'success')
    } catch {
      setError('Failed to assign role')
      notify('Failed to assign role', 'error')
    } finally {
      setAssigning(false)
    }
  }

  const handleRemoveRole = async (userId: string, roleName: string) => {
    setError('')
    const key = `${userId}:${roleName}`
    setRemovingRoleKey(key)
    try {
      await removeRole({ userId, roleName })
      await loadUsers()
      notify('Role removed', 'success')
    } catch {
      setError('Failed to remove role')
      notify('Failed to remove role', 'error')
    } finally {
      setRemovingRoleKey('')
    }
  }

  const handleApprove = async (id: string) => {
    setError('')
    setApproving(true)
    try {
      await approveUser(id)
      await loadUsers()
      notify('User approved', 'success')
    } catch {
      setError('Failed to approve user')
      notify('Failed to approve user', 'error')
    } finally {
      setApproving(false)
    }
  }

  const handleCleanupIdentity = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    if (!cleanupEmail.trim()) {
      setFieldErrors({ cleanupEmail: 'Email is required.' })
      return
    }

    setFieldErrors({})
    setCleaning(true)
    try {
      await cleanupIdentityByEmail(cleanupEmail.trim())
      setCleanupEmail('')
      notify('Identity record removed', 'success')
    } catch {
      setError('Failed to cleanup identity user')
      notify('Failed to cleanup identity user', 'error')
    } finally {
      setCleaning(false)
    }
  }

  const filteredUsers = items.filter((user) => {
    if (!userSearch) return true
    const value = userSearch.toLowerCase()
    return user.fullName.toLowerCase().includes(value) || user.email.toLowerCase().includes(value)
  })

  const filteredRoles = roles.filter((role) => {
    if (!roleSearch) return true
    const value = roleSearch.toLowerCase()
    return role.name.toLowerCase().includes(value) || role.description.toLowerCase().includes(value)
  })

  const visibleUsers = items.filter((user) => {
    if (statusFilter === 'pending') return !user.isActive
    if (statusFilter === 'active') return user.isActive
    return true
  })

  return (
    <div className="page">
      <h2>Users</h2>
      <p>Manage accounts, roles, and access rights.</p>

      <form className="form-grid" onSubmit={editId ? handleUpdate : handleCreate}>
        <input
          value={fullName}
          onChange={(event) => setFullName(event.target.value)}
          placeholder="Full name"
          required
        />
        {fieldErrors.fullName && <span className="field-error">{fieldErrors.fullName}</span>}
        <input
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="Email"
          type="email"
          required
        />
        {fieldErrors.email && <span className="field-error">{fieldErrors.email}</span>}
        {!editId && (
          <input
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Temporary password"
            type="password"
            required
          />
        )}
        {!editId && fieldErrors.password && <span className="field-error">{fieldErrors.password}</span>}
        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : editId ? 'Update' : 'Create'}
        </button>
        {editId && (
          <button type="button" className="secondary" onClick={() => setEditId('')}>
            Cancel
          </button>
        )}
      </form>

      <form className="form-grid" onSubmit={handleAssignRole}>
        <input
          value={userSearch}
          onChange={(event) => setUserSearch(event.target.value)}
          placeholder="Search users"
        />
        <select
          value={assignUserId}
          onChange={(event) => setAssignUserId(event.target.value)}
          required
        >
          <option value="">Select user</option>
          {filteredUsers.map((user) => (
            <option key={user.id} value={user.id}>
              {user.fullName} ({user.email})
            </option>
          ))}
        </select>
        <input
          value={roleSearch}
          onChange={(event) => setRoleSearch(event.target.value)}
          placeholder="Search roles"
        />
        <select
          value={assignRoleName}
          onChange={(event) => setAssignRoleName(event.target.value)}
          required
        >
          <option value="">Select role</option>
          {filteredRoles.map((role) => (
            <option key={role.id} value={role.name}>
              {role.name} - {role.description}
            </option>
          ))}
        </select>
        <button type="submit" disabled={assigning}>
          {assigning ? 'Assigning...' : 'Assign Role'}
        </button>
        {fieldErrors.assign && <span className="field-error">{fieldErrors.assign}</span>}
      </form>

      <form className="form-grid compact" onSubmit={handleCleanupIdentity}>
        <input
          value={cleanupEmail}
          onChange={(event) => setCleanupEmail(event.target.value)}
          placeholder="Cleanup identity email"
          type="email"
        />
        <button type="submit" className="secondary button-small" disabled={cleaning}>
          {cleaning ? 'Cleaning...' : 'Cleanup Identity'}
        </button>
        {fieldErrors.cleanupEmail && <span className="field-error">{fieldErrors.cleanupEmail}</span>}
      </form>

      {message && <span className="success">{message}</span>}
      {error && <span className="error">{error}</span>}

      {loading ? (
        <p>Loading users...</p>
      ) : (
        <>
          <div className="filter-bar">
            <span>Status filter</span>
            <button
              type="button"
              className={`secondary filter-button ${statusFilter === 'all' ? 'active' : ''}`}
              onClick={() => setStatusFilter('all')}
            >
              All
            </button>
            <button
              type="button"
              className={`secondary filter-button ${statusFilter === 'pending' ? 'active' : ''}`}
              onClick={() => setStatusFilter('pending')}
            >
              Pending
            </button>
            <button
              type="button"
              className={`secondary filter-button ${statusFilter === 'active' ? 'active' : ''}`}
              onClick={() => setStatusFilter('active')}
            >
              Active
            </button>
          </div>
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Status</th>
                <th>Roles</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {visibleUsers.map((user) => (
                <tr key={user.id}>
                  <td>{user.fullName}</td>
                  <td>{user.email}</td>
                  <td>{user.isActive ? 'Active' : 'Pending'}</td>
                  <td>
                    {user.roles.length ? (
                      <div className="role-chips">
                        {user.roles.map((role) => {
                          const key = `${user.id}:${role}`
                          const isRemoving = removingRoleKey === key
                          return (
                            <span key={role} className="role-chip">
                              {role}
                              <button
                                type="button"
                                className="chip-remove"
                                onClick={() => void handleRemoveRole(user.id, role)}
                                disabled={isRemoving}
                              >
                                {isRemoving ? '...' : '×'}
                              </button>
                            </span>
                          )
                        })}
                      </div>
                    ) : (
                      <span className="muted">No roles</span>
                    )}
                  </td>
                  <td>
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => {
                        setEditId(user.id)
                        setFullName(user.fullName)
                        setEmail(user.email)
                      }}
                    >
                      Edit
                    </button>
                    {!user.isActive && (
                      <button
                        type="button"
                        className="secondary"
                        onClick={() => void handleApprove(user.id)}
                        disabled={approving}
                      >
                        Approve
                      </button>
                    )}
                    {!user.isSystemAdmin && (
                      <button
                        type="button"
                        onClick={() => void handleDelete(user.id)}
                        disabled={saving}
                      >
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  )
}
