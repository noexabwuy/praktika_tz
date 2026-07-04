import React from 'react';

export interface IconToggleProps {
  isActive: boolean;
  activeIcon: React.ReactNode;
  inactiveIcon: React.ReactNode;
  className?: string;
}

export const IconToggle: React.FC<IconToggleProps> = ({
  isActive,
  activeIcon,
  inactiveIcon,
  className = '',
}) => {
  return (
    <div className={`flex items-center justify-center relative ${className}`}>
      <div 
        className={`transition-all duration-300 transform ${
          isActive ? 'opacity-100 scale-100 rotate-0' : 'opacity-0 scale-75 rotate-90 absolute'
        }`}
      >
        {activeIcon}
      </div>
      <div 
        className={`transition-all duration-300 transform ${
          !isActive ? 'opacity-100 scale-100 rotate-0' : 'opacity-0 scale-75 -rotate-90 absolute'
        }`}
      >
        {inactiveIcon}
      </div>
    </div>
  );
};
