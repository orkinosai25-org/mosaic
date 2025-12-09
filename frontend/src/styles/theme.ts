import { createLightTheme, createDarkTheme, type Theme } from '@fluentui/react-components';

// Ottoman/Iznik inspired color palette
export const ottomanColors = {
  // Primary - Ottoman Blue
  ottomanBlue: '#1e3a8a',
  ottomanBlueLight: '#2563eb',
  
  // Secondary - Iznik Turquoise
  iznikTurquoise: '#06b6d4',
  iznikTurquoiseLight: '#22d3ee',
  iznikTurquoiseDark: '#0891b2',
  
  // Accent - Royal Gold
  royalGold: '#fbbf24',
  royalGoldDark: '#f59e0b',
  
  // Neutrals
  pureWhite: '#ffffff',
  darkSlate: '#0f172a',
  slate700: '#334155',
  slate800: '#1e293b',
  slate900: '#0f172a',
};

// Light theme with Ottoman colors
export const mosaicLightTheme: Theme = createLightTheme({
  10: ottomanColors.ottomanBlue,
  20: ottomanColors.ottomanBlue,
  30: ottomanColors.ottomanBlue,
  40: ottomanColors.ottomanBlue,
  50: ottomanColors.ottomanBlue,
  60: ottomanColors.ottomanBlueLight,
  70: ottomanColors.ottomanBlueLight,
  80: ottomanColors.ottomanBlueLight,
  90: ottomanColors.ottomanBlueLight,
  100: ottomanColors.ottomanBlueLight,
  110: ottomanColors.ottomanBlueLight,
  120: ottomanColors.ottomanBlueLight,
  130: ottomanColors.ottomanBlueLight,
  140: ottomanColors.ottomanBlueLight,
  150: ottomanColors.ottomanBlueLight,
  160: ottomanColors.ottomanBlueLight,
});

// Dark theme with Ottoman colors
export const mosaicDarkTheme: Theme = createDarkTheme({
  10: ottomanColors.iznikTurquoise,
  20: ottomanColors.iznikTurquoise,
  30: ottomanColors.iznikTurquoise,
  40: ottomanColors.iznikTurquoise,
  50: ottomanColors.iznikTurquoise,
  60: ottomanColors.iznikTurquoiseLight,
  70: ottomanColors.iznikTurquoiseLight,
  80: ottomanColors.iznikTurquoiseLight,
  90: ottomanColors.iznikTurquoiseLight,
  100: ottomanColors.iznikTurquoiseLight,
  110: ottomanColors.iznikTurquoiseLight,
  120: ottomanColors.iznikTurquoiseLight,
  130: ottomanColors.iznikTurquoiseLight,
  140: ottomanColors.iznikTurquoiseLight,
  150: ottomanColors.iznikTurquoiseLight,
  160: ottomanColors.iznikTurquoiseLight,
});
