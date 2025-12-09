# MOSAIC Logo Assets

Official logo and branding assets for MOSAIC SaaS by Orkinosai.

## 📁 Directory Structure

```
logo/
├── src/              # Source SVG files (scalable, editable)
│   ├── mosaic-logo-main.svg       # Full logo - light mode
│   ├── mosaic-logo-dark.svg       # Full logo - dark mode
│   ├── mosaic-icon.svg            # Icon only - light mode
│   └── mosaic-icon-dark.svg       # Icon only - dark mode
├── exports/          # Exported PNG/WebP files (various sizes)
├── concept/          # Design concept and inspiration docs
│   └── DESIGN_CONCEPT.md
└── README.md         # This file
```

## 🎨 Design Overview

The MOSAIC logo draws inspiration from Ottoman architectural masterpieces:
- **Selimiye Mosque** (Edirne) - Geometric precision and 8-pointed star patterns
- **Blue Mosque** (Istanbul) - Famous Iznik tile work
- **Ottoman Heritage** - Geometric mosaics, arabesques, and traditional color palettes

### Key Design Elements
- **8-pointed star pattern** (Rub el Hizb) - Central geometric motif
- **Concentric circles** - Inspired by dome architecture
- **Diamond tiles** - Individual mosaic pieces in turquoise
- **Ottoman color palette** - Deep blue, turquoise, gold on white/dark backgrounds
- **Orkinosai sigil** - Company mark integrated subtly

## 🖼️ Available Versions

### Full Logo (800x300)
Use for:
- Website headers
- Email signatures
- Marketing materials
- Large displays

Files:
- `mosaic-logo-main.svg` - Light backgrounds
- `mosaic-logo-dark.svg` - Dark backgrounds

### Icon Only (512x512)
Use for:
- App icons (iOS, Android, PWA)
- Social media profile pictures
- Favicon
- Compact spaces

Files:
- `mosaic-icon.svg` - Light backgrounds
- `mosaic-icon-dark.svg` - Dark backgrounds

## 📐 Size Recommendations

### Web Usage
- **Favicon**: 16x16, 32x32, 64x64 (use icon version)
- **Apple Touch Icon**: 180x180 (use icon version)
- **PWA Icon**: 192x192, 512x512 (use icon version)
- **Header Logo**: 200-300px width (use full logo)
- **Social Share**: 1200x630 (use full logo)

### Print
- Always use vector SVG when possible
- For raster: Export at 2x-3x final size at 300 DPI

## 🎨 Color Palette

### Light Mode
```
Primary Blue:     #1e3a8a → #2563eb (gradient)
Turquoise:        #06b6d4 → #0891b2 (gradient)
Gold:             #fbbf24 → #f59e0b (gradient)
Background:       #ffffff
```

### Dark Mode
```
Primary Blue:     #60a5fa → #3b82f6 (gradient)
Turquoise:        #22d3ee → #06b6d4 (gradient)
Gold:             #fcd34d → #fbbf24 (gradient)
Background:       #0f172a
```

## 🚀 Quick Start

### Using SVG Files Directly
```html
<!-- Light mode logo -->
<img src="logo/src/mosaic-logo-main.svg" alt="MOSAIC" width="300">

<!-- Dark mode logo with picture element -->
<picture>
  <source srcset="logo/src/mosaic-logo-dark.svg" media="(prefers-color-scheme: dark)">
  <img src="logo/src/mosaic-logo-main.svg" alt="MOSAIC" width="300">
</picture>

<!-- Icon/Favicon -->
<link rel="icon" type="image/svg+xml" href="logo/src/mosaic-icon.svg">
```

### Generating PNG Exports
Use any SVG conversion tool or command line:

```bash
# Using ImageMagick
convert -background none logo/src/mosaic-icon.svg -resize 512x512 logo/exports/icon-512.png

# Using Inkscape
inkscape logo/src/mosaic-icon.svg --export-type=png --export-filename=logo/exports/icon-512.png -w 512 -h 512

# Using rsvg-convert
rsvg-convert -w 512 -h 512 logo/src/mosaic-icon.svg > logo/exports/icon-512.png
```

## 📋 Usage Guidelines

### ✅ Recommended
- Use on solid, clean backgrounds
- Maintain proper aspect ratios
- Choose light/dark version based on background
- Provide adequate spacing around logo (minimum 20px)
- Use SVG format whenever possible for scalability

### ❌ Avoid
- Don't stretch, skew, or distort the logo
- Don't modify colors or add effects
- Don't place on busy or patterned backgrounds
- Don't rotate or flip the logo
- Don't use low-resolution raster formats when avoidable

## 🔧 Customization

The SVG files can be edited in:
- **Adobe Illustrator** - Full editing capabilities
- **Inkscape** - Free, open-source alternative
- **Figma** - Modern web-based design tool
- **Any text editor** - SVGs are XML-based

When editing:
1. Maintain the geometric proportions
2. Preserve the Ottoman-inspired color palette
3. Keep gradients for depth and richness
4. Test at multiple scales

## 📄 License

These logo assets are proprietary to Orkinosai and the MOSAIC SaaS product. 
Usage outside of official MOSAIC branding requires permission.

## 📞 Contact

For questions about logo usage or custom variations:
- **Repository**: orkinosai25-org/mosaic
- **Design**: Based on Ottoman/Turkish artistic heritage

---

**Design Inspiration**: Selimiye Mosque (Edirne) • Blue Mosque (Istanbul) • Iznik Tiles  
**Created for**: MOSAIC SaaS by Orkinosai
