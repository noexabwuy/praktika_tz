import { useLocation } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import type { NavItem } from '../types/navigation';
import { useAuth } from '../context/AuthContext';
import { NAV_BY_ROLE } from '../utils/navigation';

interface MainLayoutProps {
  children: React.ReactNode;
}
function withActive(items: NavItem[], pathname: string): NavItem[] {
  const hasExactMatch = items.some((item) => item.href === pathname);
  return items.map((item) => ({
    ...item,
    active: hasExactMatch
      ? item.href === pathname
      : item.href === pathname || (pathname !== '/' && item.href !== '/' && pathname.startsWith(item.href ?? '')),
  }));
}

export const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const { user } = useAuth();
  const { pathname } = useLocation();

  const baseItems = (user ? NAV_BY_ROLE[user.role] : null) ?? NAV_BY_ROLE.Applicant;
  const navigationItems = withActive(baseItems, pathname);

  return (
    <div className="flex flex-col h-screen w-full bg-bg-page overflow-hidden">
      <Header />
      <div className="flex flex-1 overflow-hidden relative">
        <Sidebar items={navigationItems} />
        <main className="flex-1 overflow-y-auto p-lg">
          {children}
        </main>
      </div>
    </div>
  );
};
