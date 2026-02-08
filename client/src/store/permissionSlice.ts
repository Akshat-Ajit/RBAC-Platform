import { createSlice } from '@reduxjs/toolkit'

type PermissionState = {
  items: Array<{ id: string; name: string }>
  status: 'idle' | 'loading' | 'failed'
}

const initialState: PermissionState = {
  items: [],
  status: 'idle',
}

const permissionSlice = createSlice({
  name: 'permissions',
  initialState,
  reducers: {
    resetPermissions: () => initialState,
  },
})

export const { resetPermissions } = permissionSlice.actions
export default permissionSlice.reducer
