import { api } from './api';

export interface CreateApplicationRequest {
  title: string;
  description: string;
  directionId: string;
  formatId: string;
}

export interface ApplicationResponse {
  id: string;
  title: string;
  description: string;
  status: string;
  directionId: string;
  formatId: string;
  authorId: string;
  assignedToId: string | null;
  createdAt: string;
  updatedAt: string;
  directionName: string;
  formatName: string;
  authorName: string;
  assignedToName: string | null;
}

export const applicationService = {
  create: async (data: CreateApplicationRequest): Promise<ApplicationResponse> => {
    const response = await api.post<ApplicationResponse>('/applications', data);
    return response.data;
  },
};
