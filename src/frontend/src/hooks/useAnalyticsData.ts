import { useState, useEffect, useCallback } from 'react';
import { analyticsService } from '../services/analyticsService';
import type { AnalyticsResponse } from '../services/analyticsService';
import { applicationService } from '../services/applicationService';
import type { ApplicationResponse } from '../services/applicationService';

export type DynamicsPeriod = 'day' | 'week' | 'month';

export interface ChartDataPoint {
  label: string;
  value: number;
}

export const useAnalyticsData = () => {
  const [stats, setStats] = useState<AnalyticsResponse | null>(null);
  const [applications, setApplications] = useState<ApplicationResponse[]>([]);
  const [period, setPeriod] = useState<DynamicsPeriod>('day');
  
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Загружаем статистику и полный список заявок параллельно
      const [statsData, appsData] = await Promise.all([
        analyticsService.getStatistics(),
        applicationService.getList({ my: false }),
      ]);

      setStats(statsData);
      setApplications(appsData);
    } catch {
      setError('Не удалось загрузить данные аналитики. Пожалуйста, попробуйте еще раз.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Группировка заявок для графика динамики
  const getDynamicsData = useCallback((): ChartDataPoint[] => {
    if (applications.length === 0) return [];

    const now = new Date();
    const dataPoints: ChartDataPoint[] = [];

    if (period === 'day') {
      // Динамика по дням за последние 7 дней
      for (let i = 6; i >= 0; i--) {
        const d = new Date();
        d.setDate(now.getDate() - i);
        const dayString = d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
        
        // Считаем заявки за этот день
        const count = applications.filter((app) => {
          const appDate = new Date(app.createdAt);
          return appDate.toDateString() === d.toDateString();
        }).length;

        dataPoints.push({ label: dayString, value: count });
      }
    } else if (period === 'week') {
      // Динамика по неделям за последние 4 недели
      for (let i = 3; i >= 0; i--) {
        const startOfWeek = new Date();
        startOfWeek.setDate(now.getDate() - (now.getDay() || 7) + 1 - (i * 7)); // Понедельник недели
        startOfWeek.setHours(0, 0, 0, 0);

        const endOfWeek = new Date(startOfWeek);
        endOfWeek.setDate(startOfWeek.getDate() + 6);
        endOfWeek.setHours(23, 59, 59, 999);

        const pad = (num: number) => num.toString().padStart(2, '0');
        const label = `${pad(startOfWeek.getDate())}.${pad(startOfWeek.getMonth() + 1)} - ${pad(endOfWeek.getDate())}.${pad(endOfWeek.getMonth() + 1)}`;

        const count = applications.filter((app) => {
          const appDate = new Date(app.createdAt);
          return appDate >= startOfWeek && appDate <= endOfWeek;
        }).length;

        dataPoints.push({ label, value: count });
      }
    } else if (period === 'month') {
      // Динамика по месяцам за последние 6 месяцев
      for (let i = 5; i >= 0; i--) {
        const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
        const monthLabel = d.toLocaleString('ru-RU', { month: 'short' });
        
        const count = applications.filter((app) => {
          const appDate = new Date(app.createdAt);
          return appDate.getFullYear() === d.getFullYear() && appDate.getMonth() === d.getMonth();
        }).length;

        dataPoints.push({ label: monthLabel, value: count });
      }
    }

    return dataPoints;
  }, [applications, period]);

  return {
    stats,
    dynamics: getDynamicsData(),
    period,
    setPeriod,
    isLoading,
    error,
    refresh: loadData,
  };
};
