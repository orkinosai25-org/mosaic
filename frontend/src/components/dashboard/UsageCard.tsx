import React from 'react';
import {
  Card,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';
import type { UsageMetrics } from '../../types';

const useStyles = makeStyles({
  usageCard: {
    ...shorthands.padding('24px'),
  },
  title: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '16px',
    color: tokens.colorBrandForeground1,
  },
  metricsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
    ...shorthands.gap('16px'),
  },
  metricItem: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('4px'),
  },
  metricLabel: {
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground3,
  },
  metricValue: {
    fontSize: tokens.fontSizeBase600,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorBrandForeground1,
  },
});

interface UsageCardProps {
  metrics: UsageMetrics;
}

export const UsageCard: React.FC<UsageCardProps> = ({ metrics }) => {
  const styles = useStyles();

  return (
    <Card className={styles.usageCard}>
      <h3 className={styles.title}>Usage Summary</h3>
      <div className={styles.metricsGrid}>
        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Active Sites</span>
          <span className={styles.metricValue}>{metrics.sites}</span>
        </div>
        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Storage Used</span>
          <span className={styles.metricValue}>{metrics.storage} GB</span>
        </div>
        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Bandwidth</span>
          <span className={styles.metricValue}>{metrics.bandwidth} GB</span>
        </div>
        <div className={styles.metricItem}>
          <span className={styles.metricLabel}>Monthly Visitors</span>
          <span className={styles.metricValue}>{metrics.visitors.toLocaleString()}</span>
        </div>
      </div>
    </Card>
  );
};
