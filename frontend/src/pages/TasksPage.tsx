import { useState, useEffect, useCallback, useRef } from "react";
import { TaskList } from "../components/TaskList";
import { TaskForm } from "../components/TaskForm";
import { getTasks, createTask, updateTask, deleteTask } from "../api/tasks";
import type { TaskDto, CreateTaskInput, UpdateTaskInput } from "../api/types";

export function TasksPage() {
  const [tasks, setTasks] = useState<TaskDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskDto | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const formRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (showForm && formRef.current) {
      formRef.current.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }, [showForm]);

  const loadTasks = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getTasks();
      setTasks(data);
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : "Failed to load tasks.";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  function handleCreateNew() {
    setEditingTask(null);
    setShowForm(true);
  }

  function handleEdit(task: TaskDto) {
    setEditingTask(task);
    setShowForm(true);
  }

  function handleCancel() {
    setShowForm(false);
    setEditingTask(null);
  }

  async function handleSubmit(data: {
    title: string;
    description: string;
    status: string;
    dueDate: string;
  }) {
    setIsSubmitting(true);
    try {
      if (editingTask) {
        const input: UpdateTaskInput = {
          title: data.title,
          description: data.description || null,
          status: data.status,
          dueDate: data.dueDate || null,
        };
        const updated = await updateTask(editingTask.id, input);
        setTasks((prev) =>
          prev.map((t) => (t.id === updated.id ? updated : t)),
        );
      } else {
        const input: CreateTaskInput = {
          title: data.title,
          description: data.description || null,
          status: data.status,
          dueDate: data.dueDate || null,
        };
        const created = await createTask(input);
        setTasks((prev) => [...prev, created]);
      }
      setShowForm(false);
      setEditingTask(null);
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : "Failed to save task.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(taskId: string) {
    try {
      await deleteTask(taskId);
      setTasks((prev) => prev.filter((t) => t.id !== taskId));
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : "Failed to delete task.";
      setError(message);
    }
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">My Tasks</h1>
        {!showForm && (
          <button
            onClick={handleCreateNew}
            className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            + New Task
          </button>
        )}
      </div>

      {showForm && (
        <div ref={formRef} className="mb-6 rounded border border-gray-200 bg-white p-4 shadow-sm">
          <h2 className="mb-4 text-lg font-medium text-gray-900">
            {editingTask ? "Edit Task" : "New Task"}
          </h2>
          <TaskForm
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            isLoading={isSubmitting}
            initialData={editingTask ?? undefined}
          />
        </div>
      )}

      <TaskList
        tasks={tasks}
        isLoading={isLoading}
        error={error}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />
    </div>
  );
}
