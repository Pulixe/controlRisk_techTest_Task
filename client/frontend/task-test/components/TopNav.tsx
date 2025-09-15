'use client';

import Link from 'next/link';
import Image from 'next/image';
import { useMsal } from '@azure/msal-react';
import { useMemo } from 'react';

export default function TopNav() {
  const { instance, accounts } = useMsal();
  const user = useMemo(() => accounts[0], [accounts]);

  return (
    <header className="sticky top-0 z-40 w-full border-b bg-white ">
      <div className="mx-auto flex w-full items-center justify-between px-4 py-3 bg-[#0A3747]">
        <Link
          href="/tasks"
          className="flex items-center gap-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
        >
          <Image
            src="/control_risklogo.png"
            alt="Control Risk"
            width={28}
            height={28}
            priority
            className="h-17 w-28 rounded-sm object-contain"
          />
          <span className="text-lg font-semibold text-white">Task Manager - Technical Test</span>
        </Link>
        <div className="flex items-center gap-3">
          {user ? (
            <>
              <span className="hidden text-sm text-white sm:inline">{user.name ?? user.username}</span>
              <button
                onClick={() => instance.logoutRedirect()}
                className="rounded border border-slate-300 px-3 py-1.5 text-sm text-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
              >
                Logout
              </button>
            </>
          ) : (
            <span className="text-sm text-slate-500">Not signed in</span>
          )}
        </div>
      </div>
    </header>
  );
}
