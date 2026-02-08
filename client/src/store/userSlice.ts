import { createSlice } from '@reduxjs/toolkit'

type UserState = {
  items: Array<{ id: string; email: string; fullName: string }>
  status: 'idle' | 'loading' | 'failed'
}

const initialState: UserState = {
  items: [],
  status: 'idle',
}

const userSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    resetUsers: () => initialState,
  },
})

export const { resetUsers } = userSlice.actions
export default userSlice.reducer
