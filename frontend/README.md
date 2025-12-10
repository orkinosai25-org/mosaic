# MOSAIC SaaS Portal Frontend

This is the **SaaS management portal** for the MOSAIC platform, built with React, TypeScript, and Microsoft Fluent UI to closely resemble Azure's portal UI.

## 🎯 Purpose

The portal is the primary interface for:
- **User Registration & Authentication**: Sign up as admin/owner or regular user
- **Multi-Tenant Management**: Create and manage multiple websites from one dashboard
- **Billing & Subscriptions**: Handle payments through Stripe integration
- **Usage Monitoring**: View analytics, storage, bandwidth, and visitor metrics
- **Navigation to CMS**: Access each website's admin configuration (MOSAIC CMS)

**User Flow:**
1. Register/Login → Portal Dashboard
2. Create Website → Portal manages tenant
3. "Configure Site" → Navigate to CMS admin
4. Manage content in CMS, return to Portal for billing/analytics

## 🎨 Design Philosophy

The MOSAIC portal frontend is designed to:
- **Mirror Azure Portal UX**: Familiar navigation patterns and interactions for enterprise users
- **Modern Aesthetics**: Clean, professional design for any business or creative project
- **Session-Aware UI**: Different experiences for authenticated vs unauthenticated users
- **Responsive Design**: Optimized for both desktop and mobile devices

## 🏗️ Architecture

### Technology Stack

- **React 19** - Modern UI library
- **TypeScript** - Type-safe development
- **Vite** - Fast build tool and dev server
- **Fluent UI React** - Microsoft's design system for enterprise applications

### Relationship with MOSAIC CMS

- **Portal (this app)**: SaaS management, billing, site creation
- **CMS (backend)**: Content configuration, admin features, permissions

## 🚀 Getting Started

### Installation

```bash
cd frontend
npm install
```

### Development

```bash
npm run dev
# Open browser to http://localhost:5173
```

### Building

```bash
npm run build
```

## 🧩 Key Components

- **Header Bar**: Logo, platform name, user avatar with menu
- **Navigation**: Dashboard, Sites, Billing, Support, Settings
- **Auth Panel**: OAuth and traditional login options
- **Dashboard**: Usage metrics and quick actions
- **Chat Bubble**: AI-powered conversational agent

---

**Built with ❤️ by Orkinosai**
