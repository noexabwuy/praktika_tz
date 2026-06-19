export type UserRole = 'Applicant' | 'Manager' | 'Admin' | 'Director';

export interface User {
  id: string;
  fullName: string;
  login: string;
  email: string;
  role: UserRole;
}

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

export interface AuthResponse {
  token: string;
  user: User;
}

export interface AuthError {
  message: string;
  field?: 'login' | 'password' | 'email' | 'fullName' | 'confirmPassword';
}