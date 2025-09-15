'use client';

import AuthGate from '@/components/AuthGate';
import TaskForm from '@/components/TaskForm';
import { useGetTaskByIdQuery, useUpdateTaskMutation } from '@/redux/features/tasksApi';
import { useParams, useRouter } from 'next/navigation';
import { useToast } from '@/components/Toast';
import type { UpdateTask } from '@/lib/types';

export default function EditTaskPage() {
  return (
    <AuthGate>
      <EditTaskInner />
    </AuthGate>
  );
}

function EditTaskInner() {
  const params = useParams<{ id: string }>();
  const id = params?.id as string;
  const { data, isLoading, isError } = useGetTaskByIdQuery(id, { skip: !id });
  const [updateTask, { isLoading: isSaving }] = useUpdateTaskMutation();
  const router = useRouter();
  const { toast } = useToast();

  async function onSubmit(dataInput: UpdateTask) {
    try {
      await updateTask({ id, data: dataInput }).unwrap();
      toast({ title: 'Task updated', variant: 'success' });
      router.push('/tasks');
    } catch (e: any) {
      toast({ title: 'Failed to update task', description: String(e?.data || e?.message || e), variant: 'error' });
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-2">
        <div className="h-6 w-40 animate-pulse rounded bg-slate-100" />
        <div className="h-48 animate-pulse rounded bg-slate-100" />
      </div>
    );
  }

  if (isError || !data) {
    return <div className="rounded border border-red-200 bg-red-50 p-3 text-red-800">Failed to load task</div>;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-semibold">Edit Task</h1>
      <TaskForm defaultValues={data} onSubmit={onSubmit} submitting={isSaving} />
    </div>
  );
}

