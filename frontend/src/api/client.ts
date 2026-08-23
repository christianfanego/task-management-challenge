import type { ProblemDetail } from "./types";

const TOKEN_KEY = "auth_token";

let currentToken: string | null = null;
let onUnauthorizedCallback: (() => void) | null = null;

function loadToken(): string | null {
  if (currentToken === null) {
    currentToken = localStorage.getItem(TOKEN_KEY);
  }
  return currentToken;
}

export function getAuthToken(): string | null {
  return loadToken();
}

export function clearAuthToken(): void {
  currentToken = null;
  localStorage.removeItem(TOKEN_KEY);
}

export function onUnauthorized(callback: () => void): () => void {
  onUnauthorizedCallback = callback;
  return () => {
    if (onUnauthorizedCallback === callback) {
      onUnauthorizedCallback = null;
    }
  };
}

class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;

  constructor(problem: ProblemDetail) {
    super(problem.detail);
    this.name = "ApiError";
    this.status = problem.status;
    this.errors = problem.errors;
  }
}

interface ApiClientOptions {
  method?: string;
  headers?: Record<string, string>;
  body?: unknown;
}

export async function apiClient<T = unknown>(
  url: string,
  options: ApiClientOptions = {},
): Promise<T> {
  const { body, headers: customHeaders, method = "GET" } = options;

  const token = loadToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...customHeaders,
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body != null ? JSON.stringify(body) : undefined,
  });

  if (response.status === 204) {
    return {} as T;
  }

  if (!response.ok) {
    let problem: ProblemDetail;
    try {
      problem = (await response.json()) as ProblemDetail;
    } catch {
      problem = {
        type: "about:blank",
        title: "Error",
        status: response.status,
        detail: `Request failed with status ${response.status}`,
        instance: url,
      };
    }

    if (response.status === 401) {
      clearAuthToken();
      onUnauthorizedCallback?.();
    }

    throw new ApiError(problem);
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return {} as T;
  }

  return (await response.json()) as T;
}

apiClient.setToken = (token: string): void => {
  currentToken = token;
  localStorage.setItem(TOKEN_KEY, token);
};
