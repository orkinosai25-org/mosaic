# MOSAIC Portal UI Design

This document describes the Azure-style portal user interface for the MOSAIC SaaS platform, including design patterns, layout specifications, and Ottoman-inspired branding elements.

## 🎨 Design Philosophy

MOSAIC's portal UI combines three key design influences:

1. **Azure Portal Style** - Modern, professional enterprise interface
2. **Ottoman Heritage** - Geometric patterns, Iznik tiles, and cultural aesthetics
3. **Fluent Design System** - Microsoft's design language for consistency and accessibility

### Core Principles

- **Clarity**: Clear information hierarchy and intuitive navigation
- **Efficiency**: Quick access to common tasks and workflows
- **Beauty**: Ottoman-inspired visual elements that celebrate cultural heritage
- **Accessibility**: WCAG 2.1 AA compliant for all users
- **Responsiveness**: Seamless experience across devices

## 🏗️ Layout Structure

### Overall Portal Layout

```
┌─────────────────────────────────────────────────────────────┐
│                        HEADER (60px)                         │
│  [Logo] [Title]              [Search]  [Notifications] [👤] │
├──────┬──────────────────────────────────────────────────────┤
│      │                                                       │
│      │                                                       │
│  S   │                  MAIN CONTENT AREA                   │
│  I   │                                                       │
│  D   │  ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│  E   │  │   Card 1   │  │   Card 2   │  │   Card 3   │    │
│  B   │  └────────────┘  └────────────┘  └────────────┘    │
│  A   │                                                       │
│  R   │  ┌──────────────────────────────────────────────┐   │
│      │  │          Content Panel                        │   │
│ (240)│  └──────────────────────────────────────────────┘   │
│      │                                                       │
├──────┴──────────────────────────────────────────────────────┤
│                        FOOTER (40px)                         │
│  [Links]  [Privacy]  [Terms]  [Contact]       [Social]      │
└─────────────────────────────────────────────────────────────┘
```

### Responsive Breakpoints

```css
/* Desktop */
@media (min-width: 1200px) {
  .sidebar { width: 240px; }
  .main-content { margin-left: 240px; }
}

/* Tablet */
@media (max-width: 1199px) and (min-width: 768px) {
  .sidebar { width: 200px; }
  .main-content { margin-left: 200px; }
}

/* Mobile */
@media (max-width: 767px) {
  .sidebar { 
    position: absolute;
    transform: translateX(-100%);
    transition: transform 0.3s;
  }
  .sidebar.open {
    transform: translateX(0);
  }
  .main-content { margin-left: 0; }
}
```

## 🎯 Header Design

### Header Components

```html
<header class="mosaic-header">
  <div class="header-left">
    <!-- Logo & Brand -->
    <button class="hamburger-menu" aria-label="Toggle menu">
      <span></span>
      <span></span>
      <span></span>
    </button>
    <img src="/assets/logo.svg" alt="MOSAIC" class="header-logo" />
    <span class="header-title">MOSAIC</span>
    <span class="header-subtitle">SaaS Platform</span>
  </div>
  
  <div class="header-center">
    <!-- Search -->
    <div class="search-box">
      <input 
        type="search" 
        placeholder="Search sites, pages, settings..." 
        aria-label="Search"
      />
      <button class="search-button">🔍</button>
    </div>
  </div>
  
  <div class="header-right">
    <!-- Notifications -->
    <button class="notification-button" aria-label="Notifications">
      🔔
      <span class="notification-badge">3</span>
    </button>
    
    <!-- User Profile -->
    <div class="user-profile">
      <img src="/user-avatar.jpg" alt="User" class="user-avatar" />
      <span class="user-name">John Doe</span>
      <button class="dropdown-trigger" aria-label="User menu">▼</button>
    </div>
  </div>
</header>
```

### Header Styling

```css
.mosaic-header {
  height: 60px;
  background: linear-gradient(135deg, #1e3a8a 0%, #2563eb 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  position: sticky;
  top: 0;
  z-index: 1000;
}

.header-logo {
  width: 40px;
  height: 40px;
  margin-right: 12px;
}

.header-title {
  font-size: 20px;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.search-box {
  width: 400px;
  position: relative;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 4px;
  padding: 8px 12px;
  display: flex;
  align-items: center;
}

.search-box input {
  background: transparent;
  border: none;
  color: white;
  width: 100%;
  outline: none;
}

.search-box input::placeholder {
  color: rgba(255, 255, 255, 0.7);
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 2px solid white;
  margin-right: 8px;
}

.notification-badge {
  position: absolute;
  top: -4px;
  right: -4px;
  background: #ef4444;
  color: white;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  font-size: 11px;
  display: flex;
  align-items: center;
  justify-content: center;
}
```

### User Dropdown Menu

```html
<div class="user-dropdown">
  <div class="dropdown-header">
    <img src="/user-avatar.jpg" alt="User" class="dropdown-avatar" />
    <div class="dropdown-user-info">
      <div class="dropdown-name">John Doe</div>
      <div class="dropdown-email">john@example.com</div>
      <div class="dropdown-tier">Pro Plan</div>
    </div>
  </div>
  
  <div class="dropdown-divider"></div>
  
  <ul class="dropdown-menu">
    <li><a href="/profile">👤 My Profile</a></li>
    <li><a href="/settings">⚙️ Account Settings</a></li>
    <li><a href="/billing">💳 Billing</a></li>
    <li><a href="/api-keys">🔑 API Keys</a></li>
    <li class="dropdown-divider"></li>
    <li><a href="/docs">📚 Documentation</a></li>
    <li><a href="/support">🆘 Support</a></li>
    <li class="dropdown-divider"></li>
    <li><a href="/logout" class="logout">🚪 Sign Out</a></li>
  </ul>
</div>
```

## 📱 Sidebar Navigation

### Sidebar Structure

```html
<aside class="mosaic-sidebar">
  <!-- Main Navigation -->
  <nav class="sidebar-nav">
    <ul class="nav-list">
      <li class="nav-item active">
        <a href="/dashboard" class="nav-link">
          <span class="nav-icon">🏠</span>
          <span class="nav-label">Home</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/sites" class="nav-link">
          <span class="nav-icon">🌐</span>
          <span class="nav-label">Sites</span>
          <span class="nav-count">5</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/content" class="nav-link">
          <span class="nav-icon">📄</span>
          <span class="nav-label">Content</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/media" class="nav-link">
          <span class="nav-icon">🖼️</span>
          <span class="nav-label">Media</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/analytics" class="nav-link">
          <span class="nav-icon">📊</span>
          <span class="nav-label">Analytics</span>
        </a>
      </li>
      
      <li class="nav-divider"></li>
      
      <li class="nav-item">
        <a href="/billing" class="nav-link">
          <span class="nav-icon">💳</span>
          <span class="nav-label">Billing</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/team" class="nav-link">
          <span class="nav-icon">👥</span>
          <span class="nav-label">Team</span>
        </a>
      </li>
      
      <li class="nav-item">
        <a href="/settings" class="nav-link">
          <span class="nav-icon">⚙️</span>
          <span class="nav-label">Settings</span>
        </a>
      </li>
      
      <li class="nav-divider"></li>
      
      <li class="nav-item">
        <a href="/support" class="nav-link">
          <span class="nav-icon">🆘</span>
          <span class="nav-label">Support</span>
        </a>
      </li>
    </ul>
  </nav>
  
  <!-- Ottoman Pattern Decoration -->
  <div class="sidebar-pattern">
    <svg class="ottoman-pattern" viewBox="0 0 200 200">
      <!-- 8-pointed star pattern -->
      <path d="M100,50 L110,80 L140,90 L110,100 L100,130 L90,100 L60,90 L90,80 Z" 
            fill="url(#gradient)" opacity="0.1" />
    </svg>
  </div>
</aside>
```

### Sidebar Styling

```css
.mosaic-sidebar {
  width: 240px;
  height: calc(100vh - 60px);
  background: #f8fafc;
  border-right: 1px solid #e2e8f0;
  position: fixed;
  left: 0;
  top: 60px;
  overflow-y: auto;
  transition: transform 0.3s ease;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 12px 20px;
  color: #475569;
  text-decoration: none;
  transition: all 0.2s;
  border-left: 3px solid transparent;
}

.nav-link:hover {
  background: #e2e8f0;
  color: #1e3a8a;
}

.nav-item.active .nav-link {
  background: #eff6ff;
  color: #2563eb;
  border-left-color: #2563eb;
  font-weight: 600;
}

.nav-icon {
  font-size: 20px;
  margin-right: 12px;
  width: 24px;
  text-align: center;
}

.nav-label {
  flex: 1;
}

.nav-count {
  background: #e2e8f0;
  color: #64748b;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.nav-divider {
  height: 1px;
  background: #e2e8f0;
  margin: 12px 20px;
}

.sidebar-pattern {
  position: absolute;
  bottom: 20px;
  left: 0;
  right: 0;
  text-align: center;
  pointer-events: none;
}
```

## 🎴 Main Content Area

### Dashboard Layout

```html
<main class="main-content">
  <!-- Page Header -->
  <div class="page-header">
    <div class="page-title-section">
      <h1 class="page-title">Dashboard</h1>
      <p class="page-subtitle">Welcome back, John!</p>
    </div>
    <div class="page-actions">
      <button class="btn btn-primary">
        <span class="btn-icon">➕</span>
        New Site
      </button>
    </div>
  </div>
  
  <!-- Breadcrumbs -->
  <nav class="breadcrumb" aria-label="Breadcrumb">
    <ol>
      <li><a href="/">Home</a></li>
      <li aria-current="page">Dashboard</li>
    </ol>
  </nav>
  
  <!-- Content Grid -->
  <div class="content-grid">
    <!-- Stats Cards -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-icon">🌐</div>
        <div class="stat-content">
          <div class="stat-value">5</div>
          <div class="stat-label">Active Sites</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">👥</div>
        <div class="stat-content">
          <div class="stat-value">2,340</div>
          <div class="stat-label">Total Visitors</div>
          <div class="stat-change positive">+12.5%</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">📊</div>
        <div class="stat-content">
          <div class="stat-value">8.2 GB</div>
          <div class="stat-label">Bandwidth Used</div>
          <div class="stat-progress">
            <div class="progress-bar" style="width: 82%"></div>
          </div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">💾</div>
        <div class="stat-content">
          <div class="stat-value">42 GB</div>
          <div class="stat-label">Storage Used</div>
          <div class="stat-progress">
            <div class="progress-bar" style="width: 42%"></div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Main Content Sections -->
    <div class="content-panels">
      <!-- Sites Panel -->
      <div class="panel">
        <div class="panel-header">
          <h2 class="panel-title">Your Sites</h2>
          <a href="/sites" class="panel-action">View All →</a>
        </div>
        <div class="panel-content">
          <!-- Site cards list -->
        </div>
      </div>
      
      <!-- Activity Panel -->
      <div class="panel">
        <div class="panel-header">
          <h2 class="panel-title">Recent Activity</h2>
        </div>
        <div class="panel-content">
          <!-- Activity timeline -->
        </div>
      </div>
    </div>
  </div>
</main>
```

### Card Components

```css
.stat-card {
  background: white;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 16px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.stat-icon {
  font-size: 32px;
  width: 56px;
  height: 56px;
  background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: #1e293b;
  line-height: 1;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #64748b;
}

.stat-change {
  font-size: 12px;
  font-weight: 600;
  margin-top: 4px;
}

.stat-change.positive {
  color: #10b981;
}

.stat-change.negative {
  color: #ef4444;
}

.panel {
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.panel-header {
  padding: 20px 24px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-title {
  font-size: 18px;
  font-weight: 600;
  color: #1e293b;
}

.panel-content {
  padding: 24px;
}
```

## 🎨 Ottoman-Inspired Design Elements

### Color Palette

```css
:root {
  /* Primary Colors - Selimiye Collection */
  --mosaic-blue-primary: #1e3a8a;
  --mosaic-blue-secondary: #2563eb;
  --mosaic-blue-light: #3b82f6;
  --mosaic-blue-lighter: #60a5fa;
  
  /* Turquoise Accents */
  --mosaic-turquoise: #06b6d4;
  --mosaic-turquoise-light: #22d3ee;
  --mosaic-cyan: #0891b2;
  
  /* Gold Highlights */
  --mosaic-gold: #fbbf24;
  --mosaic-gold-dark: #f59e0b;
  --mosaic-amber: #fcd34d;
  
  /* Neutral Colors */
  --mosaic-white: #ffffff;
  --mosaic-pearl: #f8fafc;
  --mosaic-silver: #e2e8f0;
  --mosaic-slate: #64748b;
  --mosaic-dark: #1e293b;
  
  /* Semantic Colors */
  --mosaic-success: #10b981;
  --mosaic-warning: #f59e0b;
  --mosaic-error: #ef4444;
  --mosaic-info: #3b82f6;
}
```

### 8-Pointed Star Pattern (Rub el Hizb)

```html
<svg class="ottoman-star" viewBox="0 0 100 100">
  <defs>
    <linearGradient id="starGradient" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#2563eb;stop-opacity:1" />
      <stop offset="100%" style="stop-color:#06b6d4;stop-opacity:1" />
    </linearGradient>
  </defs>
  
  <!-- Main 8-pointed star -->
  <path d="M50,10 L55,40 L70,35 L60,50 L70,65 L55,60 L50,90 L45,60 L30,65 L40,50 L30,35 L45,40 Z" 
        fill="url(#starGradient)" 
        opacity="0.8" />
  
  <!-- Inner diamond -->
  <path d="M50,25 L60,50 L50,75 L40,50 Z" 
        fill="#fbbf24" 
        opacity="0.6" />
</svg>
```

### Geometric Border Patterns

```css
.ottoman-border {
  position: relative;
  padding: 24px;
}

.ottoman-border::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(
    90deg,
    #2563eb 0%,
    #06b6d4 25%,
    #fbbf24 50%,
    #06b6d4 75%,
    #2563eb 100%
  );
  background-size: 200% 100%;
  animation: shimmer 3s infinite;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.tile-pattern-bg {
  background-image: 
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      rgba(37, 99, 235, 0.03) 10px,
      rgba(37, 99, 235, 0.03) 20px
    ),
    repeating-linear-gradient(
      -45deg,
      transparent,
      transparent 10px,
      rgba(6, 182, 212, 0.03) 10px,
      rgba(6, 182, 212, 0.03) 20px
    );
}
```

### Iznik Tile Card Decoration

```css
.card-iznik {
  position: relative;
  overflow: hidden;
}

.card-iznik::after {
  content: '';
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(
    circle,
    rgba(37, 99, 235, 0.05) 0%,
    transparent 70%
  );
  pointer-events: none;
}

.card-iznik-corner {
  position: absolute;
  top: 0;
  right: 0;
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #2563eb 0%, #06b6d4 100%);
  clip-path: polygon(100% 0, 100% 100%, 0 0);
  opacity: 0.1;
}
```

## 📋 Welcome/Landing View (Not Logged In)

### Landing Page Structure

```html
<div class="landing-page">
  <!-- Hero Section -->
  <section class="hero">
    <div class="hero-pattern"></div>
    <div class="hero-content">
      <img src="/assets/logo-large.svg" alt="MOSAIC" class="hero-logo" />
      <h1 class="hero-title">
        Create Beautiful Websites with
        <span class="gradient-text">Ottoman-Inspired Design</span>
      </h1>
      <p class="hero-subtitle">
        The first conversational, AI-powered SaaS platform for website creation and management
      </p>
      
      <div class="hero-actions">
        <a href="/register" class="btn btn-primary btn-large">
          Get Started Free
        </a>
        <a href="/signin" class="btn btn-secondary btn-large">
          Sign In
        </a>
      </div>
      
      <div class="hero-features">
        <div class="feature-badge">
          <span class="badge-icon">🤖</span>
          <span class="badge-text">AI-Powered</span>
        </div>
        <div class="feature-badge">
          <span class="badge-icon">🎨</span>
          <span class="badge-text">Ottoman Design</span>
        </div>
        <div class="feature-badge">
          <span class="badge-icon">☁️</span>
          <span class="badge-text">Azure Powered</span>
        </div>
      </div>
    </div>
  </section>
  
  <!-- Benefits Section -->
  <section class="benefits">
    <div class="container">
      <h2 class="section-title">Why Choose MOSAIC?</h2>
      
      <div class="benefits-grid">
        <div class="benefit-card">
          <div class="benefit-icon">💬</div>
          <h3 class="benefit-title">Conversational Site Builder</h3>
          <p class="benefit-description">
            Create and manage sites via natural language with MOSAIC Agent chat
          </p>
        </div>
        
        <div class="benefit-card">
          <div class="benefit-icon">🔄</div>
          <h3 class="benefit-title">Easy Migration</h3>
          <p class="benefit-description">
            Migrate from Wix, Umbraco, SharePoint via guided workflows
          </p>
        </div>
        
        <div class="benefit-card">
          <div class="benefit-icon">🏛️</div>
          <h3 class="benefit-title">Ottoman-Inspired Themes</h3>
          <p class="benefit-description">
            Beautiful designs inspired by Selimiye Mosque and Blue Mosque
          </p>
        </div>
        
        <div class="benefit-card">
          <div class="benefit-icon">⚡</div>
          <h3 class="benefit-title">Fast Onboarding</h3>
          <p class="benefit-description">
            Quick registration, OAuth, onboarding chat, demo wizards
          </p>
        </div>
        
        <div class="benefit-card">
          <div class="benefit-icon">🔒</div>
          <h3 class="benefit-title">Azure Security</h3>
          <p class="benefit-description">
            Enterprise-grade security and scalability with Azure infrastructure
          </p>
        </div>
        
        <div class="benefit-card">
          <div class="benefit-icon">💰</div>
          <h3 class="benefit-title">Flexible Pricing</h3>
          <p class="benefit-description">
            Freemium, paid plans, Stripe integration, Azure-mapped tiers
          </p>
        </div>
      </div>
    </div>
  </section>
  
  <!-- Migration Banner -->
  <section class="migration-banner">
    <div class="container">
      <div class="banner-content">
        <h3>Migrating from another platform?</h3>
        <p>We'll help you move from Wix, Umbraco, or SharePoint with zero downtime</p>
        <a href="/migration" class="btn btn-light">Learn About Migration</a>
      </div>
    </div>
  </section>
</div>

<!-- Live Chat Bubble (Always Visible) -->
<button class="chat-bubble" aria-label="Chat with MOSAIC Agent">
  💬
</button>
```

## 🎭 Footer Design

```html
<footer class="mosaic-footer">
  <div class="footer-container">
    <div class="footer-grid">
      <!-- Brand Column -->
      <div class="footer-column">
        <img src="/assets/logo.svg" alt="MOSAIC" class="footer-logo" />
        <p class="footer-tagline">
          First conversational, AI-powered SaaS platform with Ottoman-inspired design
        </p>
        <div class="footer-social">
          <a href="#" aria-label="Twitter">🐦</a>
          <a href="#" aria-label="LinkedIn">💼</a>
          <a href="#" aria-label="GitHub">💻</a>
          <a href="#" aria-label="YouTube">📺</a>
        </div>
      </div>
      
      <!-- Product Column -->
      <div class="footer-column">
        <h4 class="footer-heading">Product</h4>
        <ul class="footer-links">
          <li><a href="/features">Features</a></li>
          <li><a href="/pricing">Pricing</a></li>
          <li><a href="/migration">Migration</a></li>
          <li><a href="/demo">Demo</a></li>
        </ul>
      </div>
      
      <!-- Resources Column -->
      <div class="footer-column">
        <h4 class="footer-heading">Resources</h4>
        <ul class="footer-links">
          <li><a href="/docs">Documentation</a></li>
          <li><a href="/api">API Reference</a></li>
          <li><a href="/tutorials">Tutorials</a></li>
          <li><a href="/blog">Blog</a></li>
        </ul>
      </div>
      
      <!-- Company Column -->
      <div class="footer-column">
        <h4 class="footer-heading">Company</h4>
        <ul class="footer-links">
          <li><a href="/about">About</a></li>
          <li><a href="/contact">Contact</a></li>
          <li><a href="/careers">Careers</a></li>
          <li><a href="/press">Press</a></li>
        </ul>
      </div>
      
      <!-- Legal Column -->
      <div class="footer-column">
        <h4 class="footer-heading">Legal</h4>
        <ul class="footer-links">
          <li><a href="/privacy">Privacy Policy</a></li>
          <li><a href="/terms">Terms of Service</a></li>
          <li><a href="/security">Security</a></li>
          <li><a href="/compliance">Compliance</a></li>
        </ul>
      </div>
    </div>
    
    <div class="footer-bottom">
      <div class="footer-copyright">
        © 2024 Orkinosai. Built with ❤️ • Inspired by Ottoman heritage • Powered by Azure
      </div>
      <div class="footer-pattern">
        <!-- Ottoman pattern decoration -->
      </div>
    </div>
  </div>
</footer>
```

## 🔔 Notification System

### Notification Panel

```html
<div class="notification-panel">
  <div class="notification-header">
    <h3>Notifications</h3>
    <button class="mark-all-read">Mark all as read</button>
  </div>
  
  <div class="notification-list">
    <div class="notification unread">
      <div class="notification-icon success">✓</div>
      <div class="notification-content">
        <div class="notification-title">Site published successfully</div>
        <div class="notification-message">
          Your site "My Portfolio" is now live at myportfolio.mosaic.app
        </div>
        <div class="notification-time">2 minutes ago</div>
      </div>
    </div>
    
    <div class="notification">
      <div class="notification-icon info">ℹ️</div>
      <div class="notification-content">
        <div class="notification-title">Domain verification required</div>
        <div class="notification-message">
          Complete DNS setup to activate your custom domain
        </div>
        <div class="notification-time">1 hour ago</div>
      </div>
    </div>
    
    <div class="notification">
      <div class="notification-icon warning">⚠️</div>
      <div class="notification-content">
        <div class="notification-title">Approaching bandwidth limit</div>
        <div class="notification-message">
          You've used 85% of your monthly bandwidth allocation
        </div>
        <div class="notification-time">3 hours ago</div>
      </div>
    </div>
  </div>
</div>
```

## 💬 Live Chat Widget

### Chat Bubble & Window

```html
<!-- Chat Bubble (Floating) -->
<button class="chat-bubble" id="chatToggle">
  <span class="chat-icon">💬</span>
  <span class="chat-badge">1</span>
</button>

<!-- Chat Window -->
<div class="chat-window" id="chatWindow">
  <div class="chat-header">
    <div class="chat-agent-info">
      <div class="chat-avatar">🤖</div>
      <div>
        <div class="chat-agent-name">MOSAIC Agent</div>
        <div class="chat-agent-status">● Online</div>
      </div>
    </div>
    <button class="chat-close">×</button>
  </div>
  
  <div class="chat-messages">
    <div class="chat-message agent">
      <div class="message-avatar">🤖</div>
      <div class="message-bubble">
        <div class="message-text">
          Hi! I'm MOSAIC Agent. How can I help you today?
        </div>
        <div class="message-time">Just now</div>
      </div>
    </div>
    
    <div class="chat-message user">
      <div class="message-bubble">
        <div class="message-text">
          How do I add a custom domain?
        </div>
        <div class="message-time">Just now</div>
      </div>
      <div class="message-avatar">👤</div>
    </div>
  </div>
  
  <div class="chat-input-area">
    <input 
      type="text" 
      class="chat-input" 
      placeholder="Type your message..." 
      aria-label="Chat message"
    />
    <button class="chat-send">➤</button>
  </div>
</div>
```

## 📚 UI Component Library

### Buttons

```css
.btn {
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: 600;
  font-size: 14px;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.btn-primary {
  background: linear-gradient(135deg, #2563eb 0%, #1e3a8a 100%);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.4);
}

.btn-secondary {
  background: white;
  color: #2563eb;
  border: 2px solid #2563eb;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-large {
  padding: 14px 28px;
  font-size: 16px;
}
```

### Form Inputs

```css
.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  color: #1e293b;
}

.form-input {
  width: 100%;
  padding: 10px 14px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-input-error {
  border-color: #ef4444;
}

.form-error-message {
  color: #ef4444;
  font-size: 12px;
  margin-top: 4px;
}
```

## 🎯 Accessibility Features

### ARIA Labels & Roles

All interactive elements include proper ARIA labels:

```html
<button aria-label="Close menu">×</button>
<nav aria-label="Main navigation">...</nav>
<input aria-describedby="email-error" />
<div role="alert" aria-live="polite">...</div>
```

### Keyboard Navigation

- Tab through all interactive elements
- Enter/Space to activate buttons and links
- Escape to close modals and dropdowns
- Arrow keys for menu navigation

### Focus Management

```css
*:focus-visible {
  outline: 2px solid #2563eb;
  outline-offset: 2px;
}

.skip-link {
  position: absolute;
  top: -40px;
  left: 0;
  background: #2563eb;
  color: white;
  padding: 8px;
  z-index: 100;
}

.skip-link:focus {
  top: 0;
}
```

## 📱 Mobile Responsive Design

### Mobile Menu

```css
@media (max-width: 767px) {
  .mobile-menu-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 999;
    opacity: 0;
    pointer-events: none;
    transition: opacity 0.3s;
  }
  
  .mobile-menu-overlay.open {
    opacity: 1;
    pointer-events: auto;
  }
  
  .mosaic-sidebar {
    transform: translateX(-100%);
    z-index: 1000;
  }
  
  .mosaic-sidebar.open {
    transform: translateX(0);
  }
}
```

## 📚 Additional Resources

- [Architecture Overview](./architecture.md)
- [SaaS Features](./SaaS_FEATURES.md)
- [Onboarding Guide](./ONBOARDING.md)
- [Migration Toolkit](./migration.md)
- [Azure Fluent UI](https://developer.microsoft.com/fluentui)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
