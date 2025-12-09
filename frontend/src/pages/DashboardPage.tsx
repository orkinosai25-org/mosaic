import React from 'react';
import {
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';
import { UsageCard } from '../components/dashboard/UsageCard';
import { QuickActions } from '../components/dashboard/QuickActions';
import { MigrationBanner } from '../components/common/MigrationBanner';
import type { UsageMetrics } from '../types';

const useStyles = makeStyles({
  dashboard: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('24px'),
    ...shorthands.padding('24px'),
  },
  welcomeSection: {
    marginBottom: '8px',
  },
  welcomeTitle: {
    fontSize: tokens.fontSizeBase600,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorBrandForeground1,
    marginBottom: '8px',
  },
  welcomeSubtitle: {
    fontSize: tokens.fontSizeBase400,
    color: tokens.colorNeutralForeground2,
  },
  cardsGrid: {
    display: 'grid',
    gridTemplateColumns: '1fr',
    ...shorthands.gap('24px'),
    '@media (min-width: 768px)': {
      gridTemplateColumns: 'repeat(2, 1fr)',
    },
  },
});

interface DashboardPageProps {
  userName: string;
  onAction: (action: string) => void;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({
  userName,
  onAction,
}) => {
  const styles = useStyles();

  // Mock data
  const metrics: UsageMetrics = {
    sites: 3,
    storage: 2.4,
    bandwidth: 15.8,
    visitors: 12453,
  };

  return (
    <div className={styles.dashboard}>
      <MigrationBanner />

      <div className={styles.welcomeSection}>
        <h1 className={styles.welcomeTitle}>Welcome back, {userName}!</h1>
        <p className={styles.welcomeSubtitle}>
          Here's what's happening with your sites today
        </p>
      </div>

      <UsageCard metrics={metrics} />
      <QuickActions onAction={onAction} />
    </div>
  );
};
