import { forwardRef } from 'react';
import type { TextInputProps } from './Input.types';

export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
  ({ label, error, helperText, rightIcon, className = '', ...props }, ref) => {
    return (
      <div className="flex flex-col gap-xs w-full">
        {label && (
          <label className="text-text-m font-medium text-text-primary text-left">
            {label}
          </label>
        )}
        
        <div className="relative flex items-center w-full">
          
          <input
            ref={ref}
            className={`
              w-full h-[42px] px-sm
              bg-bg-card border border-border rounded-base
              text-text-l text-text-primary placeholder:text-text-secondary
              transition-all duration-200 outline-none
              focus:bg-primary-light focus:border-primary
              ${rightIcon ? 'pr-[32px]' : ''}
              ${error ? 'border-status-error' : ''}
              ${className}
            `}
            {...props}
          />

          {rightIcon && (
            <div className="absolute right-sm text-text-primary flex items-center justify-center">
              {rightIcon}
            </div>
          )}
        </div>

        {(error || helperText) && (
          <span className={`text-text-m ${error ? 'text-status-error' : 'text-text-secondary'}`}>
            {error || helperText}
          </span>
        )}
      </div>
    );
  }
);

TextInput.displayName = 'TextInput';
