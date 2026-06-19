// src/hooks/useAuth.ts
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
    } catch (err: any) {
      if (err instanceof AuthError) {
        setError({
          message: err.message,
          field: err.field,
        });
      } else {
        setError({
          message: err.message || 'Ошибка входа',
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

    // === ПРОВЕРКА СОВПАДЕНИЯ ПАРОЛЕЙ ===
    if (credentials.password !== credentials.confirmPassword) {
      setError({
        message: 'Пароли не совпадают',
        field: 'confirmPassword',
      });
      setIsLoading(false);
      return false;
    }

    try {
      const response = await authService.register(credentials);
      login(response.token, response.user);
      return true;
    } catch (err: any) {
      if (err instanceof AuthError) {
        setError({
          message: err.message,
          field: err.field,
        });
      } else {
        setError({
          message: err.message || 'Ошибка регистрации',
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