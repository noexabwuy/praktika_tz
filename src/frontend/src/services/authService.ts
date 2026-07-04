import { api } from './api';
import type { LoginCredentials, RegisterCredentials, AuthResponse, User } from '../types/auth.types';
import axios from 'axios';

export class AuthError extends Error {
  public field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword';
  
  constructor(message: string, field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword') {
    super(message);
    this.name = 'AuthError';
    this.field = field;
  }
}

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

export const authService = {
  login: async (credentials: LoginCredentials): Promise<AuthResponse> => {
    try {
      const response = await api.post<AuthResponse>('/auth/login', {
        login: credentials.login,
        password: credentials.password,
      });
      return response.data;
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response) {
        const data = err.response.data as { message?: string; field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword' };
        throw new AuthError(data.message || 'Неверный логин или пароль', data.field);
      }
      throw new AuthError('Ошибка подключения к серверу. Убедитесь, что бэкенд запущен.');
    }
  },

  register: async (credentials: RegisterCredentials): Promise<AuthResponse> => {
    try {
      // 1. Отправляем запрос на регистрацию
      await api.post('/auth/register', {
        fullName: credentials.fullName,
        login: credentials.login,
        email: credentials.email,
        password: credentials.password,
      });

      // 2. После успешной регистрации автоматически авторизуемся для получения токена
      return await authService.login({
        login: credentials.login,
        password: credentials.password,
      });
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response) {
        const data = err.response.data as { message?: string; field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword' };
        throw new AuthError(data.message || 'Ошибка регистрации', data.field);
      }
      throw new AuthError('Ошибка подключения к серверу. Убедитесь, что бэкенд запущен.');
    }
  },

  setAuthData: (token: string, user: User): void => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  },

  getToken: (): string | null => {
    return localStorage.getItem(TOKEN_KEY);
  },

  getUser: (): User | null => {
    const user = localStorage.getItem(USER_KEY);
    return user ? JSON.parse(user) : null;
  },

  logout: (): void => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem(TOKEN_KEY);
  },
};