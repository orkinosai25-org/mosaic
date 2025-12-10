import React from 'react';
import {
  Button,
  Avatar,
  Menu,
  MenuTrigger,
  MenuPopover,
  MenuList,
  MenuItem,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';
import {
  PersonRegular,
  SignOutRegular,
  SettingsRegular,
} from '@fluentui/react-icons';
import type { User } from '../../types';

const useStyles = makeStyles({
  header: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    height: '48px',
    ...shorthands.padding('0', '24px'),
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
    boxShadow: tokens.shadow4,
  },
  logoSection: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('12px'),
  },
  logo: {
    height: '32px',
    width: 'auto',
  },
  platformName: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorNeutralForegroundOnBrand,
  },
  rightSection: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('12px'),
  },
  authButtons: {
    display: 'flex',
    ...shorthands.gap('8px'),
  },
});

interface HeaderProps {
  user: User | null;
  onLogin: () => void;
  onRegister: () => void;
  onLogout: () => void;
  onProfileClick: () => void;
  onSettingsClick: () => void;
}

export const Header: React.FC<HeaderProps> = ({
  user,
  onLogin,
  onRegister,
  onLogout,
  onProfileClick,
  onSettingsClick,
}) => {
  const styles = useStyles();

  return (
    <header className={styles.header}>
      <div className={styles.logoSection}>
        <img
          src="/mosaic-icon-dark.svg"
          alt="MOSAIC Logo"
          className={styles.logo}
        />
        <span className={styles.platformName}>MOSAIC</span>
      </div>

      <div className={styles.rightSection}>
        {user?.isAuthenticated ? (
          <Menu>
            <MenuTrigger>
              <Button
                appearance="transparent"
                icon={
                  <Avatar
                    name={user.name}
                    image={{ src: user.avatarUrl }}
                    size={28}
                  />
                }
                style={{ color: tokens.colorNeutralForegroundOnBrand }}
              />
            </MenuTrigger>
            <MenuPopover>
              <MenuList>
                <MenuItem icon={<PersonRegular />} onClick={onProfileClick}>
                  Profile
                </MenuItem>
                <MenuItem icon={<SettingsRegular />} onClick={onSettingsClick}>
                  Settings
                </MenuItem>
                <MenuItem icon={<SignOutRegular />} onClick={onLogout}>
                  Logout
                </MenuItem>
              </MenuList>
            </MenuPopover>
          </Menu>
        ) : (
          <div className={styles.authButtons}>
            <Button appearance="secondary" onClick={onLogin}>
              Sign In
            </Button>
            <Button appearance="primary" onClick={onRegister}>
              Register
            </Button>
          </div>
        )}
      </div>
    </header>
  );
};
