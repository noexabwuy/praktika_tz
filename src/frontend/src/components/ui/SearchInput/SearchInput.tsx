import React from 'react';
import { Search } from 'lucide-react';
import type { SearchInputProps } from './SearchInput.types';

export const SearchInput: React.FC<SearchInputProps> = ({
  placeholder = 'Введите запрос...',
  className = '',
  ...props
}) => {
  return (
    <div className="relative w-[600px] h-[42px] group">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-[18px] h-[18px] text-text-secondary group-hover:text-primary pointer-events-none transition-colors" />
      <input
        type="text"
        placeholder={placeholder}
        className={`w-full h-full pl-10 pr-4 border border-border bg-bg-card text-text-primary placeholder:text-text-secondary outline-none transition-all rounded-base group-hover:border-primary group-hover:bg-primary-light focus:border-primary focus:bg-primary-light focus:ring-2 focus:ring-primary focus:ring-opacity-20 ${className}`}
        {...props}
      />
    </div>
  );
};