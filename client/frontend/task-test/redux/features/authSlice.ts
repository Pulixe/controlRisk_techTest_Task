import { createSlice, PayloadAction } from '@reduxjs/toolkit';

type AccountSummary = {
  username: string;
  name?: string;
  homeAccountId: string;
};

type AuthState = {
  isAuthenticated: boolean;
  account?: AccountSummary;
};

const initialState: AuthState = {
  isAuthenticated: false,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setAuthenticated(
      state,
      action: PayloadAction<{ isAuthenticated: boolean; account?: AccountSummary }>
    ) {
      state.isAuthenticated = action.payload.isAuthenticated;
      state.account = action.payload.account;
    },
    resetAuth(state) {
      state.isAuthenticated = false;
      state.account = undefined;
    },
  },
});

export const { setAuthenticated, resetAuth } = authSlice.actions;
export default authSlice.reducer;

export const selectIsAuthenticated = (state: { auth: AuthState }) => state.auth.isAuthenticated;
export const selectAccount = (state: { auth: AuthState }) => state.auth.account;

