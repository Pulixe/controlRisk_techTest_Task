'use client';

import { MsalProvider } from '@azure/msal-react';
import { msalInstance } from '@/lib/msalClient';
import { Provider as ReduxProvider } from 'react-redux';
import { store } from '@/redux/store';
import { ToastProvider } from '@/components/Toast';

export default function Providers({ children }: { children: React.ReactNode }) {
  return (
    <MsalProvider instance={msalInstance}>
      <ReduxProvider store={store}>
        <ToastProvider>{children}</ToastProvider>
      </ReduxProvider>
    </MsalProvider>
  );
}

