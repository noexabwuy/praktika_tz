import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
} from 'react';

export interface AuthUser {
  id: string;
  fullName: string;
  login: string;
  email: string;
  role: 'Applicant' | 'Manager' | 'Admin' | 'Director';
}

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (token: string, user: AuthUser) => void;
  logout: () => void;
  isAuthenticated: boolean;
}

const TOKEN_KEY = 'auth_token';
const USER_KEY  = 'auth_user';

const AuthContext = createContext<AuthContextValue | null>(null);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isLoading: true,
  });
  useEffect(() => {
    try {
      const token = localStorage.getItem(TOKEN_KEY);
      const raw   = localStorage.getItem(USER_KEY);
      if (token && raw) {
        const user = JSON.parse(raw) as AuthUser;
        setState({ token, user, isLoading: false });
        return;
      }
    } catch {}
    setState({ token: null, user: null, isLoading: false });
  }, []);

  const login = useCallback((token: string, user: AuthUser) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    setState({ token, user, isLoading: false });
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setState({ token: null, user: null, isLoading: false });
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      ...state,
      login,
      logout,
      isAuthenticated: !!state.token && !!state.user,
    }),
    [state, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}
