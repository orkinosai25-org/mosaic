import { useState } from 'react';
import {
  FluentProvider,
  makeStyles,
} from '@fluentui/react-components';
import { mosaicLightTheme } from './styles/theme';
import { Header } from './components/layout/Header';
import { Navigation } from './components/layout/Navigation';
import { Footer } from './components/layout/Footer';
import { AuthPanel } from './components/auth/AuthPanel';
import { LandingPage } from './pages/LandingPage';
import { DashboardPage } from './pages/DashboardPage';
import { SitesPage } from './pages/SitesPage';
import { BillingPage } from './pages/BillingPage';
import { ThemesPage } from './pages/ThemesPage';
import { ChatBubble } from './components/common/ChatBubble';
import type { User } from './types';

const useStyles = makeStyles({
  app: {
    display: 'flex',
    flexDirection: 'column',
    minHeight: '100vh',
    backgroundColor: '#f8fafc',
  },
  mainContainer: {
    display: 'flex',
    flex: 1,
  },
  content: {
    flex: 1,
    overflowY: 'auto',
  },
  authOverlay: {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1000,
  },
});

function App() {
  const styles = useStyles();
  const [user, setUser] = useState<User | null>(null);
  const [showAuth, setShowAuth] = useState(false);
  const [activeNav, setActiveNav] = useState('dashboard');

  const handleAuthenticate = (email: string, name: string) => {
    setUser({
      id: '1',
      name,
      email,
      avatarUrl: undefined,
      isAuthenticated: true,
    });
    setShowAuth(false);
  };

  const handleLogout = () => {
    setUser(null);
    setActiveNav('dashboard');
  };

  const handleShowLogin = () => {
    setShowAuth(true);
  };

  const handleShowRegister = () => {
    setShowAuth(true);
  };

  const handleNavigation = (key: string) => {
    setActiveNav(key);
  };

  const handleQuickAction = (action: string) => {
    // Handle navigation based on action
    if (action === 'create-site') {
      setActiveNav('sites');
    } else if (action === 'configure-cms') {
      // Navigate to CMS admin interface (backend)
      // TODO: Replace with actual CMS URL once deployed
      window.open('/cms/admin', '_blank');
    } else if (action === 'billing') {
      setActiveNav('billing');
    } else if (action === 'settings') {
      setActiveNav('settings');
    } else if (action === 'analytics') {
      setActiveNav('dashboard');
    }
  };

  return (
    <FluentProvider theme={mosaicLightTheme}>
      <div className={styles.app}>
        <Header
          user={user}
          onLogin={handleShowLogin}
          onRegister={handleShowRegister}
          onLogout={handleLogout}
          onProfileClick={() => setActiveNav('dashboard')}
          onSettingsClick={() => setActiveNav('settings')}
        />

        <div className={styles.mainContainer}>
          {user?.isAuthenticated && (
            <Navigation activeKey={activeNav} onNavigate={handleNavigation} />
          )}

          <div className={styles.content}>
            {user?.isAuthenticated ? (
              activeNav === 'sites' ? (
                <SitesPage onAction={handleQuickAction} userEmail={user.email} />
              ) : activeNav === 'themes' ? (
                <ThemesPage />
              ) : activeNav === 'billing' ? (
                <BillingPage />
              ) : (
                <DashboardPage
                  userName={user.name}
                  onAction={handleQuickAction}
                />
              )
            ) : (
              <LandingPage
                onRegister={handleShowRegister}
                onLogin={handleShowLogin}
              />
            )}
          </div>
        </div>

        <Footer />

        {showAuth && (
          <div className={styles.authOverlay} onClick={() => setShowAuth(false)}>
            <div onClick={(e) => e.stopPropagation()}>
              <AuthPanel
                onAuthenticate={handleAuthenticate}
                onClose={() => setShowAuth(false)}
              />
            </div>
          </div>
        )}

        <ChatBubble />
      </div>
    </FluentProvider>
  );
}

export default App;
