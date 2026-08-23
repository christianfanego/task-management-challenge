import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, act } from "@testing-library/react";
import { AuthProvider, useAuth } from "../context/AuthContext";
import { clearAuthToken, apiClient } from "../api/client";

function TestConsumer() {
  const auth = useAuth();
  return (
    <div>
      <span data-testid="is-authenticated">
        {auth.isAuthenticated ? "yes" : "no"}
      </span>
      <span data-testid="user-email">{auth.user?.email ?? "none"}</span>
      <button
        data-testid="login-btn"
        onClick={() => auth.login("test@test.com", "Password1!")}
      >
        login
      </button>
      <button
        data-testid="register-btn"
        onClick={() => auth.register("new@test.com", "Password1!")}
      >
        register
      </button>
      <button data-testid="logout-btn" onClick={() => auth.logout()}>
        logout
      </button>
      <button
        data-testid="fetch-btn"
        onClick={() => apiClient("/api/tasks").catch(() => {})}
      >
        fetch
      </button>
    </div>
  );
}

describe("AuthContext", () => {
  beforeEach(() => {
    clearAuthToken();
    localStorage.clear();
  });

  afterEach(() => {
    clearAuthToken();
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("starts unauthenticated when no stored token", () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("no");
    expect(screen.getByTestId("user-email")).toHaveTextContent("none");
  });

  it("sets auth state on successful login", async () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const payload = btoa(
      JSON.stringify({ sub: "user-1", email: "test@test.com", exp: futureExp }),
    );
    const fakeJwt = `header.${payload}.sig`;
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({
        accessToken: fakeJwt,
        tokenType: "Bearer",
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
        user: { id: "user-1", email: "test@test.com" },
      }),
    });
    vi.stubGlobal("fetch", mockFetch);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByTestId("login-btn").click();
    });

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("yes");
    expect(screen.getByTestId("user-email")).toHaveTextContent("test@test.com");
    expect(localStorage.getItem("auth_token")).toBe(fakeJwt);
  });

  it("clears auth state on logout", async () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const payload = btoa(
      JSON.stringify({ sub: "user-1", email: "test@test.com", exp: futureExp }),
    );
    const fakeJwt = `header.${payload}.sig`;
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({
        accessToken: fakeJwt,
        tokenType: "Bearer",
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
        user: { id: "user-1", email: "test@test.com" },
      }),
    });
    vi.stubGlobal("fetch", mockFetch);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByTestId("login-btn").click();
    });

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("yes");

    await act(async () => {
      screen.getByTestId("logout-btn").click();
    });

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("no");
    expect(screen.getByTestId("user-email")).toHaveTextContent("none");
    expect(localStorage.getItem("auth_token")).toBeNull();
  });

  it("restores auth state from valid stored token on mount", async () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const payload = btoa(
      JSON.stringify({
        sub: "user-1",
        email: "stored@test.com",
        exp: futureExp,
      }),
    );
    const fakeJwt = `header.${payload}.sig`;
    localStorage.setItem("auth_token", fakeJwt);
    localStorage.setItem(
      "auth_user",
      JSON.stringify({ id: "user-1", email: "stored@test.com" }),
    );

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {});

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("yes");
    expect(screen.getByTestId("user-email")).toHaveTextContent(
      "stored@test.com",
    );
  });

  it("clears auth when stored token is expired", async () => {
    const pastExp = Math.floor(Date.now() / 1000) - 1000;
    const payload = btoa(
      JSON.stringify({ sub: "user-1", email: "test@test.com", exp: pastExp }),
    );
    const fakeJwt = `header.${payload}.sig`;
    localStorage.setItem("auth_token", fakeJwt);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {});

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("no");
    expect(localStorage.getItem("auth_token")).toBeNull();
  });

  it("clears auth state on 401 response during API call", async () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const payload = btoa(
      JSON.stringify({ sub: "user-1", email: "test@test.com", exp: futureExp }),
    );
    const fakeJwt = `header.${payload}.sig`;
    localStorage.setItem("auth_token", fakeJwt);
    localStorage.setItem(
      "auth_user",
      JSON.stringify({ id: "user-1", email: "test@test.com" }),
    );

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

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    // Initially authenticated from stored token
    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("yes");

    // Trigger an API call that returns 401
    await act(async () => {
      screen.getByTestId("fetch-btn").click();
    });

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("no");
    expect(localStorage.getItem("auth_token")).toBeNull();
  });

  it("propagates registration success", async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 201,
      headers: new Headers({ "content-type": "application/json" }),
      json: async () => ({ id: "new-user", email: "new@test.com" }),
    });
    vi.stubGlobal("fetch", mockFetch);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByTestId("register-btn").click();
    });

    expect(screen.getByTestId("is-authenticated")).toHaveTextContent("no");
  });
});
