# MOSAIC Logo - Quick Start Guide

Get started with the MOSAIC logo in under 5 minutes.

## 1. Choose Your Logo Version

### For Websites & Apps
Use the **full logo** for headers, landing pages, and marketing:
- `src/mosaic-logo-main.svg` (light backgrounds)
- `src/mosaic-logo-dark.svg` (dark backgrounds)

### For Icons & Favicons
Use the **icon only** for app icons, favicons, and profile pictures:
- `src/mosaic-icon.svg` (light backgrounds)
- `src/mosaic-icon-dark.svg` (dark backgrounds)

## 2. Add to Your Website

### Basic HTML
```html
<!-- Simple logo -->
<img src="logo/src/mosaic-logo-main.svg" alt="MOSAIC" width="300">
```

### Responsive with Dark Mode Support
```html
<!-- Automatically switches based on user's system theme -->
<picture>
  <source srcset="logo/src/mosaic-logo-dark.svg" 
          media="(prefers-color-scheme: dark)">
  <img src="logo/src/mosaic-logo-main.svg" 
       alt="MOSAIC" 
       width="300" 
       height="113">
</picture>
```

### Favicon Setup
```html
<!-- In your <head> section -->
<link rel="icon" type="image/svg+xml" href="logo/src/mosaic-icon.svg">
<link rel="apple-touch-icon" sizes="180x180" href="logo/exports/icons/icon-180.png">
```

## 3. Generate PNG Exports (Optional)

If you need PNG files for specific uses:

```bash
cd logo
./generate-exports.sh
```

This creates PNG files in various sizes:
- `exports/favicon/` - 16, 32, 64, 128 px
- `exports/icons/` - 180, 192, 512 px
- `exports/web/` - 400x150, 800x300
- `exports/social/` - 1200x630, 400x400

**Requirements**: Install one of:
- `librsvg2-bin` (recommended)
- `inkscape`
- `imagemagick`

## 4. Social Media Setup

### Twitter/X
- **Profile Picture**: Use `mosaic-icon.svg` or export to 400x400 PNG
- **Header Image**: Use `mosaic-logo-main.svg` exported to 1500x500 PNG

### LinkedIn
- **Logo**: Export `mosaic-icon.svg` to 300x300 PNG
- **Banner**: Export `mosaic-logo-main.svg` to 1584x396 PNG

### Facebook/Meta
- **Profile Picture**: Use `mosaic-icon.svg` exported to 180x180 PNG
- **Cover Photo**: Export `mosaic-logo-main.svg` to 820x312 PNG

### Open Graph (Link Previews)
```html
<meta property="og:image" content="logo/exports/social/og-image-1200x630.png">
```

## 5. React/Next.js Example

```jsx
import Image from 'next/image'

export function Logo() {
  return (
    <picture>
      <source 
        srcSet="/logo/src/mosaic-logo-dark.svg" 
        media="(prefers-color-scheme: dark)" 
      />
      <Image
        src="/logo/src/mosaic-logo-main.svg"
        alt="MOSAIC"
        width={300}
        height={113}
        priority
      />
    </picture>
  )
}
```

## 6. PWA Manifest

```json
{
  "name": "MOSAIC",
  "short_name": "MOSAIC",
  "icons": [
    {
      "src": "/logo/exports/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/logo/exports/icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ],
  "theme_color": "#1e3a8a",
  "background_color": "#ffffff"
}
```

## 7. Color Codes for Your Design System

### CSS Variables
```css
:root {
  /* Light mode */
  --mosaic-blue-from: #1e3a8a;
  --mosaic-blue-to: #2563eb;
  --mosaic-turquoise-from: #06b6d4;
  --mosaic-turquoise-to: #0891b2;
  --mosaic-gold-from: #fbbf24;
  --mosaic-gold-to: #f59e0b;
}

@media (prefers-color-scheme: dark) {
  :root {
    --mosaic-blue-from: #60a5fa;
    --mosaic-blue-to: #3b82f6;
    --mosaic-turquoise-from: #22d3ee;
    --mosaic-turquoise-to: #06b6d4;
    --mosaic-gold-from: #fcd34d;
    --mosaic-gold-to: #fbbf24;
  }
}
```

### Tailwind Config
```js
module.exports = {
  theme: {
    extend: {
      colors: {
        mosaic: {
          blue: {
            light: '#60a5fa',
            DEFAULT: '#2563eb',
            dark: '#1e3a8a',
          },
          turquoise: {
            light: '#22d3ee',
            DEFAULT: '#06b6d4',
            dark: '#0891b2',
          },
          gold: {
            light: '#fcd34d',
            DEFAULT: '#fbbf24',
            dark: '#f59e0b',
          },
        },
      },
    },
  },
}
```

## 8. Preview Your Logo

Open `preview.html` in your browser to see:
- All logo variations
- Light and dark modes
- Color palette
- Design features

Or run a local server:
```bash
cd logo
python3 -m http.server 8080
# Open http://localhost:8080/preview.html
```

## 9. Common Sizes Quick Reference

| Use Case | Size | File |
|----------|------|------|
| Website Header | 200-400px width | Full logo SVG |
| Favicon | 32x32 | Icon PNG |
| App Icon (iOS) | 180x180 | Icon PNG |
| App Icon (Android) | 192x192 | Icon PNG |
| Social Profile | 400x400 | Icon PNG |
| Open Graph | 1200x630 | Full logo PNG |
| Email Signature | 300x113 | Full logo PNG/SVG |

## 10. Dos and Don'ts

### ✅ DO
- Use SVG format whenever possible
- Maintain aspect ratios
- Use appropriate light/dark version
- Give logo breathing room (20px minimum)

### ❌ DON'T
- Stretch or distort the logo
- Change colors
- Add effects (shadows, 3D, etc.)
- Place on busy backgrounds
- Rotate or flip

## Need Help?

- 📖 **Full Documentation**: See `README.md`
- 🎨 **Design Details**: See `concept/DESIGN_CONCEPT.md`
- 🕌 **Inspirations**: See `concept/OTTOMAN_INSPIRATIONS.md`
- 📋 **Overview**: See `SUMMARY.md`

---

**Ready to go!** Your MOSAIC logo is production-ready and scalable for all uses. 🚀
