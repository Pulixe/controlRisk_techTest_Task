'use client';

import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { acquireToken, msalInstance } from '@/lib/msalClient';
import { tasksApi } from '@/redux/features/tasksApi';
import type { AppDispatch } from '@/redux/store';

type NegotiationResponse = { url: string; accessToken: string };

function getNegotiateUrl() {
  const fromEnv = process.env.NEXT_PUBLIC_SIGNALR_NEGOTIATE_URL || process.env.NEXT_PUBLIC_SIGNALR_NEGOTIATE;
  const base = process.env.NEXT_PUBLIC_API_BASE ?? '';
  if (fromEnv) {
    // If env is absolute, use as-is. If relative, prefix API base
    if (/^https?:\/\//i.test(fromEnv)) return fromEnv;
    const path = fromEnv.startsWith('/') ? fromEnv : `/${fromEnv}`;
    return `${base}${path}`;
  }
  return `${base}/api/signalr/negotiate`;
}

export function useTasksSignalR(dispatch: AppDispatch) {
  const connRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    let cancelled = true;

    async function start() {
      const disabled = (process.env.NEXT_PUBLIC_SIGNALR_DISABLE || '').toLowerCase() === 'true';
      if (disabled) return; // explicit off
      const negotiateUrl = getNegotiateUrl();
      if (!negotiateUrl) return; // no-op
      try {
        const account = msalInstance.getAllAccounts()[0];
        const token = await acquireToken(account);
        const res = await fetch(negotiateUrl, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
          },
        });
        if (!res.ok) {
          // Endpoint not ready yet -> no-op
          console.info('SignalR negotiate not ready:', res.status);
          return;
        }
        const { url, accessToken } = (await res.json()) as NegotiationResponse;
        if (!url || !accessToken) return;

        const conn = new signalR.HubConnectionBuilder()
          .withUrl(url, { accessTokenFactory: () => accessToken })
          .withAutomaticReconnect()
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        conn.on('TaskCreated', () => dispatch(tasksApi.util.invalidateTags([{ type: 'Tasks', id: 'LIST' } as any])));
        conn.on('TaskUpdated', (payload: { id: string }) =>
          dispatch(tasksApi.util.invalidateTags([{ type: 'Task', id: payload?.id } as any, { type: 'Tasks', id: 'LIST' } as any]))
        );
        conn.on('TaskDeleted', (payload: { id: string }) =>
          dispatch(tasksApi.util.invalidateTags([{ type: 'Task', id: payload?.id } as any, { type: 'Tasks', id: 'LIST' } as any]))
        );

        await conn.start();
        if (cancelled) await conn.stop();
        else connRef.current = conn;
      } catch (e) {
        console.info('SignalR disabled or failed to connect:', e);
      }
    }

    start();
    return () => {
      cancelled = true;
      if (connRef.current) {
        connRef.current.stop();
        connRef.current = null;
      }
    };
  }, [dispatch]);
}
