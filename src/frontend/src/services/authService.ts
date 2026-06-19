// src/services/authService.ts
import type { LoginCredentials, RegisterCredentials, AuthResponse, User } from '../types/auth.types';

// === МОК-ДАННЫЕ ДЛЯ ТЕСТА ===
let mockUsers: any[] = [
  {
    id: '1',
    fullName: 'Администратор',
    login: 'admin',
    email: 'admin@example.com',
    password: 'admin123',
    role: 'admin',
  },
  {
    id: '2',
    fullName: 'Иванов Иван',
    login: 'ivanov',
    email: 'ivanov@example.com',
    password: 'ivanov123',
    role: 'user',
  },
];

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, 800));

// Кастомная ошибка
export class AuthError extends Error {
  public field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword';
  
  constructor(message: string, field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword') {
    super(message);
    this.name = 'AuthError';
    this.field = field;
  }
}

// Функция для правильного кодирования в Base64 (поддерживает UTF-8)
function utf8ToBase64(str: string): string {
  const utf8Bytes = new TextEncoder().encode(str);
  let binaryString = '';
  for (let i = 0; i < utf8Bytes.length; i++) {
    binaryString += String.fromCharCode(utf8Bytes[i]);
  }
  return btoa(binaryString);
}

// API URL
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export const authService = {
  // === РЕАЛЬНЫЙ ЗАПРОС ЧЕРЕЗ FETCH ===
  loginReal: async (credentials: LoginCredentials): Promise<AuthResponse> => {
    const response = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        login: credentials.login,
        password: credentials.password,
      }),
    });

    if (!response.ok) {
      const data = await response.json();
      throw new AuthError(data.message || 'Ошибка входа', data.field);
    }

    const data = await response.json();
    return data;
  },

  registerReal: async (credentials: RegisterCredentials): Promise<AuthResponse> => {
    const response = await fetch(`${API_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        fullName: credentials.fullName,
        login: credentials.login,
        email: credentials.email,
        password: credentials.password,
      }),
    });

    if (!response.ok) {
      const data = await response.json();
      throw new AuthError(data.message || 'Ошибка регистрации', data.field);
    }

    const data = await response.json();
    return data;
  },

  // === МОК-ЗАПРОСЫ ДЛЯ ТЕСТИРОВАНИЯ ===
  login: async (credentials: LoginCredentials): Promise<AuthResponse> => {
    await delay(800);

    const user = mockUsers.find(
      (u) => u.login === credentials.login && u.password === credentials.password
    );

    if (!user) {
      throw new AuthError('Неверный логин или пароль', 'login');
    }

    const tokenData = JSON.stringify({
      userId: user.id,
      login: user.login,
      exp: Date.now() + 3600000,
    });
    const token = utf8ToBase64(tokenData);

    return {
      token,
      user: {
        id: user.id,
        fullName: user.fullName,
        login: user.login,
        email: user.email,
        role: user.role,
      },
    };
  },

  register: async (credentials: RegisterCredentials): Promise<AuthResponse> => {
    await delay(800);

    // Проверка существующего логина
    if (mockUsers.some((u) => u.login === credentials.login)) {
      throw new AuthError('Пользователь с таким логином уже существует', 'login');
    }

    // Проверка существующего email
    if (mockUsers.some((u) => u.email === credentials.email)) {
      throw new AuthError('Пользователь с таким email уже существует', 'email');
    }

    // Проверка длины пароля
    if (credentials.password.length < 6) {
      throw new AuthError('Пароль должен содержать минимум 6 символов', 'password');
    }

    const newUser = {
      id: String(mockUsers.length + 1),
      fullName: credentials.fullName,
      login: credentials.login,
      email: credentials.email,
      password: credentials.password,
      role: 'user' as const,
    };

    mockUsers.push(newUser);

    const tokenData = JSON.stringify({
      userId: newUser.id,
      login: newUser.login,
      exp: Date.now() + 3600000,
    });
    const token = utf8ToBase64(tokenData);

    return {
      token,
      user: {
        id: newUser.id,
        fullName: newUser.fullName,
        login: newUser.login,
        email: newUser.email,
        role: newUser.role,
      },
    };
  },

  // === РАБОТА С LOCALSTORAGE ===
  setAuthData: (token: string, user: User): void => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  },

  getToken: (): string | null => {
    return localStorage.getItem('token');
  },

  getUser: (): User | null => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  logout: (): void => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('token');
  },

  // === ДЛЯ ПЕРЕКЛЮЧЕНИЯ МЕЖДУ МОК И РЕАЛЬНЫМ API ===
  useRealAPI: false, // Установите true, когда бэкенд готов
};