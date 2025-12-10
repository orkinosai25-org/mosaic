import React from 'react';
import {
  makeStyles,
  shorthands,
  tokens,
  Link,
} from '@fluentui/react-components';

const useStyles = makeStyles({
  footer: {
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.padding('24px'),
    ...shorthands.borderTop('1px', 'solid', tokens.colorNeutralStroke2),
  },
  footerContent: {
    maxWidth: '1200px',
    margin: '0 auto',
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    ...shorthands.gap('32px'),
  },
  footerSection: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('12px'),
  },
  footerTitle: {
    fontSize: tokens.fontSizeBase400,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorNeutralForeground1,
    marginBottom: '8px',
  },
  footerLink: {
    fontSize: tokens.fontSizeBase300,
    color: tokens.colorNeutralForeground2,
    textDecoration: 'none',
    ':hover': {
      color: tokens.colorBrandForeground1,
    },
  },
  branding: {
    marginTop: '24px',
    textAlign: 'center',
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground3,
  },
  socialLinks: {
    display: 'flex',
    ...shorthands.gap('16px'),
    marginTop: '8px',
  },
});

export const Footer: React.FC = () => {
  const styles = useStyles();

  return (
    <footer className={styles.footer}>
      <div className={styles.footerContent}>
        <div className={styles.footerSection}>
          <div className={styles.footerTitle}>Product</div>
          <Link href="#" className={styles.footerLink}>
            Features
          </Link>
          <Link href="#" className={styles.footerLink}>
            Pricing
          </Link>
          <Link href="#" className={styles.footerLink}>
            API Documentation
          </Link>
        </div>

        <div className={styles.footerSection}>
          <div className={styles.footerTitle}>Resources</div>
          <Link href="#" className={styles.footerLink}>
            Documentation
          </Link>
          <Link href="#" className={styles.footerLink}>
            Quick Start Guide
          </Link>
          <Link href="#" className={styles.footerLink}>
            Video Tutorials
          </Link>
        </div>

        <div className={styles.footerSection}>
          <div className={styles.footerTitle}>Support</div>
          <Link href="#" className={styles.footerLink}>
            Help Center
          </Link>
          <Link href="#" className={styles.footerLink}>
            Contact Support
          </Link>
          <Link href="#" className={styles.footerLink}>
            Community Forum
          </Link>
        </div>

        <div className={styles.footerSection}>
          <div className={styles.footerTitle}>Company</div>
          <Link href="https://github.com/orkinosai25-org" className={styles.footerLink}>
            GitHub
          </Link>
          <Link href="#" className={styles.footerLink}>
            About Orkinosai
          </Link>
          <div className={styles.socialLinks}>
            <Link href="#" className={styles.footerLink}>
              Twitter
            </Link>
            <Link href="#" className={styles.footerLink}>
              LinkedIn
            </Link>
          </div>
        </div>
      </div>

      <div className={styles.branding}>
        <p>
          Built with ❤️ by{' '}
          <Link href="https://github.com/orkinosai25-org">Orkinosai</Link>
        </p>
        <p>Inspired by Ottoman heritage • Powered by modern technology</p>
      </div>
    </footer>
  );
};
