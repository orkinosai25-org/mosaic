import React from 'react';
import {
  Card,
  Button,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';

const useStyles = makeStyles({
  landing: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('32px'),
    ...shorthands.padding('48px', '24px'),
    maxWidth: '1200px',
    margin: '0 auto',
  },
  hero: {
    textAlign: 'center',
    ...shorthands.padding('48px', '24px'),
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRadius('8px'),
  },
  heroTitle: {
    fontSize: '48px',
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '16px',
    color: tokens.colorBrandForeground1,
  },
  heroSubtitle: {
    fontSize: tokens.fontSizeBase500,
    color: tokens.colorNeutralForeground2,
    marginBottom: '32px',
  },
  heroButtons: {
    display: 'flex',
    ...shorthands.gap('16px'),
    justifyContent: 'center',
  },
  featuresGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
    ...shorthands.gap('24px'),
  },
  featureCard: {
    ...shorthands.padding('24px'),
    textAlign: 'center',
  },
  featureIcon: {
    fontSize: '48px',
    marginBottom: '16px',
  },
  featureTitle: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '12px',
    color: tokens.colorBrandForeground1,
  },
  featureDescription: {
    fontSize: tokens.fontSizeBase300,
    color: tokens.colorNeutralForeground2,
  },
});

interface LandingPageProps {
  onRegister: () => void;
  onLogin: () => void;
}

export const LandingPage: React.FC<LandingPageProps> = ({
  onRegister,
  onLogin,
}) => {
  const styles = useStyles();

  const features = [
    {
      icon: '🗣️',
      title: 'Conversational Website Builder',
      description:
        'Build and manage websites using natural language conversations—no technical configuration needed.',
    },
    {
      icon: '🏢',
      title: 'Multi-Tenant Architecture',
      description:
        'Create and manage unlimited websites with enterprise-grade isolation and security.',
    },
    {
      icon: '⚡',
      title: 'Instant Onboarding & Migration',
      description:
        'Get started in minutes with streamlined sign-up and easy migration from existing platforms.',
    },
    {
      icon: '🌐',
      title: 'Ready for Business',
      description:
        'Enterprise-ready features with flexible billing, real-time analytics, and comprehensive APIs.',
    },
    {
      icon: '🤖',
      title: 'AI-Powered Automation',
      description:
        'Intelligent assistance for content creation, SEO optimization, and administrative tasks.',
    },
    {
      icon: '🔌',
      title: 'API Integration',
      description:
        'Full RESTful API access with webhooks, OAuth 2.0, and comprehensive SDKs.',
    },
  ];

  return (
    <div className={styles.landing}>
      <div className={styles.hero}>
        <h1 className={styles.heroTitle}>Welcome to MOSAIC</h1>
        <p className={styles.heroSubtitle}>
          The world's first conversational SaaS platform empowering businesses and creators to build,
          manage, and scale multi-tenant websites—using natural language, not configuration
        </p>
        <div className={styles.heroButtons}>
          <Button appearance="primary" size="large" onClick={onRegister}>
            Get Started Free
          </Button>
          <Button appearance="outline" size="large" onClick={onLogin}>
            Sign In
          </Button>
        </div>
      </div>

      <div className={styles.featuresGrid}>
        {features.map((feature, index) => (
          <Card key={index} className={styles.featureCard}>
            <div className={styles.featureIcon}>{feature.icon}</div>
            <h3 className={styles.featureTitle}>{feature.title}</h3>
            <p className={styles.featureDescription}>{feature.description}</p>
          </Card>
        ))}
      </div>
    </div>
  );
};
