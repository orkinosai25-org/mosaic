import React from 'react';
import { makeStyles, shorthands } from '@fluentui/react-components';
import { ThemeSelectionPanel } from '../components/themes/ThemeSelectionPanel';

const useStyles = makeStyles({
  themesPage: {
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    ...shorthands.overflow('auto'),
  },
});

export const ThemesPage: React.FC = () => {
  const styles = useStyles();

  const handleThemeSelect = (theme: any) => {
    console.log('Theme selected:', theme);
    // Handle theme selection (e.g., update site theme, show success message, etc.)
  };

  return (
    <div className={styles.themesPage}>
      <ThemeSelectionPanel
        onThemeSelect={handleThemeSelect}
        showApplyButton={true}
      />
    </div>
  );
};
