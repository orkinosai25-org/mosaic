import React from 'react';
import {
  Card,
  Button,
  makeStyles,
  shorthands,
  tokens,
  Badge,
} from '@fluentui/react-components';
import {
  AddRegular,
  AppsRegular,
  SettingsRegular,
  ChartMultipleRegular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  sitesPage: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('24px'),
    ...shorthands.padding('24px'),
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: '8px',
  },
  title: {
    fontSize: tokens.fontSizeBase600,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorBrandForeground1,
  },
  sitesGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))',
    ...shorthands.gap('24px'),
  },
  siteCard: {
    ...shorthands.padding('24px'),
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('16px'),
  },
  siteHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  siteName: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorNeutralForeground1,
    marginBottom: '4px',
  },
  siteUrl: {
    fontSize: tokens.fontSizeBase300,
    color: tokens.colorNeutralForeground3,
  },
  siteActions: {
    display: 'flex',
    ...shorthands.gap('8px'),
    flexWrap: 'wrap',
  },
  siteInfo: {
    display: 'flex',
    ...shorthands.gap('8px'),
    flexWrap: 'wrap',
  },
});

interface SitesPageProps {
  onAction: (action: string) => void;
}

export const SitesPage: React.FC<SitesPageProps> = ({ onAction }) => {
  const styles = useStyles();

  // Mock data - replace with real API data
  const sites = [
    {
      id: '1',
      name: 'My Portfolio',
      url: 'myportfolio.mosaic.app',
      status: 'active',
      lastUpdated: '2 hours ago',
    },
    {
      id: '2',
      name: 'Business Site',
      url: 'business.mosaic.app',
      status: 'active',
      lastUpdated: '1 day ago',
    },
    {
      id: '3',
      name: 'Blog Platform',
      url: 'blog.mosaic.app',
      status: 'staging',
      lastUpdated: '3 days ago',
    },
  ];

  const handleConfigureCMS = (siteId: string) => {
    // TODO: Navigate to specific site's CMS admin
    window.open(`/cms/admin?site=${siteId}`, '_blank');
  };

  return (
    <div className={styles.sitesPage}>
      <div className={styles.header}>
        <h1 className={styles.title}>Sites / Workspaces</h1>
        <Button
          appearance="primary"
          icon={<AddRegular />}
          onClick={() => onAction('create-site')}
        >
          Create New Site
        </Button>
      </div>

      <div className={styles.sitesGrid}>
        {sites.map((site) => (
          <Card key={site.id} className={styles.siteCard}>
            <div className={styles.siteHeader}>
              <div>
                <div className={styles.siteName}>{site.name}</div>
                <div className={styles.siteUrl}>{site.url}</div>
              </div>
              <Badge
                appearance="filled"
                color={site.status === 'active' ? 'success' : 'warning'}
              >
                {site.status}
              </Badge>
            </div>

            <div className={styles.siteInfo}>
              <span style={{ fontSize: tokens.fontSizeBase200, color: tokens.colorNeutralForeground3 }}>
                Last updated: {site.lastUpdated}
              </span>
            </div>

            <div className={styles.siteActions}>
              <Button
                appearance="primary"
                icon={<AppsRegular />}
                onClick={() => handleConfigureCMS(site.id)}
              >
                Configure CMS
              </Button>
              <Button
                appearance="outline"
                icon={<ChartMultipleRegular />}
                onClick={() => onAction('analytics')}
              >
                Analytics
              </Button>
              <Button
                appearance="outline"
                icon={<SettingsRegular />}
                onClick={() => onAction('settings')}
              >
                Settings
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
};
