import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import type { ReactNode } from "react";
import { clearAuthToken, getAuthToken, onUnauthorized } from "../api/client";
import { login as apiLogin, register as apiRegister } from "../api/auth";
import type { User } from "../api/types";

const USER_KEY = "auth_user";

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]!));
    const exp = payload.exp as number | undefined;
    if (exp == null) return true;
    return Date.now() >= exp * 1000;
  } catch {
    return true;
  }
}

function storeUser(user: User): void {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function getStoredUser(): User | null {
  try {
    const raw = localStorage.getItem(USER_KEY);
    if (raw == null) return null;
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
}

function clearStoredUser(): void {
  localStorage.removeItem(USER_KEY);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const token = getAuthToken();
    if (token && !isTokenExpired(token)) {
      return getStoredUser();
    }
    if (token) {
      clearAuthToken();
      clearStoredUser();
    }
    return null;
  });
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const unsub = onUnauthorized(() => {
      setUser(null);
      clearStoredUser();
    });
    return unsub;
  }, []);

  useEffect(() => {
    setIsLoading(false);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await apiLogin(email, password);
    storeUser(response.user);
    setUser(response.user);
  }, []);

  const register = useCallback(async (email: string, password: string) => {
    await apiRegister(email, password);
  }, []);

  const logout = useCallback(() => {
    setUser(null);
    clearStoredUser();
    clearAuthToken();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (context === null) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
