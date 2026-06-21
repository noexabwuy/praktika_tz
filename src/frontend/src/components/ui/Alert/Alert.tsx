import React, { useEffect } from 'react';
import { X, Check, AlertCircle, AlertTriangle, Info } from 'lucide-react';
import type { AlertProps } from './Alert.types';

export const Alert: React.FC<AlertProps> = ({
  variant = 'error',
  children,
  onClose,
  autoClose = true,
  className = '',
}) => {
  const styles = {
    success: 'bg-primary-light border-status-success/30 text-status-success',
    error:   'bg-red-50 border-status-error/30 text-status-error',
    warning: 'bg-status-warning/10 border-status-warning/20 text-status-warning',
    info:    'bg-status-info/10 border-status-info/20 text-status-info',
  };

  const icons = {
    success: <Check size={18} strokeWidth={3} className="text-status-success mr-xs flex-shrink-0" />,
    error:   <AlertCircle size={18} className="text-status-error mr-xs flex-shrink-0" />,
    warning: <AlertTriangle size={18} className="text-status-warning mr-xs flex-shrink-0" />,
    info:    <Info size={18} className="text-status-info mr-xs flex-shrink-0" />,
  };

  useEffect(() => {
    if (autoClose && onClose) {
      const duration = typeof autoClose === 'number' ? autoClose : 5000;
      const timer = setTimeout(() => {
        onClose();
      }, duration);
      return () => clearTimeout(timer);
    }
  }, [autoClose, onClose]);

  return (
    <div
      className={`p-md border rounded-base text-text-l flex items-center gap-xs animate-scaleIn w-full ${styles[variant]} ${className}`}
      role="alert"
    >
      {icons[variant]}
      <div className="flex-1 text-left min-w-0 break-words">
        {children}
      </div>
      {onClose && (
        <button
          type="button"
          onClick={onClose}
          className="ml-auto hover:opacity-75 transition-opacity flex items-center justify-center p-xxs focus:outline-none"
          aria-label="Закрыть"
        >
          <X size={18} />
        </button>
      )}
    </div>
  );
};
