import React from 'react';

export interface CheckboxProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
}

export const Checkbox: React.FC<CheckboxProps> = ({
  checked = false,
  onChange,
  disabled = false,
}) => {
  const handleClick = () => {
    if (disabled) return;
    if (onChange) {
      onChange(!checked);
    }
  };

  return (
    <div
      className={`inline-flex items-center justify-center w-4 h-4 border-2 rounded-[2px] transition-all duration-200 cursor-pointer ${
        disabled ? 'opacity-50 cursor-not-allowed' : ''
      } ${
        checked
          ? 'bg-primary border-primary'
          : 'bg-bg-card border-border hover:bg-primary-light hover:border-primary'
      }`}
      onClick={handleClick}
    >
      {checked && (
        <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
        </svg>
      )}
    </div>
  );
};