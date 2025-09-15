'use client';

import { useEffect } from 'react';
import { useMsal } from '@azure/msal-react';
import { loginRequest, msalReady } from '@/lib/msalClient';
import { useRouter } from 'next/navigation';

export default function LoginPage() {
  const { instance, accounts, inProgress } = useMsal();
  const router = useRouter();

  // After MSAL processes redirect (handled globally), navigate when authenticated
  useEffect(() => {
    if (accounts.length > 0 && inProgress === 'none') {
      router.replace('/tasks');
    }
  }, [accounts, inProgress, router]);

  // no-op second effect removed

  async function handleLogin() {
    try {
      if (inProgress !== 'none') return;
      await msalReady; // ensure MSAL initialized before interaction
      await instance.loginRedirect(loginRequest);
    } catch (e) {
      console.error('loginRedirect error', e);
      // Avoid alert here; some browsers throw while still navigating
    }
  }

  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center gap-4">
      <h1 className="text-2xl font-semibold">Sign in to Task Manager</h1>
      <button
        onClick={handleLogin}
        disabled={inProgress !== 'none'}
        className="rounded bg-[#0A3747] px-4 py-2 text-white hover:bg-[#0A3747] focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
      >
        {inProgress !== 'none' ? 'Working…' : 'Sign in with Microsoft'}
      </button>
    </div>
  );
}
