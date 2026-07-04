import React, { useState } from 'react';
import { Users, Clock, CheckCircle2, ClipboardList } from 'lucide-react';
import { useAnalyticsData } from '../hooks/useAnalyticsData';
import { Alert } from '../components/ui/Alert';

export const StatsPage: React.FC = () => {
  const { stats, dynamics, period, setPeriod, isLoading, error } = useAnalyticsData();
  const [hoveredPoint, setHoveredPoint] = useState<number | null>(null);
  const [tooltipPos, setTooltipPos] = useState<{ x: number; y: number }>({ x: 0, y: 0 });

  const handleMouseMove = (e: React.MouseEvent<SVGSVGElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    setTooltipPos({
      x: e.clientX - rect.left,
      y: e.clientY - rect.top,
    });
  };

  if (isLoading) {
    return (
      <div className="flex flex-col gap-md animate-pulse">
        {/* Шапка скелетона */}
        <div className="h-6 w-32 bg-gray-200 rounded" />
        {/* Карточки скелетона */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-md">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="h-24 bg-gray-200 rounded-md" />
          ))}
        </div>
        {/* Графики скелетона */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-md flex-1">
          <div className="lg:col-span-2 h-96 bg-gray-200 rounded-md" />
          <div className="h-96 bg-gray-200 rounded-md" />
        </div>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="flex flex-col gap-md">
        <Alert variant="error">
          {error || 'Не удалось загрузить данные статистики.'}
        </Alert>
      </div>
    );
  }

  // Расчет дополнительных метрик
  const newAppsCount = stats.byStatuses['New'] || 0;
  const completedAppsCount = stats.byStatuses['Completed'] || 0;

  // Подготовка координат для линейного графика (dynamics)
  const chartWidth = 500;
  const chartHeight = 200;
  const paddingX = 40;
  const paddingY = 20;

  const maxVal = Math.max(...dynamics.map((d) => d.value), 5);

  const points = dynamics.map((d, index) => {
    const x = paddingX + (index * (chartWidth - 2 * paddingX)) / (dynamics.length - 1);
    const y = chartHeight - paddingY - (d.value * (chartHeight - 2 * paddingY)) / maxVal;
    return { x, y, label: d.label, value: d.value };
  });

  // Строка пути для линии графика
  const pathD = points.length > 0 
    ? points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ')
    : '';

  // Строка пути для заливки области под графиком
  const areaD = points.length > 0
    ? `${pathD} L ${points[points.length - 1].x} ${chartHeight - paddingY} L ${points[0].x} ${chartHeight - paddingY} Z`
    : '';

  // Сортировка направлений по популярности
  const sortedDirections = Object.entries(stats.byDirections)
    .sort((a, b) => b[1] - a[1]);

  return (
    <div className="w-full flex flex-col gap-lg text-left pb-xl">
      
      {/* ── МЕТРИКИ (КАРТОЧКИ) ── */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-md w-full">
        {/* Карточка 1: Всего заявок */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-md flex items-center justify-between">
          <div>
            <p className="text-text-m text-text-secondary font-medium">Всего заявок</p>
            <h3 className="text-h1 font-bold text-text-primary mt-xs">{stats.totalApplications}</h3>
          </div>
          <div className="text-text-secondary mr-xs flex items-center justify-center">
            <ClipboardList size={24} />
          </div>
        </div>

        {/* Карточка 2: Пользователей */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-md flex items-center justify-between">
          <div>
            <p className="text-text-m text-text-secondary font-medium">Пользователей</p>
            <h3 className="text-h1 font-bold text-text-primary mt-xs">{stats.totalUsers}</h3>
          </div>
          <div className="text-text-secondary mr-xs flex items-center justify-center">
            <Users size={24} />
          </div>
        </div>

        {/* Карточка 3: Новые заявки */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-md flex items-center justify-between">
          <div>
            <p className="text-text-m text-text-secondary font-medium">Новые заявки</p>
            <h3 className="text-h1 font-bold text-text-primary mt-xs">{newAppsCount}</h3>
          </div>
          <div className="text-text-secondary mr-xs flex items-center justify-center">
            <Clock size={24} />
          </div>
        </div>

        {/* Карточка 4: Завершено */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-md flex items-center justify-between">
          <div>
            <p className="text-text-m text-text-secondary font-medium">Завершено</p>
            <h3 className="text-h1 font-bold text-text-primary mt-xs">{completedAppsCount}</h3>
          </div>
          <div className="text-text-secondary mr-xs flex items-center justify-center">
            <CheckCircle2 size={24} />
          </div>
        </div>
      </div>

      {/* ── ОСНОВНЫЕ ГРАФИКИ ── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-lg w-full items-stretch">
        
        {/* ЛЕВАЯ ЧАСТЬ: Линейный график динамики */}
        <div className="lg:col-span-2 bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md">
          <div className="flex items-center justify-between flex-wrap gap-sm">
            <div className="flex items-center gap-xs">
              <h3 className="text-text-l font-bold text-text-primary">Динамика заявок</h3>
            </div>
            
            {/* Переключатель периодов */}
            <div className="flex bg-gray-100 p-xxs rounded border border-border/60">
              {(['day', 'week', 'month'] as const).map((p) => (
                <button
                  key={p}
                  onClick={() => setPeriod(p)}
                  className={`px-sm py-xs text-text-m font-semibold rounded-sm transition-all focus:outline-none ${
                    period === p
                      ? 'bg-white text-primary shadow-sm'
                      : 'text-text-secondary hover:text-text-primary'
                  }`}
                >
                  {p === 'day' ? 'По дням' : p === 'week' ? 'По неделям' : 'По месяцам'}
                </button>
              ))}
            </div>
          </div>

          <div className="relative flex-1 flex items-center justify-center min-h-[220px]">
            {dynamics.length === 0 ? (
              <p className="text-text-m text-text-secondary">Нет данных о динамике отправки.</p>
            ) : (
              <div className="w-full h-full flex flex-col justify-between relative">
                <svg 
                  viewBox={`0 0 ${chartWidth} ${chartHeight}`} 
                  className="w-full overflow-visible"
                  onMouseMove={handleMouseMove}
                >
                  <defs>
                    <linearGradient id="chartGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#10b981" stopOpacity="0.25" />
                      <stop offset="100%" stopColor="#10b981" stopOpacity="0.0" />
                    </linearGradient>
                  </defs>

                  {/* Горизонтальные сетки */}
                  {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
                    const y = paddingY + ratio * (chartHeight - 2 * paddingY);
                    const val = Math.round(maxVal - ratio * maxVal);
                    return (
                      <g key={ratio} className="opacity-40">
                        <line 
                          x1={paddingX} 
                          y1={y} 
                          x2={chartWidth - paddingX} 
                          y2={y} 
                          stroke="#e5e7eb" 
                          strokeWidth={1} 
                          strokeDasharray="4,4"
                        />
                        <text 
                          x={paddingX - 10} 
                          y={y + 4} 
                          className="text-[9px] fill-text-secondary font-mono text-right"
                          textAnchor="end"
                        >
                          {val}
                        </text>
                      </g>
                    );
                  })}

                  {/* Заливка под графиком */}
                  {areaD && <path d={areaD} fill="url(#chartGrad)" />}

                  {/* Сама линия графика */}
                  {pathD && (
                    <path 
                      d={pathD} 
                      fill="none" 
                      className="stroke-primary" 
                      strokeWidth={3} 
                      strokeLinecap="round" 
                      strokeLinejoin="round" 
                    />
                  )}

                  {/* Вертикальная направляющая при наведении */}
                  {hoveredPoint !== null && points[hoveredPoint] && (
                    <line 
                      x1={points[hoveredPoint].x}
                      y1={paddingY}
                      x2={points[hoveredPoint].x}
                      y2={chartHeight - paddingY}
                      className="stroke-primary"
                      strokeWidth={1.5}
                      strokeDasharray="2,2"
                    />
                  )}

                  {/* Точки на графике и зоны наведения */}
                  {points.map((p, idx) => (
                    <g key={idx}>
                      {/* Точка (кружок) */}
                      <circle 
                        cx={p.x} 
                        cy={p.y} 
                        r={hoveredPoint === idx ? 5 : 3.5} 
                        fill={hoveredPoint === idx ? '#10b981' : '#ffffff'} 
                        strokeWidth={2}
                        className="stroke-primary transition-all duration-150"
                      />
                      
                      {/* Невидимый интерактивный круг для наведения */}
                      <circle 
                        cx={p.x} 
                        cy={p.y} 
                        r={20} 
                        fill="transparent" 
                        className="cursor-pointer"
                        onMouseEnter={() => setHoveredPoint(idx)}
                        onMouseLeave={() => setHoveredPoint(null)}
                      />
                    </g>
                  ))}

                  {/* Подписи оси X */}
                  {points.map((p, idx) => (
                    <text 
                      key={idx}
                      x={p.x} 
                      y={chartHeight - 4} 
                      className="text-[9px] fill-text-secondary text-center font-medium"
                      textAnchor="middle"
                    >
                      {p.label}
                    </text>
                  ))}
                </svg>

                {/* Всплывающий тултип над курсором */}
                {hoveredPoint !== null && points[hoveredPoint] && (
                  <div 
                    className="absolute bg-text-primary text-white text-text-m font-semibold py-xs px-sm rounded-sm shadow-md animate-scaleIn pointer-events-none z-10"
                    style={{
                      left: `${tooltipPos.x}px`,
                      top: `${tooltipPos.y - 35}px`,
                      transform: 'translateX(-50%)',
                    }}
                  >
                    {points[hoveredPoint].value} заяв.
                  </div>
                )}
              </div>
            )}
          </div>
        </div>

        {/* ПРАВАЯ ЧАСТЬ: Популярные направления */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md">
          <h3 className="text-text-l font-bold text-text-primary pb-xs border-b border-border/60">
            Популярные направления
          </h3>
          <div className="flex flex-col gap-md flex-1">
            {sortedDirections.length === 0 ? (
              <p className="text-text-m text-text-secondary text-center my-auto">Нет данных по направлениям.</p>
            ) : (
              sortedDirections.map(([name, count]) => {
                const percentage = stats.totalApplications > 0
                  ? Math.round((count / stats.totalApplications) * 100)
                  : 0;

                return (
                  <div key={name} className="flex flex-col gap-xxs">
                    <div className="flex justify-between items-center text-text-m font-semibold text-text-primary">
                      <span className="truncate max-w-[200px]" title={name}>{name}</span>
                      <span>{count} ({percentage}%)</span>
                    </div>
                    {/* График полосы */}
                    <div className="w-full bg-gray-100 h-2.5 rounded-full overflow-hidden">
                      <div 
                        className="bg-primary h-full rounded-full transition-all duration-1000"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

      {/* ── РАСПРЕДЕЛЕНИЕ ПО ФОРМАТАМ И СТАТУСАМ (НИЖНЯЯ ЧАСТЬ) ── */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-lg w-full">
        {/* Форматы обучения */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md">
          <h3 className="text-text-l font-bold text-text-primary pb-xs border-b border-border/60">
            Форматы обучения
          </h3>
          <div className="flex flex-col gap-md">
            {Object.keys(stats.byFormats).length === 0 ? (
              <p className="text-text-m text-text-secondary text-center py-sm">Нет данных по форматам.</p>
            ) : (
              Object.entries(stats.byFormats).map(([name, count]) => {
                const percentage = stats.totalApplications > 0
                  ? Math.round((count / stats.totalApplications) * 100)
                  : 0;

                return (
                  <div key={name} className="flex flex-col gap-xxs">
                    <div className="flex justify-between items-center text-text-m font-semibold text-text-primary">
                      <span>{name}</span>
                      <span>{count} ({percentage}%)</span>
                    </div>
                    <div className="w-full bg-gray-100 h-2.5 rounded-full overflow-hidden">
                      <div 
                        className="bg-blue-500 h-full rounded-full transition-all duration-1000"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* Статусы заявок */}
        <div className="bg-bg-card border border-border rounded-md shadow-sm p-lg flex flex-col gap-md">
          <h3 className="text-text-l font-bold text-text-primary pb-xs border-b border-border/60">
            Статусы заявок
          </h3>
          <div className="flex flex-col gap-md">
            {Object.keys(stats.byStatuses).length === 0 ? (
              <p className="text-text-m text-text-secondary text-center py-sm">Нет данных по статусам.</p>
            ) : (
              Object.entries(stats.byStatuses).map(([name, count]) => {
                const percentage = stats.totalApplications > 0
                  ? Math.round((count / stats.totalApplications) * 100)
                  : 0;

                // Перевод статусов для красивого отображения
                const statusLabels: Record<string, string> = {
                  New: 'Новая',
                  InProgress: 'В работе',
                  NeedsInfo: 'Требуется уточнение',
                  Approved: 'Согласована',
                  Rejected: 'Отклонена',
                  Completed: 'Завершена',
                };
                
                const statusColors: Record<string, string> = {
                  New: 'bg-border-dark',
                  InProgress: 'bg-status-info',
                  NeedsInfo: 'bg-status-warning',
                  Approved: 'bg-status-success',
                  Rejected: 'bg-status-error',
                  Completed: 'bg-status-finish',
                };

                const label = statusLabels[name] || name;
                const colorClass = statusColors[name] || 'bg-primary';

                return (
                  <div key={name} className="flex flex-col gap-xxs">
                    <div className="flex justify-between items-center text-text-m font-semibold text-text-primary">
                      <span>{label}</span>
                      <span>{count} ({percentage}%)</span>
                    </div>
                    <div className="w-full bg-gray-100 h-2.5 rounded-full overflow-hidden">
                      <div 
                        className={`h-full rounded-full transition-all duration-1000 ${colorClass}`}
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

    </div>
  );
};
