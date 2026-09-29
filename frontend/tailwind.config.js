/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          light: '#A5B4FC',
          DEFAULT: '#4F46E5', // Indigo-600 (WCAG 2.1 AA compliant 6.3:1)
          dark: '#3730A3',
        },
        accent: {
          light: '#FDE68A',
          DEFAULT: '#F59E0B', // Amber-500
          dark: '#D97706',
        },
        success: {
          DEFAULT: '#10B981', // Emerald-500
        },
        danger: {
          DEFAULT: '#EF4444', // Red-500
        },
        surface: {
          50: '#FAFAFA',
          100: '#F5F5F5',
          200: '#E5E5E5',
          800: '#262626',
          900: '#171717',
          950: '#0A0A0A',
        },
      },
      fontFamily: {
        sans: ['Inter', 'Noto Sans Thai', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
      },
    },
  },
  plugins: [],
}
