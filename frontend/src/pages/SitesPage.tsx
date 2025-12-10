import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  makeStyles,
  shorthands,
  tokens,
  Badge,
  Spinner,
  Text,
} from '@fluentui/react-components';
import {
  AddRegular,
  AppsRegular,
  SettingsRegular,
  ChartMultipleRegular,
} from '@fluentui/react-icons';
import { CreateSiteDialog } from '../components/sites/CreateSiteDialog';

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
  userEmail?: string;
}

export const SitesPage: React.FC<SitesPageProps> = ({ onAction, userEmail = 'user@example.com' }) => {
  const styles = useStyles();
  const [sites, setSites] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch sites from API
  useEffect(() => {
    fetchSites();
  }, [userEmail]);

  const fetchSites = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/site?userEmail=${encodeURIComponent(userEmail)}`);
      if (!response.ok) {
        throw new Error('Failed to fetch sites');
      }
      const data = await response.json();
      setSites(data);
    } catch (err: any) {
      console.error('Error fetching sites:', err);
      setError(err.message);
      // Fallback to mock data for demo
      setSites([
        {
          id: 1,
          name: 'My Portfolio',
          url: 'myportfolio',
          status: 'active',
          createdOn: new Date().toISOString(),
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSiteCreated = () => {
    // Refresh the sites list
    fetchSites();
  };

  const handleConfigureCMS = (siteId: number) => {
    window.open(`/admin?site=${siteId}`, '_blank');
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffHours < 1) return 'Just now';
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className={styles.sitesPage}>
      <div className={styles.header}>
        <h1 className={styles.title}>Sites / Workspaces</h1>
        <Button
          appearance="primary"
          icon={<AddRegular />}
          onClick={() => setIsCreateDialogOpen(true)}
        >
          Create New Site
        </Button>
      </div>

      {isLoading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
          <Spinner label="Loading sites..." />
        </div>
      ) : error && sites.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Text>No sites found. Create your first site to get started!</Text>
        </div>
      ) : (
        <div className={styles.sitesGrid}>
          {sites.map((site) => (
            <Card key={site.id} className={styles.siteCard}>
              <div className={styles.siteHeader}>
                <div>
                  <div className={styles.siteName}>{site.name}</div>
                  <div className={styles.siteUrl}>{site.url}.mosaic.app</div>
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
                  Created: {formatDate(site.createdOn)}
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
      )}

      <CreateSiteDialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        onSiteCreated={handleSiteCreated}
        userEmail={userEmail}
      />
    </div>
  );
};
