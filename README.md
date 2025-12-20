# Mosaic Conversational CMS (SaaS Platform)

**Mosaic Conversational CMS** is the world's first conversational SaaS platform empowering businesses and creators to build, manage, and scale multi-tenant websites—using natural language, not configuration. Built by [Orkinosai](https://github.com/orkinosai25-org), Mosaic combines powerful enterprise features with an innovative AI-powered conversational interface.

![MOSAIC Logo](./logo/src/mosaic-logo-main.svg)

---

## 🎨 About the MOSAIC Name

**Why "MOSAIC"?** The name embodies the essence of our platform and honors our cultural heritage:

### Not an Acronym
MOSAIC is **not an acronym** - it's a conceptual brand name representing the intersection of:

- **🧩 Modularity**: Like individual tiles forming a complete mosaic, our CMS combines modular components into cohesive websites
- **🎯 Pattern & Precision**: Geometric patterns and clean architectural design inspired by world-class craftsmanship
- **🏛️ Heritage**: Deep roots in **Ottoman architecture** - particularly the magnificent mosaic tile work of:
  - **Selimiye Mosque** (Edirne, 1568-1575) - Mimar Sinan's masterpiece with perfect geometric harmony
  - **Blue Mosque** (Istanbul, 1609-1616) - Famous for over 20,000 Iznik tiles in cobalt blue and turquoise
  - **Iznik tile art** - Centuries-old ceramic craft that inspired our logo's colors and 8-pointed star patterns
- **🤖 AI-Powered Innovation**: Conversational technology that transforms traditional CMS interaction into natural dialogue

### Visual Identity
Our logo and brand colors directly reflect this heritage:
- **8-pointed stars** (Rub el Hizb) found in Ottoman mosque decorations
- **Cobalt blue** (#1e3a8a) - the signature Iznik tile color
- **Turquoise** (#06b6d4) - common in Ottoman ceramics
- **Gold accents** (#fbbf24) - representing craftsmanship and quality

**📖 Learn more**: See [BRAND.md](./BRAND.md) for the complete story behind the MOSAIC name, including design inspirations and cultural context.

---

## ⚠️ Important: Licensing and Codebase Scope

**This codebase is proprietary software** for the **Mosaic Conversational CMS SaaS platform**.

### Key Clarifications:

1. **This is NOT open-source software** - This repository contains proprietary code for Mosaic's commercial SaaS offering and is licensed under the Apache License 2.0 with specific usage restrictions.

2. **This is NOT the on-premises OrkinosaiCMS** - A separate, open-source, on-premises CMS named "OrkinosaiCMS" is planned for future release. That project will be truly open-source and designed for self-hosting.

3. **Separation of Projects**:
   - **Mosaic Conversational CMS** (this repo): Proprietary SaaS platform with multi-tenancy, subscriptions, and cloud-native features
   - **OrkinosaiCMS** (future): Open-source, on-premises CMS for self-hosted deployments

4. **Foundation and Heritage**: This SaaS platform is built upon CMS foundations that share architectural concepts with the planned OrkinosaiCMS, but they are distinct projects with different purposes and licenses.

### Usage Rights:

- ✅ You may use this code to learn, evaluate, and understand the architecture
- ✅ You may fork and experiment with this code for personal, non-commercial purposes
- ❌ You may NOT use this code to create competing commercial SaaS offerings
- ❌ You may NOT redistribute this code as part of a commercial product
- ❌ You may NOT claim this is open-source software

For commercial licensing inquiries, please contact: support@orkinosai.com

---

## 🏛️ Heritage & Foundation

Mosaic Conversational CMS is built upon a modern, modular CMS foundation using .NET 10 and Blazor. We've transformed this foundation into a comprehensive multi-tenant SaaS platform while preserving its core architectural strengths:

- **Modular Architecture**: Plugin-based system for unlimited extensibility
- **Clean Architecture**: Clear separation between Core, Infrastructure, and UI layers
- **SharePoint-Inspired Design**: Familiar concepts like Master Pages, Web Parts (Modules), and permission levels
- **Modern Stack**: Built on .NET 10, Blazor, and Entity Framework Core
- **Flexible Permissions**: Fine-grained, SharePoint-style permission system

### Evolution to Conversational SaaS

The transformation to Mosaic Conversational CMS represents a strategic evolution:

| Aspect | CMS Foundation | Mosaic Conversational CMS (SaaS) |
|--------|----------------|----------------------------------|
| **Purpose** | Single-tenant CMS | Multi-tenant SaaS platform |
| **Target Users** | Individual websites | Businesses and creators |
| **Architecture** | Modular monolith | Multi-tenant with tenant isolation |
| **Deployment** | Self-hosted | Cloud-native (Azure) |
| **Interface** | Traditional config | Conversational AI builder |
| **AI Integration** | Basic (Zoota chat) | Dual AI agents (Mosaic + Zoota Admin) |
| **Payment** | Not included | Integrated Stripe + Microsoft Founder Hub |
| **Onboarding** | Manual setup | Streamlined sign-up with guided wizard |

## 🏗️ Architecture Overview

Mosaic Conversational CMS consists of two main components working together:

### 1. **Mosaic Portal** (Frontend - React + Fluent UI)
The SaaS management portal where users:
- **Register and authenticate** (as admin/owner or regular user)
- **Manage subscriptions and billing** through Stripe integration
- **Create and manage multiple websites** (multi-tenant dashboard)
- **Access analytics and usage metrics**
- **Navigate to CMS configuration** for each website

### 2. **Mosaic CMS Backend** (.NET 10 + Blazor)
The content management system that powers each website:
- **Configure website content and structure** through admin features
- **Manage pages, modules, and web parts** (SharePoint-inspired)
- **Control permissions and user access**
- **Customize themes and branding**
- **Use AI assistance** (Zoota Admin Agent)

**User Journey:**
1. User registers/logs in via **Portal**
2. Creates a new website from **Portal** dashboard
3. Clicks "Configure Site" to access **CMS** admin features
4. Manages content, permissions, and customization in **CMS**
5. Returns to **Portal** for billing, analytics, or other sites

## 🎯 Vision

Mosaic Conversational CMS reimagines website and content management as a service by blending:
- **Conversational AI interface** for intuitive website building without technical configuration
- **Enterprise-grade multi-tenant architecture** for scalability and security
- **AI-powered automation** with intelligent agents for both public users and administrators
- **Seamless integrations** with modern payment, analytics, and development tools

Our goal is to make powerful web infrastructure accessible to everyone through natural language interactions.

## ✨ Core Features

### 🗣️ Conversational Website Builder
- **Natural language interface**: Build websites by describing what you want in plain English
- **AI-powered design**: Smart suggestions for layouts, content, and structure
- **No technical knowledge required**: Focus on your vision, not configuration
- **Instant previews**: See changes in real-time as you converse with the AI
- **Context-aware assistance**: The AI understands your goals and guides you accordingly

### 🏗️ Create Site Workflow
- **Multi-step wizard**: Guided site creation process with 4 easy steps
  1. **Basic Info**: Enter site name and description
  2. **AI Setup Assistant**: Conversational helper to understand your site's purpose
  3. **Theme Selection**: Choose from professional, pre-built themes
  4. **Review & Confirm**: Review configuration and create your site
- **Instant provisioning**: Sites are created and ready in seconds
- **Automatic initialization**: Default home page and content structure set up automatically
- **Direct CMS access**: Immediate link to CMS dashboard after creation
- **Multi-tenant isolation**: Each site operates in its own secure environment

### 🚀 User Onboarding & Registration
- **Streamlined sign-up flow**: Get started in minutes with intuitive onboarding
- **Email verification**: Secure account activation
- **Profile customization**: Users can personalize their workspace from day one
- **Guided setup wizard**: Step-by-step process to launch your first site
- **Documentation hub**: Comprehensive guides and video tutorials

### 💳 Payment Integration & Pricing
- **Flexible subscription tiers**: Free, Starter, Pro, and Business plans
- **Transparent pricing**: From $0 to $250/month with annual discounts (~17% savings)
- **Microsoft Startup Founder Hub support**: Leverage startup credits and benefits
- **Stripe integration**: Secure payment processing with support for multiple currencies
- **No hidden fees**: Clear cost breakdowns and predictable billing
- **Invoice management**: Automated billing and receipt generation

📊 **[View Detailed Pricing →](./docs/pricing.md)** - Compare plans, features, and find the perfect fit for your needs

### 🏢 Multi-Tenant Site Management
- **Unlimited sites**: Create and manage multiple websites from one dashboard
- **Isolated environments**: Each site runs in its own secure tenant space
- **Custom domains**: Connect your own domain names
- **SSL certificates**: Automatic HTTPS for all sites
- **Staging environments**: Test changes before going live
- **Backup & restore**: Automated backups with one-click restoration

### 🎨 Theme & Branding
- **Beautiful themes**: Professional designs for any business or creative project
- **Customizable color palettes**: Choose from pre-defined palettes or create your own
- **Light & dark modes**: Elegant themes for different contexts
- **Logo integration**: Upload and manage your brand assets
- **SharePoint-inspired layouts**: Professional, enterprise-ready templates
- **Custom CSS**: Advanced users can override styles
- **Brand consistency**: Maintain unified look across all your sites

### 📊 Analytics & Insights
- **Real-time dashboards**: Monitor site performance and visitor activity
- **Traffic analytics**: Page views, unique visitors, engagement metrics
- **Conversion tracking**: Monitor form submissions and goal completions
- **User behavior analysis**: Heatmaps and session recordings (planned)
- **Custom reports**: Export data for deeper analysis
- **Performance monitoring**: Site speed and uptime tracking

### 🔌 API Integration
- **RESTful API**: Full programmatic access to platform features
- **Webhooks**: Real-time notifications for events
- **GraphQL support** (planned): Flexible data querying
- **OAuth 2.0**: Secure third-party integrations
- **SDKs**: Official client libraries for JavaScript, Python, and .NET
- **Rate limiting**: Fair usage policies with generous limits
- **API documentation**: Interactive docs with examples

### 🤖 AI-Powered Agents

#### Mosaic Public Agent
- **Conversational interface**: Natural language website building and management
- **Content assistance**: AI helps users create and optimize content
- **SEO recommendations**: Automated suggestions for better search rankings
- **Design guidance**: Smart recommendations for layouts and styling
- **Accessibility checks**: Ensure sites meet WCAG standards
- **24/7 availability**: Always-on support for common questions

#### Zoota Admin Agent (CMS Administration Assistant)
- **Administrative automation**: Streamline platform management tasks
- **User support**: Intelligent ticket routing and response suggestions
- **System monitoring**: Proactive issue detection and resolution
- **Analytics insights**: AI-powered trend analysis and recommendations
- **Resource optimization**: Smart scaling and cost management

## 📋 Requirements

- **[Frontend Design](./docs/FRONTEND_DESIGN.md)**: SaaS portal UI/UX design and wireframes
- **[Frontend README](./frontend/README.md)**: React + Fluent UI frontend application guide
- **[Quick Start - CMS](./docs/QUICK_START_CMS.md)**: Get started with CMS and Azure Blob Storage in minutes
- .NET 10 SDK
- SQL Server 2019+ / Azure SQL (for production)
- Visual Studio 2022 (17.12+) or Visual Studio 2026 (recommended)
- Azure account (for deployment)
- Stripe account (for payment processing)
### Platform Documentation
- **[Architecture](./docs/architecture.md)**: Platform architecture, multi-tenant design, and tier specifications
- **[SaaS Features Overview](./docs/SaaS_FEATURES.md)**: Detailed feature comparison and roadmap
- **[Pricing Plans](./docs/pricing.md)**: Complete pricing guide with plan comparisons and feature breakdown
- **[Application Settings](./docs/appsettings.md)**: Configuration guide for appsettings.json and Azure Key Vault

### Getting Started
- **[Quick Start - CMS](./docs/QUICK_START_CMS.md)**: Get started with CMS and Azure Blob Storage in minutes
- **[Onboarding Guide](./docs/ONBOARDING.md)**: Complete user journey from sign-up to launch
- **[Migration Toolkit](./docs/migration.md)**: Migrate from Wix, Umbraco, or SharePoint with guided workflows

### Integration & Storage
- **[Azure Blob Storage Integration](./docs/AZURE_BLOB_STORAGE.md)**: Media storage, security, and usage guide
- **[Stripe API Keys Setup](./docs/STRIPE_SETUP.md)**: Complete guide for configuring Stripe payment integration

## 🚀 Getting Started

### For End Users (Coming Soon)

1. **Sign up**: Visit mosaic.orkinosai.com (platform launching soon)
2. **Choose your plan**: Select from Free, Starter, Pro, or Business tiers ([See Pricing](./docs/pricing.md))
3. **Create your first site**: Follow the guided setup wizard
4. **Customize**: Use the conversational AI to build and design your site
5. **Launch**: Go live with your new website
6. **Integrate**: Add the MOSAIC script to existing sites if needed

For detailed onboarding instructions, see our [Onboarding Guide](./docs/ONBOARDING.md).  
For pricing comparison and plan details, see our [Pricing Guide](./docs/pricing.md).

### SaaS Architecture

Mosaic Conversational CMS uses a multi-tenant architecture with:
- **Portal Frontend (React + Fluent UI)**: Public landing page at `/` for registration and authentication
- **CMS Backend (.NET 10 + Blazor)**: Admin interface at `/admin` for site management (authenticated users only)
- **API Services**: RESTful endpoints at `/api/*` for portal-backend integration
- **CMS Pages**: Content pages at `/cms-*` for website management

**Routing Architecture:**

The ASP.NET Core application uses a layered routing approach:
1. **Static Files** (`UseStaticFiles`): Serves CSS, JS, images from wwwroot
2. **API Controllers** (`MapControllers`): Handles `/api/*` endpoints
3. **Blazor Components** (`MapRazorComponents`): Handles `/admin` and `/cms-*` routes
4. **SPA Fallback** (`MapFallbackToFile`): All other routes serve React portal's `index.html`

This ensures:
- ✅ Portal landing page appears at root URL (`/`)
- ✅ CMS admin accessible only after authentication (`/admin`, `/cms-*`)
- ✅ API endpoints work independently (`/api/*`)
- ✅ React router handles client-side navigation within portal

**User Journey:**
1. Visit root URL → See Portal landing page
2. Register/Login → Access dashboard in Portal
3. Create site → Manage via Portal dashboard
4. Configure site → Access CMS admin (authenticated)
5. Manage content → Use CMS features and modules

### For Developers

#### Frontend Development

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

See [Frontend README](./frontend/README.md) for detailed setup and architecture information.

#### Backend Development

```bash
# Navigate to backend directory
cd src/MosaicCMS

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

See [MosaicCMS README](./src/MosaicCMS/README.md) for API documentation.

#### Setting Up Environment Variables

Before running the application, configure your environment variables:

```bash
# Copy the example environment file
cp .env.example .env

# Edit .env and add your API keys
# - STRIPE_PUBLISHABLE_KEY: Get from Stripe Dashboard
# - STRIPE_SECRET_KEY: Get from Stripe Dashboard (keep secret!)
# - Other service keys as needed
```

**Important**: Never commit your `.env` file to Git. It's already in `.gitignore`.

See [Stripe API Keys Setup Guide](./docs/STRIPE_SETUP.md) for detailed instructions.

#### 1. Clone the Repository

```bash
git clone https://github.com/orkinosai25-org/mosaic.git
cd mosaic
```

#### 2. Restore Dependencies

```bash
dotnet restore OrkinosaiCMS.sln
```

#### 3. Configure Multi-Tenancy

Update `appsettings.json` to enable multi-tenant mode:
```json
{
  "MultiTenancy": {
    "Enabled": true,
    "TenantIdentificationStrategy": "Host"
  }
}
```

#### 4. Apply Database Migrations

OrkinosaiCMS includes an **enhanced migration recovery system** adapted from [Oqtane CMS](https://github.com/oqtane/oqtane.framework) that automatically handles schema drift and migration errors.

**Automatic Migration (Recommended)**

Migrations are applied automatically when the application starts. Simply run:

```bash
cd src/OrkinosaiCMS.Web
dotnet run
```

The migration service will:
- ✅ Detect and create database if it doesn't exist
- ✅ Apply all pending migrations
- ✅ Automatically recover from "object already exists" errors
- ✅ Detect schema drift and mark applied migrations
- ✅ Verify critical tables exist
- ✅ Log detailed diagnostics

**Manual Migration (Advanced)**

**Option A: Using Helper Script**

```bash
# Linux/Mac
./scripts/apply-migrations.sh update

# Windows (PowerShell)
.\scripts\apply-migrations.ps1 update
```

**Option B: EF Core Commands**

```bash
# Install EF Core tools (first time only)
dotnet tool install --global dotnet-ef --version 10.0.0

# Apply migrations
cd src/OrkinosaiCMS.Infrastructure
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

**Migration Recovery & Troubleshooting:**

The system automatically handles common migration errors:
- **SQL Error 2714** ("object already exists") - Auto-recovery via schema drift detection
- **SQL Error 208** ("table not found") - Creates missing tables
- **Schema Drift** - Intelligently marks applied migrations in history
- **Missing Identity Tables** - Applies Identity migration automatically

For detailed information, see:
- 📖 [Database Migration Recovery Guide](./docs/DATABASE_MIGRATION_RECOVERY.md) - **NEW!** Comprehensive guide with Oqtane patterns
- 📖 [Database Migration Troubleshooting](./docs/DATABASE_MIGRATION_TROUBLESHOOTING.md) - Solutions for specific errors
- 📖 [Migration Verification Guide](./docs/MIGRATION_VERIFICATION_GUIDE.md) - Verify and test migrations

**Testing Migration Recovery:**

```bash
# Run migration recovery tests
./scripts/test-migration-recovery.sh
```

#### 5. Run the Application

```bash
cd ../OrkinosaiCMS.Web
dotnet run
```

Navigate to `https://localhost:5001`

## 📚 Documentation

### MOSAIC SaaS Documentation
- **[SaaS Features Overview](./docs/SaaS_FEATURES.md)**: Detailed feature comparison and roadmap
- **[Onboarding Guide](./docs/ONBOARDING.md)**: Complete user journey from sign-up to launch
- **[Site Management API](./docs/SITE_MANAGEMENT_API.md)**: API documentation for creating and managing sites
- **[Multi-Tenancy Guide](./docs/MULTI_TENANCY.md)**: Architecture and implementation (coming soon)
- **[Payment Integration](./docs/PAYMENT_INTEGRATION.md)**: Stripe setup and configuration (coming soon)

### OrkinosaiCMS Core Documentation (Inherited)
- **[Architecture Guide](docs/ARCHITECTURE.md)** - Understand the system design and architecture
- **[Setup Guide](docs/SETUP.md)** - Detailed setup and configuration instructions
- **[Database Guide](docs/DATABASE.md)** - Database architecture and data access patterns
- **[Database Migration Recovery](docs/DATABASE_MIGRATION_RECOVERY.md)** - **NEW!** Enhanced migration system with automatic schema drift recovery
- **[Database Migration Troubleshooting](docs/DATABASE_MIGRATION_TROUBLESHOOTING.md)** - Solutions for migration errors and schema drift
- **[Migration Verification Guide](docs/MIGRATION_VERIFICATION_GUIDE.md)** - Verify and test database migrations
- **[Extensibility Guide](docs/EXTENSIBILITY.md)** - Creating custom modules, themes, and extensions
- **[Logging Guide](docs/LOGGING.md)** - Serilog logging configuration, troubleshooting, and best practices

### Design & Branding
- **[About the MOSAIC Name](./BRAND.md)**: Complete story - meaning, heritage, and design inspiration 🎨
- **[Logo & Branding](./logo/README.md)**: Official brand assets and usage guidelines
- **[Design Concept](./logo/concept/DESIGN_CONCEPT.md)**: Logo design philosophy
- **[Ottoman Inspirations](./logo/concept/OTTOMAN_INSPIRATIONS.md)**: Cultural and architectural heritage

### Migration & Deployment
- **[Azure Deployment](docs/AZURE_DEPLOYMENT.md)** - Deploy to Azure Web Apps with Azure SQL
- **[Azure Log Diagnostics](docs/AZURE_LOG_DIAGNOSTICS.md)** - Fetch and analyze Azure app logs for troubleshooting
- **[Deployment Checklist](docs/DEPLOYMENT_CHECKLIST.md)** - Complete deployment procedures

## 🚀 Deployment

MOSAIC uses GitHub Actions for automated deployment to Azure Web App.

### Automated Deployment

The `deploy.yml` workflow automatically deploys to Azure when changes are pushed to the `main` branch.

**Workflow Steps:**
1. **Build Portal (Frontend)**: Compiles React app with Vite
2. **Build CMS (Backend)**: Compiles .NET 10 solution
3. **Publish Backend**: Creates deployment package
4. **Integrate Frontend**: Copies React build to backend's wwwroot
5. **Deploy to Azure**: Uses publish profile to deploy to Azure Web App

**Required Secret:**
- `AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC`: Azure Web App publish profile from Azure Portal

### Manual Deployment

```bash
# Build frontend
cd frontend
npm ci
npm run build

# Build and publish backend
cd ..
dotnet restore OrkinosaiCMS.sln
dotnet build OrkinosaiCMS.sln --configuration Release
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj --configuration Release --output ./publish

# Copy frontend to backend wwwroot
cp -r frontend/dist/* ./publish/wwwroot/

# Deploy to Azure using Azure CLI or publish profile
az webapp deploy --resource-group <rg-name> --name mosaic-saas --src-path ./publish
```

### Production URLs

After deployment, the application is accessible at:
- **Portal (Landing)**: `https://mosaic-saas.azurewebsites.net/`
- **CMS Admin**: `https://mosaic-saas.azurewebsites.net/admin`
- **CMS Pages**: `https://mosaic-saas.azurewebsites.net/cms-*`
- **API Endpoints**: `https://mosaic-saas.azurewebsites.net/api/*`

### Deployment Verification

After deployment, verify the following:

1. **Portal Landing Page** (Root URL)
   - Visit: `https://mosaic-saas.azurewebsites.net/`
   - Expected: Mosaic portal landing page
   - Should show: Registration/login options, features, hero section
   - Should NOT show: Old CMS frontend (Counter, Weather pages)

2. **CMS Admin Interface**
   - Visit: `https://mosaic-saas.azurewebsites.net/admin`
   - Expected: Admin dashboard (requires authentication)
   - Should show: Zoota AI assistant banner, dashboard cards, quick actions

3. **CMS Content Pages**
   - Visit: `https://mosaic-saas.azurewebsites.net/cms-home`
   - Expected: CMS home page with navigation
   - Should show: Mosaic CMS branding, Features, About, Contact links

4. **API Health Check**
   - Visit: `https://mosaic-saas.azurewebsites.net/api/*` endpoints
   - Expected: API responses (may require authentication)

**Troubleshooting:**
- If the CMS pages show at root instead of portal, check that:
  - Frontend was built and copied to wwwroot
  - `index.html` exists in wwwroot
  - SPA fallback middleware is configured in Program.cs
  - Static files middleware comes before the fallback
- Check Azure Web App logs in Azure Portal for errors
- **For deployment/runtime errors**: Use the [Fetch and Diagnose App Errors workflow](docs/AZURE_LOG_DIAGNOSTICS_QUICKSTART.md) to automatically collect and analyze logs

## 🏗️ Project Structure

```
mosaic/
├── .github/
│   └── workflows/                       # CI/CD pipelines
│       ├── ci.yml                       # Continuous integration (all branches)
│       ├── deploy.yml                   # Production deployment (main branch)
│       └── docker-publish.yml           # Container publishing
├── frontend/                            # Portal (React + Fluent UI)
│   ├── src/
│   │   ├── pages/                       # Landing, Dashboard, Sites pages
│   │   ├── components/                  # Reusable UI components
│   │   ├── styles/                      # Custom themes
│   │   └── App.tsx                      # Main application component
│   ├── package.json
│   └── vite.config.ts                   # Build configuration
├── src/                                 # CMS Backend (.NET 10)
│   ├── OrkinosaiCMS.Core/               # Domain entities and interfaces
│   ├── OrkinosaiCMS.Infrastructure/     # Data access and services
│   ├── OrkinosaiCMS.Modules.Abstractions/ # Module base classes
│   ├── OrkinosaiCMS.Shared/             # Shared DTOs
│   ├── OrkinosaiCMS.Web/                # Blazor Web App (CMS Admin)
│   │   ├── Components/
│   │   │   ├── Pages/Admin/             # Admin pages (/admin/*)
│   │   │   └── Pages/                   # CMS pages (/cms-*)
│   │   ├── wwwroot/                     # Static assets (Portal deployed here)
│   │   └── Program.cs                   # App configuration
│   └── Modules/                         # CMS feature modules
│       ├── OrkinosaiCMS.Modules.Content/
│       ├── OrkinosaiCMS.Modules.Hero/
│       └── OrkinosaiCMS.Modules.Features/
├── docs/                                # Documentation
├── logo/                                # MOSAIC branding and design assets
└── scripts/                             # Utility scripts
```
## ☁️ Azure Blob Storage Integration

Mosaic Conversational CMS leverages **Azure Blob Storage** for enterprise-grade media and asset management:

### Key Features

- **Multi-Tenant Isolation**: Automatic tenant data separation for security
- **Scalable Storage**: Unlimited capacity for user uploads, images, and documents
- **Geo-Redundant**: Data replicated across regions for high availability
- **Secure by Default**: 
  - TLS 1.2 encryption in transit
  - Data encrypted at rest
  - No public access allowed
  - SAS tokens for temporary secure access

### Storage Account Details

- **Account Name**: `mosaicsaas`
- **Primary Endpoint**: `https://mosaicsaas.blob.core.windows.net/`
- **Location**: UK South (`uksouth`)
- **Redundancy**: Standard_RAGRS (Read-Access Geo-Redundant Storage)

### Supported Content Types

| Container | Purpose | File Types |
|-----------|---------|------------|
| **Images** | User images, logos, banners | JPEG, PNG, GIF, WebP, SVG |
| **Documents** | User documents, PDFs | PDF, Word, Excel, Text, CSV |
| **User Uploads** | General media assets | All supported types |
| **Backups** | Tenant backups | All files |

### Usage Example

```bash
# Upload an image (requires tenant authentication)
curl -X POST https://api.mosaic.app/api/media/images \
  -H "X-Tenant-Id: your-tenant-id" \
  -F "file=@logo.png"

# List tenant files
curl -X GET "https://api.mosaic.app/api/media/list?containerType=images" \
  -H "X-Tenant-Id: your-tenant-id"
```

### Security & Migration

- ✅ **Secure**: Public access disabled, HTTPS enforced, tenant isolation
- ✅ **Compliant**: Encryption enabled, TLS 1.2 minimum
- ✅ **Reliable**: Geo-redundant storage with 99.99% availability SLA
- ✅ **Migrated**: Easy migration from local storage or other cloud providers

For detailed information, see the [Azure Blob Storage Integration Guide](./docs/AZURE_BLOB_STORAGE.md).

## 💼 For Startups

Mosaic Conversational CMS is proud to support startups through the **Microsoft for Startups Founder Hub**:
- Access to Azure credits for infrastructure
- Preferred pricing for qualifying startups
- Technical support and architecture guidance
- Scalability consulting

[Learn more about startup benefits](./docs/SaaS_FEATURES.md#startup-support)

## 💳 Payment Integration Readiness

Mosaic Conversational CMS is architected to support payment integration out of the box:

### Current Architecture Supports:
- ✅ **User Authentication**: Portal handles registration and login
- ✅ **Multi-tenant Isolation**: Each user/organization has separate data
- ✅ **API-Ready Backend**: RESTful endpoints at `/api/*` for service integration
- ✅ **Subscription Dashboard**: Portal includes billing navigation and UI placeholders
- ✅ **Secure Configuration**: Azure Key Vault ready for API keys and secrets

### Integration Steps (Future):
1. **Stripe Setup**: Add Stripe publishable and secret keys to Azure Key Vault
2. **Webhook Configuration**: Configure Stripe webhooks to backend API
3. **Subscription API**: Implement `/api/subscriptions` endpoints
4. **Billing UI**: Connect Portal billing pages to Stripe APIs
5. **Usage Tracking**: Add metering for usage-based billing

### Microsoft Founder Hub Benefits:
- Use Azure credits to offset infrastructure costs
- Leverage startup-friendly pricing during growth phase
- Access to technical resources and support

For detailed payment integration guide, see [Payment Integration](./docs/PAYMENT_INTEGRATION.md) (coming soon).

## 🛣️ Next Steps in SaaS Development

### Phase 1: Multi-Tenancy Foundation (In Progress)
- [ ] Implement tenant isolation at database level
- [ ] Add tenant identification middleware
- [ ] Create tenant provisioning workflow
- [ ] Implement tenant-specific data access

### Phase 2: User Onboarding & Authentication
- [ ] Streamlined sign-up flow with email verification
- [ ] OAuth integration (Google, Microsoft, GitHub)
- [ ] User profile management
- [ ] Guided setup wizard

### Phase 3: Payment & Subscription Management
- [ ] Stripe integration for payment processing
- [ ] Subscription tier management
- [ ] Usage tracking and billing
- [ ] Invoice generation

### Phase 4: Enhanced Conversational Builder
- [ ] Advanced natural language processing
- [ ] Multi-step conversation flows
- [ ] Visual feedback during conversations
- [ ] Template suggestions based on industry

### Phase 5: AI Agent Enhancement
- [ ] Deploy MOSAIC Public Agent
- [ ] Enhance Zoota Admin Agent
- [ ] AI-powered content suggestions
- [ ] Automated SEO optimization

### Phase 6: Analytics & Monitoring
- [ ] Real-time analytics dashboard
- [ ] Performance monitoring
- [ ] User behavior tracking
- [ ] Custom reporting

## 🤝 Contributing

We welcome contributions! Whether it's:
- 🐛 Bug reports
- 💡 Feature suggestions
- 📝 Documentation improvements
- 🎨 Design enhancements

Please check our contribution guidelines (coming soon).

## 📜 License

**Mosaic Conversational CMS** is proprietary software.

This codebase is licensed under the **Apache License 2.0** with the following important restrictions:

- ✅ **Permitted**: Personal use, learning, evaluation, and non-commercial experimentation
- ❌ **Prohibited**: Commercial redistribution, creating competing SaaS products, or claiming this is open-source

**Important**: This is NOT open-source software. A separate open-source project called "OrkinosaiCMS" for on-premises deployments is planned for future release.

Copyright © 2024 Orkinosai. All rights reserved.

See the [LICENSE](LICENSE) file for the full Apache License 2.0 text.

## 🙏 Acknowledgments

- **Oqtane CMS** - Inspiration for the modular architecture
- **SharePoint** - Inspiration for page model and permission system
- **.NET Team** - For the amazing .NET 10 and Blazor frameworks

## 🔮 Future: Open-Source OrkinosaiCMS

**Note**: A separate open-source project called **OrkinosaiCMS** is planned for future release. This will be:
- Truly open-source (MIT or similar permissive license)
- Designed for on-premises, self-hosted deployments
- A distinct codebase from this proprietary Mosaic Conversational CMS SaaS platform
- Available for community contributions and commercial use without restrictions

Stay tuned for announcements about the OrkinosaiCMS open-source project!

## 📞 Contact & Support

- **Website**: [orkinosai.com](https://orkinosai.com) (coming soon)
- **Repository**: [github.com/orkinosai25-org/mosaic](https://github.com/orkinosai25-org/mosaic)
- **Email**: support@orkinosai.com (coming soon)
- **Documentation**: See [docs](./docs/) folder

## 🔧 SaaS Compatibility Notes

### Configurations Requiring Adjustment

1. **Connection Strings**: Update for multi-tenant database strategy
2. **Authentication**: Configure for SaaS identity provider
3. **Secrets Management**: Use Azure Key Vault for production
4. **Domain Configuration**: Set up custom domain routing per tenant
5. **Email Service**: Configure SendGrid/similar for transactional emails
6. **Payment Gateway**: Set up Stripe with webhook endpoints

See [Setup Guide](docs/SETUP.md) for detailed configuration instructions.

---

**Built with ❤️ by [Orkinosai](https://github.com/orkinosai25-org)**  
**Mosaic Conversational CMS** • **Proprietary SaaS Platform** • **Crafted with modern technology**
