import React, { useState } from 'react';
import {
  MessageBar,
  MessageBarBody,
  MessageBarTitle,
  Button,
  makeStyles,
  shorthands,
} from '@fluentui/react-components';


const useStyles = makeStyles({
  banner: {
    ...shorthands.margin('0', '0', '16px', '0'),
  },
  actions: {
    display: 'flex',
    ...shorthands.gap('8px'),
    marginTop: '8px',
  },
});

export const MigrationBanner: React.FC = () => {
  const styles = useStyles();
  const [isVisible, setIsVisible] = useState(true);

  if (!isVisible) return null;

  return (
    <MessageBar
      intent="info"
      className={styles.banner}
    >
      <MessageBarBody>
        <MessageBarTitle>🚀 Migration Toolkit Available</MessageBarTitle>
        Easily migrate your existing sites to MOSAIC with our automated migration toolkit.
        No downtime, seamless transition.
        <div className={styles.actions}>
          <Button appearance="primary" size="small">
            Start Migration
          </Button>
          <Button appearance="subtle" size="small" onClick={() => setIsVisible(false)}>
            Dismiss
          </Button>
        </div>
      </MessageBarBody>
    </MessageBar>
  );
};
