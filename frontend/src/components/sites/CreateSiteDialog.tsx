import React, { useState } from 'react';
import {
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Button,
  Input,
  Label,
  Textarea,
  makeStyles,
  shorthands,
  tokens,
  Card,
  Text,
  Badge,
  Spinner,
} from '@fluentui/react-components';
import {
  CheckmarkCircleRegular,
  DismissRegular,
  ArrowLeftRegular,
  ArrowRightRegular,
  SparkleRegular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  dialog: {
    maxWidth: '700px',
    minHeight: '500px',
  },
  dialogBody: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('20px'),
  },
  stepIndicator: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: '20px',
    ...shorthands.padding('16px', '0'),
    ...shorthands.borderBottom('1px', 'solid', tokens.colorNeutralStroke2),
  },
  step: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    ...shorthands.gap('8px'),
    flex: 1,
    position: 'relative',
  },
  stepNumber: {
    width: '32px',
    height: '32px',
    borderRadius: '50%',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: tokens.fontWeightSemibold,
    fontSize: tokens.fontSizeBase300,
  },
  stepNumberActive: {
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
  },
  stepNumberCompleted: {
    backgroundColor: tokens.colorPaletteGreenBackground3,
    color: tokens.colorNeutralForegroundOnBrand,
  },
  stepNumberInactive: {
    backgroundColor: tokens.colorNeutralBackground3,
    color: tokens.colorNeutralForeground3,
  },
  stepLabel: {
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground2,
  },
  stepLine: {
    position: 'absolute',
    top: '16px',
    left: '50%',
    width: '100%',
    height: '2px',
    backgroundColor: tokens.colorNeutralStroke2,
    zIndex: -1,
  },
  formField: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('8px'),
  },
  themeGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
    ...shorthands.gap('16px'),
  },
  themeCard: {
    ...shorthands.padding('16px'),
    cursor: 'pointer',
    ...shorthands.border('2px', 'solid', 'transparent'),
    ':hover': {
      ...shorthands.borderColor(tokens.colorBrandStroke1),
    },
  },
  themeCardSelected: {
    ...shorthands.borderColor(tokens.colorBrandBackground),
    backgroundColor: tokens.colorBrandBackground2,
  },
  conversationBox: {
    ...shorthands.padding('16px'),
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    minHeight: '200px',
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('12px'),
  },
  aiMessage: {
    display: 'flex',
    ...shorthands.gap('8px'),
    alignItems: 'flex-start',
  },
  reviewSection: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('16px'),
  },
  reviewItem: {
    display: 'flex',
    justifyContent: 'space-between',
    ...shorthands.padding('12px'),
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
  },
  successMessage: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    ...shorthands.gap('16px'),
    ...shorthands.padding('24px'),
    textAlign: 'center',
  },
  errorBox: {
    ...shorthands.padding('12px'),
    backgroundColor: tokens.colorPaletteRedBackground1,
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
    ...shorthands.border('1px', 'solid', tokens.colorPaletteRedBorder1),
  },
  errorDetailsBox: {
    ...shorthands.padding('12px'),
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    fontFamily: 'monospace',
    fontSize: tokens.fontSizeBase200,
    maxHeight: '300px',
    overflowY: 'auto',
    whiteSpace: 'pre-wrap',
    wordBreak: 'break-word',
    marginTop: '8px',
  },
  errorToggle: {
    marginTop: '8px',
    cursor: 'pointer',
    color: tokens.colorBrandForeground1,
    fontSize: tokens.fontSizeBase200,
    ':hover': {
      textDecoration: 'underline',
    },
  },
});

interface CreateSiteDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSiteCreated: (site: any) => void;
  userEmail: string;
}

interface Theme {
  id: number;
  name: string;
  description: string;
  category: string;
}

export const CreateSiteDialog: React.FC<CreateSiteDialogProps> = ({
  isOpen,
  onClose,
  onSiteCreated,
  userEmail,
}) => {
  const styles = useStyles();
  const [currentStep, setCurrentStep] = useState(1);
  const [siteName, setSiteName] = useState('');
  const [description, setDescription] = useState('');
  const [purpose, setPurpose] = useState('');
  const [selectedTheme, setSelectedTheme] = useState<Theme | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [createdSite, setCreatedSite] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [errorDetails, setErrorDetails] = useState<string | null>(null);
  const [stackTrace, setStackTrace] = useState<string | null>(null);
  const [showErrorDetails, setShowErrorDetails] = useState(false);

  // Mock themes - in production, fetch from API
  const themes: Theme[] = [
    { id: 1, name: 'Modern Business', description: 'Clean and professional', category: 'Business' },
    { id: 2, name: 'Creative Portfolio', description: 'Showcase your work', category: 'Portfolio' },
    { id: 3, name: 'E-commerce', description: 'Perfect for online stores', category: 'Commerce' },
    { id: 4, name: 'Blog', description: 'Content-focused design', category: 'Blog' },
  ];

  const steps = [
    { number: 1, label: 'Basic Info' },
    { number: 2, label: 'AI Setup' },
    { number: 3, label: 'Theme' },
    { number: 4, label: 'Review' },
  ];

  const handleNext = () => {
    if (currentStep < 4) {
      setCurrentStep(currentStep + 1);
    } else {
      handleCreateSite();
    }
  };

  const handleBack = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleCreateSite = async () => {
    setIsLoading(true);
    setError(null);
    setErrorDetails(null);
    setStackTrace(null);

    try {
      const response = await fetch('/api/site', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          name: siteName,
          description: description || undefined,
          purpose: purpose || undefined,
          themeId: selectedTheme?.id,
          adminEmail: userEmail,
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to create site');
        setErrorDetails(errorData.errorDetails || null);
        setStackTrace(errorData.stackTrace || null);
        throw new Error(errorData.message || 'Failed to create site');
      }

      const result = await response.json();
      setCreatedSite(result.site);
      setCurrentStep(5); // Success step
      onSiteCreated(result.site);
    } catch (err: any) {
      // Error details already set above
      if (!error) {
        setError(err.message || 'An error occurred while creating the site');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    // Reset state
    setCurrentStep(1);
    setSiteName('');
    setDescription('');
    setPurpose('');
    setSelectedTheme(null);
    setCreatedSite(null);
    setError(null);
    setErrorDetails(null);
    setStackTrace(null);
    setShowErrorDetails(false);
    onClose();
  };

  const canProceed = () => {
    if (currentStep === 1) return siteName.trim().length > 0;
    // Purpose is optional - user can skip AI setup if they prefer
    if (currentStep === 2) return true;
    if (currentStep === 3) return selectedTheme !== null;
    if (currentStep === 4) return true;
    return false;
  };

  const renderStepContent = () => {
    if (currentStep === 1) {
      return (
        <>
          <div className={styles.formField}>
            <Label htmlFor="siteName" required>Site Name</Label>
            <Input
              id="siteName"
              value={siteName}
              onChange={(e) => setSiteName(e.target.value)}
              placeholder="My Awesome Site"
            />
          </div>
          <div className={styles.formField}>
            <Label htmlFor="description">Description (Optional)</Label>
            <Textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Tell us about your site..."
              rows={3}
            />
          </div>
        </>
      );
    }

    if (currentStep === 2) {
      return (
        <>
          <div className={styles.conversationBox}>
            <div className={styles.aiMessage}>
              <SparkleRegular style={{ color: tokens.colorBrandForeground1 }} />
              <div>
                <Text weight="semibold" style={{ display: 'block', marginBottom: '4px' }}>
                  AI Assistant
                </Text>
                <Text>
                  Hi! I'm here to help you set up your site. What's the main purpose of your website?
                  This will help me suggest the best configuration for you.
                </Text>
              </div>
            </div>
          </div>
          <div className={styles.formField}>
            <Label htmlFor="purpose">Purpose of Your Site</Label>
            <Textarea
              id="purpose"
              value={purpose}
              onChange={(e) => setPurpose(e.target.value)}
              placeholder="E.g., 'I want to showcase my portfolio and blog about design' or 'I need an online store for my handmade crafts'"
              rows={4}
            />
          </div>
        </>
      );
    }

    if (currentStep === 3) {
      return (
        <>
          <Text size={400} weight="semibold">Choose a Theme</Text>
          <div className={styles.themeGrid}>
            {themes.map((theme) => (
              <Card
                key={theme.id}
                className={`${styles.themeCard} ${
                  selectedTheme?.id === theme.id ? styles.themeCardSelected : ''
                }`}
                onClick={() => setSelectedTheme(theme)}
              >
                <Text weight="semibold" style={{ display: 'block', marginBottom: '8px' }}>
                  {theme.name}
                </Text>
                <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>
                  {theme.description}
                </Text>
                <Badge
                  appearance="outline"
                  style={{ marginTop: '8px' }}
                >
                  {theme.category}
                </Badge>
              </Card>
            ))}
          </div>
        </>
      );
    }

    if (currentStep === 4) {
      return (
        <div className={styles.reviewSection}>
          <Text size={400} weight="semibold">Review Your Site Configuration</Text>
          <div className={styles.reviewItem}>
            <Text weight="semibold">Site Name:</Text>
            <Text>{siteName}</Text>
          </div>
          {description && (
            <div className={styles.reviewItem}>
              <Text weight="semibold">Description:</Text>
              <Text>{description}</Text>
            </div>
          )}
          <div className={styles.reviewItem}>
            <Text weight="semibold">Purpose:</Text>
            <Text>{purpose}</Text>
          </div>
          <div className={styles.reviewItem}>
            <Text weight="semibold">Theme:</Text>
            <Text>{selectedTheme?.name}</Text>
          </div>
          {error && (
            <div className={styles.errorBox}>
              <Text style={{ color: tokens.colorPaletteRedForeground1, fontWeight: tokens.fontWeightSemibold }}>
                Error: {error}
              </Text>
              {errorDetails && (
                <>
                  <Text 
                    className={styles.errorToggle}
                    onClick={() => setShowErrorDetails(!showErrorDetails)}
                  >
                    {showErrorDetails ? '▼ Hide Details' : '▶ Show Details'}
                  </Text>
                  {showErrorDetails && (
                    <div className={styles.errorDetailsBox}>
                      <Text weight="semibold" style={{ display: 'block', marginBottom: '8px' }}>
                        Error Details:
                      </Text>
                      <Text>{errorDetails}</Text>
                      {stackTrace && (
                        <>
                          <Text weight="semibold" style={{ display: 'block', marginTop: '12px', marginBottom: '8px' }}>
                            Stack Trace:
                          </Text>
                          <Text>{stackTrace}</Text>
                        </>
                      )}
                    </div>
                  )}
                </>
              )}
            </div>
          )}
        </div>
      );
    }

    if (currentStep === 5) {
      return (
        <div className={styles.successMessage}>
          <CheckmarkCircleRegular
            style={{ fontSize: '64px', color: tokens.colorPaletteGreenForeground1 }}
          />
          <Text size={500} weight="semibold">
            Site Created Successfully!
          </Text>
          <Text>
            Your site "{createdSite?.name}" has been provisioned and is ready to use.
          </Text>
          {createdSite?.url && (
            <div>
              <Text weight="semibold">Site URL: </Text>
              <Text>{createdSite.url}.mosaic.app</Text>
            </div>
          )}
          <Button
            appearance="primary"
            onClick={() => {
              window.open(`/admin?site=${createdSite?.id}`, '_blank');
            }}
          >
            Go to CMS Dashboard
          </Button>
        </div>
      );
    }

    return null;
  };

  return (
    <Dialog open={isOpen} onOpenChange={(_, data) => !data.open && handleClose()}>
      <DialogSurface className={styles.dialog}>
        <DialogBody>
          <DialogTitle
            action={
              <Button
                appearance="subtle"
                icon={<DismissRegular />}
                onClick={handleClose}
              />
            }
          >
            {currentStep === 5 ? 'Success!' : 'Create New Site'}
          </DialogTitle>
          <DialogContent className={styles.dialogBody}>
            {currentStep < 5 && (
              <div className={styles.stepIndicator}>
                {steps.map((step, index) => (
                  <div key={step.number} className={styles.step}>
                    {index > 0 && <div className={styles.stepLine} />}
                    <div
                      className={`${styles.stepNumber} ${
                        currentStep === step.number
                          ? styles.stepNumberActive
                          : currentStep > step.number
                          ? styles.stepNumberCompleted
                          : styles.stepNumberInactive
                      }`}
                    >
                      {currentStep > step.number ? '✓' : step.number}
                    </div>
                    <Text className={styles.stepLabel}>{step.label}</Text>
                  </div>
                ))}
              </div>
            )}
            {renderStepContent()}
          </DialogContent>
          {currentStep < 5 && (
            <DialogActions>
              {currentStep > 1 && (
                <Button
                  appearance="secondary"
                  icon={<ArrowLeftRegular />}
                  onClick={handleBack}
                  disabled={isLoading}
                >
                  Back
                </Button>
              )}
              <Button
                appearance="primary"
                icon={currentStep === 4 ? undefined : <ArrowRightRegular />}
                iconPosition="after"
                onClick={handleNext}
                disabled={!canProceed() || isLoading}
              >
                {isLoading ? (
                  <>
                    <Spinner size="tiny" /> Creating...
                  </>
                ) : currentStep === 4 ? (
                  'Create Site'
                ) : (
                  'Next'
                )}
              </Button>
            </DialogActions>
          )}
          {currentStep === 5 && (
            <DialogActions>
              <Button appearance="primary" onClick={handleClose}>
                Done
              </Button>
            </DialogActions>
          )}
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};
