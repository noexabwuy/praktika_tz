import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { TextArea } from '../components/ui/Input';
import { Dropdown } from '../components/ui/Dropdown';
import { useAuth } from '../context/AuthContext';
import { Alert } from '../components/ui/Alert';
import { applicationService } from '../services/applicationService';
import type { ApplicationResponse, CommentResponse } from '../services/applicationService';

const STATUS_LABELS: Record<string, string> = {
  New: 'Новая',
  InProgress: 'В работе',
  NeedsInfo: 'Требуется уточнение',
  Approved: 'Согласована',
  Rejected: 'Отклонена',
  Completed: 'Завершена',
};

const STATUS_STYLES: Record<string, string> = {
  New:        'bg-bg-page           text-text-secondary border border-border',
  InProgress: 'bg-status-info/10    text-status-info    border border-status-info/20',
  NeedsInfo:  'bg-status-warning/10 text-status-warning border border-status-warning/20',
  Approved:   'bg-status-new        text-status-success border border-status-success/20',
  Rejected:   'bg-status-error/10   text-status-error   border border-status-error/20',
  Completed:  'bg-status-new        text-status-finish  border border-status-finish/20',
};

export const ApplicationDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();

  const [application, setApplication] = useState<ApplicationResponse | null>(null);
  const [comments, setComments] = useState<CommentResponse[]>([]);
  
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmittingStatus, setIsSubmittingStatus] = useState(false);
  const [isSubmittingComment, setIsSubmittingComment] = useState(false);
  const [isSubmittingAssign, setIsSubmittingAssign] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Форма смены статуса (для менеджера)
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [statusComment, setStatusComment] = useState<string>('');

  // Форма добавления комментария
  const [newCommentText, setNewCommentText] = useState<string>('');
  const [commentError, setCommentError] = useState<string | null>(null);

  const isManager = user?.role === 'Manager';

  const loadData = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      setError(null);

      // Загружаем список заявок и фильтруем по ID на клиенте (т.к. бэкенд не предоставляет GetById)
      const isApplicant = user?.role === 'Applicant';
      const apps = await applicationService.getList({ my: isApplicant });
      const currentApp = apps.find((a) => a.id === id);

      if (!currentApp) {
        setError('Заявка не найдена или у вас нет прав на её просмотр.');
        setIsLoading(false);
        return;
      }

      setApplication(currentApp);
      setSelectedStatus(currentApp.status);

      // Загружаем комментарии
      const commentsData = await applicationService.getComments(id);
      setComments(commentsData);
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Не удалось загрузить данные заявки.');
    } finally {
      setIsLoading(false);
    }
  }, [id, user]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Смена ответственного
  const handleAssignChange = async (managerId: string) => {
    if (!id || !application) return;
    try {
      setIsSubmittingAssign(true);
      setError(null);
      await applicationService.assign(id, managerId);
      
      // Обновляем данные
      await loadData();
      setSuccessMessage('Ответственный менеджер успешно назначен.');
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Не удалось назначить менеджера.');
    } finally {
      setIsSubmittingAssign(false);
    }
  };

  // Смена статуса
  const handleStatusSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !application || !selectedStatus) return;

    // Валидация перехода в финальный статус без ответственного
    if (
      ['Approved', 'Rejected', 'Completed'].includes(selectedStatus) &&
      !application.assignedToId
    ) {
      setError('Нельзя перевести заявку в финальный статус без назначенного ответственного менеджера.');
      return;
    }

    try {
      setIsSubmittingStatus(true);
      setError(null);

      // 1. Меняем статус
      await applicationService.updateStatus(id, selectedStatus);

      // 2. Отправляем комментарий, если он заполнен (пустой комментарий не отправляем)
      if (statusComment.trim()) {
        await applicationService.addComment(id, statusComment.trim());
        setStatusComment('');
      }

      // Обновляем данные
      await loadData();
      setSuccessMessage('Статус заявки успешно обновлен.');
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Не удалось обновить статус заявки.');
    } finally {
      setIsSubmittingStatus(false);
    }
  };

  // Добавление комментария (отдельная форма)
  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    if (!newCommentText.trim()) {
      setCommentError('Комментарий не может быть пустым.');
      return;
    }

    try {
      setIsSubmittingComment(true);
      setCommentError(null);
      setError(null);

      await applicationService.addComment(id, newCommentText.trim());
      setNewCommentText('');

      // Перезагружаем комментарии
      const commentsData = await applicationService.getComments(id);
      setComments(commentsData);
      
      setSuccessMessage('Комментарий успешно добавлен.');
    } catch (err: any) {
      setCommentError(err?.response?.data?.message || 'Не удалось отправить комментарий.');
    } finally {
      setIsSubmittingComment(false);
    }
  };

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return dateStr;
    }
  };

  if (isLoading) {
    return (
      <div className="flex flex-col gap-md animate-pulse">
        <div className="h-6 w-32 bg-gray-200 rounded" />
        <div className="flex gap-lg flex-col lg:flex-row">
          <div className="flex-1 h-96 bg-gray-200 rounded-md" />
          <div className="w-full lg:w-[350px] h-96 bg-gray-200 rounded-md" />
        </div>
      </div>
    );
  }

  if (error && !application) {
    return (
      <div className="flex flex-col gap-md">
        <Link to="/applications" className="flex items-center text-primary font-medium hover:underline w-fit">
          <ChevronLeft size={16} className="mr-xs" />
          Назад к списку
        </Link>
        <Alert variant="error">
          {error}
        </Alert>
      </div>
    );
  }

  if (!application) return null;

  // Опции статусов для Dropdown
  const statusOptions = Object.entries(STATUS_LABELS).map(([value, label]) => ({
    value,
    label,
  }));

  return (
    <div className="h-auto lg:h-full flex flex-col gap-md lg:overflow-hidden w-full">
      {/* Кнопка назад */}
      <div className="flex items-center justify-between flex-wrap gap-sm">
        <Link to="/applications" className="flex items-center text-primary font-medium hover:underline text-text-l">
          <ChevronLeft size={18} className="mr-xs" />
          Назад к списку
        </Link>
      </div>

      {/* Ошибки страницы */}
      {error && (
        <Alert variant="error" onClose={() => setError(null)} className="mb-md">
          {error}
        </Alert>
      )}

      {/* Уведомления об успешных операциях */}
      {successMessage && (
        <Alert variant="success" onClose={() => setSuccessMessage(null)} autoClose={4000} className="mb-md">
          {successMessage}
        </Alert>
      )}

      {/* Основной сетка-макет */}
      <div className="flex-1 flex flex-col lg:flex-row gap-lg items-stretch lg:overflow-hidden min-h-0 w-full">
        
        {/* ЛЕВАЯ КОЛОНКА: Основные детали заявки */}
        <div className="flex-1 min-w-0 bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-lg w-full lg:overflow-y-auto lg:h-full">
          <div>
            <div className="flex items-center justify-between flex-wrap gap-md">
              <span className={`px-sm py-xs rounded-base text-text-m font-bold border ${STATUS_STYLES[application.status] || ''}`}>
                {STATUS_LABELS[application.status] || application.status}
              </span>
              <span className="text-text-m text-text-secondary">
                Создана: {formatDate(application.createdAt)}
              </span>
            </div>
            <h1 className="text-h2 font-bold text-text-primary mt-sm break-words">
              {application.title}
            </h1>
            <div className="mt-md h-[1px] bg-border/60" />
          </div>

          {/* Параметры */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-md text-text-l text-left">
            <div>
              <p className="text-text-m text-text-secondary font-medium">Автор</p>
              <p className="text-text-primary font-semibold truncate" title={application.authorName}>
                {application.authorName}
              </p>
            </div>

            <div>
              <p className="text-text-m text-text-secondary font-medium">Направление обучения</p>
              <p className="text-text-primary font-semibold truncate" title={application.directionName}>
                {application.directionName}
              </p>
            </div>

            <div>
              <p className="text-text-m text-text-secondary font-medium">Формат обучения</p>
              <p className="text-text-primary font-semibold">
                {application.formatName}
              </p>
            </div>

            <div className="w-full min-w-0">
              <p className="text-text-m text-text-secondary font-medium">Ответственный менеджер</p>
              {isManager ? (
                !application.assignedToId ? (
                  <Button
                    variant="primary"
                    onClick={() => {
                      if (!isSubmittingAssign && user?.id) {
                        handleAssignChange(user.id);
                      }
                    }}
                    disabled={isSubmittingAssign}
                    className="mt-xs !min-w-0 w-full md:w-auto px-md !h-[38px] text-text-l"
                  >
                    {isSubmittingAssign ? 'Назначение...' : 'Взять в работу'}
                  </Button>
                ) : application.assignedToId === user?.id ? (
                  <p className="text-primary font-bold text-text-l mt-xs">
                    Вы
                  </p>
                ) : (
                  <p className="text-text-primary font-semibold truncate font-medium mt-xs" title={application.assignedToName || ''}>
                    {application.assignedToName}
                  </p>
                )
              ) : (
                <p className="text-text-primary font-semibold truncate font-medium mt-xs">
                  {application.assignedToName || 'Не назначен'}
                </p>
              )}
            </div>
          </div>

          <div className="h-[1px] bg-border/60" />

          {/* Описание */}
          <div className="flex flex-col gap-xs text-left">
            <h3 className="text-text-l font-bold text-text-primary">Описание заявки</h3>
            <p className="text-text-l text-text-secondary whitespace-pre-wrap leading-relaxed break-words bg-gray-50/50 p-md rounded border border-border/40">
              {application.description || 'Описание отсутствует.'}
            </p>
          </div>
        </div>

        {/* ПРАВАЯ КОЛОНКА: Смена статуса (только для Менеджеров) и Комментарии */}
        <div className="w-full lg:w-[380px] flex flex-col gap-lg flex-shrink-0 lg:h-full lg:min-h-0">
          
          {/* Смена статуса (только для роли Manager) */}
          {isManager && (
            <form onSubmit={handleStatusSubmit} className="bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md w-full">
              <h3 className="text-text-l font-bold text-text-primary pb-xs border-b border-border/60">
                Управление статусом
              </h3>

              <Dropdown
                label="Новый статус"
                options={statusOptions}
                value={selectedStatus}
                onChange={(val) => setSelectedStatus(val)}
              />

              <TextArea
                label="Комментарий к изменению статуса"
                placeholder="Напишите причину смены статуса (например: Уточнили детали, берем в работу)..."
                value={statusComment}
                onChange={(e) => setStatusComment(e.target.value)}
                rows={3}
                disabled={isSubmittingStatus}
              />

              <Button
                type="submit"
                variant="primary"
                disabled={isSubmittingStatus || selectedStatus === application.status}
                className="w-full"
              >
                {isSubmittingStatus ? 'Обновление...' : 'Изменить статус'}
              </Button>
            </form>
          )}

          {/* История и комментарии */}
          <div className="bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md w-full lg:flex-1 lg:min-h-0">
            <h3 className="text-text-l font-bold text-text-primary pb-xs border-b border-border/60 flex-shrink-0 text-left">
              Комментарии ({comments.length})
            </h3>

            {/* Лента комментариев */}
            <div className="max-h-[300px] lg:max-h-none lg:flex-1 lg:overflow-y-auto flex flex-col gap-sm pr-xs min-h-0">
              {comments.length === 0 ? (
                <p className="text-text-m text-text-secondary py-md text-center">
                  К этой заявке пока нет комментариев.
                </p>
              ) : (
                comments.map((comment) => (
                  <div key={comment.id} className="p-sm bg-gray-50 border border-border/40 rounded flex flex-col gap-xxs text-left">
                    <div className="flex justify-between items-center text-text-m font-semibold text-text-primary">
                      <span className="truncate max-w-[150px]">{comment.authorName}</span>
                      <span className="text-[10px] text-text-secondary font-normal font-mono">
                        {formatDate(comment.createdAt)}
                      </span>
                    </div>
                    <p className="text-text-l text-text-secondary leading-normal break-words mt-xxs">
                      {comment.text}
                    </p>
                  </div>
                ))
              )}
            </div>

            <div className="h-[1px] bg-border/60 mt-xs" />

            {/* Добавление комментария */}
            <form onSubmit={handleAddComment} className="flex flex-col gap-sm flex-shrink-0">
              {commentError && (
                <Alert variant="error" onClose={() => setCommentError(null)} className="mb-sm">
                  {commentError}
                </Alert>
              )}

              <TextArea
                placeholder="Напишите комментарий..."
                value={newCommentText}
                onChange={(e) => {
                  setNewCommentText(e.target.value);
                  if (commentError) setCommentError(null);
                }}
                rows={2}
                disabled={isSubmittingComment}
                required
              />

              <Button
                type="submit"
                variant="secondary"
                disabled={isSubmittingComment || !newCommentText.trim()}
                className="w-full text-text-l px-md !h-[38px]"
              >
                {isSubmittingComment ? 'Отправка...' : 'Отправить комментарий'}
              </Button>
            </form>
          </div>
        </div>
      </div>

    </div>
  );
};
