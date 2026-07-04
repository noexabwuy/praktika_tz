import { UserRound } from 'lucide-react';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ICON_SIZE } from '../utils/iconSize';
import { PAGE_TITLES, ROLE_LABELS } from '../utils/navigation';

const UserSkeleton: React.FC = () => (
  <div className="flex items-center gap-sm animate-pulse">
    <div className="flex flex-col items-end gap-sm">
      <div className="skeleton h-[14px] w-[110px]" />
      <div className="skeleton h-[11px] w-[72px]" />
    </div>
    <div className="skeleton rounded-full ml-xs" style={{ width: ICON_SIZE.avatar, height: ICON_SIZE.avatar }} />
  </div>
);

const TitleSkeleton: React.FC = () => (
  <div className="skeleton h-[22px] w-[120px] ml-md" />
);
export const Header: React.FC = () => {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  const pageTitle = PAGE_TITLES[location.pathname] ?? null;

  return (
    <header className="flex-shrink-0 h-[60px] w-full bg-bg-card border-b-2 border-primary flex items-center justify-between px-lg z-10 relative">
      <div className="flex items-center gap-md">
        <div className="w-8 h-8 rounded-base bg-primary flex-shrink-0" />

        {isLoading ? (
          <TitleSkeleton />
        ) : (
          pageTitle && (
            <h1 className="text-h2 font-semibold text-text-primary m-0 leading-none">
              {pageTitle}
            </h1>
          )
        )}
      </div>
      <div className="flex items-center gap-sm">
        {isLoading ? (
          <UserSkeleton />
        ) : user ? (
          <>
            <div className="flex flex-col justify-center items-end h-full">
              <span className="text-text-l font-bold text-text-primary leading-none mb-xs">
                {user.fullName}
              </span>
              <span className="text-text-m font-bold text-text-secondary leading-none">
                {ROLE_LABELS[user.role] ?? user.role}
              </span>
            </div>
            <div className="flex items-center justify-center text-text-primary ml-xs">
              <UserRound size={ICON_SIZE.avatar} strokeWidth={2} />
            </div>
          </>
        ) : (
          <>
            <div className="flex flex-col justify-center items-end h-full">
              <span className="text-text-l font-bold text-text-primary leading-none mb-xs">
                Гость
              </span>
              <span className="text-text-m font-bold text-text-secondary leading-none">
                Не авторизован
              </span>
            </div>
            <div className="flex items-center justify-center text-text-primary ml-xs">
              <UserRound size={ICON_SIZE.avatar} strokeWidth={1.5} />
            </div>
          </>
        )}
      </div>

    </header>
  );
};
