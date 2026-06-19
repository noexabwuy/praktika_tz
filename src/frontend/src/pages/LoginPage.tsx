import React from 'react';
import { useAuth } from '../context/AuthContext';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();

  React.useEffect(() => {
    login('mock-jwt-token', {
      id: 'test-user',
      fullName: 'Тестовый Пользователь',
      login: 'test',
      email: 'test@example.com',
      role: 'Applicant',
    });
  }, [login]);

  return null;
};
