import { createSlice } from '@reduxjs/toolkit'

type RoleState = {
  items: Array<{ id: string; name: string }>
  status: 'idle' | 'loading' | 'failed'
}

const initialState: RoleState = {
  items: [],
  status: 'idle',
}

const roleSlice = createSlice({
  name: 'roles',
  initialState,
  reducers: {
    resetRoles: () => initialState,
  },
})

export const { resetRoles } = roleSlice.actions
export default roleSlice.reducer
