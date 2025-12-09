import React from 'react';
import {
  Card,
  Button,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';
import {
  AddRegular,
  ChartMultipleRegular,
  DocumentRegular,
  SettingsRegular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  quickActionsCard: {
    ...shorthands.padding('24px'),
  },
  title: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '16px',
    color: tokens.colorBrandForeground1,
  },
  actionsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    ...shorthands.gap('12px'),
  },
  actionButton: {
    justifyContent: 'flex-start',
    height: '48px',
  },
});

interface QuickActionsProps {
  onAction: (action: string) => void;
}

export const QuickActions: React.FC<QuickActionsProps> = ({ onAction }) => {
  const styles = useStyles();

  return (
    <Card className={styles.quickActionsCard}>
      <h3 className={styles.title}>Quick Actions</h3>
      <div className={styles.actionsGrid}>
        <Button
          appearance="outline"
          icon={<AddRegular />}
          className={styles.actionButton}
          onClick={() => onAction('create-site')}
        >
          Create New Site
        </Button>
        <Button
          appearance="outline"
          icon={<ChartMultipleRegular />}
          className={styles.actionButton}
          onClick={() => onAction('analytics')}
        >
          View Analytics
        </Button>
        <Button
          appearance="outline"
          icon={<DocumentRegular />}
          className={styles.actionButton}
          onClick={() => onAction('billing')}
        >
          Manage Billing
        </Button>
        <Button
          appearance="outline"
          icon={<SettingsRegular />}
          className={styles.actionButton}
          onClick={() => onAction('settings')}
        >
          Site Settings
        </Button>
      </div>
    </Card>
  );
};
