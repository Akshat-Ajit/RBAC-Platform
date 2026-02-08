import api from './axios'

export type UserDto = {
  id: string
  fullName: string
  email: string
  isActive: boolean
  roles: string[]
  isSystemAdmin: boolean
}

export type CreateUserPayload = {
  fullName: string
  email: string
  password: string
}

export type UpdateUserPayload = {
  fullName: string
  email: string
}

export type AssignRolePayload = {
  userId: string
  roleName: string
}

export async function fetchUsers(): Promise<UserDto[]> {
  const response = await api.get<UserDto[]>('/users')
  return response.data
}

export async function createUser(payload: CreateUserPayload): Promise<UserDto> {
  const response = await api.post<UserDto>('/users', payload)
  return response.data
}

export async function updateUser(id: string, payload: UpdateUserPayload): Promise<void> {
  await api.put(`/users/${id}`, payload)
}

export async function deleteUser(id: string): Promise<void> {
  await api.delete(`/users/${id}`)
}

export async function approveUser(id: string): Promise<void> {
  await api.post(`/users/${id}/approve`)
}

export async function assignRole(payload: AssignRolePayload): Promise<void> {
  await api.post('/users/assign-role', payload)
}

export async function removeRole(payload: AssignRolePayload): Promise<void> {
  await api.post('/users/remove-role', payload)
}

export async function cleanupIdentityByEmail(email: string): Promise<void> {
  await api.post('/users/cleanup-identity', { email })
}
