import type { TaskDto } from "../api/types";

interface TaskListProps {
  tasks: TaskDto[];
  isLoading: boolean;
  error: string | null;
  onEdit: (task: TaskDto) => void;
  onDelete: (taskId: string) => void;
}

function formatDate(dateStr: string): string {
  // Handle both "2026-01-15T00:00:00" and "2026-01-15"
  const datePart = dateStr.substring(0, 10);
  const [year, month, day] = datePart.split("-").map(Number);
  const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  return `${months[month - 1]} ${day}, ${year}`;
}

function isOverdue(task: TaskDto): boolean {
  if (!task.dueDate || task.status === "Completed") return false;
  return new Date(task.dueDate) < new Date();
}

export function TaskList({
  tasks,
  isLoading,
  error,
  onEdit,
  onDelete,
}: TaskListProps) {
  if (isLoading) {
    return (
      <div className="py-8 text-center text-gray-500">Loading tasks...</div>
    );
  }

  if (error) {
    return (
      <div role="alert" className="rounded bg-red-50 p-4 text-red-700">
        {error}
      </div>
    );
  }

  if (tasks.length === 0) {
    return (
      <div className="py-8 text-center text-gray-500">
        No tasks yet. Create one to get started!
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {tasks.map((task) => {
        const overdue = isOverdue(task);
        return (
          <div
            key={task.id}
            className={`rounded border p-4 shadow-sm ${
              overdue
                ? "border-red-300 bg-red-50"
                : "border-gray-200 bg-white"
            }`}
          >
            <div className="flex items-start justify-between gap-4">
              <div className="min-w-0 flex-1">
                <h3 className="truncate text-lg font-medium text-gray-900">
                  {task.title}
                </h3>
                {task.description && (
                  <p className="mt-1 truncate text-sm text-gray-600">
                    {task.description}
                  </p>
                )}
                <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-gray-500">
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                      task.status === "Completed"
                        ? "bg-green-100 text-green-800"
                        : task.status === "InProgress"
                          ? "bg-yellow-100 text-yellow-800"
                          : "bg-gray-100 text-gray-800"
                    }`}
                  >
                    {task.status}
                  </span>
                  {task.dueDate && (
                    <span className={overdue ? "font-medium text-red-600" : ""}>
                      Due: {formatDate(task.dueDate)}
                    </span>
                  )}
                  {overdue && (
                    <span className="rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-800">
                      Overdue
                    </span>
                  )}
                </div>
              </div>
              <div className="flex shrink-0 gap-1">
                <button
                  onClick={() => onEdit(task)}
                  className="rounded px-2 py-1 text-sm text-blue-600 hover:bg-blue-50"
                >
                  Edit
                </button>
                <button
                  onClick={() => {
                    if (window.confirm("Are you sure you want to permanently delete this task? This action cannot be undone.")) {
                      onDelete(task.id);
                    }
                  }}
                  className="rounded px-2 py-1 text-sm text-red-600 hover:bg-red-50"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
