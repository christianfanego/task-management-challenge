import { apiClient } from "./client";
import type { CreateTaskInput, TaskDto, UpdateTaskInput } from "./types";

export async function getTasks(): Promise<TaskDto[]> {
  return apiClient<TaskDto[]>("/api/tasks");
}

export async function getTask(id: string): Promise<TaskDto> {
  return apiClient<TaskDto>(`/api/tasks/${id}`);
}

export async function createTask(input: CreateTaskInput): Promise<TaskDto> {
  return apiClient<TaskDto>("/api/tasks", {
    method: "POST",
    body: input,
  });
}

export async function updateTask(
  id: string,
  input: UpdateTaskInput,
): Promise<TaskDto> {
  return apiClient<TaskDto>(`/api/tasks/${id}`, {
    method: "PUT",
    body: input,
  });
}

export async function deleteTask(id: string): Promise<void> {
  await apiClient(`/api/tasks/${id}`, { method: "DELETE" });
}
