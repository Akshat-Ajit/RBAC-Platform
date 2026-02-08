import api from './axios'

export type PermissionDto = {
  id: string
  name: string
  description: string
}

export type CreatePermissionPayload = {
  name: string
  description: string
}

export async function fetchPermissions(): Promise<PermissionDto[]> {
  const response = await api.get<PermissionDto[]>('/permissions')
  return response.data
}

export async function createPermission(payload: CreatePermissionPayload): Promise<PermissionDto> {
  const response = await api.post<PermissionDto>('/permissions', payload)
  return response.data
}

export async function updatePermission(id: string, payload: CreatePermissionPayload): Promise<void> {
  await api.put(`/permissions/${id}`, payload)
}

export async function deletePermission(id: string): Promise<void> {
  await api.delete(`/permissions/${id}`)
}
