import { configureStore } from '@reduxjs/toolkit';
import { useDispatch, useSelector, TypedUseSelectorHook } from 'react-redux';
import authReducer from '@/redux/features/authSlice';
import { tasksApi } from '@/redux/features/tasksApi';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    [tasksApi.reducerPath]: tasksApi.reducer,
  },
  middleware: (gDM) => gDM().concat(tasksApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;

