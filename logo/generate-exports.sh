#!/bin/bash

# MOSAIC Logo Export Generator
# Generates PNG exports at various sizes from SVG source files

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SRC_DIR="$SCRIPT_DIR/src"
EXPORT_DIR="$SCRIPT_DIR/exports"

# Create export directories
mkdir -p "$EXPORT_DIR"/{favicon,icons,social,web}

echo "🎨 MOSAIC Logo Export Generator"
echo "================================"
echo ""

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Detect available converter
if command_exists rsvg-convert; then
    CONVERTER="rsvg"
    echo "✓ Using rsvg-convert"
elif command_exists inkscape; then
    CONVERTER="inkscape"
    echo "✓ Using Inkscape"
elif command_exists convert; then
    CONVERTER="imagemagick"
    echo "✓ Using ImageMagick"
else
    echo "❌ Error: No SVG converter found!"
    echo "Please install one of: rsvg-convert, inkscape, or imagemagick"
    exit 1
fi
echo ""

# Function to convert SVG to PNG
convert_svg() {
    local input="$1"
    local output="$2"
    local width="$3"
    local height="${4:-$width}"
    
    case $CONVERTER in
        rsvg)
            rsvg-convert -w "$width" -h "$height" "$input" > "$output"
            ;;
        inkscape)
            inkscape "$input" --export-type=png --export-filename="$output" -w "$width" -h "$height" 2>/dev/null
            ;;
        imagemagick)
            convert -background none "$input" -resize "${width}x${height}" "$output"
            ;;
    esac
}

echo "📦 Generating favicon sizes..."
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/favicon/favicon-16.png" 16
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/favicon/favicon-32.png" 32
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/favicon/favicon-64.png" 64
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/favicon/favicon-128.png" 128
echo "  ✓ Generated 16x16, 32x32, 64x64, 128x128"

echo ""
echo "📱 Generating app icon sizes..."
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/icons/icon-180.png" 180
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/icons/icon-192.png" 192
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/icons/icon-512.png" 512
convert_svg "$SRC_DIR/mosaic-icon-dark.svg" "$EXPORT_DIR/icons/icon-512-dark.png" 512
echo "  ✓ Generated 180x180 (iOS), 192x192 (Android), 512x512"

echo ""
echo "🌐 Generating web logo sizes..."
convert_svg "$SRC_DIR/mosaic-logo-main.svg" "$EXPORT_DIR/web/logo-main-400.png" 400 150
convert_svg "$SRC_DIR/mosaic-logo-main.svg" "$EXPORT_DIR/web/logo-main-800.png" 800 300
convert_svg "$SRC_DIR/mosaic-logo-dark.svg" "$EXPORT_DIR/web/logo-dark-400.png" 400 150
convert_svg "$SRC_DIR/mosaic-logo-dark.svg" "$EXPORT_DIR/web/logo-dark-800.png" 800 300
echo "  ✓ Generated 400x150, 800x300 (light and dark)"

echo ""
echo "📱 Generating social media sizes..."
convert_svg "$SRC_DIR/mosaic-logo-main.svg" "$EXPORT_DIR/social/og-image-1200x630.png" 1200 630
convert_svg "$SRC_DIR/mosaic-icon.svg" "$EXPORT_DIR/social/profile-400x400.png" 400 400
echo "  ✓ Generated 1200x630 (Open Graph), 400x400 (Profile)"

echo ""
echo "✨ Export complete!"
echo ""
echo "📁 Files created in: $EXPORT_DIR"
echo "   - favicon/     (16, 32, 64, 128 px)"
echo "   - icons/       (180, 192, 512 px)"
echo "   - web/         (400x150, 800x300)"
echo "   - social/      (1200x630, 400x400)"
echo ""
echo "💡 Tip: Copy these files to your web project's public directory"
