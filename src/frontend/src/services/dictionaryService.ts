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
};
