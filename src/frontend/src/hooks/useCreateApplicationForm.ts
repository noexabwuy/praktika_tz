import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { applicationService } from '../services/applicationService';

interface FormData {
  title: string;
  description: string;
  directionId: string;
  formatId: string;
}

interface FormErrors {
  title?: string;
  description?: string;
  directionId?: string;
  formatId?: string;
}

export const useCreateApplicationForm = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState<FormData>({
    title: '',
    description: '',
    directionId: '',
    formatId: '',
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.title.trim()) {
      newErrors.title = 'Укажите тему заявки';
    } else if (formData.title.trim().length < 3) {
      newErrors.title = 'Тема должна содержать минимум 3 символа';
    } else if (formData.title.trim().length > 200) {
      newErrors.title = 'Тема не должна превышать 200 символов';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Укажите описание заявки';
    } else if (formData.description.trim().length < 10) {
      newErrors.description = 'Описание должно содержать минимум 10 символов';
    } else if (formData.description.trim().length > 2000) {
      newErrors.description = 'Описание не должно превышать 2000 символов';
    }

    if (!formData.directionId) {
      newErrors.directionId = 'Выберите направление обучения';
    }

    if (!formData.formatId) {
      newErrors.formatId = 'Выберите желаемый формат обучения';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const clearFieldError = (field: keyof FormErrors) => {
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (isSubmitting || isSuccess) return;
    if (!validate()) return;

    setIsSubmitting(true);
    try {
      await applicationService.create({
        title: formData.title.trim(),
        description: formData.description.trim(),
        directionId: formData.directionId,
        formatId: formData.formatId,
      });
      setIsSuccess(true);
      setTimeout(() => {
        navigate('/applications');
      }, 1500);
    } catch (err: any) {
      let message = 'Ошибка при отправке заявки. Попробуйте еще раз.';
      if (err?.response) {
        if (err.response.status === 403) {
          message = 'У вас нет прав для выполнения этой операции (403 Forbidden).';
        } else if (err.response.status === 401) {
          message = 'Сессия истекла. Пожалуйста, войдите в систему заново (401 Unauthorized).';
        } else {
          message = err.response.data?.message || message;
        }
      }
      setSubmitError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    formData,
    errors,
    isSubmitting,
    isSuccess,
    submitError,
    setFormData,
    clearFieldError,
    handleSubmit,
  };
};
