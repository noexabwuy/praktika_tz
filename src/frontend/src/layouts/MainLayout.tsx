import { Home, List, Users, BookOpen, BarChart2 } from 'lucide-react';
import { useLocation } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import type { NavItem } from './Sidebar';
import { useAuth } from '../context/AuthContext';
import { ICON_SIZE } from '../utils/iconSize';

interface MainLayoutProps {
  children: React.ReactNode;
}
const NAV_BY_ROLE: Record<string, NavItem[]> = {
  Applicant: [
    { icon: <Home size={ICON_SIZE.base} />, label: 'Главная', href: '/' },
    { icon: <List size={ICON_SIZE.base} />, label: 'Мои заявки', href: '/applications' },
  ],
  Manager: [
    { icon: <Home size={ICON_SIZE.base} />, label: 'Главная', href: '/' },
    { icon: <List size={ICON_SIZE.base} />, label: 'Заявки', href: '/applications' },
  ],
  Admin: [
    { icon: <Home size={ICON_SIZE.base} />, label: 'Главная', href: '/' },
    { icon: <List size={ICON_SIZE.base} />, label: 'Заявки', href: '/applications' },
    { icon: <BookOpen size={ICON_SIZE.base} />, label: 'Справочники', href: '/dictionaries' },
    { icon: <Users size={ICON_SIZE.base} />, label: 'Пользователи', href: '/users' },
  ],
  Director: [
    { icon: <Home size={ICON_SIZE.base} />, label: 'Главная', href: '/' },
    { icon: <List size={ICON_SIZE.base} />, label: 'Заявки', href: '/applications' },
    { icon: <BarChart2 size={ICON_SIZE.base} />, label: 'Статистика', href: '/stats' },
    { icon: <Users size={ICON_SIZE.base} />, label: 'Пользователи', href: '/users' },
  ],
};
function withActive(items: NavItem[], pathname: string): NavItem[] {
  return items.map((item) => ({
    ...item,
    active: item.href === pathname || (pathname !== '/' && item.href !== '/' && pathname.startsWith(item.href ?? '')),
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
