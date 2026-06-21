import { api } from './api';

export interface DictionaryItem {
  id: string;
  name: string;
}

export const dictionaryService = {
  getDirections: async (): Promise<DictionaryItem[]> => {
    const response = await api.get<DictionaryItem[]>('/dictionaries/directions');
    return response.data;
  },

  getStudyFormats: async (): Promise<DictionaryItem[]> => {
    const response = await api.get<DictionaryItem[]>('/dictionaries/training-formats');
    return response.data;
  },

  createDirection: async (name: string): Promise<DictionaryItem> => {
    const response = await api.post<DictionaryItem>('/dictionaries/directions', { name });
    return response.data;
  },

  updateDirection: async (id: string, name: string): Promise<DictionaryItem> => {
    const response = await api.put<DictionaryItem>(`/dictionaries/directions/${id}`, { name });
    return response.data;
  },

  deleteDirection: async (id: string): Promise<void> => {
    await api.delete(`/dictionaries/directions/${id}`);
  },

  createStudyFormat: async (name: string): Promise<DictionaryItem> => {
    const response = await api.post<DictionaryItem>('/dictionaries/training-formats', { name });
    return response.data;
  },

  updateStudyFormat: async (id: string, name: string): Promise<DictionaryItem> => {
    const response = await api.put<DictionaryItem>(`/dictionaries/training-formats/${id}`, { name });
    return response.data;
  },

  deleteStudyFormat: async (id: string): Promise<void> => {
    await api.delete(`/dictionaries/training-formats/${id}`);
  },
};
