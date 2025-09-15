'use client';

import AuthGate from '@/components/AuthGate';
import TaskForm from '@/components/TaskForm';
import { useCreateTaskMutation } from '@/redux/features/tasksApi';
import { useRouter } from 'next/navigation';
import { useToast } from '@/components/Toast';
import type { NewTask } from '@/lib/types';

export default function NewTaskPage() {
  return (
    <AuthGate>
      <NewTaskInner />
    </AuthGate>
  );
}

function NewTaskInner() {
  const [createTask, { isLoading }] = useCreateTaskMutation();
  const router = useRouter();
  const { toast } = useToast();

  async function onSubmit(data: NewTask) {
    try {
      await createTask(data).unwrap();
      toast({ title: 'Task created', variant: 'success' });
      router.push('/tasks');
    } catch (e: any) {
      toast({ title: 'Failed to create task', description: String(e?.data || e?.message || e), variant: 'error' });
    }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-xl font-semibold">New Task</h1>
      <TaskForm onSubmit={onSubmit} submitting={isLoading} />
    </div>
  );
}

