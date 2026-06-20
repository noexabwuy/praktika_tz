import React from 'react';
import { Check } from 'lucide-react';
import { TextInput, TextArea } from '../components/ui/Input';
import { Dropdown } from '../components/ui/Dropdown';
import { Button } from '../components/ui/Button';
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

  return (
    <div className="h-full flex flex-col">

      {/* Поясняющий текст над формой */}
      <div className="mb-lg">
        <p className="text-h2 font-semibold text-text-primary">
          Заполните форму, и наш менеджер свяжется с вами для уточнения деталей.
        </p>
      </div>

      {/* Ошибка загрузки справочников */}
      {dictionaryError && (
        <div className="mb-md p-md bg-red-50 border border-status-error/30 rounded-base text-status-error text-text-l">
          {dictionaryError}
        </div>
      )}

      {/* Ошибка отправки */}
      {submitError && (
        <div className="mb-md p-md bg-red-50 border border-status-error/30 rounded-base text-status-error text-text-l">
          {submitError}
        </div>
      )}

      {/* Успешная отправка */}
      {isSuccess && (
        <div className="mb-md p-md bg-primary-light border border-status-success/30 rounded-base text-status-success text-text-l flex items-center gap-xs">
          <Check size={18} strokeWidth={3} className="text-status-success mr-1" />
          <span>Заявка успешно отправлена! Перенаправление к списку...</span>
        </div>
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
