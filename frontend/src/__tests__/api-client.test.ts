import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { apiClient, clearAuthToken, getAuthToken } from "../api/client";

describe("apiClient", () => {
  beforeEach(() => {
    vi.stubGlobal("fetch", vi.fn());
    clearAuthToken();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
    clearAuthToken();
  });

  it("sends request with correct method and headers", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({ status: "ok" }),
    });
    vi.stubGlobal("fetch", mockFetch);

    await apiClient("/api/health");

    expect(mockFetch).toHaveBeenCalledOnce();
    const [url, options] = mockFetch.mock.calls[0]!;
    expect(url).toBe("/api/health");
    expect(options.method).toBe("GET");
    expect(options.headers).toEqual({
      "Content-Type": "application/json",
    });
  });

  it("attaches Authorization header when token is set", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({}),
    });
    vi.stubGlobal("fetch", mockFetch);

    apiClient.setToken("test-jwt-token");

    await apiClient("/api/tasks");

    const [, options] = mockFetch.mock.calls[0]!;
    expect(options.headers).toEqual({
      "Content-Type": "application/json",
      Authorization: "Bearer test-jwt-token",
    });
  });

  it("does not attach Authorization header when no token is set", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({}),
    });
    vi.stubGlobal("fetch", mockFetch);

    await apiClient("/api/auth/login");

    const [, options] = mockFetch.mock.calls[0]!;
    expect(options.headers).toEqual({
      "Content-Type": "application/json",
    });
  });

  it("returns parsed JSON on success", async () => {
    const mockData = { id: "1", title: "Test" };
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => mockData,
    });
    vi.stubGlobal("fetch", mockFetch);

    const result = await apiClient("/api/tasks/1");

    expect(result).toEqual(mockData);
  });

  it("returns empty object for 204 No Content", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
      headers: new Headers(),
    });
    vi.stubGlobal("fetch", mockFetch);

    const result = await apiClient("/api/tasks/1", { method: "DELETE" });

    expect(result).toEqual({});
  });

  it("throws error with status and detail for non-ok response", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
      headers: new Headers({ "content-type": "application/problem+json" }),
      json: async () => ({
        type: "about:blank",
        title: "Unauthorized",
        status: 401,
        detail: "Authentication required or credentials are invalid.",
        instance: "/api/tasks",
      }),
    });
    vi.stubGlobal("fetch", mockFetch);

    await expect(apiClient("/api/tasks")).rejects.toThrow(
      "Authentication required or credentials are invalid.",
    );
  });

  it("includes errors field from ProblemDetail when available", async () => {
    const problemDetail = {
      type: "about:blank",
      title: "Validation Error",
      status: 400,
      detail: "Validation failed.",
      instance: "/api/auth/register",
      errors: { email: ["Email is required."] },
    };
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      headers: new Headers({ "content-type": "application/problem+json" }),
      json: async () => problemDetail,
    });
    vi.stubGlobal("fetch", mockFetch);

    try {
      await apiClient("/api/auth/register", { method: "POST" });
      expect.fail("Should have thrown");
    } catch (error: unknown) {
      expect(error).toBeInstanceOf(Error);
      if (error instanceof Error && "errors" in error) {
        expect(
          (error as Error & { errors: Record<string, string[]> }).errors,
        ).toEqual({ email: ["Email is required."] });
      }
    }
  });

  it("clears token and redirects to login on 401", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
      headers: new Headers({ "content-type": "application/problem+json" }),
      json: async () => ({
        type: "about:blank",
        title: "Unauthorized",
        status: 401,
        detail: "Authentication required or credentials are invalid.",
        instance: "/api/tasks",
      }),
    });
    vi.stubGlobal("fetch", mockFetch);

    apiClient.setToken("expired-token");
    expect(getAuthToken()).toBe("expired-token");

    await expect(apiClient("/api/tasks")).rejects.toThrow();

    expect(getAuthToken()).toBeNull();
  });

  it("merges custom headers with default headers", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({}),
    });
    vi.stubGlobal("fetch", mockFetch);

    await apiClient("/api/tasks", {
      headers: { "X-Custom": "value" },
    });

    const [, options] = mockFetch.mock.calls[0]!;
    expect(options.headers).toEqual({
      "Content-Type": "application/json",
      "X-Custom": "value",
    });
  });

  it("sends body as JSON string", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 201,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({ id: "1" }),
    });
    vi.stubGlobal("fetch", mockFetch);

    await apiClient("/api/tasks", {
      method: "POST",
      body: { title: "New Task" },
    });

    const [, options] = mockFetch.mock.calls[0]!;
    expect(options.body).toBe(JSON.stringify({ title: "New Task" }));
  });
});

describe("token management", () => {
  beforeEach(() => {
    clearAuthToken();
  });

  it("stores and retrieves token", () => {
    apiClient.setToken("my-token");
    expect(getAuthToken()).toBe("my-token");
  });

  it("clears token", () => {
    apiClient.setToken("my-token");
    clearAuthToken();
    expect(getAuthToken()).toBeNull();
  });
});
