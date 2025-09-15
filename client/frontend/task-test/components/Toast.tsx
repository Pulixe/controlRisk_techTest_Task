'use client';

import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import { nanoid } from 'nanoid';

type ToastItem = {
  id: string;
  title: string;
  description?: string;
  variant?: 'success' | 'error' | 'info' | 'warning';
};

type ToastContextValue = {
  toast: (t: Omit<ToastItem, 'id'>) => void;
  dismiss: (id: string) => void;
};

const ToastContext = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const toast = useCallback((t: Omit<ToastItem, 'id'>) => {
    const id = nanoid();
    setToasts((prev) => [...prev, { ...t, id }]);
    setTimeout(() => setToasts((prev) => prev.filter((x) => x.id !== id)), 4000);
  }, []);

  const dismiss = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const value = useMemo(() => ({ toast, dismiss }), [toast, dismiss]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="pointer-events-none fixed bottom-4 right-4 z-50 flex w-full max-w-sm flex-col gap-2">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={
              'pointer-events-auto rounded-lg border p-3 shadow-md transition ' +
              (t.variant === 'error'
                ? 'border-red-300 bg-red-50 text-red-900'
                : t.variant === 'success'
                ? 'border-green-300 bg-green-50 text-green-900'
                : t.variant === 'warning'
                ? 'border-yellow-300 bg-yellow-50 text-yellow-900'
                : 'border-slate-300 bg-white text-slate-900')
            }
            role="status"
            aria-live="polite"
          >
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="font-medium">{t.title}</p>
                {t.description ? (
                  <p className="text-sm text-slate-600">{t.description}</p>
                ) : null}
              </div>
              <button
                onClick={() => dismiss(t.id)}
                className="rounded p-1 text-sm text-slate-500 hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
                aria-label="Dismiss notification"
              >
                ✕
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) return { toast: (t: any) => alert(t?.title ?? 'Notification'), dismiss: (_: string) => {} };
  return ctx;
}
