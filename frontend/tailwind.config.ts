import type { Config } from 'tailwindcss';

const config: Config = {
  content: ["./app/**/*.{js,ts,jsx,tsx}", "./components/**/*.{js,ts,jsx,tsx}", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: {
          900: '#070b16',
          800: '#0c1224',
          700: '#111a31',
        },
        accent: {
          50: '#ecfdf5',
          100: '#d1fae5',
          500: '#10b981',
          600: '#059669',
        },
      },
      boxShadow: {
        glow: '0 0 0 1px rgba(16, 185, 129, 0.12), 0 16px 48px rgba(2, 6, 23, 0.35)',
      },
    },
  },
  plugins: [],
};

export default config;
