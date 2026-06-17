import React, { InputHTMLAttributes } from 'react';

export interface BaseInputProps extends InputHTMLAttributes<HTMLInputElement | HTMLTextAreaElement> {
  label?: string;
  error?: string;
  helperText?: string;
}

export interface TextInputProps extends BaseInputProps {
  rightIcon?: React.ReactNode;
}
