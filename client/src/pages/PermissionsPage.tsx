import { useEffect, useState } from 'react'
import {
  createPermission,
  deletePermission,
  fetchPermissions,
  type PermissionDto,
  updatePermission,
} from '../api/permissions'
import { useToast } from '../components/ToastProvider'

export default function PermissionsPage() {
  const [items, setItems] = useState<PermissionDto[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [editId, setEditId] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const { notify } = useToast()

  const loadPermissions = async () => {
    setLoading(true)
    setError('')
    try {
      const data = await fetchPermissions()
      setItems(data)
    } catch {
      setError('Failed to load permissions')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadPermissions()
  }, [])

  const handleCreate = async (event: React.FormEvent) => {
    event.preventDefault()
    setMessage('')
    setError('')
    setFieldErrors({})

    const nameExists = items.some((permission) => permission.name.toLowerCase() === name.toLowerCase())
    if (nameExists) {
      setFieldErrors({ name: 'Permission name already exists.' })
      return
    }
    setSaving(true)
    try {
      await createPermission({ name, description })
      setName('')
      setDescription('')
      await loadPermissions()
      setMessage('Permission created')
      notify('Permission created', 'success')
    } catch {
      setError('Failed to create permission')
      notify('Failed to create permission', 'error')
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

    const nameExists = items.some((permission) => permission.id !== editId && permission.name.toLowerCase() === name.toLowerCase())
    if (nameExists) {
      setFieldErrors({ name: 'Permission name already exists.' })
      return
    }
    setSaving(true)
    try {
      await updatePermission(editId, { name, description })
      setEditId('')
      setName('')
      setDescription('')
      await loadPermissions()
      setMessage('Permission updated')
      notify('Permission updated', 'success')
    } catch {
      setError('Failed to update permission')
      notify('Failed to update permission', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    setError('')
    setSaving(true)
    try {
      await deletePermission(id)
      await loadPermissions()
      notify('Permission deleted', 'success')
    } catch {
      setError('Failed to delete permission')
      notify('Failed to delete permission', 'error')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page">
      <h2>Permissions</h2>
      <p>Define the fine-grained access your roles can grant.</p>

      <form className="form-grid" onSubmit={editId ? handleUpdate : handleCreate}>
        <input
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="Permission name"
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
      {message && <span className="success">{message}</span>}
      {error && <span className="error">{error}</span>}

      {loading ? (
        <p>Loading permissions...</p>
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
            {items.map((permission) => (
              <tr key={permission.id}>
                <td>{permission.name}</td>
                <td>{permission.description}</td>
                <td>
                  <button
                    type="button"
                    className="secondary"
                    onClick={() => {
                      setEditId(permission.id)
                      setName(permission.name)
                      setDescription(permission.description)
                    }}
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    onClick={() => void handleDelete(permission.id)}
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
