import React, { useState, useEffect } from 'react';
import { UserMinus } from 'lucide-react';
import { userService } from '../services/userService';
import type { UserResponse } from '../services/userService';
import { SearchInput } from '../components/ui/SearchInput';
import { Dropdown } from '../components/ui/Dropdown';
import { Alert } from '../components/ui/Alert';

export const UsersPage: React.FC = () => {
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [searchVal, setSearchVal] = useState('');
  const [selectedRole, setSelectedRole] = useState('');

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await userService.getUsers();
        setUsers(data);
      } catch (err: any) {
        setError(err?.response?.data?.message || 'Не удалось загрузить список пользователей.');
      } finally {
        setIsLoading(false);
      }
    };
    fetchUsers();
  }, []);

  // Фильтрация и поиск
  const filteredUsers = users.filter((u) => {
    const matchesRole = !selectedRole || u.role.toLowerCase() === selectedRole.toLowerCase();
    const matchesSearch = !searchVal || u.fullName.toLowerCase().includes(searchVal.toLowerCase());
    return matchesRole && matchesSearch;
  });

  // Опции для фильтра по ролям
  const roleOptions = [
    { value: '', label: 'Все роли' },
    { value: 'Applicant', label: 'Заявитель' },
    { value: 'Manager', label: 'Менеджер' },
    { value: 'Admin', label: 'Администратор' },
    { value: 'Director', label: 'Руководитель' },
  ];

  // Маппинг ролей на русский язык
  const roleLabels: Record<string, string> = {
    Applicant: 'Заявитель',
    Manager: 'Менеджер',
    Admin: 'Администратор',
    Director: 'Руководитель',
  };



  return (
    <div className="w-full flex flex-col gap-lg text-left pb-xl">
      


      {/* ── ФИЛЬТРЫ И ПОИСК ── */}
      <div className="flex gap-md items-center justify-between w-full flex-wrap">
        <div className="flex items-center gap-md w-full sm:w-auto flex-1 sm:flex-initial flex-wrap">
          <div className="w-full sm:w-[320px]">
            <SearchInput
              placeholder="Поиск по ФИО пользователя..."
              value={searchVal}
              onChange={(e) => setSearchVal(e.target.value)}
              className="w-full"
            />
          </div>
          <div className="w-full sm:w-[220px]">
            <Dropdown
              options={roleOptions}
              value={selectedRole}
              onChange={(val) => setSelectedRole(val)}
              placeholder="Фильтр по роли"
            />
          </div>
        </div>
        <div className="h-[42px] text-text-l text-text-secondary font-medium whitespace-nowrap bg-bg-card border border-border rounded-base px-md shadow-sm flex items-center gap-xs ml-auto">
          Найдено: <span className="text-primary font-bold">{filteredUsers.length}</span>
        </div>
      </div>

      {/* ── ТАБЛИЦА ПОЛЬЗОВАТЕЛЕЙ ── */}
      <div className="bg-bg-card border border-border rounded-md shadow-sm overflow-hidden w-full">
        {isLoading ? (
          // Скелетон загрузки таблицы
          <div className="p-lg flex flex-col gap-md">
            <div className="flex gap-md border-b border-border pb-md">
              <div className="h-6 bg-gray-200 rounded animate-pulse w-1/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-3/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-5/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-3/12" />
            </div>
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex gap-md py-md border-b border-border/50 items-center">
                <div className="h-5 bg-gray-200 rounded animate-pulse w-1/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-3/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-5/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-3/12" />
              </div>
            ))}
          </div>
        ) : error ? (
          <div className="p-lg">
            <Alert variant="error">{error}</Alert>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-xl gap-md text-center">
            <UserMinus size={48} className="text-text-secondary opacity-40 animate-pulse" />
            <div className="flex flex-col gap-xs">
              <h3 className="text-h3 font-semibold text-text-primary">Пользователи не найдены</h3>
              <p className="text-text-l text-text-secondary max-w-sm">
                Попробуйте изменить параметры поиска или сбросить фильтр по роли.
              </p>
            </div>
          </div>
        ) : (
          <div className="overflow-x-auto w-full">
            <table className="w-full text-left border-collapse table-auto min-w-[600px]">
              <thead className="bg-gray-100 border-b border-border">
                <tr className="h-[52px]">
                  <th className="px-lg py-md text-text-l font-bold text-text-primary w-[80px]">№</th>
                  <th className="px-lg py-md text-text-l font-bold text-text-primary">ID пользователя</th>
                  <th className="px-lg py-md text-text-l font-bold text-text-primary">ФИО</th>
                  <th className="px-lg py-md text-text-l font-bold text-text-primary w-[200px] text-center">Роль</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((user, idx) => (
                  <tr 
                    key={user.id}
                    className="border-b border-border/50 hover:bg-primary-light/10 transition-colors h-[56px] text-text-l"
                  >
                    <td className="px-lg py-md text-text-secondary font-mono">{idx + 1}</td>
                    <td className="px-lg py-md text-text-secondary font-mono text-xs">{user.id}</td>
                    <td className="px-lg py-md font-semibold text-text-primary truncate max-w-[300px]" title={user.fullName}>
                      {user.fullName}
                    </td>
                    <td className="px-lg py-md text-center text-text-primary font-medium">
                      {roleLabels[user.role] || user.role}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

    </div>
  );
};
