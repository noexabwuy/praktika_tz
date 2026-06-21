import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextInput, PasswordInput } from '../components/ui/Input';
import { Alert } from '../components/ui/Alert';
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
    const success = await handleLogin(loginData);
    if (success) {
      navigate('/');
    }
  };

  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const success = await handleRegister(registerData);
    if (success) {
      navigate('/');
    }
  };

  return (
    <div className="min-h-screen bg-bg-page flex items-center justify-center p-md font-sans">
      <div className="w-full max-w-md bg-bg-card rounded-md border border-border shadow-lg p-lg">
        {/* Логотип / Шапка */}
        <div className="flex flex-col items-center gap-xs mb-lg">
          <h1 className="text-h1 font-bold text-text-primary text-center">
            {mode === 'login' ? 'Вход в систему' : 'Регистрация'}
          </h1>
        </div>

        {error && (
          <Alert variant="error" onClose={clearError} className="mb-md">
            {error.message}
          </Alert>
        )}

        {mode === 'login' ? (
          <form onSubmit={handleLoginSubmit} className="flex flex-col gap-md">
            <TextInput
              label="Логин"
              placeholder="Введите логин"
              value={loginData.login}
              onChange={(e) => setLoginData({ ...loginData, login: e.target.value })}
              required
              disabled={isLoading}
            />

            <PasswordInput
              label="Пароль"
              placeholder="Введите пароль"
              value={loginData.password}
              onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
              required
              disabled={isLoading}
            />

            <button
              type="submit"
              disabled={isLoading}
              className="h-[42px] mt-sm w-full bg-primary hover:bg-primary-hover active:shadow-[inset_0_3px_5px_rgba(0,0,0,0.15)] text-white font-semibold rounded-base transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Вход...' : 'Войти'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} className="flex flex-col gap-md">
            <TextInput
              label="ФИО"
              placeholder="Иванов Иван Иванович"
              value={registerData.fullName}
              onChange={(e) => setRegisterData({ ...registerData, fullName: e.target.value })}
              required
              disabled={isLoading}
            />

            <TextInput
              label="Логин"
              placeholder="Придумайте логин"
              value={registerData.login}
              onChange={(e) => setRegisterData({ ...registerData, login: e.target.value })}
              required
              disabled={isLoading}
            />

            <TextInput
              label="Email"
              type="email"
              placeholder="ivanov@mail.ru"
              value={registerData.email}
              onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
              required
              disabled={isLoading}
            />

            <PasswordInput
              label="Пароль"
              placeholder="Минимум 8 символов"
              value={registerData.password}
              onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
              required
              disabled={isLoading}
            />

            <PasswordInput
              label="Подтверждение пароля"
              placeholder="Повторите пароль"
              value={registerData.confirmPassword}
              onChange={(e) => setRegisterData({ ...registerData, confirmPassword: e.target.value })}
              required
              disabled={isLoading}
            />

            <button
              type="submit"
              disabled={isLoading}
              className="h-[42px] mt-sm w-full bg-primary hover:bg-primary-hover active:shadow-[inset_0_3px_5px_rgba(0,0,0,0.15)] text-white font-semibold rounded-base transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Регистрация...' : 'Зарегистрироваться'}
            </button>
          </form>
        )}

        <div className="mt-lg text-center border-t border-border pt-md">
          <button
            type="button"
            onClick={() => {
              setMode(mode === 'login' ? 'register' : 'login');
              clearError();
            }}
            className="text-primary hover:text-primary-hover hover:underline transition-colors text-text-l font-medium"
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