import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { X, Filter, FilterX, FileText, ChevronLeft, ChevronRight } from 'lucide-react';
import { Dropdown } from '../components/ui/Dropdown';
import { Button } from '../components/ui/Button';
import { SearchInput } from '../components/ui/SearchInput';
import { IconButton } from '../components/ui/IconButton';
import { Alert } from '../components/ui/Alert';
import { useAuth } from '../context/AuthContext';
import { useApplicationDictionaries } from '../hooks/useApplicationDictionaries';
import { useApplicationsList } from '../hooks/useApplicationsList';
import { ApplicationsTable } from '../components/ApplicationsTable';

const STATUS_LABELS: Record<string, string> = {
  New: 'Новая',
  InProgress: 'В работе',
  NeedsInfo: 'Требуется уточнение',
  Approved: 'Согласована',
  Rejected: 'Отклонена',
  Completed: 'Завершена',
};

const PAGE_SIZE = 10;

export const ApplicationsPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  
  const {
    directions,
    formats,
    isLoading: isDictLoading,
  } = useApplicationDictionaries();

  const {
    applications,
    totalCount,
    totalPages,
    currentPage,
    setPage,
    isLoading: isListLoading,
    error,
    filters,
    updateFilter,
    resetFilters,
    handleAssign,
  } = useApplicationsList(PAGE_SIZE);

  const [assignError, setAssignError] = useState<string | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [searchVal, setSearchVal] = useState(filters.search);
  const [showError, setShowError] = useState(true);

  useEffect(() => {
    if (error) setShowError(true);
  }, [error]);

  // Синхронизируем локальный ввод с изменениями URL (например, при полном сбросе)
  useEffect(() => {
    setSearchVal(filters.search);
  }, [filters.search]);

  const isApplicant = user?.role === 'Applicant';
  const isManager = user?.role === 'Manager';

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchVal(e.target.value);
    updateFilter('search', e.target.value);
  };

  const handleManagerSelectChange = async (appId: string, value: string) => {
    setAssignError(null);
    try {
      await handleAssign(appId, value);
    } catch (err: any) {
      setAssignError(err.message || 'Ошибка при назначении ответственного');
    }
  };

  // Опции для фильтров
  const statusOptions = [
    { value: '', label: 'Все статусы' },
    ...Object.entries(STATUS_LABELS).map(([value, label]) => ({ value, label })),
  ];

  const directionOptions = [
    { value: '', label: 'Все направления' },
    ...directions,
  ];

  const formatOptions = [
    { value: '', label: 'Все форматы' },
    ...formats,
  ];

  const hasActiveFilters = !!(filters.status || filters.directionId || filters.formatId || filters.search);

  return (
    <div className="h-full flex flex-col gap-md overflow-hidden">

      {/* Ошибки назначения */}
      {assignError && (
        <Alert variant="error" onClose={() => setAssignError(null)} className="mb-md">
          {assignError}
        </Alert>
      )}

      {/* Глобальная ошибка загрузки */}
      {error && showError && (
        <Alert variant="error" onClose={() => setShowError(false)} className="mb-md">
          {error}
        </Alert>
      )}

      {/* Панель управления поиском и фильтрами */}
      <div className="flex gap-md items-center justify-between w-full flex-wrap">
        {/* Слева: сгруппированные поиск и кнопки фильтрации */}
        <div className="flex items-center gap-sm w-full sm:w-auto flex-1 sm:flex-initial">
          {!isApplicant && (
            <div className="w-full sm:w-[320px]">
              <SearchInput
                placeholder="Поиск по ФИО заявителя..."
                value={searchVal}
                onChange={handleSearchChange}
                className="w-full"
              />
            </div>
          )}
          <IconButton
            variant="default"
            icon={<Filter size={18} />}
            onClick={() => setIsFiltersOpen(true)}
            tooltip="Открыть фильтры"
            ariaLabel="Открыть фильтры"
            className="flex-shrink-0"
          />
          <IconButton
            variant={hasActiveFilters ? 'primary' : 'default'}
            icon={<FilterX size={18} />}
            disabled={!hasActiveFilters}
            onClick={resetFilters}
            tooltip="Сбросить все фильтры"
            ariaLabel="Сбросить все фильтры"
            className={`flex-shrink-0 ${!hasActiveFilters ? 'opacity-60 cursor-not-allowed bg-border/10 text-text-secondary/40' : ''}`}
          />
        </div>

        {/* Справа: основное действие или статистика */}
        <div className="flex items-center justify-end ml-auto">
          {isApplicant ? (
            <Button
              variant="primary"
              onClick={() => navigate('/applications/new')}
              className="!min-w-0 !w-auto animate-scaleIn"
            >
              Подать заявку
            </Button>
          ) : (
            <div className="h-[42px] text-text-l text-text-secondary font-medium whitespace-nowrap bg-bg-card border border-border rounded-base px-md shadow-sm flex items-center gap-xs">
              Всего заявок: <span className="text-primary font-bold">{totalCount}</span>
            </div>
          )}
        </div>
      </div>

      {/* Модальное окно параметров фильтрации (поверх всего) */}
      {isFiltersOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-[100] p-md">
          {/* Область клика для закрытия модалки */}
          <div className="absolute inset-0" onClick={() => setIsFiltersOpen(false)} />
          
          {/* Контент модального окна */}
          <div className="bg-bg-card rounded-md border border-border shadow-xl w-full max-w-md p-lg flex flex-col gap-lg z-[110] relative animate-scaleIn">
            <div className="flex justify-between items-center pb-xs border-b border-border">
              <h2 className="text-h3 font-semibold text-text-primary">
                Параметры фильтрации
              </h2>
              <button
                onClick={() => setIsFiltersOpen(false)}
                className="text-text-secondary hover:text-text-primary transition-colors"
              >
                <X size={20} />
              </button>
            </div>

            <div className="flex flex-col gap-md">
              <Dropdown
                label="Статус"
                options={statusOptions}
                value={filters.status}
                onChange={(val) => updateFilter('status', val)}
              />

              <Dropdown
                label="Направление"
                placeholder={isDictLoading ? 'Загрузка...' : 'Выберите направление'}
                options={directionOptions}
                value={filters.directionId}
                onChange={(val) => updateFilter('directionId', val)}
              />

              <Dropdown
                label="Формат обучения"
                placeholder={isDictLoading ? 'Загрузка...' : 'Выберите формат'}
                options={formatOptions}
                value={filters.formatId}
                onChange={(val) => updateFilter('formatId', val)}
              />
            </div>

            <div className="flex justify-end gap-sm pt-xs border-t border-border mt-xs">
              {hasActiveFilters && (
                <Button
                  variant="secondary"
                  onClick={() => {
                    resetFilters();
                    setIsFiltersOpen(false);
                  }}
                  className="!min-w-0 px-md !h-[38px] text-text-l"
                >
                  Сбросить всё
                </Button>
              )}
              <Button
                variant="primary"
                onClick={() => setIsFiltersOpen(false)}
                className="!min-w-0 px-md !h-[38px] text-text-l"
              >
                Применить
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Основная таблица / Карточка */}
      <div className="bg-bg-card border border-border rounded-md shadow-sm overflow-hidden flex-initial flex flex-col justify-between">
        
        {isListLoading ? (
          // Скелетон загрузки таблицы
          <div className="p-lg flex flex-col gap-md flex-1">
            <div className="flex gap-md border-b border-border pb-md">
              <div className="h-6 bg-gray-200 rounded animate-pulse w-1/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-2/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-3/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-2/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-2/12" />
              <div className="h-6 bg-gray-200 rounded animate-pulse w-2/12" />
            </div>
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex gap-md py-md border-b border-border/50 items-center">
                <div className="h-5 bg-gray-200 rounded animate-pulse w-1/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-2/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-3/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-2/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-2/12" />
                <div className="h-5 bg-gray-200 rounded animate-pulse w-2/12" />
              </div>
            ))}
          </div>
        ) : applications.length === 0 ? (
          // Состояние пустой таблицы
          <div className="flex flex-col items-center justify-center py-xl gap-md text-center flex-1">
            <FileText size={48} className="text-text-secondary opacity-40 animate-pulse" />
            <div className="flex flex-col gap-xs">
              <h3 className="text-h3 font-semibold text-text-primary">Заявки не найдены</h3>
              <p className="text-text-l text-text-secondary max-w-sm">
                {hasActiveFilters
                  ? 'Попробуйте изменить параметры поиска или сбросить фильтры.'
                  : 'В системе пока нет созданных заявок.'}
              </p>
            </div>
            {hasActiveFilters && (
              <Button variant="secondary" onClick={resetFilters} className="w-auto">
                Сбросить фильтры
              </Button>
            )}
          </div>
        ) : (
          // Рендер выделенного компонента таблицы
          <ApplicationsTable
            applications={applications}
            isManager={isManager}
            currentUserId={user?.id}
            onAssignManager={handleManagerSelectChange}
          />
        )}

        {/* Пагинация */}
        {totalPages > 1 && !isListLoading && (
          <div className="flex justify-between items-center bg-bg-card border-t border-border px-lg py-md gap-md flex-wrap">
            {/* Счётчик — скрывается на узких экранах */}
            <span className="text-text-m text-text-secondary whitespace-nowrap hidden sm:inline">
              {(currentPage - 1) * PAGE_SIZE + 1}–{Math.min(currentPage * PAGE_SIZE, totalCount)} из {totalCount}
            </span>

            <div className="flex gap-xs items-center flex-wrap">
              <IconButton
                variant="default"
                icon={<ChevronLeft size={18} />}
                disabled={currentPage === 1}
                onClick={() => setPage(currentPage - 1)}
                tooltip="Предыдущая страница"
                ariaLabel="Предыдущая страница"
              />

              {/* Умная пагинация с эллипсисом */}
              {(() => {
                const WING = 2; // кнопок вокруг текущей страницы
                const pages: (number | 'ellipsis')[] = [];

                if (totalPages <= 7) {
                  // Мало страниц — показываем все
                  for (let i = 1; i <= totalPages; i++) pages.push(i);
                } else {
                  pages.push(1);
                  const left  = Math.max(2, currentPage - WING);
                  const right = Math.min(totalPages - 1, currentPage + WING);
                  if (left > 2)             pages.push('ellipsis');
                  for (let i = left; i <= right; i++) pages.push(i);
                  if (right < totalPages - 1) pages.push('ellipsis');
                  pages.push(totalPages);
                }

                return pages.map((p, idx) =>
                  p === 'ellipsis' ? (
                    <span key={`ellipsis-${idx}`} className="w-[42px] h-[42px] flex items-center justify-center text-text-m text-text-secondary select-none">
                      …
                    </span>
                  ) : (
                    <Button
                      key={p}
                      variant={currentPage === p ? 'primary' : 'secondary'}
                      onClick={() => setPage(p)}
                      className="!w-[42px] !min-w-0 p-0 flex items-center justify-center"
                    >
                      {p}
                    </Button>
                  )
                );
              })()}

              <IconButton
                variant="default"
                icon={<ChevronRight size={18} />}
                disabled={currentPage === totalPages}
                onClick={() => setPage(currentPage + 1)}
                tooltip="Следующая страница"
                ariaLabel="Следующая страница"
              />
            </div>
          </div>
        )}

      </div>
    </div>
  );
};
