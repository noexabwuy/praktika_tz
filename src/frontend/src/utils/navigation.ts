import React from 'react';
import { Home, List, Users, BookOpen, BarChart2 } from 'lucide-react';
import { ICON_SIZE } from './iconSize';
import type { NavItem } from '../types/navigation';

export const ROLE_LABELS: Record<string, string> = {
  Applicant: 'Заявитель',
  Manager:   'Менеджер',
  Admin:     'Администратор',
  Director:  'Руководитель',
};

export const PAGE_TITLES: Record<string, string> = {
  '/':              'Главная',
  '/applications':  'Заявки',
  '/dictionaries':  'Справочники',
  '/users':         'Пользователи',
  '/stats':         'Статистика',
};

export const NAV_BY_ROLE: Record<string, NavItem[]> = {
  Applicant: [
    { icon: React.createElement(Home, { size: ICON_SIZE.base }), label: 'Главная', href: '/' },
    { icon: React.createElement(List, { size: ICON_SIZE.base }), label: 'Мои заявки', href: '/applications' },
  ],
  Manager: [
    { icon: React.createElement(Home, { size: ICON_SIZE.base }), label: 'Главная', href: '/' },
    { icon: React.createElement(List, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
  ],
  Admin: [
    { icon: React.createElement(Home, { size: ICON_SIZE.base }), label: 'Главная', href: '/' },
    { icon: React.createElement(List, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
    { icon: React.createElement(BookOpen, { size: ICON_SIZE.base }), label: 'Справочники', href: '/dictionaries' },
    { icon: React.createElement(Users, { size: ICON_SIZE.base }), label: 'Пользователи', href: '/users' },
  ],
  Director: [
    { icon: React.createElement(Home, { size: ICON_SIZE.base }), label: 'Главная', href: '/' },
    { icon: React.createElement(List, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
    { icon: React.createElement(BarChart2, { size: ICON_SIZE.base }), label: 'Статистика', href: '/stats' },
    { icon: React.createElement(Users, { size: ICON_SIZE.base }), label: 'Пользователи', href: '/users' },
  ],
};
