import { createLightTheme, createDarkTheme, type Theme } from '@fluentui/react-components';

// Professional color palette
export const mosaicColors = {
  // Primary - Brand Blue
  brandBlue: '#1e3a8a',
  brandBlueLight: '#2563eb',
  
  // Secondary - Turquoise
  turquoise: '#06b6d4',
  turquoiseLight: '#22d3ee',
  turquoiseDark: '#0891b2',
  
  // Accent - Gold
  gold: '#fbbf24',
  goldDark: '#f59e0b',
  
  // Neutrals
  pureWhite: '#ffffff',
  darkSlate: '#0f172a',
  slate700: '#334155',
  slate800: '#1e293b',
  slate900: '#0f172a',
};

// Light theme with professional colors
export const mosaicLightTheme: Theme = createLightTheme({
  10: mosaicColors.brandBlue,
  20: mosaicColors.brandBlue,
  30: mosaicColors.brandBlue,
  40: mosaicColors.brandBlue,
  50: mosaicColors.brandBlue,
  60: mosaicColors.brandBlueLight,
  70: mosaicColors.brandBlueLight,
  80: mosaicColors.brandBlueLight,
  90: mosaicColors.brandBlueLight,
  100: mosaicColors.brandBlueLight,
  110: mosaicColors.brandBlueLight,
  120: mosaicColors.brandBlueLight,
  130: mosaicColors.brandBlueLight,
  140: mosaicColors.brandBlueLight,
  150: mosaicColors.brandBlueLight,
  160: mosaicColors.brandBlueLight,
});

// Dark theme with professional colors
export const mosaicDarkTheme: Theme = createDarkTheme({
  10: mosaicColors.turquoise,
  20: mosaicColors.turquoise,
  30: mosaicColors.turquoise,
  40: mosaicColors.turquoise,
  50: mosaicColors.turquoise,
  60: mosaicColors.turquoiseLight,
  70: mosaicColors.turquoiseLight,
  80: mosaicColors.turquoiseLight,
  90: mosaicColors.turquoiseLight,
  100: mosaicColors.turquoiseLight,
  110: mosaicColors.turquoiseLight,
  120: mosaicColors.turquoiseLight,
  130: mosaicColors.turquoiseLight,
  140: mosaicColors.turquoiseLight,
  150: mosaicColors.turquoiseLight,
  160: mosaicColors.turquoiseLight,
});
