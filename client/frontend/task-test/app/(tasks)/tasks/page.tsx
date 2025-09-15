'use client';

import AuthGate from '@/components/AuthGate';
import ConfirmDialog from '@/components/ConfirmDialog';
import TaskList from '@/components/TaskList';
import { useTasksSignalR } from '@/hooks/useSignalR';
import { useToast } from '@/components/Toast';
import { useEffect, useMemo, useState } from 'react';
import { useDeleteTaskMutation, useGetTasksQuery, useUpdateTaskMutation } from '@/redux/features/tasksApi';
import { useAppDispatch } from '@/redux/store';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import type { TaskStatus } from '@/lib/types';

const pageSizeDefault = 10;

export default function TasksPage() {
  return (
    <AuthGate>
      <TasksInner />
    </AuthGate>
  );
}

function TasksInner() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const dispatch = useAppDispatch();
  const { toast } = useToast();
  useTasksSignalR(dispatch);

  const [q, setQ] = useState<string>(searchParams.get('q') || '');
  const [status, setStatus] = useState<TaskStatus | undefined>(
    (searchParams.get('status') as TaskStatus) || undefined
  );
  const [dueDate, setDueDate] = useState<string>(searchParams.get('dueDate') || '');
  const [page, setPage] = useState<number>(parseInt(searchParams.get('page') || '1', 10));
  const pageSize = pageSizeDefault;

  // Sync URL
  useEffect(() => {
    const params = new URLSearchParams(searchParams.toString());
    if (q) params.set('q', q); else params.delete('q');
    if (status) params.set('status', status); else params.delete('status');
    if (dueDate) params.set('dueDate', dueDate); else params.delete('dueDate');
    params.set('page', String(page));
    router.replace(`${pathname}?${params.toString()}`);
  }, [q, status, dueDate, page, pathname, router]);

  const { data, isLoading, isError, error, isFetching, refetch } = useGetTasksQuery({
    q: q || undefined,
    status,
    dueDate: dueDate || undefined,
    // Preserve prior behavior: createdAt descending
    sortBy: 'createdAt',
    desc: true,
    page,
    pageSize,
  });

  const [confirmId, setConfirmId] = useState<string | null>(null);
  const [deleteTask, { isLoading: isDeleting }] = useDeleteTaskMutation();
  const [updateTask] = useUpdateTaskMutation();

  const total = data?.total ?? 0;
  const items = data?.items ?? [];
  const totalPages = useMemo(() => Math.max(1, Math.ceil(total / pageSize)), [total]);

  async function handleDelete(id: string) {
    setConfirmId(id);
  }

  async function confirmDelete() {
    if (!confirmId) return;
    try {
      await deleteTask(confirmId).unwrap();
      toast({ title: 'Task deleted', variant: 'success' });
      setConfirmId(null);
      // RTK Query invalidation will refresh list
    } catch (e: any) {
      toast({ title: 'Failed to delete task', description: String(e?.data || e?.message || e), variant: 'error' });
    }
  }

  async function handleMarkDone(id: string) {
    try {
      await updateTask({ id, data: { status: 'Done' } as any }).unwrap();
      toast({ title: 'Task marked as done', variant: 'success' });
      // Invalidations in RTK Query will refresh the list
    } catch (e: any) {
      toast({ title: 'Failed to update task', description: String(e?.data || e?.message || e), variant: 'error' });
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div className="flex flex-1 flex-col gap-2 sm:flex-row sm:items-end">
          <div className="flex-1">
            <label htmlFor="q" className="mb-1 block text-sm font-medium">
              Search
            </label>
            <input
              id="q"
              type="search"
              value={q}
              onChange={(e) => { setQ(e.target.value); setPage(1); }}
              placeholder="Search tasks..."
              className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
            />
          </div>
          <div className="sm:w-64">
            <label htmlFor="status" className="mb-1 block text-sm font-medium">
              Status
            </label>
            <select
              id="status"
              value={status ?? ''}
              onChange={(e) => { const v = e.target.value || undefined; setStatus(v as any); setPage(1); }}
              className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
            >
              <option value="">All</option>
              <option value="Pending">Pending</option>
              <option value="InProgress">InProgress :)</option>
              <option value="Done">Done</option>
            </select>
          </div>
          <div className="sm:w-64">
            <label htmlFor="dueDate" className="mb-1 block text-sm font-medium">
              Due date
            </label>
            <input
              id="dueDate"
              type="date"
              value={dueDate}
              onChange={(e) => { setDueDate(e.target.value); setPage(1); }}
              className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
            />
          </div>
        </div>
        <a
          href="/tasks/new"
          className="inline-flex items-center justify-center rounded bg-[#0A3747] px-4 py-2 text-white hover:bg-[#0A3747] focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
        >
          New Task
        </a>
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-16 animate-pulse rounded-md bg-slate-100" />
          ))}
        </div>
      ) : isError ? (
        <div className="rounded border border-red-200 bg-red-50 p-3 text-red-800">
          Failed to load tasks
        </div>
      ) : (
        <TaskList tasks={items} onDelete={handleDelete} onMarkDone={handleMarkDone} />
      )}

      {/* Pagination */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-slate-600">
          Page {page} of {totalPages} {isFetching && <span className="ml-2 animate-pulse">Updating…</span>}
        </p>
        <div className="flex items-center gap-2">
          <button
            disabled={page <= 1}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            className="rounded border border-slate-300 px-3 py-1.5 text-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          >
            Prev
          </button>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            className="rounded border border-slate-300 px-3 py-1.5 text-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          >
            Next
          </button>
        </div>
      </div>

      <ConfirmDialog
        open={!!confirmId}
        title="Delete task?"
        description="This action cannot be undone."
        onCancel={() => setConfirmId(null)}
        onConfirm={confirmDelete}
        confirmText={isDeleting ? 'Deleting…' : 'Delete'}
      />
    </div>
  );
}
