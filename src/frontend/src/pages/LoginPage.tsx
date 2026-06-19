import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextInput } from '../components/ui/Input/TextInput';
import { PasswordInput } from '../components/ui/Input/PasswordInput';
import { Button } from '../components/ui/Button/Button';
import { useAuthActions } from '../hooks/useAuth';

type AuthMode = 'login' | 'register';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { handleLogin, handleRegister, isLoading, error, clearError } = useAuthActions();
  const [mode, setMode] = useState<AuthMode>('login');

  const [loginData, setLoginData] = useState({ login: '', password: '' });
  const [registerData, setRegisterData] = useState({
    fullName: '',
    login: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    clearError();
    const success = await handleLogin(loginData);
    if (success) {
      navigate('/'); // ← РЕДИРЕКТ НА ГЛАВНУЮ
    }
  };

  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    clearError();
    const success = await handleRegister(registerData);
    if (success) {
      navigate('/'); // ← РЕДИРЕКТ НА ГЛАВНУЮ
    }
  };

  return (
    <div className="min-h-screen bg-bg-page flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-bg-card rounded-xl shadow-lg p-8">
        <h1 className="text-h2 text-text-primary text-center mb-6">
          {mode === 'login' ? 'Вход' : 'Регистрация'}
        </h1>

        {error && (
          <div className="mb-4 p-3 bg-status-error/10 border border-status-error rounded-base text-status-error text-sm">
            {error.message}
          </div>
        )}

        {mode === 'login' ? (
          <form onSubmit={handleLoginSubmit} className="space-y-4">
            <TextInput
              label="Логин"
              value={loginData.login}
              onChange={(e) => setLoginData({ ...loginData, login: e.target.value })}
              required
              placeholder="Введите логин"
            />
            <PasswordInput
              label="Пароль"
              value={loginData.password}
              onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
              required
              placeholder="Введите пароль"
            />
            <Button type="submit" disabled={isLoading} className="w-full">
              {isLoading ? 'Загрузка...' : 'Войти'}
            </Button>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} className="space-y-4">
            <TextInput
              label="ФИО"
              value={registerData.fullName}
              onChange={(e) => setRegisterData({ ...registerData, fullName: e.target.value })}
              required
              placeholder="Иванов Иван Иванович"
            />
            <TextInput
              label="Логин"
              value={registerData.login}
              onChange={(e) => setRegisterData({ ...registerData, login: e.target.value })}
              required
              placeholder="ivanov_i"
            />
            <TextInput
              label="Email"
              type="email"
              value={registerData.email}
              onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
              required
              placeholder="ivanov@mail.ru"
            />
            <PasswordInput
              label="Пароль"
              value={registerData.password}
              onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
              required
              placeholder="Введите пароль"
            />
            <PasswordInput
              label="Подтверждение пароля"
              value={registerData.confirmPassword}
              onChange={(e) => setRegisterData({ ...registerData, confirmPassword: e.target.value })}
              required
              placeholder="Повторите пароль"
            />
            <Button type="submit" disabled={isLoading} className="w-full">
              {isLoading ? 'Загрузка...' : 'Зарегистрироваться'}
            </Button>
          </form>
        )}

        <div className="mt-4 text-center">
          <button
            type="button"
            onClick={() => {
              setMode(mode === 'login' ? 'register' : 'login');
              clearError();
            }}
            className="text-primary hover:text-primary-hover underline transition-colors text-sm"
          >
            {mode === 'login'
              ? 'Нет аккаунта? Зарегистрироваться'
              : 'Уже есть аккаунт? Войти'}
          </button>
        </div>
      </div>
    </div>
  );
};