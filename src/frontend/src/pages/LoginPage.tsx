import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { TextInput, PasswordInput } from '../components/ui/Input';

type AuthMode = 'login' | 'register';

export const LoginPage: React.FC = () => {
const navigate = useNavigate();
const { login } = useAuth();
const [isLoading, setIsLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [mode, setMode] = useState<AuthMode>('login');

const [loginData, setLoginData] = useState({ login: '', password: '' });
const [registerData, setRegisterData] = useState({
fullName: '',
login: '',
email: '',
password: '',
confirmPassword: '',
});

// ===== ЗАГРУЗКА ПОЛЬЗОВАТЕЛЕЙ ИЗ localStorage =====
const loadUsers = () => {
const saved = localStorage.getItem('myUsers');
if (saved) {
return JSON.parse(saved);
}
return [
{ id: '1', fullName: 'Администратор', login: 'admin', email: 'admin@example.com', password: 'admin123', role: 'Admin' },
{ id: '2', fullName: 'Иванов Иван', login: 'ivanov', email: 'ivanov@example.com', password: 'ivanov123', role: 'Applicant' },
];
};

const [mockUsers, setMockUsers] = useState(loadUsers);

// ===== СОХРАНЕНИЕ В localStorage =====
useEffect(() => {
localStorage.setItem('myUsers', JSON.stringify(mockUsers));
}, [mockUsers]);

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

const utf8ToBase64 = (str: string): string => {
const utf8Bytes = new TextEncoder().encode(str);
let binaryString = '';
for (let i = 0; i < utf8Bytes.length; i++) {
binaryString += String.fromCharCode(utf8Bytes[i]);
}
return btoa(binaryString);
};

const handleLoginSubmit = async (e: React.FormEvent) => {
e.preventDefault();
setIsLoading(true);
setError(null);
await delay(800);

const user = mockUsers.find(
(u) => u.login === loginData.login && u.password === loginData.password
);

if (!user) {
setError('Неверный логин или пароль');
setIsLoading(false);
return;
}

const tokenData = JSON.stringify({
userId: user.id,
login: user.login,
exp: Date.now() + 3600000,
});
const token = utf8ToBase64(tokenData);

login(token, {
id: user.id,
fullName: user.fullName,
login: user.login,
email: user.email,
role: user.role,
});

navigate('/');
setIsLoading(false);
};

const handleRegisterSubmit = async (e: React.FormEvent) => {
e.preventDefault();
setIsLoading(true);
setError(null);

if (registerData.password !== registerData.confirmPassword) {
setError('Пароли не совпадают');
setIsLoading(false);
return;
}

if (registerData.password.length < 8) {
setError('Пароль должен содержать минимум 8 символов');
setIsLoading(false);
return;
}

await delay(800);

if (mockUsers.some((u) => u.login === registerData.login)) {
setError('Пользователь с таким логином уже существует');
setIsLoading(false);
return;
}

if (mockUsers.some((u) => u.email === registerData.email)) {
setError('Пользователь с таким email уже существует');
setIsLoading(false);
return;
}

const newUser = {
id: String(Date.now()),
fullName: registerData.fullName,
login: registerData.login,
email: registerData.email,
password: registerData.password,
role: 'Applicant' as const,
};

const updatedUsers = [...mockUsers, newUser];
setMockUsers(updatedUsers);
localStorage.setItem('myUsers', JSON.stringify(updatedUsers));

const tokenData = JSON.stringify({
userId: newUser.id,
login: newUser.login,
exp: Date.now() + 3600000,
});
const token = utf8ToBase64(tokenData);

login(token, {
id: newUser.id,
fullName: newUser.fullName,
login: newUser.login,
email: newUser.email,
role: newUser.role,
});

navigate('/');
setIsLoading(false);
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
<div className="mb-md p-md bg-red-50 border border-status-error/30 rounded-base text-status-error text-text-l font-medium">
{error}
</div>
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
setError(null);
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