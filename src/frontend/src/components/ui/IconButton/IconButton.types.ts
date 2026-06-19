import React from "react";
export interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'primary' | 'outline' | 'hover';
  icon?: React.ReactNode;
  className?: string;
  tooltip: string;
  ariaLabel: string;
}