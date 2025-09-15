'use client';

import Link from 'next/link';
import type { Task } from '@/lib/types';

type Props = {
  tasks: Task[];
  onDelete?: (id: string) => void;
  onMarkDone?: (id: string) => void | Promise<void>;
};

function StatusBadge({ status }: { status: Task['status'] }) {
  const cls =
    status === 'done'
      ? 'bg-green-100 text-green-800 border-green-200'
      : status === 'inProgress'
      ? 'bg-yellow-100 text-yellow-800 border-yellow-200'
      : 'bg-slate-100 text-slate-800 border-slate-200';
  return <span className={`inline-flex rounded-full border px-2 py-0.5 text-xs ${cls}`}>{status}</span>;
}

export default function TaskList({ tasks, onDelete, onMarkDone }: Props) {
  if (!tasks.length) {
    return <p className="text-sm text-slate-500">No tasks found.</p>;
  }

  return (
    <div className="space-y-3">
      {/* Cards on mobile */}
      <div className="grid gap-3 md:hidden">
        {tasks.map((t) => (
          <div key={t.id} className="rounded-lg border p-3">
            <div className="mb-1 flex items-center justify-between">
              <h3 className="font-medium">{t.title}</h3>
              <StatusBadge status={t.status} />
            </div>
            {t.dueDate ? (
              <p className="text-xs text-slate-500">Due: {new Date(t.dueDate).toLocaleDateString()}</p>
            ) : null}
            <div className="mt-2 flex items-center gap-2">
              <Link
                href={`/tasks/${t.id}/edit`}
                className="rounded border border-slate-300 px-3 py-1.5 text-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
              >
                Edit
              </Link>
              {t.status !== 'Done' && (
                <button
                  onClick={() => onMarkDone?.(t.id)}
                  className="rounded bg-green-600 px-3 py-1.5 text-sm text-white hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
                >
                  Done
                </button>
              )}
              <button
                onClick={() => onDelete?.(t.id)}
                className="rounded bg-red-600 px-3 py-1.5 text-sm text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Table on md+ */}
      <div className="hidden md:block">
        <div className="grid grid-cols-12 gap-2 rounded-t-lg border-b bg-[#0A3747] text-white px-3 py-2 text-sm font-medium">
          <div className="col-span-6">Title</div>
          <div className="col-span-2">Status</div>
          <div className="col-span-2">Due</div>
          <div className="col-span-2 text-right">Actions</div>
        </div>
        <ul className="divide-y">
          {tasks.map((t) => (
            <li key={t.id} className="grid grid-cols-12 items-center gap-2 px-3 py-2">
              <div className="col-span-6 truncate">{t.title}</div>
              <div className="col-span-2">
                <StatusBadge status={t.status} />
              </div>
              <div className="col-span-2 text-sm text-slate-600">
                {t.dueDate ? new Date(t.dueDate).toLocaleDateString() : '-'}
              </div>
              <div className="col-span-2 flex items-center justify-end gap-2">
                <Link
                  href={`/tasks/${t.id}/edit`}
                  className="rounded border border-slate-300 px-3 py-1.5 text-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
                >
                  Edit
                </Link>
                {t.status !== 'Done' && (
                  <button
                    onClick={() => onMarkDone?.(t.id)}
                    className="rounded bg-green-600 px-3 py-1.5 text-sm text-white hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
                  >
                    Done
                  </button>
                )}
                <button
                  onClick={() => onDelete?.(t.id)}
                  className="rounded bg-red-600 px-3 py-1.5 text-sm text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
                >
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
