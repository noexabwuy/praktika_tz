import { forwardRef } from 'react';
import type { TextAreaProps } from './Input.types';
import { useTextAreaResize } from './useTextAreaResize';
export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(
  ({ label, error, helperText, className = '', value, onInput, ...props }, ref) => {
    const { textAreaRef, resize } = useTextAreaResize(value);

    // Объединяем рефы
    const setRefs = (node: HTMLTextAreaElement | null) => {
      if (node) {
        (textAreaRef as React.MutableRefObject<HTMLTextAreaElement | null>).current = node;
        if (typeof ref === 'function') {
          ref(node);
        } else if (ref) {
          (ref as React.MutableRefObject<HTMLTextAreaElement | null>).current = node;
        }
      }
    };

    const handleInput = (e: React.FormEvent<HTMLTextAreaElement>) => {
      resize();
      if (onInput) (onInput as React.FormEventHandler<HTMLTextAreaElement>)(e);
    };

    return (
      <div className="flex flex-col gap-xs w-full">
        {label && (
          <label className="text-text-m font-medium text-text-primary text-left">
            {label}
          </label>
        )}

        <textarea
          ref={setRefs}
          value={value}
          onInput={handleInput}
          className={`
            w-full min-h-[84px] max-h-[120px] px-sm py-[9px]
            bg-bg-card border border-border rounded-base
            text-text-l text-text-primary placeholder:text-text-secondary
            transition-all duration-75 outline-none resize-none overflow-y-auto
            focus:bg-primary-light focus:border-primary
            ${error ? 'border-status-error' : ''}
            ${className}
          `}
          {...props}
        />


        {(error || helperText) && (
          <span className={`text-text-m ${error ? 'text-status-error' : 'text-text-secondary'}`}>
            {error || helperText}
          </span>
        )}
      </div>
    );
  }
);

TextArea.displayName = 'TextArea';
