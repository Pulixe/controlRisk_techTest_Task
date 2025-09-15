'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { NewTaskSchema, TaskSchema, TaskStatusEnum, type NewTask, type UpdateTask, type Task } from '@/lib/types';
import { useMemo } from 'react';
import Link from 'next/link';
import { useGetUsersQuery } from '@/redux/features/tasksApi';

type Props = {
  defaultValues?: Partial<Task>;
  onSubmit: (data: NewTask | UpdateTask) => Promise<void> | void;
  submitting?: boolean;
};

export default function TaskForm({ defaultValues, onSubmit, submitting }: Props) {
  // Use TaskSchema as requested; it allows id optional.
  const form = useForm<NewTask | UpdateTask>({
    resolver: zodResolver(TaskSchema),
    mode: 'onChange',
    defaultValues: useMemo(
      () => ({
        title: defaultValues?.title ?? '',
        description: defaultValues?.description ?? '',
        dueDate: defaultValues?.dueDate ?? '',
        status: (defaultValues?.status as any) ?? 'Pending',
        AssignedTo: (defaultValues as any)?.AssignedTo ?? '',
      }),
      [defaultValues]
    ),
  });

  const handleSubmit = form.handleSubmit(async (data) => {
    await onSubmit({
      title: data.title,
      description: data.description || undefined,
      dueDate: data.dueDate || undefined,
      status: data.status as any,
      // Backend expects AssignedTo to be the selected user's email
      AssignedTo: (data as any).AssignedTo || undefined,
    });
  });

  const {
    register,
    formState: { errors, isValid },
  } = form;

  const disabled = submitting || !isValid;
  const { data: users = [], isLoading: loadingUsers, isError: usersError } = useGetUsersQuery({ skip: 0, take: 50 });

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="title" className="mb-1 block text-sm font-medium">
          Title
        </label>
        <input
          id="title"
          type="text"
          {...register('title')}
          className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          placeholder="Task title"
        />
        {errors.title && <p className="mt-1 text-sm text-red-600">{String(errors.title.message)}</p>}
      </div>

      <div>
        <label htmlFor="description" className="mb-1 block text-sm font-medium">
          Description
        </label>
        <textarea
          id="description"
          rows={4}
          {...register('description')}
          className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          placeholder="Optional description"
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-600">{String(errors.description.message)}</p>
        )}
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="dueDate" className="mb-1 block text-sm font-medium">
            Due date
          </label>
          <input
            id="dueDate"
            type="date"
            {...register('dueDate')}
            className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          />
          {errors.dueDate && <p className="mt-1 text-sm text-red-600">{String(errors.dueDate.message)}</p>}
        </div>

        <div>
          <label htmlFor="status" className="mb-1 block text-sm font-medium">
            Status
          </label>
          <select
            id="status"
            {...register('status')}
            className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          >
            {TaskStatusEnum.options.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
          {errors.status && <p className="mt-1 text-sm text-red-600">{String(errors.status.message)}</p>}
        </div>
      </div>

      <div>
        <label htmlFor="AssignedTo" className="mb-1 block text-sm font-medium">
          Assign task to:
        </label>
        <select
          id="AssignedTo"
          {...register('AssignedTo')}
          className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
          disabled={loadingUsers}
        >
          <option value="">Unassigned</option>
          {users.map((u) => (
            <option key={u.id} value={u.email}>
              {u.name} ({u.email})
            </option>
          ))}
        </select>
        {usersError && <p className="mt-1 text-sm text-red-600">Failed to load users</p>}
        {errors as any && (errors as any).AssignedTo && (
          <p className="mt-1 text-sm text-red-600">{String((errors as any).AssignedTo.message)}</p>
        )}
      </div>

      <div className="flex items-center gap-3 pt-2">
        <button
          type="submit"
          disabled={disabled}
          className="rounded bg-[#0A3747] px-4 py-2 text-white hover:bg-[#0A3747] disabled:cursor-not-allowed disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
        >
          Save
        </button>
        <Link
          href="/tasks"
          className="rounded border border-slate-300 px-4 py-2 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-[#0A3747]"
        >
          Cancel
        </Link>
      </div>
    </form>
  );
}
