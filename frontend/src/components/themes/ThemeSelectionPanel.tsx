import React, { useState, useEffect } from 'react';
import {
  makeStyles,
  shorthands,
  tokens,
  Card,
  Text,
  Button,
  Spinner,
  Badge,
  Input,
  Dropdown,
  Option,
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Label,
  Textarea,
} from '@fluentui/react-components';
import {
  PaintBrushRegular,
  CheckmarkCircleRegular,
  AddRegular,
  SearchRegular,
  DismissRegular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('24px'),
    ...shorthands.padding('24px'),
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  controls: {
    display: 'flex',
    ...shorthands.gap('12px'),
    alignItems: 'center',
    flexWrap: 'wrap',
  },
  searchBox: {
    minWidth: '250px',
  },
  themeGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
    ...shorthands.gap('20px'),
  },
  themeCard: {
    ...shorthands.padding('0'),
    cursor: 'pointer',
    ...shorthands.border('2px', 'solid', 'transparent'),
    transition: 'all 0.2s ease',
    height: '100%',
    display: 'flex',
    flexDirection: 'column',
    ':hover': {
      ...shorthands.borderColor(tokens.colorBrandStroke1),
      boxShadow: tokens.shadow8,
    },
  },
  themeCardSelected: {
    ...shorthands.borderColor(tokens.colorBrandBackground),
    backgroundColor: tokens.colorBrandBackground2,
  },
  themePreview: {
    width: '100%',
    height: '160px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    ...shorthands.borderRadius(tokens.borderRadiusMedium, tokens.borderRadiusMedium, '0', '0'),
    backgroundSize: 'cover',
    backgroundPosition: 'center',
    position: 'relative',
    overflow: 'hidden',
  },
  themePreviewPlaceholder: {
    background: `linear-gradient(135deg, ${tokens.colorBrandBackground} 0%, ${tokens.colorBrandBackground2} 100%)`,
  },
  colorSwatch: {
    display: 'flex',
    ...shorthands.gap('4px'),
    position: 'absolute',
    bottom: '8px',
    right: '8px',
  },
  colorDot: {
    width: '16px',
    height: '16px',
    ...shorthands.borderRadius('50%'),
    ...shorthands.border('2px', 'solid', '#fff'),
    boxShadow: '0 2px 4px rgba(0,0,0,0.2)',
  },
  themeContent: {
    ...shorthands.padding('16px'),
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('8px'),
    flex: 1,
  },
  themeName: {
    fontSize: tokens.fontSizeBase400,
    fontWeight: tokens.fontWeightSemibold,
  },
  themeDescription: {
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground3,
    lineHeight: '1.4',
  },
  themeFooter: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    ...shorthands.padding('12px', '16px'),
    ...shorthands.borderTop('1px', 'solid', tokens.colorNeutralStroke2),
  },
  categoryBadge: {
    fontSize: tokens.fontSizeBase100,
  },
  applyButton: {
    fontSize: tokens.fontSizeBase200,
  },
  emptyState: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    ...shorthands.padding('48px'),
    textAlign: 'center',
    ...shorthands.gap('16px'),
  },
  uploadDialog: {
    minWidth: '500px',
  },
  formField: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('8px'),
  },
  previewModal: {
    minWidth: '800px',
    minHeight: '600px',
  },
  previewFrame: {
    width: '100%',
    height: '500px',
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
  },
});

interface Theme {
  id: number;
  name: string;
  description: string;
  category: string;
  layoutType: string;
  thumbnailUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  isSystem?: boolean;
}

interface ThemeSelectionPanelProps {
  onThemeSelect?: (theme: Theme) => void;
  selectedThemeId?: number;
  showApplyButton?: boolean;
}

export const ThemeSelectionPanel: React.FC<ThemeSelectionPanelProps> = ({
  onThemeSelect,
  selectedThemeId,
  showApplyButton = true,
}) => {
  const styles = useStyles();
  const [themes, setThemes] = useState<Theme[]>([]);
  const [filteredThemes, setFilteredThemes] = useState<Theme[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');
  const [selectedTheme, setSelectedTheme] = useState<Theme | null>(null);
  const [showUploadDialog, setShowUploadDialog] = useState(false);
  const [showPreviewDialog, setShowPreviewDialog] = useState(false);
  const [previewTheme, setPreviewTheme] = useState<Theme | null>(null);

  // Custom theme upload form state
  const [customThemeName, setCustomThemeName] = useState('');
  const [customThemeDescription, setCustomThemeDescription] = useState('');
  const [customThemeCategory, setCustomThemeCategory] = useState('Business');
  const [customThemePrimaryColor, setCustomThemePrimaryColor] = useState('#0066cc');
  const [customThemeSecondaryColor, setCustomThemeSecondaryColor] = useState('#00a86b');
  const [customThemeAccentColor, setCustomThemeAccentColor] = useState('#ff6b35');

  useEffect(() => {
    fetchThemes();
  }, []);

  useEffect(() => {
    filterThemes();
  }, [themes, searchQuery, categoryFilter]);

  const fetchThemes = async () => {
    setLoading(true);
    try {
      const response = await fetch('/api/theme/enabled');
      if (response.ok) {
        const data = await response.json();
        setThemes(data);
      } else {
        console.error('Failed to fetch themes');
      }
    } catch (error) {
      console.error('Error fetching themes:', error);
    } finally {
      setLoading(false);
    }
  };

  const filterThemes = () => {
    let filtered = themes;

    // Filter by category
    if (categoryFilter !== 'all') {
      filtered = filtered.filter(theme => theme.category === categoryFilter);
    }

    // Filter by search query
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(
        theme =>
          theme.name.toLowerCase().includes(query) ||
          theme.description.toLowerCase().includes(query)
      );
    }

    setFilteredThemes(filtered);
  };

  const handleThemeClick = (theme: Theme) => {
    setSelectedTheme(theme);
    if (onThemeSelect) {
      onThemeSelect(theme);
    }
  };

  const handlePreviewClick = (theme: Theme, e: React.MouseEvent) => {
    e.stopPropagation();
    setPreviewTheme(theme);
    setShowPreviewDialog(true);
  };

  const handleApplyTheme = async (theme: Theme, e: React.MouseEvent) => {
    e.stopPropagation();
    // Theme application logic would go here
    // For now, just select the theme
    handleThemeClick(theme);
  };

  const handleCreateCustomTheme = async () => {
    try {
      const response = await fetch('/api/theme', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          name: customThemeName,
          description: customThemeDescription,
          category: customThemeCategory,
          layoutType: 'TopNavigation',
          primaryColor: customThemePrimaryColor,
          secondaryColor: customThemeSecondaryColor,
          accentColor: customThemeAccentColor,
        }),
      });

      if (response.ok) {
        // Refresh themes list
        await fetchThemes();
        setShowUploadDialog(false);
        // Reset form
        setCustomThemeName('');
        setCustomThemeDescription('');
        setCustomThemeCategory('Business');
        setCustomThemePrimaryColor('#0066cc');
        setCustomThemeSecondaryColor('#00a86b');
        setCustomThemeAccentColor('#ff6b35');
      }
    } catch (error) {
      console.error('Error creating custom theme:', error);
    }
  };

  const categories = [...new Set(themes.map(t => t.category))];

  if (loading) {
    return (
      <div className={styles.emptyState}>
        <Spinner size="large" label="Loading themes..." />
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div>
          <Text size={600} weight="semibold">
            Theme Gallery
          </Text>
          <Text size={300} style={{ display: 'block', marginTop: '4px' }}>
            Choose a professional theme for your site
          </Text>
        </div>
        <Button
          appearance="primary"
          icon={<AddRegular />}
          onClick={() => setShowUploadDialog(true)}
        >
          Create Custom Theme
        </Button>
      </div>

      <div className={styles.controls}>
        <Input
          className={styles.searchBox}
          contentBefore={<SearchRegular />}
          placeholder="Search themes..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
        <Dropdown
          placeholder="All Categories"
          value={categoryFilter === 'all' ? 'All Categories' : categoryFilter}
          onOptionSelect={(_, data) => setCategoryFilter(data.optionValue as string || 'all')}
        >
          <Option value="all">All Categories</Option>
          {categories.map(category => (
            <Option key={category} value={category}>
              {category}
            </Option>
          ))}
        </Dropdown>
      </div>

      {filteredThemes.length === 0 ? (
        <div className={styles.emptyState}>
          <PaintBrushRegular style={{ fontSize: '48px', color: tokens.colorNeutralForeground3 }} />
          <Text size={400}>No themes found</Text>
          <Text size={300} style={{ color: tokens.colorNeutralForeground3 }}>
            Try adjusting your filters or create a custom theme
          </Text>
        </div>
      ) : (
        <div className={styles.themeGrid}>
          {filteredThemes.map(theme => (
            <Card
              key={theme.id}
              className={`${styles.themeCard} ${
                selectedThemeId === theme.id || selectedTheme?.id === theme.id
                  ? styles.themeCardSelected
                  : ''
              }`}
              onClick={() => handleThemeClick(theme)}
            >
              <div
                className={`${styles.themePreview} ${
                  !theme.thumbnailUrl ? styles.themePreviewPlaceholder : ''
                }`}
                style={
                  theme.thumbnailUrl
                    ? { backgroundImage: `url(${theme.thumbnailUrl})` }
                    : {
                        background: `linear-gradient(135deg, ${theme.primaryColor || tokens.colorBrandBackground} 0%, ${theme.secondaryColor || tokens.colorBrandBackground2} 100%)`,
                      }
                }
              >
                {!theme.thumbnailUrl && (
                  <PaintBrushRegular
                    style={{ fontSize: '48px', color: 'rgba(255,255,255,0.5)' }}
                  />
                )}
                {(theme.primaryColor || theme.secondaryColor || theme.accentColor) && (
                  <div className={styles.colorSwatch}>
                    {theme.primaryColor && (
                      <div
                        className={styles.colorDot}
                        style={{ backgroundColor: theme.primaryColor }}
                      />
                    )}
                    {theme.secondaryColor && (
                      <div
                        className={styles.colorDot}
                        style={{ backgroundColor: theme.secondaryColor }}
                      />
                    )}
                    {theme.accentColor && (
                      <div
                        className={styles.colorDot}
                        style={{ backgroundColor: theme.accentColor }}
                      />
                    )}
                  </div>
                )}
              </div>
              <div className={styles.themeContent}>
                <Text className={styles.themeName}>{theme.name}</Text>
                <Text className={styles.themeDescription}>{theme.description}</Text>
              </div>
              <div className={styles.themeFooter}>
                <Badge className={styles.categoryBadge} appearance="outline">
                  {theme.category}
                </Badge>
                {showApplyButton && (
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <Button
                      size="small"
                      appearance="subtle"
                      onClick={(e) => handlePreviewClick(theme, e)}
                    >
                      Preview
                    </Button>
                    <Button
                      size="small"
                      appearance="primary"
                      className={styles.applyButton}
                      icon={<CheckmarkCircleRegular />}
                      onClick={(e) => handleApplyTheme(theme, e)}
                    >
                      Apply
                    </Button>
                  </div>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Custom Theme Upload Dialog */}
      <Dialog open={showUploadDialog} onOpenChange={(_, data) => setShowUploadDialog(data.open)}>
        <DialogSurface className={styles.uploadDialog}>
          <DialogBody>
            <DialogTitle
              action={
                <Button
                  appearance="subtle"
                  icon={<DismissRegular />}
                  onClick={() => setShowUploadDialog(false)}
                />
              }
            >
              Create Custom Theme
            </DialogTitle>
            <DialogContent style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              <div className={styles.formField}>
                <Label htmlFor="themeName" required>
                  Theme Name
                </Label>
                <Input
                  id="themeName"
                  value={customThemeName}
                  onChange={(e) => setCustomThemeName(e.target.value)}
                  placeholder="My Custom Theme"
                />
              </div>
              <div className={styles.formField}>
                <Label htmlFor="themeDescription">Description</Label>
                <Textarea
                  id="themeDescription"
                  value={customThemeDescription}
                  onChange={(e) => setCustomThemeDescription(e.target.value)}
                  placeholder="Describe your theme..."
                  rows={3}
                />
              </div>
              <div className={styles.formField}>
                <Label htmlFor="themeCategory">Category</Label>
                <Dropdown
                  id="themeCategory"
                  value={customThemeCategory}
                  onOptionSelect={(_, data) => setCustomThemeCategory(data.optionValue as string)}
                >
                  <Option value="Business">Business</Option>
                  <Option value="Portfolio">Portfolio</Option>
                  <Option value="Blog">Blog</Option>
                  <Option value="Commerce">Commerce</Option>
                  <Option value="Education">Education</Option>
                  <Option value="Health">Health</Option>
                  <Option value="Food">Food</Option>
                  <Option value="Modern">Modern</Option>
                </Dropdown>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '12px' }}>
                <div className={styles.formField}>
                  <Label htmlFor="primaryColor">Primary Color</Label>
                  <input
                    id="primaryColor"
                    type="color"
                    value={customThemePrimaryColor}
                    onChange={(e) => setCustomThemePrimaryColor(e.target.value)}
                    style={{ width: '100%', height: '32px', cursor: 'pointer' }}
                  />
                </div>
                <div className={styles.formField}>
                  <Label htmlFor="secondaryColor">Secondary Color</Label>
                  <input
                    id="secondaryColor"
                    type="color"
                    value={customThemeSecondaryColor}
                    onChange={(e) => setCustomThemeSecondaryColor(e.target.value)}
                    style={{ width: '100%', height: '32px', cursor: 'pointer' }}
                  />
                </div>
                <div className={styles.formField}>
                  <Label htmlFor="accentColor">Accent Color</Label>
                  <input
                    id="accentColor"
                    type="color"
                    value={customThemeAccentColor}
                    onChange={(e) => setCustomThemeAccentColor(e.target.value)}
                    style={{ width: '100%', height: '32px', cursor: 'pointer' }}
                  />
                </div>
              </div>
            </DialogContent>
            <DialogActions>
              <Button appearance="secondary" onClick={() => setShowUploadDialog(false)}>
                Cancel
              </Button>
              <Button
                appearance="primary"
                onClick={handleCreateCustomTheme}
                disabled={!customThemeName.trim()}
              >
                Create Theme
              </Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>

      {/* Theme Preview Dialog */}
      <Dialog open={showPreviewDialog} onOpenChange={(_, data) => setShowPreviewDialog(data.open)}>
        <DialogSurface className={styles.previewModal}>
          <DialogBody>
            <DialogTitle
              action={
                <Button
                  appearance="subtle"
                  icon={<DismissRegular />}
                  onClick={() => setShowPreviewDialog(false)}
                />
              }
            >
              Preview: {previewTheme?.name}
            </DialogTitle>
            <DialogContent>
              <div className={styles.previewFrame}>
                <div style={{ padding: '24px', textAlign: 'center' }}>
                  <Text size={500} weight="semibold">
                    Theme Preview
                  </Text>
                  <Text style={{ display: 'block', marginTop: '16px' }}>
                    Full theme preview would be rendered here with actual site layout
                  </Text>
                  {previewTheme && (
                    <div style={{ marginTop: '24px' }}>
                      <div
                        style={{
                          display: 'flex',
                          gap: '16px',
                          justifyContent: 'center',
                          marginTop: '16px',
                        }}
                      >
                        <div>
                          <Text weight="semibold">Primary:</Text>
                          <div
                            style={{
                              width: '60px',
                              height: '60px',
                              backgroundColor: previewTheme.primaryColor,
                              borderRadius: '8px',
                              marginTop: '8px',
                            }}
                          />
                        </div>
                        <div>
                          <Text weight="semibold">Secondary:</Text>
                          <div
                            style={{
                              width: '60px',
                              height: '60px',
                              backgroundColor: previewTheme.secondaryColor,
                              borderRadius: '8px',
                              marginTop: '8px',
                            }}
                          />
                        </div>
                        <div>
                          <Text weight="semibold">Accent:</Text>
                          <div
                            style={{
                              width: '60px',
                              height: '60px',
                              backgroundColor: previewTheme.accentColor,
                              borderRadius: '8px',
                              marginTop: '8px',
                            }}
                          />
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </DialogContent>
            <DialogActions>
              <Button appearance="secondary" onClick={() => setShowPreviewDialog(false)}>
                Close
              </Button>
              <Button
                appearance="primary"
                onClick={() => {
                  if (previewTheme) {
                    handleThemeClick(previewTheme);
                    setShowPreviewDialog(false);
                  }
                }}
              >
                Select This Theme
              </Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </div>
  );
};
