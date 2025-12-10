# MOSAIC SaaS Platform

**MOSAIC** is a next-generation SaaS platform that empowers businesses and creators to build, manage, and scale multi-tenant websites with beautiful Ottoman-inspired design aesthetics. Built by [Orkinosai](https://github.com/orkinosai25-org), MOSAIC combines powerful enterprise features with the elegance of historical Turkish artistic heritage.

![MOSAIC Logo](./logo/src/mosaic-logo-main.svg)

## 🏛️ Heritage & Foundation

MOSAIC is built upon **OrkinosaiCMS**, a modern, modular Content Management System built on .NET 10 and Blazor. We've taken the solid foundation of OrkinosaiCMS and transformed it into a comprehensive multi-tenant SaaS platform while preserving its core architectural strengths:

- **Modular Architecture**: Plugin-based system for unlimited extensibility
- **Clean Architecture**: Clear separation between Core, Infrastructure, and UI layers
- **SharePoint-Inspired Design**: Familiar concepts like Master Pages, Web Parts (Modules), and permission levels
- **Modern Stack**: Built on .NET 10, Blazor, and Entity Framework Core
- **Flexible Permissions**: Fine-grained, SharePoint-style permission system

### OrkinosaiCMS to MOSAIC Evolution

The transformation from OrkinosaiCMS to MOSAIC represents a strategic evolution:

| Aspect | OrkinosaiCMS (Foundation) | MOSAIC (SaaS Platform) |
|--------|---------------------------|------------------------|
| **Purpose** | Single-tenant CMS | Multi-tenant SaaS platform |
| **Target Users** | Individual websites | Businesses and creators |
| **Architecture** | Modular monolith | Multi-tenant with tenant isolation |
| **Deployment** | Self-hosted | Cloud-native (Azure) |
| **Theming** | Generic themes | Ottoman/Iznik-inspired design system |
| **AI Integration** | Basic (Zoota chat) | Dual AI agents (MOSAIC + Zoota Admin) |
| **Payment** | Not included | Integrated Stripe + Microsoft Founder Hub |
| **Onboarding** | Manual setup | Streamlined sign-up with guided wizard |

## 🏗️ Architecture Overview

MOSAIC consists of two main components working together:

### 1. **MOSAIC Portal** (Frontend - React + Fluent UI)
The SaaS management portal where users:
- **Register and authenticate** (as admin/owner or regular user)
- **Manage subscriptions and billing** through Stripe integration
- **Create and manage multiple websites** (multi-tenant dashboard)
- **Access analytics and usage metrics**
- **Navigate to CMS configuration** for each website

### 2. **MOSAIC CMS** (Backend - .NET 10 + Blazor)
The content management system that powers each website:
- **Configure website content and structure** through admin features
- **Manage pages, modules, and web parts** (SharePoint-inspired)
- **Control permissions and user access**
- **Customize themes and branding** with Ottoman/Iznik designs
- **Use AI assistance** (Zoota Admin Agent)

**User Journey:**
1. User registers/logs in via **Portal**
2. Creates a new website from **Portal** dashboard
3. Clicks "Configure Site" to access **CMS** admin features
4. Manages content, permissions, and customization in **CMS**
5. Returns to **Portal** for billing, analytics, or other sites

## 🎯 Vision

MOSAIC reimagines website and content management as a service by blending:
- **Enterprise-grade multi-tenant architecture** for scalability and security
- **Ottoman/Iznik design inspiration** bringing unique cultural aesthetics to modern web
- **AI-powered automation** with intelligent agents for both public users and administrators
- **Seamless integrations** with modern payment, analytics, and development tools

Our goal is to make powerful web infrastructure accessible to everyone while celebrating Turkish cultural heritage through design.

## ✨ Core Features

### 🚀 User Onboarding & Registration
- **Streamlined sign-up flow**: Get started in minutes with intuitive onboarding
- **Email verification**: Secure account activation
- **Profile customization**: Users can personalize their workspace from day one
- **Guided setup wizard**: Step-by-step process to launch your first site
- **Documentation hub**: Comprehensive guides and video tutorials

### 💳 Payment Integration
- **Microsoft Startup Founder Hub support**: Leverage startup credits and benefits
- **Stripe integration**: Secure payment processing with support for multiple currencies
- **Flexible subscription tiers**: Free, Pro, Business, and Enterprise plans
- **Usage-based billing**: Pay only for what you use
- **Transparent pricing**: No hidden fees, clear cost breakdowns
- **Invoice management**: Automated billing and receipt generation

### 🏢 Multi-Tenant Site Management
- **Unlimited sites**: Create and manage multiple websites from one dashboard
- **Isolated environments**: Each site runs in its own secure tenant space
- **Custom domains**: Connect your own domain names
- **SSL certificates**: Automatic HTTPS for all sites
- **Staging environments**: Test changes before going live
- **Backup & restore**: Automated backups with one-click restoration

### 🎨 Theme & Branding
- **Ottoman-inspired themes**: Beautiful designs drawing from Selimiye Mosque and Blue Mosque aesthetics
- **Iznik tile patterns**: Authentic geometric patterns with 8-pointed stars, diamond tiles, and arabesques
- **Customizable color palettes**: 
  - Deep cobalt blues (#1e3a8a, #2563eb)
  - Turquoise accents (#06b6d4, #22d3ee)
  - Gold highlights (#fbbf24, #f59e0b)
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

#### MOSAIC Public Agent
- **Content assistance**: AI helps users create and optimize content
- **SEO recommendations**: Automated suggestions for better search rankings
- **Design guidance**: Smart recommendations for layouts and styling
- **Accessibility checks**: Ensure sites meet WCAG standards
- **24/7 availability**: Always-on support for common questions

#### Zoota Admin Agent (Inherited from OrkinosaiCMS)
- **Administrative automation**: Streamline platform management tasks
- **User support**: Intelligent ticket routing and response suggestions
- **System monitoring**: Proactive issue detection and resolution
- **Analytics insights**: AI-powered trend analysis and recommendations
- **Resource optimization**: Smart scaling and cost management

## 🏛️ Design Heritage

MOSAIC's visual identity is deeply rooted in Ottoman architectural masterpieces:

### Selimiye Mosque (Edirne)
- Geometric precision of Mimar Sinan's masterpiece
- 8-pointed star patterns (Rub el Hizb)
- Perfect mathematical proportions

### Blue Mosque (Istanbul)
- Famous Iznik tile work with over 20,000 handmade tiles
- Cobalt blue color palette
- Cascading dome architecture

### Iznik Tiles
- Traditional Turkish ceramic art
- Diamond and geometric patterns
- Rich color combinations

Learn more about our design inspiration in the [logo documentation](./logo/README.md) and [Ottoman inspirations](./logo/concept/OTTOMAN_INSPIRATIONS.md).

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
- **[Application Settings](./docs/appsettings.md)**: Configuration guide for appsettings.json and Azure Key Vault
- **[Portal UI Design](./docs/portal-ui.md)**: Azure-style portal interface and Ottoman-inspired design elements

### Getting Started
- **[Quick Start - CMS](./docs/QUICK_START_CMS.md)**: Get started with CMS and Azure Blob Storage in minutes
- **[Onboarding Guide](./docs/ONBOARDING.md)**: Complete user journey from sign-up to launch
- **[Migration Toolkit](./docs/migration.md)**: Migrate from Wix, Umbraco, or SharePoint with guided workflows

### Integration & Storage
- **[Azure Blob Storage Integration](./docs/AZURE_BLOB_STORAGE.md)**: Media storage, security, and usage guide

### Branding & Design
- **[Logo & Branding](./logo/README.md)**: Official brand assets and usage guidelines
- **[Design Concept](./logo/concept/DESIGN_CONCEPT.md)**: Logo design philosophy
- **[Ottoman Inspirations](./logo/concept/OTTOMAN_INSPIRATIONS.md)**: Cultural and architectural heritage

## 🚀 Getting Started

### For End Users (Coming Soon)

1. **Sign up**: Visit mosaic.orkinosai.com (platform launching soon)
2. **Choose your plan**: Select a subscription tier that fits your needs
3. **Create your first site**: Follow the guided setup wizard
4. **Customize**: Apply Ottoman-inspired themes and branding
5. **Launch**: Go live with your new website
6. **Integrate**: Add the MOSAIC script to existing sites if needed

For detailed onboarding instructions, see our [Onboarding Guide](./docs/ONBOARDING.md).

### SaaS Architecture

MOSAIC uses a multi-tenant architecture with:
- **Portal Frontend (React + Fluent UI)**: Public landing page at `/` for registration and authentication
- **CMS Backend (.NET 10 + Blazor)**: Admin interface at `/admin` for site management (authenticated users only)
- **API Services**: RESTful endpoints at `/api/*` for portal-backend integration
- **CMS Pages**: Content pages at `/cms-*` for website management

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

```bash
# Install EF Core tools (first time only)
dotnet tool install --global dotnet-ef --version 10.0.0

# Apply migrations
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
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
- **[Multi-Tenancy Guide](./docs/MULTI_TENANCY.md)**: Architecture and implementation (coming soon)
- **[Payment Integration](./docs/PAYMENT_INTEGRATION.md)**: Stripe setup and configuration (coming soon)

### OrkinosaiCMS Core Documentation (Inherited)
- **[Architecture Guide](docs/ARCHITECTURE.md)** - Understand the system design and architecture
- **[Setup Guide](docs/SETUP.md)** - Detailed setup and configuration instructions
- **[Database Guide](docs/DATABASE.md)** - Database architecture and data access patterns
- **[Extensibility Guide](docs/EXTENSIBILITY.md)** - Creating custom modules, themes, and extensions

### Design & Branding
- **[Logo & Branding](./logo/README.md)**: Official brand assets and usage guidelines
- **[Design Concept](./logo/concept/DESIGN_CONCEPT.md)**: Logo design philosophy
- **[Ottoman Inspirations](./logo/concept/OTTOMAN_INSPIRATIONS.md)**: Cultural and architectural heritage

### Migration & Deployment
- **[Azure Deployment](docs/AZURE_DEPLOYMENT.md)** - Deploy to Azure Web Apps with Azure SQL
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
- **API Endpoints**: `https://mosaic-saas.azurewebsites.net/api/*`

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
│   │   ├── styles/                      # Ottoman-inspired themes
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

MOSAIC leverages **Azure Blob Storage** for enterprise-grade media and asset management:

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

MOSAIC is proud to support startups through the **Microsoft for Startups Founder Hub**:
- Access to Azure credits for infrastructure
- Preferred pricing for qualifying startups
- Technical support and architecture guidance
- Scalability consulting

[Learn more about startup benefits](./docs/SaaS_FEATURES.md#startup-support)

## 💳 Payment Integration Readiness

MOSAIC is architected to support payment integration out of the box:

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

### Phase 4: Ottoman Design System
- [ ] Implement 6+ Ottoman-inspired themes
- [ ] Iznik tile pattern library
- [ ] Theme customization UI
- [ ] Brand asset management

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

This project is proprietary software by Orkinosai. See LICENSE file for details.

## 🙏 Acknowledgments

- **OrkinosaiCMS** - The foundational CMS that powers MOSAIC
- **Oqtane CMS** - Inspiration for the modular architecture
- **SharePoint** - Inspiration for page model and permission system
- **.NET Team** - For the amazing .NET 10 and Blazor frameworks
- **Ottoman Architects** - Mimar Sinan and the masters who created timeless beauty

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
4. **Branding**: Apply Ottoman-inspired theme system
5. **Domain Configuration**: Set up custom domain routing per tenant
6. **Email Service**: Configure SendGrid/similar for transactional emails
7. **Payment Gateway**: Set up Stripe with webhook endpoints

See [Setup Guide](docs/SETUP.md) for detailed configuration instructions.

---

**Built with ❤️ by [Orkinosai](https://github.com/orkinosai25-org)**  
**Inspired by Ottoman heritage** • **Powered by OrkinosaiCMS** • **Crafted with modern technology**
