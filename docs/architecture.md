# MOSAIC Platform Architecture

This document describes the architecture of the MOSAIC SaaS platform, including infrastructure design, multi-tenant architecture, scalability patterns, and deployment strategies.

## 🏗️ Architecture Overview

MOSAIC is built as a modern, cloud-native SaaS platform on **Microsoft Azure**, designed for scalability, security, and multi-tenancy from the ground up.

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Users & Clients                      │
└──────────────┬──────────────────────────┬───────────────────┘
               │                          │
               │                          │
    ┌──────────▼──────────┐    ┌─────────▼──────────┐
    │   Web Portal        │    │   API Clients      │
    │   (Azure Portal UI) │    │   (REST/GraphQL)   │
    └──────────┬──────────┘    └─────────┬──────────┘
               │                          │
               └──────────┬───────────────┘
                          │
               ┌──────────▼──────────┐
               │  Azure Front Door   │
               │  (CDN + WAF)        │
               └──────────┬──────────┘
                          │
               ┌──────────▼──────────────────┐
               │  Azure App Service          │
               │  (Web Apps)                 │
               │  ┌──────────────────────┐   │
               │  │ MOSAIC CMS           │   │
               │  │ (.NET 8 / ASP.NET)   │   │
               │  └──────────────────────┘   │
               └──┬────────────┬──────────────┘
                  │            │
        ┌─────────▼────┐   ┌──▼──────────────┐
        │ Database     │   │ Blob Storage    │
        │ (Tier-based) │   │ (Multi-tenant)  │
        └──────────────┘   └─────────────────┘
```

### Core Components

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Web Application** | ASP.NET Core 8, Blazor | User interface and API endpoints |
| **Database** | SQLite (Free) / Azure SQL (Paid) | Tenant data and metadata |
| **Blob Storage** | Azure Blob Storage | Media, documents, backups |
| **CDN** | Azure Front Door | Global content delivery |
| **Authentication** | Azure AD B2C, OAuth 2.0 | User authentication and SSO |
| **AI Agents** | Azure OpenAI Service | MOSAIC Public & Zoota Admin |
| **Monitoring** | Azure Application Insights | Telemetry and diagnostics |
| **Payment** | Stripe API | Payment processing |

## 🏢 Multi-Tenant Architecture

### Tenant Isolation Strategy

MOSAIC implements a **hybrid multi-tenant architecture** that varies by subscription tier:

#### Free Tier: SQLite Per Tenant
```
┌─────────────────────────────────────────┐
│  Azure App Service (Shared)             │
│  ┌───────────────────────────────────┐  │
│  │ Tenant A: SQLite DB (Local)       │  │
│  │ Tenant B: SQLite DB (Local)       │  │
│  │ Tenant C: SQLite DB (Local)       │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Azure Blob Storage                     │
│  ├── tenant-a/ (optional, small files)  │
│  ├── tenant-b/                          │
│  └── tenant-c/                          │
└─────────────────────────────────────────┘
```

**Characteristics:**
- Each tenant has isolated SQLite database file
- Fast onboarding with zero database provisioning
- Lower operational cost
- Limited to single-instance scaling
- Suitable for demos and small sites

#### Paid Tier: Shared Azure SQL
```
┌─────────────────────────────────────────┐
│  Azure App Service (Shared)             │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│  Azure SQL Database (Shared)            │
│  ┌───────────────────────────────────┐  │
│  │ Schema: tenant_a                  │  │
│  │ Schema: tenant_b                  │  │
│  │ Schema: tenant_c                  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Azure Blob Storage                     │
│  ├── tenant-a/ (dedicated container)    │
│  ├── tenant-b/                          │
│  └── tenant-c/                          │
└─────────────────────────────────────────┘
```

**Characteristics:**
- Shared Azure SQL with schema-based isolation
- Scalable to multiple app instances
- Dedicated blob containers per tenant
- Cost-effective for growing businesses
- Automatic backups and geo-redundancy

#### Enterprise Tier: Dedicated Infrastructure
```
┌─────────────────────────────────────────┐
│  Azure App Service (Dedicated)          │
│  - Tenant A Only                        │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│  Azure SQL Database (Dedicated)         │
│  - Tenant A Only                        │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Azure Blob Storage (Dedicated)         │
│  - Tenant A Only                        │
└─────────────────────────────────────────┘
```

**Characteristics:**
- Completely isolated infrastructure
- Maximum security and compliance
- Custom scaling and performance tuning
- Dedicated resources guarantee
- White-label capability

### Tenant Identification

Tenants are identified using multiple mechanisms:

1. **Domain-Based Routing**
   - Custom domains: `www.customer.com` → Tenant ID lookup
   - Subdomains: `customer.mosaic.app` → Direct mapping

2. **Authentication Context**
   - JWT token contains `TenantId` claim
   - Session stores tenant context
   - API requests include `X-Tenant-Id` header

3. **Database Schema Resolution**
   ```csharp
   public class TenantContext
   {
       public string TenantId { get; set; }
       public string DatabaseSchema { get; set; }
       public string BlobStoragePrefix { get; set; }
       public TierType Tier { get; set; }
   }
   ```

## 📊 Architecture by Tier

### Comparison Table

| Component | Free Tier | Paid Tier | Enterprise Tier |
|-----------|-----------|-----------|-----------------|
| **Web App Hosting** | Shared Azure App Service | Shared Azure App Service | Dedicated Azure App Service |
| **Database** | SQLite (per tenant file) | Azure SQL (shared, schema isolation) | Azure SQL (dedicated database) |
| **Storage** | Local/small blob (optional) | Azure Blob (per tenant container) | Azure Blob (dedicated storage account) |
| **Compute** | Shared B1 instance | Shared S1 instances | Dedicated P1V3+ instances |
| **Scaling** | Vertical only | Horizontal (3-10 instances) | Horizontal (unlimited) |
| **Backup** | Manual export | Daily automated | Hourly automated + geo-redundant |
| **SSL** | Shared certificate | SNI SSL | Dedicated IP SSL |
| **CDN** | Shared Front Door | Shared Front Door | Dedicated CDN endpoint |
| **Monitoring** | Basic metrics | Application Insights | Application Insights + custom dashboards |
| **SLA** | Best effort | 99.9% | 99.95% (custom SLA available) |
| **Support** | Community | Email (24h) | 24/7 phone + dedicated account manager |

### Free Tier Architecture

**Purpose:** Quick onboarding, demos, personal projects, MVP validation

**Infrastructure:**
```yaml
App Service Plan: B1 (Basic)
  - 1 Core, 1.75 GB RAM
  - Auto-scaling: Disabled
  - Cost: ~$13/month (shared across all free tenants)

Database: SQLite
  - File-based: /data/tenants/{tenant-id}.db
  - Max size: 100 MB per tenant
  - Backup: Manual export only

Storage: Local + Optional Blob
  - Local: 1 GB per tenant
  - Optional Blob: 5 GB per tenant
  - Cost: Included in tier

Bandwidth: 10 GB/month
  - Overage: Throttled or upgrade prompt
```

**Limitations:**
- 1 active site per tenant
- MOSAIC branding required
- Community support only
- No custom domain
- Limited API access

**Migration Path:**
- Automatic upgrade to Paid tier
- Zero downtime migration
- SQLite → Azure SQL automated

### Paid Tier Architecture

**Purpose:** Small businesses, startups, freelancers, multi-site projects

**Infrastructure:**
```yaml
App Service Plan: S1 (Standard)
  - 1 Core, 1.75 GB RAM per instance
  - Auto-scaling: 1-3 instances
  - Cost: ~$70/month (shared)

Database: Azure SQL S0
  - Shared database, schema per tenant
  - 10 DTUs shared
  - 250 GB max database size
  - Automated backups (7 days retention)
  - Geo-redundant backup

Storage: Azure Blob (Standard_RAGRS)
  - Dedicated container per tenant
  - 100 GB included per tenant
  - Additional storage: $0.018/GB/month
  - Automatic geo-replication

Bandwidth: 100 GB/month
  - Overage: $0.08/GB
```

**Features:**
- Up to 5 active sites
- Custom domain support
- Remove MOSAIC branding
- Email support (24h response)
- Basic analytics
- API access (10k requests/month)

### Enterprise Tier Architecture

**Purpose:** Large agencies, enterprises, white-label, compliance requirements

**Infrastructure:**
```yaml
App Service Plan: P1V3 or higher (Premium)
  - Dedicated instances
  - 2+ Cores, 8+ GB RAM per instance
  - Auto-scaling: 3-20 instances
  - VNet integration
  - Custom domain SSL
  - Cost: Custom pricing

Database: Azure SQL S3+ or Premium
  - Dedicated database per tenant
  - 100+ DTUs
  - Up to 1 TB per database
  - Point-in-time restore (35 days)
  - Active geo-replication
  - Advanced threat protection

Storage: Azure Blob (Premium_LRS or Premium_ZRS)
  - Dedicated storage account
  - Unlimited capacity
  - Premium performance tier
  - Private endpoints
  - Custom CDN configuration

Bandwidth: Unlimited
  - Enterprise peering agreements
```

**Features:**
- Unlimited sites
- White-label capability
- Full API access (unlimited)
- GraphQL API
- 24/7 phone support
- Custom SLA
- Dedicated account manager
- Compliance certifications
- Private VNet integration

## 🔐 Security Architecture

### Authentication & Authorization

```
┌─────────────┐
│   User      │
└──────┬──────┘
       │ 1. Login
       ▼
┌─────────────────┐
│ Azure AD B2C    │
│ (OAuth 2.0)     │
└──────┬──────────┘
       │ 2. Token
       ▼
┌─────────────────┐
│ API Gateway     │
│ (Validation)    │
└──────┬──────────┘
       │ 3. Validated Request
       ▼
┌─────────────────┐
│ MOSAIC App      │
│ (Tenant Context)│
└─────────────────┘
```

**Authentication Methods:**
1. **Email + Password**: Traditional with strong password requirements
2. **OAuth 2.0**: Google, GitHub, Microsoft
3. **Azure AD B2C**: Enterprise SSO
4. **API Keys**: For programmatic access
5. **JWT Tokens**: Bearer token authentication

**Authorization Model:**
```
User
  └─ Tenant Membership
       └─ Role (Owner, Admin, Editor, Viewer)
            └─ Permissions (CRUD operations)
                 └─ Resource Access (Sites, Content, Settings)
```

### Network Security

**Ingress Protection:**
- Azure Front Door with Web Application Firewall (WAF)
- DDoS protection (Standard)
- Rate limiting per tenant
- IP allowlist/blocklist (Enterprise)

**Data Protection:**
- TLS 1.2+ for all connections
- Certificate auto-renewal
- Data encryption at rest (Azure Storage Service Encryption)
- Transparent Data Encryption (TDE) for Azure SQL

**Egress Control:**
- Managed identities for Azure resource access
- No hardcoded credentials
- Azure Key Vault for secrets
- Secure connection strings

### Compliance & Certifications

| Standard | Status | Tier Availability |
|----------|--------|-------------------|
| **GDPR** | Compliant | All tiers |
| **SOC 2 Type II** | In progress | Enterprise |
| **ISO 27001** | Planned | Enterprise |
| **HIPAA** | Planned | Enterprise (on request) |
| **PCI DSS** | Via Stripe | All tiers (payment only) |

## 📈 Scalability & Performance

### Horizontal Scaling

**Auto-Scaling Rules:**

```yaml
Free Tier:
  - Instances: 1 (fixed)
  - Scaling: None

Paid Tier:
  - Min Instances: 1
  - Max Instances: 3
  - Scale-out: CPU > 70% for 5 minutes
  - Scale-in: CPU < 30% for 10 minutes

Enterprise Tier:
  - Min Instances: 3
  - Max Instances: 20+
  - Custom rules based on:
    - CPU utilization
    - Memory pressure
    - Request queue length
    - Custom metrics
```

### Database Scaling

**Paid Tier:**
- Automatic storage growth
- Manual DTU scaling
- Read replicas (planned)

**Enterprise Tier:**
- Elastic pools for multiple tenants
- Premium tier with high DTU
- Active geo-replication
- Failover groups

### Caching Strategy

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Azure CDN    │ ← Static assets (CSS, JS, images)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Redis Cache  │ ← Session data, API responses
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ App Service  │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Database     │
└──────────────┘
```

**Cache Layers:**
1. **CDN (Azure Front Door)**: Static content, 7-day TTL
2. **Redis Cache**: Session state, API responses, 1-hour TTL
3. **In-Memory**: EF Core query cache, 5-minute TTL
4. **HTTP Response Cache**: Page-level caching, 1-minute TTL

### Performance Targets

| Metric | Free Tier | Paid Tier | Enterprise Tier |
|--------|-----------|-----------|-----------------|
| **Page Load Time** | < 3s | < 2s | < 1s |
| **API Response Time** | < 500ms | < 300ms | < 100ms |
| **Time to First Byte** | < 500ms | < 300ms | < 150ms |
| **Concurrent Users** | 100 | 1,000 | 10,000+ |
| **Uptime SLA** | Best effort | 99.9% | 99.95% |

## 🔄 Data Architecture

### Data Flow

```
┌─────────────────────────────────────────────┐
│              User Actions                    │
└─────┬────────────────────────────────┬──────┘
      │                                │
      ▼                                ▼
┌─────────────┐                 ┌──────────────┐
│ Write Path  │                 │  Read Path   │
└─────┬───────┘                 └──────┬───────┘
      │                                │
      ▼                                ▼
┌─────────────┐                 ┌──────────────┐
│ Validation  │                 │ Cache Check  │
└─────┬───────┘                 └──────┬───────┘
      │                                │
      ▼                                ├─── Cache Hit → Return
┌─────────────┐                       │
│ Database    │                       └─── Cache Miss
│ Write       │                              │
└─────┬───────┘                              ▼
      │                         ┌────────────────────┐
      ├─── Success             │ Database Query     │
      │                         └────────┬───────────┘
      ▼                                  │
┌─────────────┐                          ▼
│ Cache       │                 ┌────────────────────┐
│ Invalidate  │                 │ Update Cache       │
└─────────────┘                 └────────────────────┘
```

### Database Schema Design

**Schema Isolation (Paid Tier):**
```sql
-- Each tenant has a dedicated schema
CREATE SCHEMA tenant_abc123;
CREATE SCHEMA tenant_xyz789;

-- Tenant-specific tables
CREATE TABLE tenant_abc123.Sites (...);
CREATE TABLE tenant_abc123.Pages (...);
CREATE TABLE tenant_abc123.Users (...);

CREATE TABLE tenant_xyz789.Sites (...);
CREATE TABLE tenant_xyz789.Pages (...);
CREATE TABLE tenant_xyz789.Users (...);
```

**Shared Platform Tables:**
```sql
-- Global, cross-tenant tables
CREATE TABLE Platform.Tenants (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255),
    Tier NVARCHAR(50),
    CreatedAt DATETIME2,
    SubscriptionStatus NVARCHAR(50)
);

CREATE TABLE Platform.Subscriptions (
    SubscriptionId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES Platform.Tenants,
    PlanTier NVARCHAR(50),
    BillingCycle NVARCHAR(50),
    Amount DECIMAL(18,2),
    StripeSubscriptionId NVARCHAR(255)
);
```

### Blob Storage Structure

```
mosaicsaas.blob.core.windows.net/
├── images/
│   ├── tenant-abc123/
│   │   ├── logo.png
│   │   ├── banner.jpg
│   │   └── gallery/
│   │       ├── img1.jpg
│   │       └── img2.png
│   └── tenant-xyz789/
│       └── profile.jpg
├── documents/
│   ├── tenant-abc123/
│   │   ├── invoice-2024-01.pdf
│   │   └── contract.docx
│   └── tenant-xyz789/
│       └── report.xlsx
├── backups/
│   ├── tenant-abc123/
│   │   ├── 2024-12-01-full.bak
│   │   └── 2024-12-08-incremental.bak
│   └── tenant-xyz789/
│       └── 2024-12-01-full.bak
└── media-assets/
    ├── tenant-abc123/
    │   └── videos/
    │       └── intro.mp4
    └── tenant-xyz789/
```

## 🚀 Deployment Architecture

### CI/CD Pipeline

```
┌─────────────┐
│   GitHub    │
└──────┬──────┘
       │ Push
       ▼
┌─────────────────┐
│ GitHub Actions  │
│ - Build         │
│ - Test          │
│ - Security Scan │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Azure Container │
│ Registry        │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Staging Slot    │
│ (Validation)    │
└──────┬──────────┘
       │ Approve
       ▼
┌─────────────────┐
│ Production Slot │
│ (Blue/Green)    │
└─────────────────┘
```

**Deployment Stages:**

1. **Build & Test**
   - Compile .NET application
   - Run unit tests
   - Run integration tests
   - Code quality analysis

2. **Security Scanning**
   - SAST (Static Analysis)
   - Dependency vulnerability scan
   - Container image scanning
   - Secret detection

3. **Deploy to Staging**
   - Deploy to staging slot
   - Run smoke tests
   - Performance benchmarking
   - Manual QA approval

4. **Deploy to Production**
   - Blue/green deployment
   - Gradual rollout (10% → 50% → 100%)
   - Health check monitoring
   - Automatic rollback on failure

### Environment Configuration

```
┌──────────────┐
│ Development  │ ← Local development
├──────────────┤
│ Staging      │ ← Pre-production testing
├──────────────┤
│ Production   │ ← Live environment
└──────────────┘
```

**Per-Environment Resources:**

| Resource | Development | Staging | Production |
|----------|-------------|---------|------------|
| **App Service** | Free F1 | Standard S1 | Premium P1V3 |
| **Database** | SQLite | Azure SQL Basic | Azure SQL Standard/Premium |
| **Blob Storage** | Local emulator | Standard_LRS | Standard_RAGRS |
| **Redis Cache** | Local/Docker | Basic C0 | Standard C1+ |
| **Application Insights** | Shared | Dedicated | Dedicated |

## 📊 Monitoring & Observability

### Telemetry Collection

```
┌────────────────────┐
│  Application       │
│  Instrumentation   │
└─────────┬──────────┘
          │
          ▼
┌─────────────────────────────────┐
│  Application Insights           │
│  - Request telemetry            │
│  - Exception tracking           │
│  - Custom events                │
│  - Dependency tracking          │
│  - Performance counters         │
└─────────┬───────────────────────┘
          │
          ▼
┌─────────────────────────────────┐
│  Azure Monitor                  │
│  - Metrics & alerts             │
│  - Log Analytics                │
│  - Workbooks & dashboards       │
└─────────────────────────────────┘
```

### Key Metrics

**Application Metrics:**
- Request rate (req/sec)
- Response time (p50, p95, p99)
- Error rate (%)
- Dependency call duration
- Cache hit ratio

**Infrastructure Metrics:**
- CPU utilization (%)
- Memory usage (%)
- Disk I/O (IOPS)
- Network throughput (Mbps)
- SQL DTU utilization (%)

**Business Metrics:**
- Active tenants
- New sign-ups per day
- Conversion rate (free → paid)
- Monthly recurring revenue (MRR)
- Churn rate

### Alerting Rules

**Critical Alerts (P1):**
- Application unresponsive (5xx errors > 5%)
- Database connection failure
- Blob storage unavailable
- SSL certificate expiration < 7 days

**Warning Alerts (P2):**
- CPU utilization > 80% for 10 minutes
- Memory pressure > 85%
- Response time p95 > 2 seconds
- Failed login attempts > 10/minute

**Info Alerts (P3):**
- New tenant sign-up
- Subscription upgrade/downgrade
- High bandwidth usage (approaching limit)

## 🔄 Disaster Recovery

### Backup Strategy

**Database Backups:**
```
Free Tier:
  - Manual export (user-initiated)
  - Frequency: On-demand

Paid Tier:
  - Automated daily backups
  - Retention: 7 days
  - Point-in-time restore

Enterprise Tier:
  - Automated hourly backups
  - Retention: 35 days
  - Geo-redundant storage
  - Point-in-time restore
```

**Blob Storage:**
- Geo-redundant replication (RAGRS)
- Soft delete enabled (7-day retention)
- Versioning for critical containers
- Cross-region backup for Enterprise

### Recovery Procedures

**RTO (Recovery Time Objective):**
- Free Tier: Best effort
- Paid Tier: 4 hours
- Enterprise Tier: 1 hour

**RPO (Recovery Point Objective):**
- Free Tier: N/A
- Paid Tier: 24 hours
- Enterprise Tier: 1 hour

**Failover Process:**
1. Detect failure (automated monitoring)
2. Assess impact and scope
3. Initiate failover to secondary region
4. Update DNS records
5. Verify service availability
6. Notify customers

## 📚 Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [Azure Blob Storage Best Practices](https://docs.microsoft.com/azure/storage/blobs/)
- [Multi-Tenant SaaS Patterns](https://docs.microsoft.com/azure/architecture/guide/multitenant/)
- [MOSAIC SaaS Features](./SaaS_FEATURES.md)
- [Azure Blob Storage Integration](./AZURE_BLOB_STORAGE.md)

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
