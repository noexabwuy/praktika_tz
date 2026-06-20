import { useState, useEffect } from 'react';
import { dictionaryService } from '../services/dictionaryService';
import type { DropdownOption } from '../components/ui/Dropdown';

export const useApplicationDictionaries = () => {
  const [directions, setDirections] = useState<DropdownOption[]>([]);
  const [formats, setFormats] = useState<DropdownOption[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadDictionaries = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const [directionsData, formatsData] = await Promise.all([
          dictionaryService.getDirections(),
          dictionaryService.getStudyFormats(),
        ]);

        setDirections(directionsData.map((d) => ({ value: d.id, label: d.name })));
        setFormats(formatsData.map((f) => ({ value: f.id, label: f.name })));
      } catch {
        setError('Не удалось загрузить справочники. Проверьте подключение к серверу.');
      } finally {
        setIsLoading(false);
      }
    };

    loadDictionaries();
  }, []);

  return { directions, formats, isLoading, error };
};
