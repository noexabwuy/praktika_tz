import React, { useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import type { ApplicationResponse } from '../services/applicationService';
import { Button } from './ui/Button';

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

interface ApplicationsTableProps {
  applications: ApplicationResponse[];
  isManager: boolean;
  currentUserId?: string;
  onAssignManager: (appId: string, value: string) => void;
}

export const ApplicationsTable: React.FC<ApplicationsTableProps> = ({
  applications,
  isManager,
  currentUserId,
  onAssignManager,
}) => {
  const navigate = useNavigate();

  const headerRef = useRef<HTMLDivElement>(null);
  const bodyRef = useRef<HTMLDivElement>(null);

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
    if (headerRef.current) {
      headerRef.current.scrollLeft = e.currentTarget.scrollLeft;
    }
  };

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      });
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="flex flex-col flex-1 overflow-hidden w-full">
      {/* Шапка таблицы — скроллится только по горизонтали синхронно с телом */}
      <div 
        ref={headerRef} 
        className="overflow-hidden w-full bg-[#f3f4f6] border-b border-border"
        style={{ scrollbarGutter: 'stable' }}
      >
        <table className="w-full text-left border-collapse table-fixed min-w-[900px]">
          <thead>
            <tr className="border-b border-border h-[52px]">
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[90px]">ID</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[110px]">Дата</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary min-w-[150px]">Тема</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[180px]">ФИО</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[220px]">Направление</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[140px] text-center">Статус</th>
              <th className="px-lg py-md text-text-l font-bold text-text-primary w-[220px] text-center">Ответственный</th>
            </tr>
          </thead>
        </table>
      </div>

      {/* Тело таблицы — скроллится во все стороны, вертикальный скроллбар визуально только здесь */}
      <div 
        ref={bodyRef} 
        onScroll={handleScroll}
        className="overflow-auto w-full flex-1"
        style={{ scrollbarGutter: 'stable' }}
      >
        <table className="w-full text-left border-collapse table-fixed min-w-[900px]">
          {/* Невидимая шапка для фиксации ширины колонок */}
          <thead className="invisible">
            <tr className="h-0 border-0">
              <th className="p-0 h-0 w-[90px]"></th>
              <th className="p-0 h-0 w-[110px]"></th>
              <th className="p-0 h-0 min-w-[150px]"></th>
              <th className="p-0 h-0 w-[180px]"></th>
              <th className="p-0 h-0 w-[220px]"></th>
              <th className="p-0 h-0 w-[140px]"></th>
              <th className="p-0 h-0 w-[220px]"></th>
            </tr>
          </thead>
          <tbody>
            {applications.map((app) => (
              <tr
                key={app.id}
                onClick={() => navigate(`/applications/${app.id}`)}
                className="group border-b border-border/60 last:border-0 hover:bg-primary-light transition-colors cursor-pointer h-[52px]"
              >
                <td className="px-lg py-md text-text-l font-mono text-text-secondary text-left w-[90px] truncate" title={app.id}>
                  {app.id.substring(0, 8)}
                </td>
                <td className="px-lg py-md text-text-l text-text-secondary whitespace-nowrap w-[110px]">
                  {formatDate(app.createdAt)}
                </td>
                <td className="px-lg py-md text-text-l">
                  <div className="w-full truncate font-medium text-primary group-hover:underline" title={app.title}>
                    {app.title}
                  </div>
                </td>
                <td className="px-lg py-md text-text-l text-text-primary">
                  <div className="w-full truncate" title={app.authorName}>
                    {app.authorName}
                  </div>
                </td>
                <td className="px-lg py-md text-text-l text-text-primary">
                  <div className="w-full truncate" title={app.directionName}>
                    {app.directionName}
                  </div>
                </td>
                <td className="px-lg py-md w-[140px]" onClick={(e) => e.stopPropagation()}>
                  <span
                    className={`flex w-full items-center justify-center px-sm py-xs rounded-base text-text-m font-semibold border ${
                      STATUS_STYLES[app.status] || 'bg-bg-page text-text-secondary border-border'
                    }`}
                  >
                    {STATUS_LABELS[app.status] || app.status}
                  </span>
                </td>
                <td className="px-lg py-0 w-[220px]" onClick={(e) => e.stopPropagation()}>
                  <div className="flex justify-center items-center w-full h-full">
                    {isManager ? (
                      !app.assignedToId ? (
                        <Button
                          variant="primary"
                          onClick={() => onAssignManager(app.id, currentUserId || '')}
                          className="!min-w-0 !w-full px-sm whitespace-nowrap"
                        >
                          Взять в работу
                        </Button>
                      ) : app.assignedToId === currentUserId ? (
                        <span className="text-text-l font-semibold text-primary">
                          Вы
                        </span>
                      ) : (
                        <div className="w-full truncate text-text-l text-text-primary text-center" title={app.assignedToName || ''}>
                          {app.assignedToName}
                        </div>
                      )
                    ) : (
                      <div className="w-full truncate text-text-l text-text-primary text-center" title={app.assignedToName || 'Не назначен'}>
                        {app.assignedToName || 'Не назначен'}
                      </div>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
