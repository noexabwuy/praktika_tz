import React, { useState, useCallback } from 'react';
import { LogOut, Pin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { IconToggle } from '../components/ui/IconToggle';
import { useAuth } from '../context/AuthContext';
import { ICON_SIZE } from '../utils/iconSize';
import type { NavItem } from '../types/navigation';

interface SidebarProps {
  items: NavItem[];
}

const SIDEBAR_PIN_KEY = 'sidebar_pinned';

export const Sidebar: React.FC<SidebarProps> = ({ items }) => {
  const [isPinned, setIsPinned] = useState<boolean>(
    () => localStorage.getItem(SIDEBAR_PIN_KEY) === 'true'
  );
  const { logout } = useAuth();

  const togglePin = useCallback(() => {
    setIsPinned((prev) => {
      const next = !prev;
      localStorage.setItem(SIDEBAR_PIN_KEY, String(next));
      return next;
    });
  }, []);

  return (
    <aside 
      className={`
        flex-shrink-0 bg-bg-card border-r-2 border-primary transition-all duration-300 z-20 overflow-hidden flex flex-col group py-md relative
        ${isPinned ? 'w-[240px]' : 'w-[48px] hover:w-[240px]'}
      `}
    >
      
      <nav className="flex-1 flex flex-col gap-xs relative z-0">
        <div className={`absolute top-0 right-md h-[42px] flex items-center z-10 transition-opacity duration-300 ${isPinned ? 'opacity-100' : 'opacity-0 group-hover:opacity-100'}`}>
          <button
            type="button"
            onClick={togglePin}
            className="p-xs rounded-sm hover:bg-bg-page transition-colors text-text-secondary hover:text-primary focus:outline-none"
            aria-label={isPinned ? "Unpin Sidebar" : "Pin Sidebar"}
          >
            <IconToggle 
              isActive={isPinned}
              activeIcon={<Pin size={ICON_SIZE.sm} className="fill-primary text-primary" />}
              inactiveIcon={<Pin size={ICON_SIZE.sm} />}
            />
          </button>
        </div>

        {items.map((item, index) => (
          <Link
            key={index}
            to={item.href || "#"}
            className={`
              flex items-center h-[42px] px-md transition-colors relative
              ${item.active 
                ? 'text-primary bg-primary-light' 
                : 'text-text-secondary hover:text-primary hover:bg-primary-light'
              }
            `}
          >
            {item.active && (
              <div className="absolute left-0 top-0 bottom-0 w-[3px] bg-primary rounded-r-sm" />
            )}
            
            <div className="flex-shrink-0 flex items-center justify-center">
              {item.icon}
            </div>
            
            <span className={`ml-md text-text-l font-medium transition-all duration-300 whitespace-nowrap overflow-hidden ${isPinned ? 'opacity-100 w-auto' : 'opacity-0 w-0 group-hover:opacity-100 group-hover:w-auto'}`}>
              {item.label}
            </span>
          </Link>
        ))}
      </nav>

      <div className="mt-auto">
        <button 
          type="button"
          onClick={logout}
          className="w-full flex items-center h-[42px] px-md text-text-secondary hover:text-status-error hover:bg-bg-page transition-colors relative"
        >
          <div className="flex-shrink-0 flex items-center justify-center">
            <LogOut size={ICON_SIZE.base} />
          </div>
          <span className={`ml-md text-text-l font-medium transition-all duration-300 whitespace-nowrap overflow-hidden ${isPinned ? 'opacity-100 w-auto' : 'opacity-0 w-0 group-hover:opacity-100 group-hover:w-auto'}`}>
            Выйти
          </span>
        </button>
      </div>

    </aside>
  );
};
