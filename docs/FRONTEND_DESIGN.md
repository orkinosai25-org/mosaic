# MOSAIC SaaS Portal - Frontend Design Documentation

## Overview

The MOSAIC SaaS portal frontend is designed to closely resemble Azure's portal UI while providing a modern, professional aesthetic. The application provides an enterprise-grade user experience for managing multi-tenant websites.

## Design Principles

### 1. Azure Portal Similarity
- **Familiar Navigation**: Left sidebar navigation matching Azure's pattern
- **Top Header Bar**: Consistent branding and user controls
- **Card-Based Layout**: Modular dashboard panels
- **Enterprise UX**: Professional, clean, and functional

### 2. Modern Aesthetics
- **Color Palette**: Deep cobalt blues, turquoise accents, gold highlights
- **Professional Design**: Clean, contemporary implementation
- **Geometric Patterns**: Modern visual elements (in logo)
- **Contemporary Application**: Professional and polished

### 3. Session-Aware Interface
- **Unauthenticated State**: Marketing landing page with features
- **Authenticated State**: Full dashboard with navigation
- **Seamless Transition**: Smooth authentication flow

## UI Components

### Header Bar (48px height)

```
┌─────────────────────────────────────────────────────────────┐
│ [Logo] MOSAIC                    [Sign In] [Register]       │
│                                   or [User Avatar ▼]        │
└─────────────────────────────────────────────────────────────┘
```

**Components:**
- MOSAIC logo (32px height)
- Platform name "MOSAIC"
- Authentication buttons (unauthenticated)
- User avatar with dropdown menu (authenticated)

**Colors:**
- Background: Brand Blue (`#1e3a8a`)
- Text: White on brand color

### Navigation Sidebar (240px width)

```
┌──────────────────┐
│ 🏠 Dashboard     │ ← Active state
│ 🏢 Sites         │
│ 💳 Billing       │
│ ❓ Support       │
│ ⚙️  Settings     │
└──────────────────┘
```

**Navigation Items:**
1. Dashboard - Home view with metrics
2. Sites / Workspaces - Manage websites
3. Billing / Subscription - Payment management
4. Support / Help - Documentation and support
5. Settings - User and platform settings

**Interaction:**
- Subtle hover effects
- Active state highlighting
- Icon + text labels
- Responsive collapsing on mobile

### Authentication Panel (Modal Overlay)

```
┌─────────────────────────────────┐
│         Sign In / Register       │
├─────────────────────────────────┤
│ Email:    [_________________]   │
│ Password: [_________________]   │
│                                  │
│ [        Sign In Button       ] │
│                                  │
│ ──────────── OR ──────────────  │
│                                  │
│ [ Continue with Google    ]     │
│ [ Continue with GitHub    ]     │
│ [ Continue with Microsoft ]     │
│                                  │
│ Don't have an account? Register │
└─────────────────────────────────┘
```

**Features:**
- Toggle between Sign In / Register
- Traditional email/password
- OAuth options (Google, GitHub, Microsoft)
- Toggle link to switch modes
- Semi-transparent overlay background

### Dashboard (Authenticated)

```
┌────────────────────────────────────────────────┐
│ 🚀 Migration Toolkit Available                 │
│ [Start Migration] [Dismiss]                    │
├────────────────────────────────────────────────┤
│ Welcome back, [User Name]!                     │
│ Here's what's happening with your sites today  │
├────────────────────────────────────────────────┤
│ Usage Summary                                  │
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐          │
│ │Sites │ │Storage│ │Bandwidth│ │Visitors│      │
│ │  3   │ │2.4 GB │ │15.8 GB│ │12,453│        │
│ └──────┘ └──────┘ └──────┘ └──────┘          │
├────────────────────────────────────────────────┤
│ Quick Actions                                  │
│ [Create Site] [Analytics] [Billing] [Settings]│
└────────────────────────────────────────────────┘
```

**Cards:**
1. **Migration Banner**: Info banner with action buttons
2. **Welcome Section**: Personalized greeting
3. **Usage Summary**: Key metrics in grid
4. **Quick Actions**: Common tasks as buttons

### Landing Page (Unauthenticated)

```
┌────────────────────────────────────────────┐
│        Welcome to MOSAIC                    │
│   [Hero text and description]               │
│   [Get Started Free] [Sign In]              │
├────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐    │
│ │ Ottoman  │ │  Multi-  │ │ Flexible │    │
│ │ Design   │ │  Tenant  │ │ Billing  │    │
│ └──────────┘ └──────────┘ └──────────┘    │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐    │
│ │Analytics │ │    AI    │ │   API    │    │
│ │          │ │  Agents  │ │Integration│   │
│ └──────────┘ └──────────┘ └──────────┘    │
└────────────────────────────────────────────┘
```

**Sections:**
1. **Hero**: Large title, value proposition, CTA buttons
2. **Features Grid**: 6 cards showcasing platform capabilities

### Footer

```
┌─────────────────────────────────────────────┐
│ Product     Resources   Support    Company  │
│ Features    Docs        Help       GitHub   │
│ Pricing     Guides      Contact    About    │
│ API Docs    Tutorials   Forum      Social   │
├─────────────────────────────────────────────┤
│    Built with ❤️ by Orkinosai                │
│    Inspired by Ottoman heritage •           │
│    Powered by modern technology             │
└─────────────────────────────────────────────┘
```

**Four Columns:**
- Product links
- Resource links
- Support links
- Company information

### Chat Bubble (Fixed Position)

```
                                    ┌──────────┐
                                    │ MOSAIC   │
                                    │ Agent  [X]│
                                    ├──────────┤
                                    │ 👋 Hi!   │
                                    │ How can  │
                                    │ I help?  │
                                    └──────────┘
                                         │
                                    ┌────┴────┐
                                    │   💬    │
                                    └─────────┘
```

**Position:** Bottom-right corner (24px from edges)
**Features:**
- Expandable chat panel
- AI conversational agent
- Fixed floating button

## Color System

### Primary Colors

| Color Name | Hex Code | Usage |
|------------|----------|-------|
| Ottoman Blue | `#1e3a8a` | Header background, primary buttons |
| Ottoman Blue Light | `#2563eb` | Hover states, accents |
| Iznik Turquoise | `#06b6d4` | Secondary buttons, links |
| Iznik Turquoise Light | `#22d3ee` | Hover states, highlights |
| Royal Gold | `#fbbf24` | Important highlights, badges |
| Royal Gold Dark | `#f59e0b` | Gold hover states |

### Neutral Colors

| Color Name | Hex Code | Usage |
|------------|----------|-------|
| Pure White | `#ffffff` | Backgrounds, cards |
| Light Gray | `#f8fafc` | Page background |
| Dark Slate | `#0f172a` | Dark mode background |
| Text Dark | `#1e293b` | Primary text |
| Text Gray | `#64748b` | Secondary text |

## Typography

- **Font Family**: System fonts (Segoe UI, Roboto, Arial)
- **Heading Sizes**: 
  - H1: 48px (Hero)
  - H2: 32px (Section titles)
  - H3: 24px (Card titles)
- **Body Text**: 14-16px
- **Small Text**: 12px (Labels, captions)

## Responsive Breakpoints

| Breakpoint | Width | Layout |
|------------|-------|--------|
| Desktop | 1200px+ | Full navigation, multi-column |
| Tablet | 768px - 1199px | Adapted layouts |
| Mobile | < 768px | Stacked, mobile navigation |

## Wireframes

### Unauthenticated View
![Landing Page](https://github.com/user-attachments/assets/702146b5-8a9d-4748-8132-0656247e3c00)

### Authentication Panel
![Auth Panel](https://github.com/user-attachments/assets/b78cfe41-3984-4962-b162-32da279f2006)

### Authenticated Dashboard
![Dashboard](https://github.com/user-attachments/assets/c7fe5b1f-5971-408a-bf90-f8c9a0783f11)

### User Menu
![User Menu](https://github.com/user-attachments/assets/14b8acc0-b508-4739-bc68-aba4679fd173)

### Chat Agent
![Chat Bubble](https://github.com/user-attachments/assets/7fc15089-f7c3-4e6d-9b69-273814dea43a)

## Implementation Notes

### Fluent UI Components Used

- `Button` - All interactive buttons
- `Card` - Dashboard cards and feature cards
- `Avatar` - User profile avatar
- `Menu` - User dropdown menu
- `Input` - Form inputs
- `MessageBar` - Migration banner
- `Divider` - Visual separators
- `Link` - Footer links

### State Management

Currently using React hooks:
- `useState` - Component state
- Future: Redux or Zustand for global state

### Authentication Flow

1. User clicks "Sign In" or "Register"
2. Modal overlay appears with auth panel
3. User selects OAuth or enters credentials
4. On success: User object is set, UI updates
5. Avatar replaces auth buttons
6. Navigation sidebar appears
7. Dashboard loads with user data

## Accessibility

- Semantic HTML structure
- ARIA labels on interactive elements
- Keyboard navigation support
- Focus management
- Screen reader compatibility
- High contrast ratios (WCAG AA compliant)

## Browser Support

- Chrome/Edge (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Mobile browsers (iOS Safari, Chrome Mobile)

## Performance

- Code splitting with Vite
- Lazy loading for routes
- Optimized bundle size (~570KB gzipped)
- Fast initial load (<3s on 3G)

## Future Enhancements

- [ ] Dark mode toggle
- [ ] Internationalization (i18n)
- [ ] Advanced routing
- [ ] State management library
- [ ] Real-time updates via SignalR
- [ ] Progressive Web App features
- [ ] Offline support
- [ ] Analytics integration

---

**Last Updated**: December 2024  
**Version**: 1.0.0  
**Design Team**: Orkinosai
