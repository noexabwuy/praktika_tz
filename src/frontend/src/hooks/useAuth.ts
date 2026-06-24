import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { authService, AuthError } from '../services/authService';
import type { LoginCredentials, RegisterCredentials, AuthError as AuthErrorType } from '../types/auth.types';

export const useAuthActions = () => {
  const { login, logout } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AuthErrorType | null>(null);

  const handleLogin = async (credentials: LoginCredentials): Promise<boolean> => {
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await authService.login(credentials);
      login(response.token, response.user);
      return true;
    } catch (err: unknown) {
      if (err instanceof AuthError) {
        setError({
          message: err.message,
          field: err.field,
        });
      } else {
        setError({
          message: (err as Error).message || 'Ошибка входа',
        });
      }
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegister = async (credentials: RegisterCredentials): Promise<boolean> => {
    setIsLoading(true);
    setError(null);

    // Проверка совпадения паролей
    if (credentials.password !== credentials.confirmPassword) {
      setError({
        message: 'Пароли не совпадают',
        field: 'confirmPassword',
      });
      setIsLoading(false);
      return false;
    }

    // Проверка длины пароля (минимум 8 символов для соответствия бэкенду)
    if (credentials.password.length < 8) {
      setError({
        message: 'Пароль должен содержать минимум 8 символов',
        field: 'password',
      });
      setIsLoading(false);
      return false;
    }

    try {
      const response = await authService.register(credentials);
      login(response.token, response.user);
      return true;
    } catch (err: unknown) {
      if (err instanceof AuthError) {
        setError({
          message: err.message,
          field: err.field,
        });
      } else {
        setError({
          message: (err as Error).message || 'Ошибка регистрации',
        });
      }
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const clearError = () => setError(null);

  return {
    handleLogin,
    handleRegister,
    logout,
    isLoading,
    error,
    clearError,
  };
};  