import React, { useState, useEffect } from 'react';
import {
  makeStyles,
  shorthands,
  tokens,
  Button,
  Card,
  CardHeader,
  Text,
  Spinner,
  Badge,
} from '@fluentui/react-components';
import {
  PaymentRegular,
  ArrowUploadRegular,
  SettingsRegular,
} from '@fluentui/react-icons';
import { subscriptionService } from '../services/subscriptionService.ts';
import type { Subscription, Plan } from '../types/index.ts';

const useStyles = makeStyles({
  container: {
    ...shorthands.padding('32px'),
    maxWidth: '1200px',
    ...shorthands.margin('0', 'auto'),
  },
  header: {
    marginBottom: '32px',
  },
  title: {
    fontSize: '32px',
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '8px',
  },
  subtitle: {
    fontSize: '16px',
    color: tokens.colorNeutralForeground2,
  },
  section: {
    marginBottom: '32px',
  },
  sectionTitle: {
    fontSize: '20px',
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '16px',
  },
  currentPlanCard: {
    marginBottom: '24px',
  },
  planDetails: {
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
  },
  planRow: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  plansGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
    gap: '16px',
  },
  planCard: {
    cursor: 'pointer',
    transition: 'transform 0.2s, box-shadow 0.2s',
    ':hover': {
      transform: 'translateY(-4px)',
      boxShadow: tokens.shadow16,
    },
  },
  planHeader: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
    marginBottom: '16px',
  },
  planName: {
    fontSize: '24px',
    fontWeight: tokens.fontWeightSemibold,
  },
  planPrice: {
    fontSize: '20px',
    color: tokens.colorBrandForeground1,
  },
  planDescription: {
    color: tokens.colorNeutralForeground2,
    marginBottom: '16px',
  },
  planFeatures: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
    marginBottom: '16px',
  },
  buttonGroup: {
    display: 'flex',
    gap: '12px',
    marginTop: '16px',
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    minHeight: '400px',
  },
  errorText: {
    color: tokens.colorPaletteRedForeground1,
  },
});

export const BillingPage: React.FC = () => {
  const styles = useStyles();
  const [loading, setLoading] = useState(true);
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [error, setError] = useState<string | null>(null);
  
  // Mock user email - in real app, get from auth context
  const userEmail = 'user@example.com';

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [subscriptionData, plansData] = await Promise.all([
          subscriptionService.getCurrentSubscription(userEmail),
          subscriptionService.getPlans(),
        ]);
        setSubscription(subscriptionData);
        setPlans(plansData);
        setError(null);
      } catch (err) {
        setError('Failed to load billing information');
        console.error('Error loading billing data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [userEmail]);

  const handleUpgrade = async (tier: string) => {
    try {
      const successUrl = `${window.location.origin}/dashboard?tab=billing&success=true`;
      const cancelUrl = `${window.location.origin}/dashboard?tab=billing`;
      
      const sessionUrl = await subscriptionService.createCheckoutSession(
        userEmail,
        tier,
        'Monthly',
        successUrl,
        cancelUrl
      );
      
      window.location.href = sessionUrl;
    } catch (err) {
      console.error('Error creating checkout session:', err);
      setError('Failed to start upgrade process');
    }
  };

  const handleManageBilling = async () => {
    try {
      const returnUrl = `${window.location.origin}/dashboard?tab=billing`;
      const portalUrl = await subscriptionService.createBillingPortalSession(userEmail, returnUrl);
      window.location.href = portalUrl;
    } catch (err) {
      console.error('Error opening billing portal:', err);
      setError('Failed to open billing portal');
    }
  };

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  };

  if (loading) {
    return (
      <div className={styles.loadingContainer}>
        <Spinner label="Loading billing information..." size="large" />
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Text className={styles.title}>Billing & Subscription</Text>
        <Text className={styles.subtitle}>
          Manage your subscription and billing details
        </Text>
      </div>

      {error && (
        <Text className={styles.errorText}>{error}</Text>
      )}

      {subscription && (
        <div className={styles.section}>
          <Text className={styles.sectionTitle}>Current Plan</Text>
          <Card className={styles.currentPlanCard}>
            <CardHeader
              image={<PaymentRegular fontSize={24} />}
              header={<Text weight="semibold">{subscription.tier} Plan</Text>}
              description={
                <Badge
                  appearance="filled"
                  color={subscription.status === 'Active' ? 'success' : 'warning'}
                >
                  {subscription.status}
                </Badge>
              }
            />
            <div className={styles.planDetails}>
              <div className={styles.planRow}>
                <Text>Billing Interval:</Text>
                <Text weight="semibold">{subscription.billingInterval}</Text>
              </div>
              <div className={styles.planRow}>
                <Text>Price:</Text>
                <Text weight="semibold">
                  ${subscription.priceAmount.toFixed(2)}/{subscription.billingInterval === 'Monthly' ? 'mo' : 'yr'}
                </Text>
              </div>
              <div className={styles.planRow}>
                <Text>Websites Limit:</Text>
                <Text weight="semibold">{subscription.limits.maxWebsites}</Text>
              </div>
              <div className={styles.planRow}>
                <Text>Storage Limit:</Text>
                <Text weight="semibold">{formatBytes(subscription.limits.maxStorageBytes)}</Text>
              </div>
              <div className={styles.planRow}>
                <Text>Bandwidth Limit:</Text>
                <Text weight="semibold">{formatBytes(subscription.limits.maxBandwidthBytes)}</Text>
              </div>
              {subscription.tier !== 'Free' && (
                <>
                  <div className={styles.planRow}>
                    <Text>Current Period:</Text>
                    <Text weight="semibold">
                      {new Date(subscription.currentPeriodStart).toLocaleDateString()} - {new Date(subscription.currentPeriodEnd).toLocaleDateString()}
                    </Text>
                  </div>
                  {subscription.cancelAtPeriodEnd && (
                    <Text className={styles.errorText}>
                      Your subscription will be canceled at the end of the current period.
                    </Text>
                  )}
                </>
              )}
            </div>
            {subscription.tier !== 'Free' && (
              <div className={styles.buttonGroup}>
                <Button
                  appearance="primary"
                  icon={<SettingsRegular />}
                  onClick={handleManageBilling}
                >
                  Manage Billing
                </Button>
              </div>
            )}
          </Card>
        </div>
      )}

      <div className={styles.section}>
        <Text className={styles.sectionTitle}>Available Plans</Text>
        <div className={styles.plansGrid}>
          {plans.map((plan) => (
            <Card
              key={plan.tier}
              className={styles.planCard}
            >
              <div className={styles.planHeader}>
                <Text className={styles.planName}>{plan.name}</Text>
                <Text className={styles.planPrice}>
                  ${plan.monthlyPrice}/mo
                </Text>
                <Text size={200}>or ${plan.yearlyPrice}/yr (save 17%)</Text>
              </div>
              <Text className={styles.planDescription}>{plan.description}</Text>
              <div className={styles.planFeatures}>
                <Text size={200}>✓ {plan.limits.maxWebsites} websites</Text>
                <Text size={200}>✓ {formatBytes(plan.limits.maxStorageBytes)} storage</Text>
                <Text size={200}>✓ {formatBytes(plan.limits.maxBandwidthBytes)} bandwidth</Text>
                <Text size={200}>
                  {plan.limits.hasAds ? '✗' : '✓'} {plan.limits.hasAds ? 'Has ads' : 'No ads'}
                </Text>
              </div>
              {subscription?.tier !== plan.tier && plan.tier !== 'Free' && (
                <Button
                  appearance="primary"
                  icon={<ArrowUploadRegular />}
                  onClick={() => handleUpgrade(plan.tier)}
                  style={{ width: '100%' }}
                >
                  {subscription && getTierLevel(subscription.tier) < getTierLevel(plan.tier)
                    ? 'Upgrade'
                    : 'Switch to'} {plan.name}
                </Button>
              )}
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
};

const getTierLevel = (tier: string): number => {
  const levels: Record<string, number> = {
    Free: 0,
    Starter: 1,
    Pro: 2,
    Business: 3,
  };
  return levels[tier] || 0;
};
