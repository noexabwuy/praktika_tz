import { useState, useEffect, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { applicationService } from '../services/applicationService';
import type { ApplicationResponse } from '../services/applicationService';
import { userService } from '../services/userService';
import type { UserResponse } from '../services/userService';
import { useAuth } from '../context/AuthContext';

export const useApplicationsList = (pageSize: number = 10) => {
  const { user } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const [applications, setApplications] = useState<ApplicationResponse[]>([]);
  const [managers, setManagers] = useState<UserResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Извлекаем параметры из URL
  const search = searchParams.get('search') || '';
  const status = searchParams.get('status') || '';
  const directionId = searchParams.get('directionId') || '';
  const formatId = searchParams.get('formatId') || '';
  const pageStr = searchParams.get('page') || '1';
  const page = parseInt(pageStr, 10) || 1;

  const isManagerOrAdmin = user?.role === 'Manager' || user?.role === 'Admin' || user?.role === 'Director';

  // Метод получения заявок с сервера (фильтруем по статусу, направлению и формату на бэке)
  const fetchApplications = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      const params: any = {
        my: user?.role === 'Applicant',
      };
      if (status) params.status = status;
      if (directionId) params.directionId = directionId;
      if (formatId) params.formatId = formatId;

      const data = await applicationService.getList(params);
      setApplications(data);
    } catch {
      setError('Не удалось загрузить список заявок. Пожалуйста, попробуйте еще раз.');
    } finally {
      setIsLoading(false);
    }
  }, [user, status, directionId, formatId]);

  // Загружаем заявки при монтировании или изменении серверных фильтров
  useEffect(() => {
    if (user) {
      fetchApplications();
    }
  }, [fetchApplications, user]);

  // Загружаем список менеджеров
  useEffect(() => {
    const fetchManagers = async () => {
      if (isManagerOrAdmin) {
        try {
          const data = await userService.getManagers();
          setManagers(data);
        } catch {
          console.error('Failed to load managers list');
        }
      }
    };
    fetchManagers();
  }, [isManagerOrAdmin]);

  // 1. Поиск по ФИО на клиенте
  const filteredApplications = useMemo(() => {
    return applications.filter((app) => {
      if (search && !app.authorName.toLowerCase().includes(search.toLowerCase())) {
        return false;
      }
      return true;
    });
  }, [applications, search]);

  // 2. Пагинация на клиенте (размер страницы передаётся снаружи)
  const totalCount = filteredApplications.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  
  // Убеждаемся, что текущая страница не выходит за рамки допустимого
  const currentPage = Math.min(Math.max(1, page), totalPages);

  const paginatedApplications = useMemo(() => {
    const startIndex = (currentPage - 1) * pageSize;
    return filteredApplications.slice(startIndex, startIndex + pageSize);
  }, [filteredApplications, currentPage, pageSize]);

  const handleAssign = async (applicationId: string, managerId: string) => {
    try {
      await applicationService.assign(applicationId, managerId);
      await fetchApplications();
    } catch (err: any) {
      const errMsg = err?.response?.data?.message || 'Не удалось назначить ответственного';
      throw new Error(errMsg);
    }
  };

  const handleUpdateStatus = async (applicationId: string, status: string) => {
    try {
      await applicationService.updateStatus(applicationId, status);
      await fetchApplications();
    } catch (err: any) {
      const errMsg = err?.response?.data?.message || 'Не удалось обновить статус';
      throw new Error(errMsg);
    }
  };

  // Обновление конкретного фильтра в URL
  const updateFilter = (key: string, value: string) => {
    setSearchParams((prev) => {
      if (value) {
        prev.set(key, value);
      } else {
        prev.delete(key);
      }
      prev.set('page', '1'); // Сбрасываем страницу на 1-ю при изменении фильтров
      return prev;
    });
  };

  const setPage = (newPage: number) => {
    setSearchParams((prev) => {
      prev.set('page', newPage.toString());
      return prev;
    });
  };

  const resetFilters = () => {
    setSearchParams(new URLSearchParams());
  };

  return {
    applications: paginatedApplications, // Возвращаем только текущую страницу
    totalCount,
    totalPages,
    currentPage,
    setPage,
    managers,
    isLoading,
    error,
    filters: {
      search,
      status,
      directionId,
      formatId,
    },
    updateFilter,
    resetFilters,
    handleAssign,
    handleUpdateStatus,
    refresh: fetchApplications,
  };
};
