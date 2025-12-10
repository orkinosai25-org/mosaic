import React from 'react';
import {
  makeStyles,
  shorthands,
  tokens,
  Button,
} from '@fluentui/react-components';
import {
  HomeRegular,
  BuildingRegular,
  PaymentRegular,
  QuestionCircleRegular,
  SettingsRegular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  nav: {
    width: '240px',
    height: '100%',
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRight('1px', 'solid', tokens.colorNeutralStroke2),
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.padding('16px', '8px'),
  },
  navItem: {
    width: '100%',
    justifyContent: 'flex-start',
    marginBottom: '4px',
  },
  navItemActive: {
    backgroundColor: tokens.colorBrandBackground2,
  },
});

interface NavigationProps {
  activeKey: string;
  onNavigate: (key: string) => void;
}

export const Navigation: React.FC<NavigationProps> = ({
  activeKey,
  onNavigate,
}) => {
  const styles = useStyles();

  const navItems = [
    { key: 'dashboard', text: 'Dashboard', icon: <HomeRegular /> },
    { key: 'sites', text: 'Sites / Workspaces', icon: <BuildingRegular /> },
    { key: 'billing', text: 'Billing / Subscription', icon: <PaymentRegular /> },
    { key: 'support', text: 'Support / Help', icon: <QuestionCircleRegular /> },
    { key: 'settings', text: 'Settings', icon: <SettingsRegular /> },
  ];

  return (
    <nav className={styles.nav}>
      {navItems.map((item) => (
        <Button
          key={item.key}
          appearance="subtle"
          icon={item.icon}
          className={`${styles.navItem} ${
            activeKey === item.key ? styles.navItemActive : ''
          }`}
          onClick={() => onNavigate(item.key)}
        >
          {item.text}
        </Button>
      ))}
    </nav>
  );
};
