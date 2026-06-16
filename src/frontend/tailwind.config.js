/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#10b981',
          hover: '#087A54',
          light: '#ecfdf5',
        },
        status: {
          new: '#ECFDF5',
          success: '#10b981',
          info: '#3b82f6',
          warning: '#f59e0b',
          error: '#ef4444',
          finish: '#087A54',
        },
        bg: {
          page: '#e5e7eb',
          card: '#FFFFFF',
        },
        border: {
          DEFAULT: '#d1d5db',
          dark: '#787C82',
        },
        text: {
          primary: '#111827',
          secondary: '#4B5563',
          light: '#E5E7EA',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        'h1': ['32px', { lineHeight: '1.2' }],
        'h2': ['24px', { lineHeight: '1.2' }],
        'h3': ['20px', { lineHeight: '1.2' }],
        'text-l': ['16px', { lineHeight: '1.5' }],
        'text-m': ['12px', { lineHeight: '1.5' }],
      },
      borderRadius: {
        'xs': '2px',
        'sm': '4px',
        'base': '6px',
        'none': '0px',
      },
      spacing: {
        'xs': '4px',
        'sm': '6px',
        'md': '12px',
        'lg': '24px',
        'xl': '48px',
      },
    },
  },
  plugins: [],
}

