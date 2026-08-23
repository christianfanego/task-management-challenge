import { useState } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { TASK_STATUS_OPTIONS } from "../api/types";
import type { TaskDto } from "../api/types";

interface TaskFormProps {
  onSubmit: (data: {
    title: string;
    description: string;
    status: string;
    dueDate: string;
  }) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
  initialData?: TaskDto;
}

interface ValidationErrors {
  title?: string;
  description?: string;
  status?: string;
  dueDate?: string;
}

export function TaskForm({
  onSubmit,
  onCancel,
  isLoading,
  initialData,
}: TaskFormProps) {
  const [title, setTitle] = useState(initialData?.title ?? "");
  const [description, setDescription] = useState(
    initialData?.description ?? "",
  );
  const [status, setStatus] = useState(initialData?.status ?? "Pending");
  const [dueDate, setDueDate] = useState(
    initialData?.dueDate ? initialData.dueDate.substring(0, 10) : "",
  );
  const [errors, setErrors] = useState<ValidationErrors>({});

  const isEdit = initialData != null;

  function validate(): ValidationErrors {
    const e: ValidationErrors = {};
    if (!title.trim()) {
      e.title = "Title is required.";
    } else if (title.trim().length > 120) {
      e.title = "Title must be at most 120 characters.";
    }
    if (description && description.length > 2000) {
      e.description = "Description must be at most 2000 characters.";
    }
    const validStatuses = ["Pending", "InProgress", "Completed"];
    if (!validStatuses.includes(status)) {
      e.status = "Status must be Pending, InProgress, or Completed.";
    }
    if (dueDate) {
      if (!/^\d{4}-\d{2}-\d{2}$/.test(dueDate)) {
        e.dueDate = "Due date must be in YYYY-MM-DD format.";
      } else {
        const [year, month, day] = dueDate.split("-").map(Number);
        const date = new Date(Date.UTC(year, month - 1, day));
        if (date.getUTCFullYear() !== year || date.getUTCMonth() !== month - 1 || date.getUTCDate() !== day) {
          e.dueDate = "Due date must be a valid calendar date.";
        }
      }
    }
    return e;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const validationErrors = validate();
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;
    await onSubmit({ title: title.trim(), description, status, dueDate });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="task-title" className="mb-1 block text-sm font-medium">
          Title <span className="text-red-500">*</span>
        </label>
        <input
          id="task-title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className={`w-full rounded border px-3 py-2 focus:ring-1 focus:outline-none ${
            errors.title
              ? "border-red-500 focus:border-red-500 focus:ring-red-500"
              : "border-gray-300 focus:border-blue-500 focus:ring-blue-500"
          }`}
          placeholder="Task title"
        />
        {errors.title && (
          <p className="mt-1 text-sm text-red-600">{errors.title}</p>
        )}
      </div>

      <div>
        <label
          htmlFor="task-description"
          className="mb-1 block text-sm font-medium"
        >
          Description
        </label>
        <textarea
          id="task-description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className={`w-full rounded border px-3 py-2 focus:ring-1 focus:outline-none ${
            errors.description
              ? "border-red-500 focus:border-red-500 focus:ring-red-500"
              : "border-gray-300 focus:border-blue-500 focus:ring-blue-500"
          }`}
          placeholder="Optional description"
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-600">{errors.description}</p>
        )}
      </div>

      <div>
        <label htmlFor="task-status" className="mb-1 block text-sm font-medium">
          Status
        </label>
        <select
          id="task-status"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          className={`w-full rounded border px-3 py-2 focus:ring-1 focus:outline-none ${
            errors.status
              ? "border-red-500 focus:border-red-500 focus:ring-red-500"
              : "border-gray-300 focus:border-blue-500 focus:ring-blue-500"
          }`}
        >
          {TASK_STATUS_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
        {errors.status && (
          <p className="mt-1 text-sm text-red-600">{errors.status}</p>
        )}
      </div>

      <div>
        <label
          htmlFor="task-due-date"
          className="mb-1 block text-sm font-medium"
        >
          Due Date
        </label>
        <DatePicker
          selected={
            dueDate
              ? new Date(
                  Number(dueDate.substring(0, 4)),
                  Number(dueDate.substring(5, 7)) - 1,
                  Number(dueDate.substring(8, 10)),
                )
              : null
          }
          onChange={(date: Date | null) => {
            if (date) {
              const y = date.getFullYear();
              const m = String(date.getMonth() + 1).padStart(2, "0");
              const d = String(date.getDate()).padStart(2, "0");
              setDueDate(`${y}-${m}-${d}`);
            } else {
              setDueDate("");
            }
          }}
          dateFormat="yyyy-MM-dd"
          placeholderText="YYYY-MM-DD"
          className={`w-full rounded border px-3 py-2 focus:ring-1 focus:outline-none ${
            errors.dueDate
              ? "border-red-500 focus:border-red-500 focus:ring-red-500"
              : "border-gray-300 focus:border-blue-500 focus:ring-blue-500"
          }`}
          wrapperClassName="w-full"
        />
        {errors.dueDate && (
          <p className="mt-1 text-sm text-red-600">{errors.dueDate}</p>
        )}
      </div>

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isLoading}
          className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isLoading ? "Saving..." : isEdit ? "Update Task" : "Create Task"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}
