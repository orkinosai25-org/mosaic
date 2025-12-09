# OrkinosaiCMS Deployment Checklist

## Overview

This checklist ensures a smooth deployment of OrkinosaiCMS to production (Azure Web Apps) or other hosting environments.

## Pre-Deployment

### 1. Code Quality
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] No compiler warnings
- [ ] Code review completed
- [ ] Security scan completed (CodeQL)
- [ ] Dependencies updated to latest stable versions

### 2. Configuration
- [ ] Connection strings configured for production database
- [ ] Application settings updated (appsettings.Production.json)
- [ ] CORS policies configured (if needed)
- [ ] Logging configured (Application Insights)
- [ ] Error handling configured
- [ ] Health check endpoints configured

### 3. Database
- [ ] Migrations tested locally
- [ ] Production database created
- [ ] Firewall rules configured
- [ ] Seed data reviewed and tested
- [ ] Backup strategy in place
- [ ] Connection pooling configured

### 4. Security
- [ ] HTTPS enforced
- [ ] Authentication configured
- [ ] Authorization policies defined
- [ ] API keys secured (Azure Key Vault)
- [ ] CORS properly configured
- [ ] SQL injection prevention verified
- [ ] XSS protection verified
- [ ] CSRF protection enabled

## Azure Deployment

### Phase 1: Infrastructure Setup

#### 1. Create Resource Group
```bash
az group create \
  --name rg-orkinosaicms-prod \
  --location eastus
```

#### 2. Create Azure SQL Database
```bash
# Create SQL Server
az sql server create \
  --name sql-orkinosaicms-prod \
  --resource-group rg-orkinosaicms-prod \
  --location eastus \
  --admin-user cmadmin \
  --admin-password <SecurePassword>

# Configure firewall
az sql server firewall-rule create \
  --resource-group rg-orkinosaicms-prod \
  --server sql-orkinosaicms-prod \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group rg-orkinosaicms-prod \
  --server sql-orkinosaicms-prod \
  --name db-orkinosaicms \
  --service-objective S1 \
  --backup-storage-redundancy Local
```

#### 3. Create App Service Plan
```bash
az appservice plan create \
  --name plan-orkinosaicms-prod \
  --resource-group rg-orkinosaicms-prod \
  --sku B1 \
  --is-linux
```

#### 4. Create Web App
```bash
az webapp create \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --plan plan-orkinosaicms-prod \
  --runtime "DOTNET|10.0"
```

#### 5. Create Application Insights
```bash
az monitor app-insights component create \
  --app ai-orkinosaicms \
  --location eastus \
  --resource-group rg-orkinosaicms-prod \
  --application-type web
```

### Phase 2: Configuration

#### 1. Configure Connection Strings
```bash
# Get connection string
CONN_STRING=$(az sql db show-connection-string \
  --client ado.net \
  --server sql-orkinosaicms-prod \
  --name db-orkinosaicms | tr -d '"')

# Set connection string in web app
az webapp config connection-string set \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --settings DefaultConnection="$CONN_STRING" \
  --connection-string-type SQLAzure
```

#### 2. Configure Application Settings
```bash
az webapp config appsettings set \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    WEBSITE_TIME_ZONE="Eastern Standard Time"
```

#### 3. Enable HTTPS Only
```bash
az webapp update \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --set httpsOnly=true
```

### Phase 3: Deployment

#### Option A: Deploy from Azure DevOps / GitHub Actions
- [ ] CI/CD pipeline configured
- [ ] Build triggers set up
- [ ] Deployment slots configured (staging)
- [ ] Automated testing in pipeline
- [ ] Approval gates configured

#### Option B: Deploy using Azure CLI
```bash
# Build and publish
dotnet publish -c Release -o ./publish

# Create ZIP file
cd publish && zip -r ../app.zip * && cd ..

# Deploy
az webapp deploy \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --src-path app.zip \
  --type zip
```

#### Option C: Deploy using Visual Studio
- [ ] Publish profile created
- [ ] Connection tested
- [ ] Published successfully
- [ ] Database migrated

### Phase 4: Post-Deployment

#### 1. Apply Database Migrations
```bash
# Using dotnet ef
dotnet ef database update \
  --project src/OrkinosaiCMS.Infrastructure \
  --startup-project src/OrkinosaiCMS.Web \
  --connection "<ProductionConnectionString>"
```

#### 2. Verify Seed Data
- [ ] Site created
- [ ] Themes loaded
- [ ] Master pages created
- [ ] Modules registered
- [ ] Sample pages created
- [ ] Roles and permissions configured

#### 3. Configure Custom Domain (Optional)
```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --hostname www.orkinosaicms.com

# Enable SSL
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod
```

## Post-Deployment Verification

### 1. Functional Testing
- [ ] Home page loads correctly
- [ ] Navigation works
- [ ] Master pages render properly
- [ ] Modules display content
- [ ] Forms submit successfully
- [ ] Database connectivity verified
- [ ] Static assets load (CSS, images)

### 2. Performance Testing
- [ ] Page load times < 3 seconds
- [ ] No memory leaks
- [ ] Database queries optimized
- [ ] CDN configured (if needed)
- [ ] Caching strategy verified

### 3. Security Testing
- [ ] HTTPS working
- [ ] No security headers missing
- [ ] Authentication working
- [ ] Authorization working
- [ ] No exposed secrets
- [ ] SQL injection prevention verified

### 4. Monitoring Setup
- [ ] Application Insights configured
- [ ] Alerts configured
  - [ ] High error rate alert
  - [ ] High response time alert
  - [ ] Database connection failures
  - [ ] Memory/CPU threshold alerts
- [ ] Log aggregation verified
- [ ] Dashboard created

## Monitoring & Maintenance

### Daily Checks
- [ ] Review Application Insights dashboard
- [ ] Check for any errors or warnings
- [ ] Verify all services are running
- [ ] Check database performance metrics

### Weekly Tasks
- [ ] Review and analyze logs
- [ ] Check for security updates
- [ ] Review performance metrics
- [ ] Verify backup success

### Monthly Tasks
- [ ] Update dependencies
- [ ] Review and optimize database
- [ ] Review hosting costs
- [ ] Capacity planning review
- [ ] Security audit

## Rollback Plan

### If Deployment Fails

#### 1. Immediate Actions
```bash
# Swap back to previous deployment slot
az webapp deployment slot swap \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod \
  --slot staging \
  --target-slot production \
  --action swap
```

#### 2. Database Rollback
- Restore from last backup
- Revert migrations if necessary
- Verify data integrity

#### 3. Communication
- [ ] Notify stakeholders
- [ ] Update status page
- [ ] Document issues
- [ ] Create incident report

## Success Criteria

- ✅ Application accessible at production URL
- ✅ All pages loading correctly
- ✅ No errors in Application Insights
- ✅ Database connectivity working
- ✅ Authentication/Authorization working
- ✅ Performance meets SLA requirements
- ✅ Monitoring and alerts configured
- ✅ Backup strategy verified
- ✅ Documentation updated
- ✅ Team training completed

## Cost Optimization

### Azure Resources Monthly Estimate
| Resource | SKU | Estimated Cost |
|----------|-----|---------------|
| App Service Plan | B1 (Basic) | $13.00 |
| Azure SQL Database | S1 (Standard) | $30.00 |
| Application Insights | Pay-as-you-go | $5-10 |
| Storage Account | Standard LRS | $2-5 |
| **Total** | | **~$50-58/month** |

### Cost Savings Tips
- Use reserved instances for long-term deployments (30-40% savings)
- Configure auto-shutdown for dev/test environments
- Use Azure SQL DTU-based pricing for predictable workloads
- Implement caching to reduce database calls
- Configure CDN for static assets
- Use deployment slots efficiently

## Support & Troubleshooting

### Common Issues

#### Issue: Database Connection Timeout
**Solution**:
```bash
# Increase connection timeout in connection string
Server=...;Connection Timeout=60;

# Check firewall rules
az sql server firewall-rule list \
  --resource-group rg-orkinosaicms-prod \
  --server sql-orkinosaicms-prod
```

#### Issue: Application Won't Start
**Solution**:
```bash
# Check logs
az webapp log tail \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod

# Check environment variables
az webapp config appsettings list \
  --name app-orkinosaicms \
  --resource-group rg-orkinosaicms-prod
```

#### Issue: High Memory Usage
**Solution**:
- Review Application Insights for memory leaks
- Check for large query results
- Implement pagination
- Consider scaling up App Service Plan

### Getting Help
- **Documentation**: `/docs` folder
- **Application Insights**: Monitor for errors
- **Azure Support**: Create support ticket
- **GitHub Issues**: Report bugs

## Conclusion

Following this checklist ensures a successful deployment of OrkinosaiCMS to production. Regular monitoring and maintenance are crucial for long-term success.

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Next Review**: Monthly  
**Owner**: DevOps Team
