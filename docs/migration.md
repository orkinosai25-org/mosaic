# MOSAIC Migration Toolkit

This document provides comprehensive guidance for migrating websites and content from legacy platforms (Wix, Umbraco, SharePoint) to the MOSAIC SaaS platform.

## 🎯 Migration Overview

MOSAIC offers guided migration workflows and conversational AI assistance to make platform transitions seamless and efficient. Our migration toolkit supports three primary legacy platforms:

- **Wix** - Popular website builder
- **Umbraco** - Open-source .NET CMS
- **SharePoint** - Microsoft enterprise collaboration platform

### Migration Benefits

✅ **Zero Data Loss** - Complete content and media preservation  
✅ **Conversational Guidance** - MOSAIC Agent walks you through each step  
✅ **Automated Tools** - Script-based content extraction and import  
✅ **Flexible Timing** - Migrate at your own pace with staging environment  
✅ **Support Included** - Migration assistance with Paid and Enterprise tiers

## 🚀 Getting Started

### Pre-Migration Checklist

Before starting your migration:

- [ ] **Audit Current Site**: Document all pages, posts, media, and users
- [ ] **Choose MOSAIC Plan**: Select appropriate tier for your needs
- [ ] **Backup Everything**: Create complete backup of current platform
- [ ] **Prepare Team**: Notify team members about migration timeline
- [ ] **Set Up MOSAIC**: Create account and workspace
- [ ] **Review Migration Guide**: Read platform-specific instructions below

### Migration Process Overview

```
1. Assessment      → Analyze current site and content
2. Planning        → Create migration timeline and strategy
3. Preparation     → Export data from source platform
4. Import          → Load content into MOSAIC
5. Configuration   → Set up themes, domains, settings
6. Validation      → Test and verify all content
7. Launch          → Switch DNS and go live
8. Post-Migration  → Monitor and optimize
```

## 📦 Migrating from Wix

### Overview

Wix is a popular drag-and-drop website builder. MOSAIC provides tools to migrate your Wix site while preserving content and design.

### What Gets Migrated

| Content Type | Supported | Notes |
|--------------|-----------|-------|
| **Pages** | ✅ Yes | HTML content and structure |
| **Blog Posts** | ✅ Yes | Posts, categories, tags |
| **Images** | ✅ Yes | Uploaded to Azure Blob Storage |
| **Documents** | ✅ Yes | PDFs, documents preserved |
| **Custom CSS** | ⚠️ Partial | May require adaptation |
| **Forms** | ⚠️ Partial | Basic forms; custom forms need rebuild |
| **E-commerce** | ❌ Planned | Not in initial release |
| **Wix Apps** | ❌ No | Requires alternative solutions |

### Migration Steps

#### Step 1: Export Content from Wix

**Option A: Manual Export** (Small sites)

1. **Export Pages**:
   - In Wix Editor, go to each page
   - Copy content to a document
   - Note page structure and hierarchy

2. **Export Blog**:
   - Dashboard → Blog → Manage Posts
   - Export as RSS feed (limited) or manual copy
   - Download: `https://yoursite.wix.com/blog-feed.xml`

3. **Download Media**:
   - Media Manager → Select all images
   - Download to local folder
   - Organize by page/section

4. **Export Form Submissions** (if needed):
   - Dashboard → Forms & Submissions
   - Export to CSV

**Option B: Automated Tool** (Larger sites - Paid/Enterprise)

Use MOSAIC Migration CLI:

```bash
# Install MOSAIC CLI
npm install -g @mosaic/migration-cli

# Authenticate
mosaic login

# Configure Wix site
mosaic configure wix \
  --site-url https://yoursite.wixsite.com/site \
  --api-key YOUR_WIX_API_KEY

# Run migration assessment
mosaic assess wix

# Export content
mosaic export wix \
  --output ./wix-export \
  --include pages,blog,media

# Review export
ls -la ./wix-export/
```

#### Step 2: Create MOSAIC Site

1. **Sign in to MOSAIC**: Navigate to dashboard
2. **Create New Site**: Click "New Site" button
3. **Select "Migrate from Wix"**: Choose migration wizard
4. **Choose Theme**: Select Ottoman-inspired theme closest to your Wix design

#### Step 3: Import Content to MOSAIC

**Using MOSAIC Dashboard** (Recommended for small sites):

1. **Navigate**: Dashboard → Sites → Your Site → Import
2. **Select Source**: Choose "Wix"
3. **Upload Export**: Upload your exported content files
4. **Map Content**: 
   - Map Wix pages to MOSAIC pages
   - Match blog categories
   - Assign content types
5. **Upload Media**: Bulk upload images and documents
6. **Review**: Preview imported content

**Using MOSAIC CLI** (Recommended for larger sites):

```bash
# Authenticate with your MOSAIC account
mosaic login

# Select target site
mosaic site select my-new-site

# Import pages
mosaic import wix \
  --source ./wix-export \
  --type pages \
  --tenant-id your-tenant-id

# Import blog posts
mosaic import wix \
  --source ./wix-export \
  --type blog \
  --tenant-id your-tenant-id

# Upload media
mosaic upload media \
  --source ./wix-export/media \
  --container images \
  --tenant-id your-tenant-id

# Verify import
mosaic status
```

#### Step 4: Adjust Styling

1. **Select Base Theme**: Choose Ottoman theme that matches Wix style
2. **Customize Colors**: Extract colors from Wix site and apply
3. **Upload Logo**: Upload your existing logo
4. **Adjust Layout**: Use MOSAIC's page builder to match original layout
5. **Add Custom CSS**: If needed (Pro+ plans only)

#### Step 5: Configure Settings

1. **Set Up Navigation**: Recreate menu structure
2. **Configure SEO**: Add meta titles and descriptions
3. **Set Up Forms**: Rebuild contact forms using MOSAIC forms
4. **Connect Domain**: Point your existing domain to MOSAIC
5. **Configure Redirects**: Set up URL redirects if structure changed

#### Step 6: Test & Launch

1. **Preview Site**: Test all pages and functionality
2. **Check Mobile**: Verify responsive design
3. **Test Forms**: Submit test form entries
4. **Verify Media**: Ensure all images load correctly
5. **Update DNS**: Point domain to MOSAIC
6. **Monitor**: Check analytics and error logs

### Wix to MOSAIC Feature Mapping

| Wix Feature | MOSAIC Equivalent |
|-------------|-------------------|
| Wix Editor | MOSAIC Page Builder |
| Wix Blog | MOSAIC Blog System |
| Wix Forms | MOSAIC Forms (rebuild required) |
| Wix Bookings | Third-party integration |
| Wix Stores | Planned (e-commerce module) |
| Wix SEO Wiz | Built-in SEO tools + MOSAIC AI |
| Wix Mobile | Responsive by default |
| Wix Analytics | MOSAIC Analytics Dashboard |

### Common Wix Migration Challenges

**Challenge 1: Custom Apps/Widgets**
- **Solution**: Replace with MOSAIC features or third-party integrations
- **Example**: Calendar widget → Integrate with Google Calendar API

**Challenge 2: Wix Stores (E-commerce)**
- **Solution**: Wait for MOSAIC e-commerce (Q4 2025) or use Shopify integration
- **Workaround**: Use external payment links temporarily

**Challenge 3: Complex Animations**
- **Solution**: Rebuild using CSS animations or lightweight JavaScript
- **Alternative**: Simplify design for faster load times

**Challenge 4: Wix-Specific URLs**
- **Solution**: Set up 301 redirects in MOSAIC
- **Example**: `/services-1` → `/services`

### Estimated Migration Time

| Site Size | Pages | Blog Posts | Estimated Time |
|-----------|-------|------------|----------------|
| **Small** | 1-10 | 0-20 | 2-4 hours |
| **Medium** | 11-50 | 21-100 | 1-2 days |
| **Large** | 51+ | 100+ | 3-5 days |

## 🏢 Migrating from Umbraco

### Overview

Umbraco is an open-source .NET CMS. Migration to MOSAIC involves exporting content from Umbraco database and transforming it for MOSAIC.

### What Gets Migrated

| Content Type | Supported | Notes |
|--------------|-----------|-------|
| **Content Nodes** | ✅ Yes | All content types |
| **Media Library** | ✅ Yes | Images, documents, videos |
| **Document Types** | ⚠️ Partial | Map to MOSAIC content types |
| **Templates** | ❌ No | Rebuild using MOSAIC themes |
| **Macros** | ❌ No | Replace with MOSAIC components |
| **Users** | ✅ Yes | User accounts and roles |
| **Settings** | ⚠️ Partial | Manual configuration |

### Migration Steps

#### Step 1: Export from Umbraco

**Option A: Umbraco Content Export Package**

1. **Install uSync Package** (if not already installed):
   ```bash
   Install-Package uSync -Version 8.13.0
   ```

2. **Export All Content**:
   - Umbraco Backoffice → Settings → uSync
   - Click "Export" → "Export All"
   - Files saved to `/uSync/v9/` directory

3. **Export Media**:
   - Navigate to Media section
   - Select all media items
   - Export or copy from `/media/` directory

4. **Export Database** (backup):
   ```sql
   -- For SQL Server
   BACKUP DATABASE UmbracoDb 
   TO DISK = 'C:\Backups\UmbracoDb.bak'
   WITH FORMAT, COMPRESSION;
   ```

**Option B: Direct Database Export**

```sql
-- Export content nodes
SELECT * FROM cmsContent
JOIN cmsContentVersion ON cmsContent.nodeId = cmsContentVersion.nodeId
JOIN cmsPropertyData ON cmsContentVersion.id = cmsPropertyData.versionId
ORDER BY cmsContent.nodeId;

-- Export to CSV
-- Use SQL Server Management Studio or similar tool
```

**Option C: Use MOSAIC Umbraco Connector** (Enterprise only)

```bash
# Install MOSAIC Umbraco Migration Tool
dotnet tool install --global Mosaic.Migration.Umbraco

# Connect to Umbraco database
mosaic-umbraco configure \
  --connection-string "Server=localhost;Database=UmbracoDb;..." \
  --umbraco-version 8

# Analyze content
mosaic-umbraco analyze

# Export content
mosaic-umbraco export \
  --output ./umbraco-export \
  --include-media true \
  --include-users true
```

#### Step 2: Transform Content

**Map Umbraco Document Types to MOSAIC Content Types**:

```json
{
  "documentTypeMappings": {
    "homePage": "page",
    "contentPage": "page",
    "blogPost": "blog-post",
    "newsArticle": "blog-post",
    "productPage": "product"
  },
  "propertyMappings": {
    "pageTitle": "title",
    "bodyText": "content",
    "metaDescription": "description",
    "publishDate": "published_at",
    "author": "author"
  }
}
```

**Transform Script**:

```bash
# Use MOSAIC CLI to transform
mosaic transform umbraco \
  --input ./umbraco-export \
  --output ./mosaic-import \
  --mapping ./mapping-config.json
```

#### Step 3: Import to MOSAIC

```bash
# Authenticate
mosaic login

# Select target site
mosaic site select my-umbraco-migration

# Import pages
mosaic import generic \
  --source ./mosaic-import/pages \
  --type pages

# Import blog posts
mosaic import generic \
  --source ./mosaic-import/blog \
  --type blog

# Upload media
mosaic upload media \
  --source ./mosaic-import/media \
  --container images
```

#### Step 4: Rebuild Templates

1. **Choose MOSAIC Theme**: Select base theme
2. **Create Custom Layouts**: Use page builder for custom layouts
3. **Add Components**: Rebuild Umbraco macros as MOSAIC components
4. **Test Rendering**: Preview all content types

#### Step 5: Configure & Launch

1. **User Migration**: Import users or invite team members
2. **Role Mapping**: Map Umbraco roles to MOSAIC roles
3. **URL Structure**: Maintain URLs or set up redirects
4. **Domain Setup**: Point domain to MOSAIC
5. **Go Live**: Update DNS and launch

### Umbraco to MOSAIC Feature Mapping

| Umbraco Feature | MOSAIC Equivalent |
|-----------------|-------------------|
| Document Types | Content Types (customizable) |
| Content Nodes | Pages/Posts |
| Media Library | Azure Blob Storage |
| Templates | MOSAIC Themes |
| Macros | Components/Widgets |
| Users & Members | MOSAIC User Management |
| Forms | MOSAIC Forms |
| Multi-site | Multi-tenant architecture |

### Estimated Migration Time

| Site Size | Content Nodes | Media Files | Estimated Time |
|-----------|---------------|-------------|----------------|
| **Small** | 1-100 | 0-500 | 3-5 days |
| **Medium** | 101-1000 | 501-2000 | 1-2 weeks |
| **Large** | 1000+ | 2000+ | 2-4 weeks |

## 📊 Migrating from SharePoint

### Overview

SharePoint is Microsoft's enterprise collaboration platform. Migrating to MOSAIC involves extracting content from SharePoint lists, libraries, and pages.

### What Gets Migrated

| Content Type | Supported | Notes |
|--------------|-----------|-------|
| **Pages** | ✅ Yes | Modern and classic pages |
| **Document Libraries** | ✅ Yes | Files and metadata |
| **Lists** | ⚠️ Partial | Exported as structured data |
| **Images** | ✅ Yes | From image libraries and pages |
| **Navigation** | ✅ Yes | Site navigation structure |
| **Permissions** | ⚠️ Partial | Role mapping required |
| **Workflows** | ❌ No | Rebuild using MOSAIC automation |
| **Custom Web Parts** | ❌ No | Replace with MOSAIC components |

### Migration Steps

#### Step 1: Export from SharePoint

**Option A: SharePoint Migration Tool** (Recommended)

1. **Download Tool**:
   - Download from [Microsoft SharePoint Migration Tool](https://aka.ms/spmt)
   - Install on Windows machine

2. **Connect to SharePoint**:
   - Launch SPMT
   - Enter SharePoint site URL
   - Authenticate with admin account

3. **Select Content**:
   - Choose site, libraries, or lists to migrate
   - Preview content structure

4. **Export to Local**:
   - Export to local folder
   - Choose JSON format for metadata

**Option B: PnP PowerShell** (Advanced)

```powershell
# Install PnP PowerShell
Install-Module PnP.PowerShell -Force

# Connect to SharePoint
Connect-PnPOnline -Url https://yourtenant.sharepoint.com/sites/yoursite -Interactive

# Export site pages
$pages = Get-PnPClientSidePage
foreach ($page in $pages) {
    $pageName = $page.Name
    $content = Get-PnPClientSidePage -Identity $page.Name
    $content | Export-Clixml -Path "./export/pages/$pageName.xml"
}

# Export document library
$library = Get-PnPList -Identity "Documents"
$files = Get-PnPListItem -List $library
foreach ($file in $files) {
    $fileRef = $file["FileRef"]
    Get-PnPFile -Url $fileRef -Path "./export/documents" -AsFile
}

# Export list data
$list = Get-PnPList -Identity "YourList"
$items = Get-PnPListItem -List $list
$items | Export-Csv -Path "./export/lists/yourlist.csv"
```

**Option C: MOSAIC SharePoint Connector** (Enterprise only)

```bash
# Install connector
dotnet tool install --global Mosaic.Migration.SharePoint

# Configure SharePoint connection
mosaic-sharepoint configure \
  --tenant-url https://yourtenant.sharepoint.com \
  --site /sites/yoursite \
  --client-id YOUR_CLIENT_ID \
  --client-secret YOUR_CLIENT_SECRET

# Analyze site
mosaic-sharepoint analyze

# Export content
mosaic-sharepoint export \
  --output ./sharepoint-export \
  --include pages,libraries,lists \
  --preserve-metadata true
```

#### Step 2: Transform SharePoint Content

**Map SharePoint Components to MOSAIC**:

```json
{
  "contentTypeMappings": {
    "Site Page": "page",
    "Article Page": "blog-post",
    "News Post": "blog-post",
    "Wiki Page": "page"
  },
  "columnMappings": {
    "Title": "title",
    "Created": "created_at",
    "Modified": "updated_at",
    "Author": "author",
    "Editor": "last_edited_by"
  }
}
```

**Transform Pages**:

```bash
# Transform SharePoint pages to MOSAIC format
mosaic transform sharepoint \
  --input ./sharepoint-export \
  --output ./mosaic-import \
  --mapping ./sp-mapping.json \
  --convert-webparts true
```

#### Step 3: Import to MOSAIC

```bash
# Authenticate
mosaic login

# Select site
mosaic site select my-sharepoint-migration

# Import pages
mosaic import sharepoint \
  --source ./mosaic-import/pages \
  --type pages \
  --preserve-urls true

# Upload documents
mosaic upload media \
  --source ./mosaic-import/documents \
  --container documents \
  --preserve-structure true

# Import structured data
mosaic import data \
  --source ./mosaic-import/lists \
  --format csv
```

#### Step 4: Rebuild Functionality

**Navigation**:
1. Recreate site navigation in MOSAIC
2. Maintain URL structure or set up redirects
3. Configure breadcrumbs

**Web Parts Replacement**:
| SharePoint Web Part | MOSAIC Replacement |
|---------------------|-------------------|
| Document Library | File gallery component |
| List View | Data table component |
| News Feed | Blog listing |
| Image Gallery | Image carousel |
| Contact List | Team directory |
| Calendar | Calendar integration |

**Permissions**:
```bash
# Export SharePoint permissions
mosaic-sharepoint export-permissions \
  --output ./permissions.csv

# Map to MOSAIC roles:
# - Owner → Owner
# - Site Admin → Admin
# - Contributor → Editor
# - Visitor → Viewer

# Import users
mosaic import users \
  --source ./permissions.csv \
  --send-invites true
```

#### Step 5: Configure & Launch

1. **Branding**: Apply MOSAIC theme similar to SharePoint design
2. **Navigation**: Set up header and sidebar navigation
3. **Domain**: Configure custom domain
4. **Test**: Comprehensive testing of all content
5. **Launch**: Update DNS and go live

### SharePoint to MOSAIC Feature Mapping

| SharePoint Feature | MOSAIC Equivalent |
|-------------------|-------------------|
| Site Pages | MOSAIC Pages |
| Document Libraries | Azure Blob Storage + File Manager |
| Lists | Structured data tables |
| News Posts | Blog posts |
| Web Parts | MOSAIC Components |
| Site Navigation | MOSAIC Navigation |
| Permissions | Role-based access control |
| Search | MOSAIC search functionality |

### Estimated Migration Time

| Site Size | Pages | Libraries | Lists | Estimated Time |
|-----------|-------|-----------|-------|----------------|
| **Small** | 1-50 | 1-5 | 0-10 | 1-2 weeks |
| **Medium** | 51-200 | 6-20 | 11-50 | 2-4 weeks |
| **Large** | 201+ | 21+ | 51+ | 1-3 months |

## 🤖 Conversational Migration with MOSAIC Agent

### Using MOSAIC Agent for Guided Migration

MOSAIC Agent provides conversational guidance throughout your migration:

**Example Conversation**:

```
User: "I want to migrate my Wix site to MOSAIC"

MOSAIC Agent: "Great! I'll help you migrate from Wix. First, let me ask a few questions:
1. How many pages does your Wix site have?
2. Do you have a blog?
3. What's your current domain name?
4. When would you like to complete the migration?"

User: "About 15 pages, yes I have a blog with 30 posts, my domain is example.com, 
and I'd like to migrate next week"

MOSAIC Agent: "Perfect! Based on your site size, this migration will take approximately 
4-6 hours. Here's your personalized migration plan:

Week 1:
- Day 1: Export Wix content (I'll provide detailed instructions)
- Day 2: Create MOSAIC site and import content
- Day 3: Customize theme and styling
- Day 4: Test and verify content

I'll guide you through each step. Would you like to start with exporting your Wix content now?"

User: "Yes, let's start"

MOSAIC Agent: "Excellent! Here's what you need to do..."
```

### Agent-Assisted Features

**Content Analysis**:
```
"I've analyzed your export file. You have:
- 15 pages (all will migrate successfully)
- 30 blog posts (all compatible)
- 127 images (will upload to Azure Blob Storage)
- 3 contact forms (I'll help you rebuild these)
- 1 custom widget (needs alternative solution)

Estimated migration success: 95%"
```

**Step-by-Step Guidance**:
- Interactive checklists
- Real-time progress tracking
- Troubleshooting assistance
- Best practice recommendations

**Post-Migration Support**:
- SEO optimization tips
- Performance recommendations
- Content improvement suggestions
- Feature discovery guidance

## 🎁 Migration Incentives & Support

### Founding Member Benefits

Early adopters who migrate to MOSAIC receive:

- 🏆 **Founding Member Badge**: Display on your site
- 💰 **Migration Discount**: 25% off first 3 months (Pro/Business)
- 🚀 **Priority Migration**: Dedicated migration assistance
- 📞 **Live Support**: Direct access to migration specialists
- 🎓 **Training Sessions**: Personalized platform training

### Migration Support by Tier

| Support Level | Free | Pro | Business | Enterprise |
|---------------|------|-----|----------|------------|
| **Migration Guide** | ✅ | ✅ | ✅ | ✅ |
| **MOSAIC Agent** | ✅ | ✅ | ✅ | ✅ |
| **CLI Tools** | ❌ | ✅ | ✅ | ✅ |
| **Email Support** | ❌ | ✅ | ✅ | ✅ |
| **Live Chat** | ❌ | ❌ | ✅ | ✅ |
| **Dedicated Specialist** | ❌ | ❌ | ❌ | ✅ |
| **Hands-On Migration** | ❌ | ❌ | ❌ | ✅ (Optional) |

### Early Adopter Programs

**For Agencies**:
- Manage multiple client migrations
- Bulk migration discounts
- White-label options (Enterprise)
- Revenue sharing program

**For Businesses**:
- Enterprise migration planning
- Staged rollout support
- Training for internal teams
- Custom integration assistance

## 🔧 Troubleshooting Common Migration Issues

### Issue 1: Content Not Importing

**Symptoms**: Import completes but pages are empty

**Solutions**:
1. Check export file format (must be valid JSON/XML)
2. Verify tenant ID in API calls
3. Check file upload size limits (10 MB max)
4. Review import logs for errors

### Issue 2: Media Files Missing

**Symptoms**: Images show broken links

**Solutions**:
1. Verify media uploaded to correct container
2. Check tenant isolation (files in correct tenant folder)
3. Ensure file types are supported
4. Re-upload missing files individually

### Issue 3: Broken URLs/Links

**Symptoms**: Internal links don't work

**Solutions**:
1. Set up URL redirects (301)
2. Use MOSAIC's find-and-replace tool
3. Update link references in bulk
4. Configure custom slug patterns

### Issue 4: Styling Issues

**Symptoms**: Site looks different than original

**Solutions**:
1. Extract and apply original color palette
2. Adjust theme settings to match
3. Add custom CSS (Pro+ plans)
4. Work with MOSAIC Agent for design suggestions

## 📞 Getting Migration Help

### Support Channels

**Documentation**:
- [Migration FAQ](https://docs.mosaic.orkinosai.com/migration-faq)
- [Video Tutorials](https://tutorials.mosaic.orkinosai.com/migration)
- [Community Forum](https://community.mosaic.orkinosai.com/migration)

**Direct Support**:
- Email: migration@mosaic.orkinosai.com
- Live Chat: Available in dashboard (Business+)
- Phone: Enterprise customers only

**MOSAIC Agent**:
- Available 24/7 in dashboard
- Migration-specific guidance
- Real-time troubleshooting

## 📚 Additional Resources

- [Architecture Overview](./architecture.md)
- [SaaS Features](./SaaS_FEATURES.md)
- [Onboarding Guide](./ONBOARDING.md)
- [Azure Blob Storage](./AZURE_BLOB_STORAGE.md)
- [Portal UI Design](./portal-ui.md)

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team

**Ready to migrate?** Start your journey with MOSAIC today! 🚀
