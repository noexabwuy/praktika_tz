import React, { useState, useEffect, useCallback, useRef } from 'react';
import { X, Pen, Check } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { TextInput } from '../components/ui/Input/TextInput';
import { IconButton } from '../components/ui/IconButton';
import { SearchInput } from '../components/ui/SearchInput';
import { dictionaryService } from '../services/dictionaryService';
import type { DictionaryItem } from '../services/dictionaryService';

export const DictionariesPage: React.FC = () => {
  const [directions, setDirections] = useState<DictionaryItem[]>([]);
  const [formats, setFormats] = useState<DictionaryItem[]>([]);
  const [activeTab, setActiveTab] = useState<'directions' | 'formats'>('directions');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // Состояние модалки создания/редактирования
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editItem, setEditItem] = useState<{ id?: string; name: string } | null>(null);
  const [modalError, setModalError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  // Состояние модалки подтверждения удаления
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [deleteItem, setDeleteItem] = useState<{ id: string; name: string } | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Ссылки для синхронизации горизонтального скролла шапки и тела
  const headerRef = useRef<HTMLDivElement>(null);
  const bodyRef = useRef<HTMLDivElement>(null);

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
    if (headerRef.current) {
      headerRef.current.scrollLeft = e.currentTarget.scrollLeft;
    }
  };

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [dirsData, formatsData] = await Promise.all([
        dictionaryService.getDirections(),
        dictionaryService.getStudyFormats(),
      ]);
      setDirections(dirsData);
      setFormats(formatsData);
    } catch {
      setError('Не удалось загрузить списки справочников. Пожалуйста, обновите страницу.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleOpenAdd = () => {
    setModalError(null);
    setEditItem({ name: '' });
    setIsEditModalOpen(true);
  };

  const handleOpenEdit = (item: DictionaryItem) => {
    setModalError(null);
    setEditItem({ id: item.id, name: item.name });
    setIsEditModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editItem || !editItem.name.trim()) return;

    try {
      setIsSaving(true);
      setModalError(null);

      if (editItem.id) {
        // Редактирование
        if (activeTab === 'directions') {
          await dictionaryService.updateDirection(editItem.id, editItem.name.trim());
        } else {
          await dictionaryService.updateStudyFormat(editItem.id, editItem.name.trim());
        }
      } else {
        // Создание
        if (activeTab === 'directions') {
          await dictionaryService.createDirection(editItem.name.trim());
        } else {
          await dictionaryService.createStudyFormat(editItem.name.trim());
        }
      }

      await loadData();
      setIsEditModalOpen(false);
      const actionText = editItem.id ? 'сохранено' : 'добавлено';
      const entityText = activeTab === 'directions' ? 'Направление' : 'Формат обучения';
      setSuccessMessage(`${entityText} успешно ${actionText}.`);
      setError(null);
      setEditItem(null);
    } catch (err: any) {
      const errMsg = err?.response?.data?.message || 'Ошибка при сохранении изменений.';
      setModalError(errMsg);
    } finally {
      setIsSaving(false);
    }
  };

  const handleOpenDelete = (item: DictionaryItem) => {
    setError(null);
    setDeleteItem(item);
    setIsDeleteModalOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteItem) return;

    try {
      setIsDeleting(true);
      setError(null);

      if (activeTab === 'directions') {
        await dictionaryService.deleteDirection(deleteItem.id);
      } else {
        await dictionaryService.deleteStudyFormat(deleteItem.id);
      }

      await loadData();
      setIsDeleteModalOpen(false);
      const entityText = activeTab === 'directions' ? 'Направление' : 'Формат обучения';
      setSuccessMessage(`${entityText} успешно удалено.`);
      setError(null);
      setDeleteItem(null);
    } catch (err: any) {
      const errMsg = err?.response?.data?.message || 'Не удалось удалить элемент справочника.';
      setError(errMsg);
      setIsDeleteModalOpen(false);
    } finally {
      setIsDeleting(false);
    }
  };

  const currentItems = activeTab === 'directions' ? directions : formats;
  const filteredItems = currentItems.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="h-full flex flex-col gap-md overflow-hidden">
      
      {/* Переключатель вкладок (Табы) */}
      <div className="flex border-b border-border gap-sm flex-shrink-0">
        <button
          onClick={() => {
            setActiveTab('directions');
            setSearchQuery('');
            setError(null);
            setSuccessMessage(null);
          }}
          className={`flex items-center gap-xs px-md py-sm font-semibold text-text-l border-b-2 transition-all ${
            activeTab === 'directions'
              ? 'border-primary text-primary'
              : 'border-transparent text-text-secondary hover:text-text-primary'
          }`}
        >
          Направления обучения
        </button>
        <button
          onClick={() => {
            setActiveTab('formats');
            setSearchQuery('');
            setError(null);
            setSuccessMessage(null);
          }}
          className={`flex items-center gap-xs px-md py-sm font-semibold text-text-l border-b-2 transition-all ${
            activeTab === 'formats'
              ? 'border-primary text-primary'
              : 'border-transparent text-text-secondary hover:text-text-primary'
          }`}
        >
          Форматы обучения
        </button>
      </div>

      {/* Ошибки валидации или удаления */}
      {error && (
        <div className="p-md bg-red-50 border border-status-error/30 rounded-base text-status-error text-text-l mb-md flex-shrink-0">
          {error}
        </div>
      )}

      {/* Уведомления об успешных операциях */}
      {successMessage && (
        <div className="p-md bg-primary-light border border-status-success/30 rounded-base text-status-success text-text-l flex items-center gap-xs animate-scaleIn flex-shrink-0">
          <Check size={18} strokeWidth={3} className="text-status-success mr-1" />
          <span>{successMessage}</span>
          <button onClick={() => setSuccessMessage(null)} className="ml-auto">
            <X size={18} />
          </button>
        </div>
      )}

      {/* Панель поиска и добавления (снаружи карточки, как на странице заявок) */}
      <div className="flex gap-md items-center justify-between w-full flex-wrap flex-shrink-0">
        <div className="w-full sm:w-[320px]">
          <SearchInput
            placeholder="Поиск по названию..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full"
          />
        </div>
        
        <Button
          variant="primary"
          onClick={handleOpenAdd}
          className="!min-w-0 !w-auto"
        >
          Добавить {activeTab === 'directions' ? 'направление' : 'формат'}
        </Button>
      </div>

      {/* Основная панель с таблицей справочника */}
      <div className="bg-bg-card border border-border rounded-md shadow-sm overflow-hidden flex-1 flex flex-col">
        
        {/* Шапка таблицы — скроллится только по горизонтали синхронно с телом */}
        <div 
          ref={headerRef} 
          className="overflow-hidden w-full bg-[#f3f4f6] border-b border-border flex-shrink-0"
        >
          <table className="w-full text-left border-collapse table-fixed min-w-[500px]">
            <thead>
              <tr className="border-b border-border h-[52px]">
                <th className="px-lg py-md text-text-l font-bold text-text-primary w-[80px]">№</th>
                <th className="px-lg py-md text-text-l font-bold text-text-primary min-w-[200px]">Название</th>
                <th className="px-lg py-md text-text-l font-bold text-text-primary w-[150px] text-center">Действия</th>
              </tr>
            </thead>
          </table>
        </div>

        {/* Тело таблицы — скроллится во все стороны */}
        <div 
          ref={bodyRef} 
          onScroll={handleScroll}
          className="overflow-auto w-full flex-1"
        >
          {isLoading ? (
            // Скелетон загрузки
            <div className="p-lg flex flex-col gap-md">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="flex gap-md py-md border-b border-border/50 items-center">
                  <div className="h-5 bg-gray-200 rounded animate-pulse w-1/12" />
                  <div className="h-5 bg-gray-200 rounded animate-pulse w-9/12" />
                  <div className="h-5 bg-gray-200 rounded animate-pulse w-2/12" />
                </div>
              ))}
            </div>
          ) : filteredItems.length === 0 ? (
            // Пустое состояние
            <div className="flex flex-col items-center justify-center py-xl gap-xs text-center">
              <span className="text-text-l font-medium text-text-primary">Элементы не найдены</span>
              <span className="text-text-m text-text-secondary">
                {searchQuery ? 'Попробуйте изменить поисковый запрос.' : 'В справочнике пока нет записей.'}
              </span>
            </div>
          ) : (
            <table className="w-full text-left border-collapse table-fixed min-w-[500px]">
              {/* Невидимая шапка для фиксации ширины колонок */}
              <thead className="invisible">
                <tr className="h-0 border-0">
                  <th className="p-0 h-0 w-[80px]"></th>
                  <th className="p-0 h-0 min-w-[200px]"></th>
                  <th className="p-0 h-0 w-[150px]"></th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.map((item, idx) => (
                  <tr
                    key={item.id}
                    className="group border-b border-border/60 last:border-0 hover:bg-primary-light transition-colors h-[52px]"
                  >
                    <td className="px-lg py-md text-text-l text-text-secondary w-[80px] font-mono">
                      {idx + 1}
                    </td>
                    <td className="px-lg py-md text-text-l text-text-primary font-medium truncate" title={item.name}>
                      {item.name}
                    </td>
                    <td className="px-lg py-0 w-[150px]" onClick={(e) => e.stopPropagation()}>
                      <div className="flex justify-center items-center gap-xs h-full">
                        <IconButton
                          variant="default"
                          icon={<Pen size={18} className="text-text-primary" />}
                          onClick={() => handleOpenEdit(item)}
                          tooltip="Редактировать"
                          ariaLabel="Редактировать"
                        />
                        <IconButton
                          variant="default"
                          icon={<X size={18} className="text-text-primary" />}
                          onClick={() => handleOpenDelete(item)}
                          tooltip="Удалить"
                          ariaLabel="Удалить"
                        />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {/* Модальное окно Добавления/Редактирования */}
      {isEditModalOpen && editItem && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-[100] p-md">
          <div className="absolute inset-0" onClick={() => !isSaving && setIsEditModalOpen(false)} />
          
          <form
            onSubmit={handleSave}
            className="bg-bg-card rounded-md border border-border shadow-xl w-full max-w-md p-lg flex flex-col gap-lg z-[110] relative text-left"
          >
            <div className="flex justify-between items-center pb-xs border-b border-border">
              <h2 className="text-h3 font-semibold text-text-primary">
                {editItem.id ? 'Редактировать' : 'Добавить'} {activeTab === 'directions' ? 'направление' : 'формат'}
              </h2>
              <button
                type="button"
                disabled={isSaving}
                onClick={() => setIsEditModalOpen(false)}
                className="text-text-secondary hover:text-text-primary transition-colors disabled:opacity-50"
              >
                <X size={20} />
              </button>
            </div>

            {modalError && (
              <div className="p-sm bg-red-50 border border-status-error/30 rounded-base text-status-error text-text-m">
                {modalError}
              </div>
            )}

            <TextInput
              label="Название"
              placeholder={`Введите название ${activeTab === 'directions' ? 'направления' : 'формата'}...`}
              value={editItem.name}
              onChange={(e) => setEditItem({ ...editItem, name: e.target.value })}
              required
              disabled={isSaving}
              autoFocus
            />

            <div className="flex justify-end gap-sm pt-xs border-t border-border mt-xs">
              <Button
                type="button"
                variant="secondary"
                disabled={isSaving}
                onClick={() => setIsEditModalOpen(false)}
                className="!min-w-0 px-md !h-[38px] text-text-l"
              >
                Отмена
              </Button>
              <Button
                type="submit"
                variant="primary"
                disabled={isSaving || !editItem.name.trim()}
                className="!min-w-0 px-md !h-[38px] text-text-l"
              >
                {isSaving ? 'Сохранение...' : 'Сохранить'}
              </Button>
            </div>
          </form>
        </div>
      )}

      {/* Модальное окно подтверждения удаления */}
      {isDeleteModalOpen && deleteItem && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-[100] p-md">
          <div className="absolute inset-0" onClick={() => !isDeleting && setIsDeleteModalOpen(false)} />
          
          <div className="bg-bg-card rounded-md border border-border shadow-xl w-full max-w-md p-lg flex flex-col gap-lg z-[110] relative text-left">
            <div className="flex justify-between items-center pb-xs border-b border-border">
              <h2 className="text-h3 font-semibold text-text-primary">
                Подтверждение удаления
              </h2>
              <button
                disabled={isDeleting}
                onClick={() => setIsDeleteModalOpen(false)}
                className="text-text-secondary hover:text-text-primary transition-colors"
              >
                <X size={20} />
              </button>
            </div>

            <p className="text-text-l text-text-primary">
              Вы действительно хотите удалить {activeTab === 'directions' ? 'направление' : 'формат'} обучения{' '}
              <strong className="text-status-error">«{deleteItem.name}»</strong>?
            </p>
            <p className="text-text-m text-text-secondary">
              Это действие необратимо. Удаление будет возможно только если элемент не привязан к существующим заявкам.
            </p>

            <div className="flex justify-end gap-sm pt-xs border-t border-border mt-xs">
              <Button
                variant="secondary"
                disabled={isDeleting}
                onClick={() => setIsDeleteModalOpen(false)}
                className="!min-w-0 px-md !h-[38px] text-text-l"
              >
                Отмена
              </Button>
              <Button
                variant="primary"
                disabled={isDeleting}
                onClick={handleDelete}
                className="!min-w-0 px-md !h-[38px] text-text-l !bg-status-error hover:!bg-red-700 hover:!border-red-700 active:!bg-red-800"
              >
                {isDeleting ? 'Удаление...' : 'Удалить'}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
