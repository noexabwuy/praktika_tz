import { api } from './api';

export interface UserResponse {
  id: string;
  fullName: string;
  role: string;
}

export const userService = {
  getManagers: async (): Promise<UserResponse[]> => {
    // Получение списка менеджеров для назначения ответственных
    const response = await api.get<UserResponse[]>('/users', {
      params: { role: 'Manager' },
    });
    return response.data;
  },
};
