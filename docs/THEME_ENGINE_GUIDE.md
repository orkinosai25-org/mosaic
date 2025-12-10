# OrkinosaiCMS Theme Engine Guide

## Overview

OrkinosaiCMS includes a powerful, flexible theme engine that allows you to customize the appearance of your website. The theme engine supports ready-made themes, custom themes, and SharePoint-inspired layouts.

## Table of Contents

1. [Ready-Made Themes](#ready-made-themes)
2. [Theme Architecture](#theme-architecture)
3. [Using Themes](#using-themes)
4. [Customizing Themes](#customizing-themes)
5. [Creating Custom Themes](#creating-custom-themes)
6. [SharePoint Integration](#sharepoint-integration)
7. [Zoota Agent Theme Switching](#zoota-agent-theme-switching)
8. [API Reference](#api-reference)

## Ready-Made Themes

OrkinosaiCMS includes 6 professionally designed themes out of the box:

### 1. Orkinosai Professional
- **Category**: Modern
- **Layout**: Top Navigation
- **Description**: Modern, clean professional theme with blue and green color scheme
- **Best For**: Corporate websites, business portals
- **Features**: 
  - Responsive design
  - Professional color palette
  - Clean typography
  - Card-based layouts

### 2. SharePoint Portal
- **Category**: SharePoint
- **Layout**: Left Navigation
- **Description**: SharePoint-inspired portal theme with modern UI
- **Best For**: Intranet sites, enterprise portals
- **Features**:
  - Left quick launch navigation
  - Suite bar
  - Command bar
  - SharePoint-style web parts
  - Familiar SharePoint user experience

### 3. Top Navigation
- **Category**: Modern
- **Layout**: Top Navigation
- **Description**: Clean modern theme with horizontal navigation
- **Best For**: Marketing sites, product pages
- **Features**:
  - Sticky header
  - Horizontal navigation
  - Hero sections
  - Grid layouts

### 4. Dashboard
- **Category**: Dashboard
- **Layout**: Left Navigation
- **Description**: Modern dashboard theme for admin interfaces
- **Best For**: Admin panels, data visualization, analytics
- **Features**:
  - Dark sidebar
  - Stats cards
  - Data tables
  - Dashboard panels
  - Chart-ready layouts

### 5. Minimal
- **Category**: Minimal
- **Layout**: Top Navigation
- **Description**: Clean and simple design focused on content
- **Best For**: Blogs, documentation, content-heavy sites
- **Features**:
  - Minimalist aesthetics
  - Focus on typography
  - High readability
  - Distraction-free layout

### 6. Marketing Landing
- **Category**: Marketing
- **Layout**: Top Navigation
- **Description**: Bold, conversion-focused theme
- **Best For**: Landing pages, product launches, campaigns
- **Features**:
  - Eye-catching hero sections
  - CTA buttons
  - Feature grids
  - Pricing sections
  - Social proof sections

## Theme Architecture

### Theme Entity Properties

Each theme includes the following properties:

- **Name**: Unique theme identifier
- **Description**: Brief description of the theme
- **Version**: Theme version (e.g., "1.0.0")
- **Author**: Theme creator
- **Category**: Theme category (Modern, SharePoint, Dashboard, etc.)
- **LayoutType**: Navigation layout (TopNavigation, LeftNavigation, Portal, etc.)
- **PrimaryColor**: Main brand color (hex)
- **SecondaryColor**: Secondary brand color (hex)
- **AccentColor**: Accent color for highlights (hex)
- **LogoUrl**: Path to custom logo
- **AssetsPath**: Path to CSS file
- **ThumbnailUrl**: Preview image
- **IsEnabled**: Whether theme is active
- **IsSystem**: System theme (cannot be deleted)
- **IsMobileResponsive**: Responsive design support
- **CustomCss**: Custom CSS overrides

### Theme Structure

```
wwwroot/
├── css/
│   └── themes/
│       ├── orkinosai-theme.css
│       ├── sharepoint-portal-theme.css
│       ├── top-navigation-theme.css
│       ├── dashboard-theme.css
│       ├── minimal-theme.css
│       └── marketing-theme.css
└── images/
    └── themes/
        ├── sharepoint-portal.png
        ├── top-navigation.png
        └── ...
```

## Using Themes

### From Admin Panel

1. Navigate to **Admin Panel** → **Themes**
2. Browse available themes by category
3. Click **Preview** to see theme in action
4. Click **Apply Theme** to activate
5. Theme is immediately applied to your site

### Filtering Themes

Themes can be filtered by:
- All Themes
- Modern
- SharePoint
- Dashboard
- Minimal
- Marketing

### Theme Information

Each theme card displays:
- Theme name and description
- Category and layout type
- Color swatches (primary, secondary, accent)
- Mobile responsive badge
- System/Custom badge
- Available actions

## Customizing Themes

### Method 1: Clone and Modify

1. Find a system theme you like
2. Click **Clone** to create a custom copy
3. Click **Edit** on your cloned theme
4. Modify colors, logo, and settings
5. Apply your custom theme

### Method 2: Update Branding

For custom themes only:

1. Go to **Admin Panel** → **Themes**
2. Click **Edit** on a custom theme
3. Update:
   - Primary Color
   - Secondary Color
   - Accent Color
   - Logo URL
   - Custom CSS

### Color Customization

Themes use CSS variables for easy customization:

```css
:root {
    --primary-color: #0066cc;
    --secondary-color: #00a86b;
    --accent-color: #ff6b35;
}
```

Update these values to match your brand.

### Logo Customization

1. Upload logo to `/wwwroot/images/logos/`
2. Update theme's LogoUrl property
3. Logo appears in header/navigation

## Creating Custom Themes

### Step 1: Create CSS File

Create a new CSS file in `wwwroot/css/themes/`:

```css
/* my-custom-theme.css */
:root {
    --primary: #your-color;
    --secondary: #your-color;
    --accent: #your-color;
}

/* Your theme styles */
.my-header {
    background: var(--primary);
}
```

### Step 2: Register Theme

Use the Admin Panel or API:

```csharp
var theme = new Theme
{
    Name = "My Custom Theme",
    Description = "Custom theme for my site",
    Category = "Custom",
    LayoutType = "TopNavigation",
    PrimaryColor = "#your-color",
    SecondaryColor = "#your-color",
    AccentColor = "#your-color",
    AssetsPath = "/css/themes/my-custom-theme.css",
    IsEnabled = true,
    IsSystem = false,
    IsMobileResponsive = true
};
```

### Step 3: Apply Theme

Apply your theme from the Admin Panel or use the API.

## SharePoint Integration

### SharePoint-Inspired Features

The SharePoint Portal theme includes:

1. **Suite Bar**: Top application bar
2. **Quick Launch**: Left navigation menu
3. **Command Bar**: Action toolbar
4. **Canvas Area**: Main content area
5. **Web Part Zones**: Content regions

### SharePoint Color Schemes

Use SharePoint color variables:

```css
--sp-primary: #0078d4;
--sp-neutral-white: #ffffff;
--sp-neutral-lighter: #f3f2f1;
--sp-neutral-light: #edebe9;
```

### Layout Types

- **LeftNavigation**: SharePoint-style with quick launch
- **TopNavigation**: Modern SharePoint communication site
- **Portal**: Full SharePoint portal experience

## Zoota Agent Theme Switching

The Zoota admin agent can help you switch themes conversationally.

### Example Commands

**Viewing Themes:**
```
"Show me available themes"
"What themes do we have?"
"List all themes by category"
```

**Applying Themes:**
```
"Apply the SharePoint Portal theme"
"Switch to the minimal theme"
"Use the marketing landing theme"
```

**Customizing Themes:**
```
"Change primary color to blue"
"Update logo to /images/my-logo.png"
"Clone the professional theme"
```

### API Endpoints for Zoota

The Zoota agent uses these endpoints:

- `GET /api/theme` - List all themes
- `GET /api/theme/enabled` - List enabled themes
- `GET /api/theme/{id}` - Get theme details
- `GET /api/theme/category/{category}` - Filter by category
- `POST /api/theme/apply` - Apply theme to site
- `POST /api/theme/clone` - Clone a theme
- `PUT /api/theme/{id}/branding` - Update theme colors/logo

## API Reference

### Get All Themes

```http
GET /api/theme
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Orkinosai Professional",
    "description": "Modern, clean professional theme",
    "category": "Modern",
    "layoutType": "TopNavigation",
    "primaryColor": "#0066cc",
    "secondaryColor": "#00a86b",
    "accentColor": "#ff6b35",
    "isEnabled": true,
    "isSystem": true,
    "isMobileResponsive": true
  }
]
```

### Apply Theme to Site

```http
POST /api/theme/apply
Authorization: Bearer {token}
Content-Type: application/json

{
  "siteId": 1,
  "themeId": 2
}
```

**Response:**
```json
{
  "message": "Theme 2 applied to site 1"
}
```

### Update Theme Branding

```http
PUT /api/theme/{id}/branding
Authorization: Bearer {token}
Content-Type: application/json

{
  "themeId": 5,
  "primaryColor": "#FF5733",
  "secondaryColor": "#C70039",
  "accentColor": "#900C3F",
  "logoUrl": "/images/logos/my-logo.png"
}
```

**Response:**
```json
{
  "id": 5,
  "name": "My Custom Theme",
  "primaryColor": "#FF5733",
  "secondaryColor": "#C70039",
  "accentColor": "#900C3F",
  "logoUrl": "/images/logos/my-logo.png"
}
```

### Clone Theme

```http
POST /api/theme/clone
Authorization: Bearer {token}
Content-Type: application/json

{
  "sourceThemeId": 1,
  "newName": "My Professional Theme",
  "newDescription": "Customized professional theme"
}
```

**Response:**
```json
{
  "id": 7,
  "name": "My Professional Theme",
  "description": "Customized professional theme",
  "isSystem": false
}
```

## Best Practices

1. **Always Clone System Themes**: Never modify system themes directly
2. **Use Consistent Colors**: Stick to your brand color palette
3. **Test Responsiveness**: Preview themes on mobile devices
4. **Optimize Assets**: Compress images and minify CSS
5. **Document Changes**: Keep track of custom CSS modifications
6. **Version Control**: Save theme configurations for rollback

## Troubleshooting

### Theme Not Applying

1. Check theme is enabled
2. Verify AssetsPath is correct
3. Clear browser cache
4. Check for CSS conflicts

### Colors Not Updating

1. Ensure custom CSS is loading
2. Check CSS variable names
3. Use browser DevTools to inspect

### Layout Issues

1. Verify LayoutType matches template
2. Check master page compatibility
3. Review responsive breakpoints

## Future Enhancements

- Theme marketplace
- Visual theme editor
- A/B testing themes
- Theme analytics
- Dark mode variants
- Accessibility themes
- Industry-specific templates

## Support

For theme-related questions:
- Check documentation
- Use Zoota agent for assistance
- Submit issues on GitHub
- Contact support team

---

**Last Updated**: December 2025
**Version**: 1.0.0
