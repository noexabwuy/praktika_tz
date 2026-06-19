import React from 'react';

export type NavItem = {
  icon: React.ReactNode;
  label: string;
  href?: string;
  active?: boolean;
};
