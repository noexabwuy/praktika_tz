import React from 'react';
import type { ButtonProps } from './Button.types';

export const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  children,
  className = '',
  disabled,
  ...props
}) => {
  const baseClasses =
  'h-[42px] min-w-[200px] w-full font-semibold text-base flex items-center justify-center transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed rounded-base';
  const variants = {
    primary:
      'bg-primary text-white hover:bg-primary-hover active:bg-primary-hover active:shadow-[inset_0_2px_8px_rgba(0,0,0,0.7)] disabled:bg-border px-4',
    secondary:
      'bg-bg-card text-text-primary border border-border hover:bg-border hover:border-border-dark active:bg-border active:border-border-dark active:shadow-[inset_0_2px_8px_rgba(0,0,0,0.6)] disabled:bg-border disabled:text-text-secondary px-4',
  };

  return (
    <button
      className={`${baseClasses} ${variants[variant]} ${className}`}
      disabled={disabled}
      {...props}
    >
      {children}
    </button>
  );
};