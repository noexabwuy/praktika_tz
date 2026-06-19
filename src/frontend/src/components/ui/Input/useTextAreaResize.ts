import { useEffect, useRef } from 'react';

export const useTextAreaResize = (value: string | number | readonly string[] | undefined) => {
  const textAreaRef = useRef<HTMLTextAreaElement>(null);

  const resize = () => {
    const textArea = textAreaRef.current;
    if (textArea) {
      // Принудительно сбрасываем высоту в 'auto', чтобы браузер пересчитал scrollHeight
      textArea.style.height = 'auto';
      // Устанавливаем новую высоту на основе scrollHeight
      textArea.style.height = `${textArea.scrollHeight}px`;
    }
  };

  useEffect(() => {
    resize();
  }, [value]);

  return { textAreaRef, resize };
};
