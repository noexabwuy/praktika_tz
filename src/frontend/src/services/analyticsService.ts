import { api } from './api';

export interface AnalyticsResponse {
  totalApplications: number;
  totalUsers: number;
  byStatuses: Record<string, number>;
  byDirections: Record<string, number>;
  byFormats: Record<string, number>;
}

export const analyticsService = {
  getStatistics: async (): Promise<AnalyticsResponse> => {
    const response = await api.get<AnalyticsResponse>('/analytics/statistics');
    return response.data;
  },
};
