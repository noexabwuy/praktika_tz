import React, { forwardRef, useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';
import type { TextInputProps } from './Input.types';
import { TextInput } from './TextInput';
import { IconToggle } from '../IconToggle';

export const PasswordInput = forwardRef<HTMLInputElement, TextInputProps>(
  (props, ref) => {
    const [showPassword, setShowPassword] = useState(false);

    const togglePasswordVisibility = () => {
      setShowPassword((prev) => !prev);
    };

    return (
      <TextInput
        {...props}
        ref={ref}
        type={showPassword ? 'text' : 'password'}
        rightIcon={
          <button
            type="button"
            onClick={togglePasswordVisibility}
            className="flex items-center justify-center focus:outline-none hover:text-primary transition-colors"
            tabIndex={-1}
          >
            <IconToggle 
              isActive={showPassword}
              activeIcon={<EyeOff size={20} strokeWidth={1.5} />}
              inactiveIcon={<Eye size={20} strokeWidth={1.5} />}
            />
          </button>
        }
      />
    );
  }
);

PasswordInput.displayName = 'PasswordInput';
