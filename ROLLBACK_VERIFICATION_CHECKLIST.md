# Post-Rollback Verification Checklist

## Immediate Verification Steps (DEV Environment)

### 1. Configuration Validation ✅
- [x] appsettings.json is valid JSON
- [x] appsettings.Production.json is valid JSON
- [x] Connection string is present and not empty
- [x] Database configuration includes AutoApplyMigrations=true

### 2. Build Verification ✅
- [x] Solution restores successfully
- [x] Solution builds without errors
- [x] Only pre-existing warnings remain (4 warnings, 0 errors)
- [x] No new compilation issues introduced

### 3. Security Scan ✅
- [x] CodeQL security scan completed
- [x] No new vulnerabilities detected
- [x] Code review completed and feedback addressed

### 4. Code Review ✅
- [x] Changes reviewed
- [x] Security concerns documented
- [x] Connection pooling handled by Program.cs
- [x] Azure App Service override instructions documented

## Production Deployment Verification

### Pre-Deployment Checklist
- [ ] Verify Azure SQL Database is accessible
  ```bash
  # Test from Azure Portal > SQL Database > Query Editor
  # OR using Azure CLI
  az sql db show --name mosaic-saas --server orkinosai --resource-group orkinosai_group
  ```

- [ ] Confirm Azure App Service is running
  ```bash
  az webapp show --name orkinosai-cms --resource-group orkinosai_group
  ```

- [ ] Check firewall rules allow App Service to database
  ```bash
  az sql server firewall-rule list --server orkinosai --resource-group orkinosai_group
  ```

### Post-Deployment Verification

#### 1. Application Startup (Expected: 2-3 minutes)
- [ ] Monitor Application Insights for startup logs
- [ ] Check for "Application started" message
- [ ] No HTTP 500.30 errors in logs
- [ ] No "Invalid object name 'AspNetUsers'" errors

**Command:**
```bash
az webapp log tail --name orkinosai-cms --resource-group orkinosai_group
```

**Expected Output:**
```
[2025-12-23 HH:MM:SS] [Information] Starting OrkinosaiCMS
[2025-12-23 HH:MM:SS] [Information] Using SQL Server database provider
[2025-12-23 HH:MM:SS] [Information] Connection string (sanitized): Server=tcp:orkinosai.database.windows.net,1433;...;Password=***
[2025-12-23 HH:MM:SS] [Information] Pending migrations: 0
[2025-12-23 HH:MM:SS] [Information] Database is up to date
[2025-12-23 HH:MM:SS] [Information] Seeding admin user...
[2025-12-23 HH:MM:SS] [Information] Application started
```

#### 2. Health Check Verification
- [ ] Health endpoint returns 200 OK
- [ ] Database health check passes

**Command:**
```bash
curl https://orkinosai-cms.azurewebsites.net/api/health
```

**Expected Response:**
```
Status: 200 OK
Body: Healthy
```

#### 3. Admin Login Test
- [ ] Navigate to: https://orkinosai-cms.azurewebsites.net/admin/login
- [ ] Page loads without errors
- [ ] "Sign In" button (green allow button) is visible
- [ ] Enter credentials:
  - Username: `admin`
  - Password: `Admin@123`
- [ ] Click "Sign In" button
- [ ] Login succeeds (no HTTP 400/500 errors)
- [ ] Redirected to admin dashboard

#### 4. Database Table Verification
- [ ] Connect to Azure SQL Database via Azure Portal Query Editor
- [ ] Run verification query:

```sql
-- Verify Identity tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'AspNet%'
ORDER BY TABLE_NAME;

-- Expected results (7 tables):
-- AspNetRoleClaims
-- AspNetRoles
-- AspNetUserClaims
-- AspNetUserLogins
-- AspNetUserRoles
-- AspNetUsers
-- AspNetUserTokens

-- Verify admin user exists
SELECT Id, UserName, Email, EmailConfirmed 
FROM AspNetUsers 
WHERE UserName = 'admin';

-- Expected: 1 row with admin user
```

#### 5. Error Monitoring (First 24 Hours)
- [ ] Check Application Insights for exceptions
- [ ] Monitor for SQL connection errors
- [ ] Verify no connection pool exhaustion (HTTP 503)
- [ ] Check average response times are normal

**Azure Portal Path:**
Application Insights > Failures > Exceptions

## Troubleshooting Common Issues

### Issue: HTTP 500.30 on Startup
**Symptoms:**
- Application fails to start
- "Process failure" error in Azure logs

**Possible Causes:**
1. Connection string is still empty or invalid
2. Azure SQL firewall blocking App Service
3. Database credentials incorrect

**Resolution:**
```bash
# Check current connection string (from Azure Portal)
az webapp config connection-string list --name orkinosai-cms --resource-group orkinosai_group

# If empty, the appsettings.Production.json value should be used
# Verify firewall rules allow App Service IP
az sql server firewall-rule create \
  --server orkinosai \
  --resource-group orkinosai_group \
  --name AllowAppService \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Issue: "Invalid object name 'AspNetUsers'"
**Symptoms:**
- Login page loads but login fails
- SQL error 208 in logs

**Possible Causes:**
- Migrations not applied
- Database is empty

**Resolution:**
1. Check if AutoApplyMigrations is set to true
2. Restart the App Service to trigger migration
3. Manually apply migrations if needed:
```bash
# From local development machine
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web --connection "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=sqladmin;Password=Sarica-Ali-DedeI1974;..."
```

### Issue: Admin Login Fails with HTTP 400
**Symptoms:**
- Login page loads
- Credentials entered
- HTTP 400 error on submit

**Possible Causes:**
- Antiforgery token validation failure
- Stale browser cookies

**Resolution:**
1. Clear browser cookies for the site
2. Try incognito/private browsing mode
3. Check browser developer console for JavaScript errors

### Issue: HTTP 503 Service Unavailable
**Symptoms:**
- Intermittent 503 errors
- "Service Temporarily Unavailable"

**Possible Causes:**
- Connection pool exhaustion
- App Service plan resources exhausted

**Resolution:**
1. Connection pooling is automatically configured in Program.cs
2. Check App Service metrics in Azure Portal
3. Consider scaling up App Service plan if needed

## Success Criteria

All of the following must be true:
- ✅ Application starts without errors
- ✅ Health check endpoint returns 200 OK
- ✅ Admin login page loads
- ✅ Admin can log in successfully
- ✅ No database connection errors in logs
- ✅ All 7 AspNet* tables exist in database
- ✅ Admin user record exists in AspNetUsers table

## Rollback Plan (If Issues Occur)

If the deployment fails and cannot be recovered:

1. **Immediate Rollback to Previous Deployment:**
   ```bash
   # List previous deployment slots
   az webapp deployment slot list --name orkinosai-cms --resource-group orkinosai_group
   
   # Swap back to previous slot
   az webapp deployment slot swap --name orkinosai-cms --resource-group orkinosai_group --slot staging --target-slot production
   ```

2. **Alternative: Redeploy Previous Commit:**
   - Identify last known good commit
   - Deploy that commit via GitHub Actions
   - Monitor startup logs

3. **Emergency Connection String Override:**
   If the issue is connection string related:
   ```bash
   # Set connection string via Azure CLI
   az webapp config connection-string set \
     --name orkinosai-cms \
     --resource-group orkinosai_group \
     --connection-string-type SQLServer \
     --settings DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=sqladmin;Password=Sarica-Ali-DedeI1974;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
   ```

## Contact Information

For issues during deployment or verification:
- **GitHub Issue:** [Link to rollback issue]
- **Documentation:** ROLLBACK_TO_PR82_SUMMARY.md
- **Original PR #82:** https://github.com/orkinosai25-org/mosaic/pull/82
- **Original PR #81:** https://github.com/orkinosai25-org/mosaic/pull/81

---

**Verification Date:** ____________  
**Verified By:** ____________  
**Environment:** □ DEV  □ STAGING  □ PRODUCTION  
**Result:** □ PASSED  □ FAILED  
**Notes:** _______________________________________________________
