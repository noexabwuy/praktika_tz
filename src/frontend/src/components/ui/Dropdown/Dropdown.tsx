import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import type { DropdownProps } from './Dropdown.types';
import { IconToggle } from '../IconToggle';

export const Dropdown: React.FC<DropdownProps> = ({
  label,
  options,
  value,
  onChange,
  placeholder = 'Select option',
  error,
  className = '',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((opt) => opt.value === value);

  // Закрытие при клике вне компонента
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className={`flex flex-col gap-xs w-full ${className}`} ref={containerRef}>
      {label && (
        <label className="text-text-m font-medium text-text-primary text-left">
          {label}
        </label>
      )}

      <div className="relative w-full">
        <button
          type="button"
          onClick={() => setIsOpen(!isOpen)}
          className={`
            flex items-center justify-between
            w-full h-[42px] px-sm
            bg-bg-card transition-all duration-200 outline-none border relative
            ${isOpen 
              ? 'border-primary bg-primary-light z-[60] shadow-xl rounded-t-base rounded-b-none' 
              : 'border-border rounded-base'
            }
            ${error ? 'border-status-error' : ''}
          `}
        >
          <span className={`text-text-l ${!selectedOption ? 'text-text-secondary' : 'text-text-primary'}`}>
            {selectedOption ? selectedOption.label : placeholder}
          </span>
          <IconToggle 
            isActive={isOpen}
            activeIcon={<ChevronUp size={20} />}
            inactiveIcon={<ChevronDown size={20} />}
            className="text-text-primary"
          />
        </button>

        {isOpen && (
          <div className="absolute top-full left-0 w-full z-[50] -mt-[1px]
                          bg-bg-card border border-primary border-t-0 rounded-b-base shadow-xl 
                          overflow-hidden">
            <div className="max-h-[126px] overflow-y-auto">
              {options.map((option, index) => (
                <React.Fragment key={option.value}>
                  <button
                    type="button"
                    onClick={() => {
                      onChange(option.value);
                      setIsOpen(false);
                    }}
                    className="w-full h-[42px] px-sm flex items-center 
                               text-text-l text-text-primary text-left
                               hover:bg-primary-light transition-colors"
                  >
                    {option.label}
                  </button>
                  {index < options.length - 1 && (
                    <div className="h-[1px] mx-sm bg-primary" />
                  )}
                </React.Fragment>
              ))}
            </div>
          </div>
        )}
      </div>

      {error && (
        <span className="text-text-m text-status-error">
          {error}
        </span>
      )}
    </div>
  );
};
