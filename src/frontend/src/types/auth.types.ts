// src/types/auth.types.ts
export interface LoginCredentials {
  login: string;
  password: string;
}

export interface RegisterCredentials {
  fullName: string;
  login: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface User {
  id: string;
  fullName: string;
  login: string;
  email: string;
  role: 'admin' | 'manager' | 'user'; // Ваши роли
  // Если нужны роли второго фронтендера, раскомментируйте:
  // role: 'admin' | 'manager' | 'user' | 'Applicant' | 'Director';
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface AuthError {
  message: string;
  field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword';
}