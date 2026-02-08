import api from '../api/axios'

export type User = {
  id: string
  fullName: string
  email: string
  roles: string[]
  isActive?: boolean
  isSystemAdmin?: boolean
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  fullName: string
  email: string
  password: string
}

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/login', payload)
  return response.data
}

export async function register(payload: RegisterRequest): Promise<{ message: string }> {
  const response = await api.post<{ message: string }>('/auth/register', payload)
  return response.data
}

export async function refresh(refreshToken: string): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/refresh-token', {
    refreshToken,
  })
  return response.data
}

export async function logout(refreshToken: string): Promise<void> {
  await api.post('/auth/logout', { refreshToken })
}

export async function checkEmailAvailable(email: string): Promise<boolean> {
  const response = await api.get<{ available: boolean }>(`/auth/email-available`, {
    params: { email },
  })
  return response.data.available
}
