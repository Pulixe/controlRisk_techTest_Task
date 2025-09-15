'use client';

import { useEffect } from 'react';
import { useMsal } from '@azure/msal-react';
import { usePathname, useRouter } from 'next/navigation';
import { useAppDispatch } from '@/redux/store';
import { setAuthenticated } from '@/redux/features/authSlice';

export default function AuthGate({ children }: { children: React.ReactNode }) {
  const { accounts, inProgress } = useMsal();
  const router = useRouter();
  const pathname = usePathname();
  const dispatch = useAppDispatch();

  const isAuthed = accounts && accounts.length > 0;

  useEffect(() => {
    const acct = accounts?.[0];
    dispatch(
      setAuthenticated({
        isAuthenticated: !!acct,
        account: acct
          ? {
              username: acct.username,
              name: acct.name,
              homeAccountId: acct.homeAccountId,
            }
          : undefined,
      })
    );
  }, [accounts, dispatch]);

  useEffect(() => {
    if (inProgress !== 'none') return; // wait for msal
    if (!isAuthed) {
      if (pathname !== '/login') router.replace('/login');
    }
  }, [isAuthed, inProgress, pathname, router]);

  if (inProgress !== 'none') {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="h-6 w-6 animate-spin rounded-full border-2 border-[#0A3747] border-t-transparent" aria-label="Loading" />
      </div>
    );
  }

  if (!isAuthed) return null; // redirecting
  return <>{children}</>;
}
