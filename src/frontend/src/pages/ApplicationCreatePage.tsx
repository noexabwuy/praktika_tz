import React, { useState, useEffect } from 'react';
import { Check } from 'lucide-react';
import { TextInput, TextArea } from '../components/ui/Input';
import { Dropdown } from '../components/ui/Dropdown';
import { Button } from '../components/ui/Button';
import { Alert } from '../components/ui/Alert';
import { useApplicationDictionaries } from '../hooks/useApplicationDictionaries';
import { useCreateApplicationForm } from '../hooks/useCreateApplicationForm';

export const ApplicationCreatePage: React.FC = () => {
  const {
    directions,
    formats,
    isLoading: isDictionaryLoading,
    error: dictionaryError,
  } = useApplicationDictionaries();

  const {
    formData,
    errors,
    isSubmitting,
    isSuccess,
    submitError,
    setFormData,
    clearFieldError,
    handleSubmit,
  } = useCreateApplicationForm();

  const [showDictError, setShowDictError] = useState(true);
  const [showSubmitError, setShowSubmitError] = useState(true);

  useEffect(() => {
    if (dictionaryError) {
      Promise.resolve().then(() => setShowDictError(true));
    }
  }, [dictionaryError]);

  useEffect(() => {
    if (submitError) {
      Promise.resolve().then(() => setShowSubmitError(true));
    }
  }, [submitError]);

  return (
    <div className="h-full flex flex-col">

      {/* Поясняющий текст над формой */}
      <div className="mb-lg">
        <p className="text-h2 font-semibold text-text-primary">
          Заполните форму, и наш менеджер свяжется с вами для уточнения деталей.
        </p>
      </div>

      {/* Ошибка загрузки справочников */}
      {dictionaryError && showDictError && (
        <Alert variant="error" onClose={() => setShowDictError(false)} className="mb-md">
          {dictionaryError}
        </Alert>
      )}

      {/* Ошибка отправки */}
      {submitError && showSubmitError && (
        <Alert variant="error" onClose={() => setShowSubmitError(false)} className="mb-md">
          {submitError}
        </Alert>
      )}

      {/* Успешная отправка */}
      {isSuccess && (
        <Alert variant="success" className="mb-md">
          Заявка успешно отправлена! Перенаправление к списку...
        </Alert>
      )}

      <div className="bg-bg-card rounded-md p-lg">
      <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-lg">

        {/* Группа полей ввода с общим состоянием блокировки */}
        <div className={`flex flex-col gap-lg transition-opacity duration-300 ${
          isSubmitting || isSuccess || isDictionaryLoading ? 'pointer-events-none opacity-60' : ''
        }`}>
          {/* ── Секция 1: Основная информация ── */}
          <div className="flex flex-col gap-md">
            <div>
              <h2 className="text-h3 font-semibold text-text-primary">Основная информация</h2>
              <div className="mt-xs h-[1px] bg-border" />
            </div>

            <TextInput
              label="Тема заявки"
              placeholder="Например: Курс по Python для начинающих"
              value={formData.title}
              onChange={(e) => {
                setFormData((prev) => ({ ...prev, title: e.target.value }));
                clearFieldError('title');
              }}
              error={errors.title}
              disabled={isSubmitting || isSuccess || isDictionaryLoading}
              required
            />
          </div>

          {/* ── Секция 2: Детали обучения ── */}
          <div className="flex flex-col gap-md">
            <div>
              <h2 className="text-h3 font-semibold text-text-primary">Детали обучения</h2>
              <div className="mt-xs h-[1px] bg-border" />
            </div>

            {/* Два дропдауна друг под другом */}
            <Dropdown
              label="Направление обучения"
              placeholder={isDictionaryLoading ? 'Загрузка...' : 'Выберите направление'}
              options={directions}
              value={formData.directionId}
              onChange={(value) => {
                setFormData((prev) => ({ ...prev, directionId: value }));
                clearFieldError('directionId');
              }}
              error={errors.directionId}
            />

            <Dropdown
              label="Желаемый формат"
              placeholder={isDictionaryLoading ? 'Загрузка...' : 'Выберите формат'}
              options={formats}
              value={formData.formatId}
              onChange={(value) => {
                setFormData((prev) => ({ ...prev, formatId: value }));
                clearFieldError('formatId');
              }}
              error={errors.formatId}
            />

            <TextArea
              label="Описание заявки"
              placeholder="Опишите, чему хотите научиться, ваш текущий уровень и пожелания к формату..."
              value={formData.description}
              onChange={(e) => {
                setFormData((prev) => ({ ...prev, description: e.target.value }));
                clearFieldError('description');
              }}
              error={errors.description}
              disabled={isSubmitting || isSuccess || isDictionaryLoading}
              required
            />
          </div>
        </div>

        {/* Кнопка отправки */}
        <div className="flex justify-end pt-xs">
          <Button
            type={isSuccess ? 'button' : 'submit'}
            variant="primary"
            disabled={(isSubmitting || isDictionaryLoading || !!dictionaryError) && !isSuccess}
            className={`min-w-[220px] w-auto transition-all duration-300 ${
              isSuccess ? '!bg-status-success !hover:bg-status-success cursor-default shadow-none border-none' : ''
            }`}
          >
            {isSuccess ? (
              <span className="flex items-center gap-xs animate-scaleIn">
                <Check size={18} strokeWidth={3} className="mr-1" />
                Заявка отправлена!
              </span>
            ) : isSubmitting ? (
              'Отправка...'
            ) : (
              'Отправить заявку'
            )}
          </Button>
        </div>

      </form>
      </div>
    </div>
  );
};
