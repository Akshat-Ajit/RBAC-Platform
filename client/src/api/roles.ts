import api from './axios'

export type RoleDto = {
  id: string
  name: string
  description: string
}

export type CreateRolePayload = {
  name: string
  description: string
}

export type AssignPermissionPayload = {
  roleId: string
  permissionId: string
}

export async function fetchRoles(): Promise<RoleDto[]> {
  const response = await api.get<RoleDto[]>('/roles')
  return response.data
}

export async function createRole(payload: CreateRolePayload): Promise<RoleDto> {
  const response = await api.post<RoleDto>('/roles', payload)
  return response.data
}

export async function updateRole(id: string, payload: CreateRolePayload): Promise<void> {
  await api.put(`/roles/${id}`, payload)
}

export async function deleteRole(id: string): Promise<void> {
  await api.delete(`/roles/${id}`)
}

export async function assignPermission(payload: AssignPermissionPayload): Promise<void> {
  await api.post('/roles/assign-permission', payload)
}
