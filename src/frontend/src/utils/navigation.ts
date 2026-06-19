import React from 'react';
import { File, FilePlus, Users, BookOpen, BarChart2 } from 'lucide-react';
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
  '/applications/new': 'Подача заявки',
  '/dictionaries':  'Справочники',
  '/users':         'Пользователи',
  '/stats':         'Статистика',
};

export const NAV_BY_ROLE: Record<string, NavItem[]> = {
  Applicant: [
    { icon: React.createElement(File, { size: ICON_SIZE.base }), label: 'Мои заявки', href: '/applications' },
    { icon: React.createElement(FilePlus, { size: ICON_SIZE.base }), label: 'Подать заявку', href: '/applications/new' },
  ],
  Manager: [
    { icon: React.createElement(File, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
  ],
  Admin: [
    { icon: React.createElement(File, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
    { icon: React.createElement(BookOpen, { size: ICON_SIZE.base }), label: 'Справочники', href: '/dictionaries' },
    { icon: React.createElement(Users, { size: ICON_SIZE.base }), label: 'Пользователи', href: '/users' },
  ],
  Director: [
    { icon: React.createElement(File, { size: ICON_SIZE.base }), label: 'Заявки', href: '/applications' },
    { icon: React.createElement(BarChart2, { size: ICON_SIZE.base }), label: 'Статистика', href: '/stats' },
    { icon: React.createElement(Users, { size: ICON_SIZE.base }), label: 'Пользователи', href: '/users' },
  ],
};
