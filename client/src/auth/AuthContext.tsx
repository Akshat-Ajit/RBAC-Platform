import { createContext, useCallback, useMemo, useState } from 'react'
import axios from 'axios'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from './authService'
import { login as loginApi, refresh as refreshApi, register as registerApi, logout as logoutApi } from './authService'

export type AuthContextValue = {
  user: User | null
  accessToken: string | null
  login: (payload: LoginRequest) => Promise<{ ok: boolean; error?: string }>
  register: (payload: RegisterRequest) => Promise<{ ok: boolean; message?: string; error?: string }>
  logout: () => Promise<void>
  refresh: () => Promise<boolean>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const STORAGE_KEY = 'erbms.auth'

function loadFromStorage(): AuthResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as AuthResponse
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const initial = loadFromStorage()
  const [auth, setAuth] = useState<AuthResponse | null>(initial)

  const persist = useCallback((value: AuthResponse | null) => {
    setAuth(value)
    if (!value) {
      localStorage.removeItem(STORAGE_KEY)
      return
    }

    localStorage.setItem(STORAGE_KEY, JSON.stringify(value))
  }, [])

  const handleLogin = useCallback(async (payload: LoginRequest) => {
    try {
      const response = await loginApi(payload)
      persist(response)
      return { ok: true }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 403) {
        return { ok: false, error: 'Account pending admin approval.' }
      }

      return { ok: false, error: 'Invalid credentials.' }
    }
  }, [persist])

  const handleRegister = useCallback(async (payload: RegisterRequest) => {
    try {
      const response = await registerApi(payload)
      return { ok: true, message: response.message }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        return { ok: false, error: 'Email already in use.' }
      }

      return { ok: false, error: 'Registration failed.' }
    }
  }, [])

  const handleLogout = useCallback(async () => {
    const refreshToken = auth?.refreshToken
    persist(null)
    if (refreshToken) {
      try {
        await logoutApi(refreshToken)
      } catch {
        // ignore logout errors
      }
    }
  }, [auth?.refreshToken, persist])

  const handleRefresh = useCallback(async () => {
    if (!auth?.refreshToken) {
      return false
    }

    try {
      const response = await refreshApi(auth.refreshToken)
      persist(response)
      return true
    } catch {
      persist(null)
      return false
    }
  }, [auth, persist])

  const value = useMemo<AuthContextValue>(() => ({
    user: auth?.user ?? null,
    accessToken: auth?.accessToken ?? null,
    login: handleLogin,
    register: handleRegister,
    logout: handleLogout,
    refresh: handleRefresh,
  }), [auth, handleLogin, handleRegister, handleLogout, handleRefresh])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
