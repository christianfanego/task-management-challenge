import { apiClient } from "./client";
import type { LoginResponse, User } from "./types";

export async function register(email: string, password: string): Promise<User> {
  return apiClient<User>("/api/auth/register", {
    method: "POST",
    body: { email, password },
  });
}

export async function login(
  email: string,
  password: string,
): Promise<LoginResponse> {
  const response = await apiClient<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: { email, password },
  });
  apiClient.setToken(response.accessToken);
  return response;
}
