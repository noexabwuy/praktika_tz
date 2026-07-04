import React from 'react';
import type { CheckboxProps } from './Checkbox.types';

export const Checkbox: React.FC<CheckboxProps> = ({
  checked = false,
  onChange,
  disabled = false,
}) => {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (disabled) return;
    onChange?.(e.target.checked);
  };

  return (
    <label className="inline-flex items-center cursor-pointer">
      <input
        type="checkbox"
        checked={checked}
        onChange={handleChange}
        disabled={disabled}
        className="w-4 h-4 border-2 rounded-sm transition-all duration-200 bg-bg-card border-border text-primary focus:ring-primary focus:ring-2 focus:ring-offset-0 disabled:opacity-50 disabled:cursor-not-allowed"
      />
    </label>
  );
};