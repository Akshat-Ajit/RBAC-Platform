import { useEffect, useState } from 'react'
import {
  assignPermission,
  createRole,
  deleteRole,
  fetchRoles,
  updateRole,
  type RoleDto,
} from '../api/roles'
import { fetchPermissions, type PermissionDto } from '../api/permissions'
import { useToast } from '../components/ToastProvider'

export default function RolesPage() {
  const [items, setItems] = useState<RoleDto[]>([])
  const [permissions, setPermissions] = useState<PermissionDto[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [editId, setEditId] = useState('')
  const [assignRoleId, setAssignRoleId] = useState('')
  const [assignPermissionId, setAssignPermissionId] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [assigning, setAssigning] = useState(false)
  const [roleSearch, setRoleSearch] = useState('')
  const [permissionSearch, setPermissionSearch] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const { notify } = useToast()

  const loadRoles = async () => {
    setLoading(true)
    setError('')
    try {
      const data = await fetchRoles()
      setItems(data)
    } catch {
      setError('Failed to load roles')
    } finally {
      setLoading(false)
    }
  }

  const loadPermissions = async () => {
    try {
      const data = await fetchPermissions()
      setPermissions(data)
    } catch {
      setError('Failed to load permissions')
    }
  }

  useEffect(() => {
    void loadRoles()
    void loadPermissions()
  }, [])

  const handleCreate = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})

    const nameExists = items.some((role) => role.name.toLowerCase() === name.toLowerCase())
    if (nameExists) {
      setFieldErrors({ name: 'Role name already exists.' })
      return
    }
    setSaving(true)
    try {
      await createRole({ name, description })
      setName('')
      setDescription('')
      await loadRoles()
      setMessage('Role created')
      notify('Role created', 'success')
    } catch {
      setError('Failed to create role')
      notify('Failed to create role', 'error')
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

    const nameExists = items.some((role) => role.id !== editId && role.name.toLowerCase() === name.toLowerCase())
    if (nameExists) {
      setFieldErrors({ name: 'Role name already exists.' })
      return
    }
    setSaving(true)
    try {
      await updateRole(editId, { name, description })
      setEditId('')
      setName('')
      setDescription('')
      await loadRoles()
      setMessage('Role updated')
      notify('Role updated', 'success')
    } catch {
      setError('Failed to update role')
      notify('Failed to update role', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    setError('')
    setSaving(true)
    try {
      await deleteRole(id)
      await loadRoles()
      notify('Role deleted', 'success')
    } catch {
      setError('Failed to delete role')
      notify('Failed to delete role', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleAssignPermission = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})
    if (!assignRoleId || !assignPermissionId) {
      setFieldErrors({ assign: 'Select a role and permission.' })
      return
    }
    setAssigning(true)
    try {
      await assignPermission({ roleId: assignRoleId, permissionId: assignPermissionId })
      setAssignRoleId('')
      setAssignPermissionId('')
      setMessage('Permission assigned')
      notify('Permission assigned', 'success')
    } catch {
      setError('Failed to assign permission')
      notify('Failed to assign permission', 'error')
    } finally {
      setAssigning(false)
    }
  }

  const filteredRoles = items.filter((role) => {
    if (!roleSearch) return true
    const value = roleSearch.toLowerCase()
    return role.name.toLowerCase().includes(value) || role.description.toLowerCase().includes(value)
  })

  const filteredPermissions = permissions.filter((permission) => {
    if (!permissionSearch) return true
    const value = permissionSearch.toLowerCase()
    return permission.name.toLowerCase().includes(value) || permission.description.toLowerCase().includes(value)
  })

  return (
    <div className="page">
      <h2>Roles</h2>
      <p>Define role access levels and map permissions here.</p>

      <form className="form-grid" onSubmit={editId ? handleUpdate : handleCreate}>
        <input
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="Role name"
          required
        />
        {fieldErrors.name && <span className="field-error">{fieldErrors.name}</span>}
        <input
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="Description"
          required
        />
        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : editId ? 'Update' : 'Create'}
        </button>
        {editId && (
          <button type="button" className="secondary" onClick={() => setEditId('')}>
            Cancel
          </button>
        )}
      </form>

      <form className="form-grid" onSubmit={handleAssignPermission}>
        <input
          value={roleSearch}
          onChange={(event) => setRoleSearch(event.target.value)}
          placeholder="Search roles"
        />
        <select
          value={assignRoleId}
          onChange={(event) => setAssignRoleId(event.target.value)}
          required
        >
          <option value="">Select role</option>
          {filteredRoles.map((role) => (
            <option key={role.id} value={role.id}>
              {role.name} - {role.description}
            </option>
          ))}
        </select>
        <input
          value={permissionSearch}
          onChange={(event) => setPermissionSearch(event.target.value)}
          placeholder="Search permissions"
        />
        <select
          value={assignPermissionId}
          onChange={(event) => setAssignPermissionId(event.target.value)}
          required
        >
          <option value="">Select permission</option>
          {filteredPermissions.map((permission) => (
            <option key={permission.id} value={permission.id}>
              {permission.name} - {permission.description}
            </option>
          ))}
        </select>
        <button type="submit" disabled={assigning}>
          {assigning ? 'Assigning...' : 'Assign Permission'}
        </button>
        {fieldErrors.assign && <span className="field-error">{fieldErrors.assign}</span>}
      </form>

      {message && <span className="success">{message}</span>}
      {error && <span className="error">{error}</span>}

      {loading ? (
        <p>Loading roles...</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {items.map((role) => (
              <tr key={role.id}>
                <td>{role.name}</td>
                <td>{role.description}</td>
                <td>
                  <button
                    type="button"
                    className="secondary"
                    onClick={() => {
                      setEditId(role.id)
                      setName(role.name)
                      setDescription(role.description)
                    }}
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    onClick={() => void handleDelete(role.id)}
                    disabled={saving}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
