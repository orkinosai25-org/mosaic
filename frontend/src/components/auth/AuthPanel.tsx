import React, { useState } from 'react';
import {
  Button,
  Input,
  Label,
  Card,
  makeStyles,
  shorthands,
  tokens,
  Divider,
} from '@fluentui/react-components';

const useStyles = makeStyles({
  authCard: {
    maxWidth: '400px',
    ...shorthands.padding('32px'),
    ...shorthands.margin('0', 'auto'),
  },
  title: {
    fontSize: tokens.fontSizeBase600,
    fontWeight: tokens.fontWeightSemibold,
    marginBottom: '24px',
    textAlign: 'center',
    color: tokens.colorBrandForeground1,
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('16px'),
  },
  inputGroup: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('8px'),
  },
  submitButton: {
    marginTop: '8px',
  },
  dividerContainer: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('16px'),
    ...shorthands.margin('16px', '0'),
  },
  oauthButtons: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('12px'),
  },
  toggleText: {
    textAlign: 'center',
    marginTop: '16px',
    fontSize: tokens.fontSizeBase300,
  },
  toggleLink: {
    color: tokens.colorBrandForeground1,
    cursor: 'pointer',
    textDecoration: 'underline',
  },
});

interface AuthPanelProps {
  onAuthenticate: (email: string, name: string) => void;
  onClose: () => void;
}

export const AuthPanel: React.FC<AuthPanelProps> = ({
  onAuthenticate,
}) => {
  const styles = useStyles();
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Basic validation
    if (!email || !email.includes('@')) {
      alert('Please enter a valid email address');
      return;
    }
    
    if (!password || password.length < 6) {
      alert('Password must be at least 6 characters');
      return;
    }
    
    // TODO: Replace with actual authentication API call
    // Mock authentication for demonstration
    onAuthenticate(email, name || email.split('@')[0]);
  };

  const handleOAuthLogin = (provider: string) => {
    // TODO: Implement proper OAuth flow with provider
    // Mock OAuth login for demonstration only
    onAuthenticate(`user@${provider}.com`, `${provider} User`);
  };

  return (
    <Card className={styles.authCard}>
      <h2 className={styles.title}>{isLogin ? 'Sign In' : 'Register'}</h2>

      <form className={styles.form} onSubmit={handleSubmit}>
        {!isLogin && (
          <div className={styles.inputGroup}>
            <Label htmlFor="name">Full Name</Label>
            <Input
              id="name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required={!isLogin}
            />
          </div>
        )}

        <div className={styles.inputGroup}>
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className={styles.inputGroup}>
          <Label htmlFor="password">Password</Label>
          <Input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <Button
          appearance="primary"
          type="submit"
          className={styles.submitButton}
        >
          {isLogin ? 'Sign In' : 'Create Account'}
        </Button>
      </form>

      <div className={styles.dividerContainer}>
        <Divider style={{ flex: 1 }} />
        <span>OR</span>
        <Divider style={{ flex: 1 }} />
      </div>

      <div className={styles.oauthButtons}>
        <Button
          appearance="outline"
          onClick={() => handleOAuthLogin('google')}
        >
          Continue with Google
        </Button>
        <Button
          appearance="outline"
          onClick={() => handleOAuthLogin('github')}
        >
          Continue with GitHub
        </Button>
        <Button
          appearance="outline"
          onClick={() => handleOAuthLogin('microsoft')}
        >
          Continue with Microsoft
        </Button>
      </div>

      <div className={styles.toggleText}>
        {isLogin ? "Don't have an account? " : 'Already have an account? '}
        <span
          className={styles.toggleLink}
          onClick={() => setIsLogin(!isLogin)}
        >
          {isLogin ? 'Register' : 'Sign In'}
        </span>
      </div>
    </Card>
  );
};
