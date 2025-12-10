import React, { useState } from 'react';
import {
  Button,
  makeStyles,
  shorthands,
  tokens,
} from '@fluentui/react-components';
import { ChatRegular, DismissRegular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  chatBubble: {
    position: 'fixed',
    bottom: '24px',
    right: '24px',
    zIndex: 1000,
  },
  chatButton: {
    width: '56px',
    height: '56px',
    ...shorthands.borderRadius('50%'),
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
    boxShadow: tokens.shadow16,
    ':hover': {
      backgroundColor: tokens.colorBrandBackgroundHover,
      transform: 'scale(1.05)',
    },
    transition: 'all 0.2s ease',
  },
  chatPanel: {
    position: 'fixed',
    bottom: '90px',
    right: '24px',
    width: '350px',
    height: '500px',
    backgroundColor: tokens.colorNeutralBackground1,
    ...shorthands.borderRadius('8px'),
    boxShadow: tokens.shadow28,
    display: 'flex',
    flexDirection: 'column',
    zIndex: 1001,
  },
  chatHeader: {
    ...shorthands.padding('16px'),
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
    ...shorthands.borderRadius('8px', '8px', '0', '0'),
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  chatTitle: {
    fontWeight: tokens.fontWeightSemibold,
    fontSize: tokens.fontSizeBase400,
  },
  chatBody: {
    flex: 1,
    ...shorthands.padding('16px'),
    overflowY: 'auto',
  },
  welcomeMessage: {
    fontSize: tokens.fontSizeBase300,
    color: tokens.colorNeutralForeground2,
  },
});

export const ChatBubble: React.FC = () => {
  const styles = useStyles();
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      {isOpen && (
        <div className={styles.chatPanel}>
          <div className={styles.chatHeader}>
            <span className={styles.chatTitle}>MOSAIC Agent</span>
            <Button
              appearance="transparent"
              icon={<DismissRegular />}
              onClick={() => setIsOpen(false)}
              style={{ color: tokens.colorNeutralForegroundOnBrand }}
            />
          </div>
          <div className={styles.chatBody}>
            <div className={styles.welcomeMessage}>
              👋 Hi! I'm the MOSAIC conversational agent. How can I help you today?
            </div>
          </div>
        </div>
      )}

      <div className={styles.chatBubble}>
        <Button
          appearance="primary"
          icon={isOpen ? <DismissRegular /> : <ChatRegular />}
          className={styles.chatButton}
          onClick={() => setIsOpen(!isOpen)}
        />
      </div>
    </>
  );
};
