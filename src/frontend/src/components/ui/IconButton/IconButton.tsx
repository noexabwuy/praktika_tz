import React from 'react';
import type { IconButtonProps } from './IconButton.types';

export const IconButton: React.FC<IconButtonProps> = ({
  variant = 'default',
  icon,
  className = '',
  disabled = false,
  tooltip,
  ariaLabel,
  ...props
}) => {
  const baseClasses =
    'w-[42px] h-[42px] flex items-center justify-center transition-all duration-200 border rounded-sm disabled:opacity-60 disabled:cursor-not-allowed';

  const variantClasses: Record<string, string> = {
    default:
      'bg-bg-card border-border text-text-secondary hover:bg-primary-light hover:border-primary hover:text-primary active:bg-primary active:border-primary active:text-white active:shadow-[inset_0_2px_6px_rgba(0,0,0,0.5)]',
    primary:
      'bg-primary border-primary text-white hover:bg-primary-hover hover:border-primary-hover active:bg-primary-hover active:border-primary-hover active:shadow-[inset_0_2px_6px_rgba(0,0,0,0.5)]',
    outline:
      'bg-transparent border-primary text-primary hover:bg-primary-light hover:border-primary-hover hover:text-primary-hover active:bg-primary active:border-primary active:text-white active:shadow-[inset_0_2px_6px_rgba(0,0,0,0.4)]',
    hover:
      'bg-primary-light border-primary text-primary hover:bg-primary hover:border-primary hover:text-white active:bg-primary-hover active:border-primary-hover active:text-white active:shadow-[inset_0_2px_6px_rgba(0,0,0,0.5)]',
  };

  const defaultIcon = <span className="text-2xl font-mono">×</span>;

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant] || variantClasses.default} ${className}`}
      disabled={disabled}
      title={tooltip}
      aria-label={ariaLabel}
      {...props}
    >
      {icon || defaultIcon}
    </button>
  );
};