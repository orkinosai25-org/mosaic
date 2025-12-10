# MOSAIC Deployment Test Plan

## Overview
This document provides a comprehensive test plan to verify the deployment routing fix works correctly after deployment to Azure.

## Test Environment
- **Azure Web App**: `mosaic-saas.azurewebsites.net`
- **Frontend**: React + Fluent UI portal
- **Backend**: .NET 10 + Blazor CMS
- **Authentication**: Custom authentication with redirect

## Pre-Deployment Checklist

- [ ] Frontend build completes successfully (`npm run build`)
- [ ] Backend build completes successfully (`dotnet build`)
- [ ] React portal assets copied to `wwwroot/`
- [ ] `index.html` exists in `wwwroot/`
- [ ] No build errors or warnings
- [ ] CodeQL security scan passes
- [ ] Code review comments addressed

## Test Scenarios

### 1. Portal Landing Page (Unauthenticated)

**Test Case 1.1: Root URL serves React portal**
- **URL**: `https://mosaic-saas.azurewebsites.net/`
- **Expected**: 
  - React portal landing page loads
  - MOSAIC branding visible
  - Ottoman-inspired design present
  - "Get Started" and "Sign In" buttons visible
  - No Blazor components shown
- **How to verify**:
  ```bash
  curl -I https://mosaic-saas.azurewebsites.net/
  # Should return 200 OK with Content-Type: text/html
  
  curl https://mosaic-saas.azurewebsites.net/ | grep "MOSAIC"
  # Should find MOSAIC branding in HTML
  ```

**Test Case 1.2: React routing works**
- **URLs**: 
  - `https://mosaic-saas.azurewebsites.net/dashboard`
  - `https://mosaic-saas.azurewebsites.net/sites`
- **Expected**: 
  - React portal serves the page
  - React router handles navigation
  - No 404 errors
- **How to verify**: Visit URLs in browser, check console for errors

**Test Case 1.3: Portal handles authentication internally**
- **Action**: Click "Sign In" in portal
- **Expected**: 
  - Portal shows authentication UI
  - No redirect to Blazor login
  - Can authenticate through portal

### 2. CMS Access Protection (Unauthenticated)

**Test Case 2.1: Admin route requires auth**
- **URL**: `https://mosaic-saas.azurewebsites.net/admin`
- **Expected**: 
  - Redirects to `/admin/login?returnUrl=%2Fadmin`
  - Shows MOSAIC CMS Admin login page
  - Ottoman blue color scheme (#1e3a8a, #2563eb)
- **How to verify**:
  ```bash
  curl -I https://mosaic-saas.azurewebsites.net/admin
  # Should return 302 Found with Location header
  ```

**Test Case 2.2: CMS pages require auth**
- **URLs**:
  - `https://mosaic-saas.azurewebsites.net/cms-home`
  - `https://mosaic-saas.azurewebsites.net/cms-about`
  - `https://mosaic-saas.azurewebsites.net/cms-features`
- **Expected**: 
  - Each redirects to `/admin/login?returnUrl=...`
  - Return URL preserved in query string
- **How to verify**: Visit in private/incognito browser window

**Test Case 2.3: Demo pages removed**
- **URLs**:
  - `https://mosaic-saas.azurewebsites.net/counter`
  - `https://mosaic-saas.azurewebsites.net/weather`
- **Expected**: 
  - React portal serves (fallback routing)
  - Portal handles as unknown route
  - No "Counter" or "Weather" Blazor pages shown

### 3. Authentication Flow

**Test Case 3.1: Login page accessible**
- **URL**: `https://mosaic-saas.azurewebsites.net/admin/login`
- **Expected**: 
  - Login form visible
  - "MOSAIC CMS Admin" title
  - Ottoman-inspired gradient background
  - Username and password fields
  - "Sign In" button
  - Demo credentials shown: "admin / Admin@123"

**Test Case 3.2: Login with valid credentials**
- **Action**: 
  1. Visit `/admin/login`
  2. Enter username: `admin`
  3. Enter password: `Admin@123`
  4. Click "Sign In"
- **Expected**: 
  - Successful authentication
  - Redirects to `/admin` dashboard
  - Can now access CMS pages

**Test Case 3.3: Login with return URL**
- **Action**: 
  1. Visit `/cms-home` (unauthenticated)
  2. Gets redirected to `/admin/login?returnUrl=%2Fcms-home`
  3. Enter valid credentials
  4. Click "Sign In"
- **Expected**: 
  - After login, redirects back to `/cms-home`
  - Shows CMS home page content

**Test Case 3.4: Login with invalid credentials**
- **Action**: Enter wrong username/password
- **Expected**: 
  - Error message: "Invalid username or password"
  - Stays on login page
  - Can retry

### 4. CMS Access (Authenticated)

**Test Case 4.1: Admin dashboard accessible**
- **URL**: `https://mosaic-saas.azurewebsites.net/admin`
- **Expected**: 
  - After login, admin interface loads
  - Can see admin features
  - MOSAIC CMS navigation visible

**Test Case 4.2: CMS pages accessible**
- **URLs**:
  - `/cms-home`
  - `/cms-about`
  - `/cms-features`
  - `/cms-contact`
- **Expected**: 
  - Each page loads successfully
  - Content is visible
  - Navigation works

**Test Case 4.3: Navigation menu updated**
- **Location**: CMS sidebar
- **Expected**: 
  - Brand shows "MOSAIC CMS" (not "OrkinosaiCMS.Web")
  - Links to: Home, About, Features, Admin
  - No Counter or Weather links

### 5. Static Assets and Framework

**Test Case 5.1: Blazor framework loads**
- **URL**: `https://mosaic-saas.azurewebsites.net/_framework/blazor.web.js`
- **Expected**: 
  - JavaScript file loads successfully
  - 200 OK status

**Test Case 5.2: React assets load**
- **URLs**:
  - `/assets/index-*.js`
  - `/assets/index-*.css`
  - `/mosaic-icon.svg`
- **Expected**: 
  - All assets load with 200 OK
  - Correct MIME types
  - Assets cached appropriately

**Test Case 5.3: Static files served**
- **URLs**:
  - `/favicon.png`
  - `/mosaic-logo-main.svg`
- **Expected**: 
  - Files accessible
  - Correct content type

### 6. API Endpoints

**Test Case 6.1: API controllers accessible**
- **URL**: `https://mosaic-saas.azurewebsites.net/api/*`
- **Expected**: 
  - API endpoints respond
  - Proper HTTP status codes
  - JSON responses (if applicable)

### 7. Error Handling

**Test Case 7.1: 404 handling**
- **URL**: `https://mosaic-saas.azurewebsites.net/nonexistent-page`
- **Expected**: 
  - React portal serves (fallback)
  - Portal handles 404 internally
  - User-friendly error message (if implemented)

**Test Case 7.2: Error page**
- **URL**: `https://mosaic-saas.azurewebsites.net/error`
- **Expected**: 
  - Blazor error page loads
  - Appropriate error message

### 8. Performance and Caching

**Test Case 8.1: Index.html caching**
- **URL**: `https://mosaic-saas.azurewebsites.net/`
- **Expected headers**:
  ```
  Cache-Control: no-cache, no-store, must-revalidate
  Expires: 0
  ```
- **How to verify**:
  ```bash
  curl -I https://mosaic-saas.azurewebsites.net/
  # Check response headers
  ```

**Test Case 8.2: Asset caching**
- **URL**: `https://mosaic-saas.azurewebsites.net/assets/index-*.js`
- **Expected**: 
  - Appropriate caching headers
  - Efficient delivery

### 9. Security

**Test Case 9.1: HTTPS enforced**
- **URL**: `http://mosaic-saas.azurewebsites.net/` (HTTP)
- **Expected**: 
  - Redirects to HTTPS
  - 301 or 302 status

**Test Case 9.2: Authentication required**
- **Test**: Try accessing protected routes without auth
- **Expected**: 
  - All `/cms-*` and `/admin/*` routes protected
  - Redirect to login
  - Cannot bypass authentication

**Test Case 9.3: No SQL injection**
- **Test**: Try SQL injection in login form
- **Expected**: 
  - Input sanitized
  - No database errors
  - Login fails safely

### 10. Browser Compatibility

**Test Case 10.1: Chrome/Edge**
- **Action**: Test all scenarios in Chrome/Edge
- **Expected**: Full functionality

**Test Case 10.2: Firefox**
- **Action**: Test all scenarios in Firefox
- **Expected**: Full functionality

**Test Case 10.3: Safari**
- **Action**: Test all scenarios in Safari
- **Expected**: Full functionality

**Test Case 10.4: Mobile browsers**
- **Action**: Test on mobile devices
- **Expected**: Responsive design works

## Automated Testing Commands

```bash
# Test root URL
curl -I https://mosaic-saas.azurewebsites.net/

# Test admin redirect
curl -I https://mosaic-saas.azurewebsites.net/admin

# Test CMS page redirect
curl -I https://mosaic-saas.azurewebsites.net/cms-home

# Test login page
curl -I https://mosaic-saas.azurewebsites.net/admin/login

# Check for MOSAIC branding
curl -s https://mosaic-saas.azurewebsites.net/ | grep -i "mosaic"

# Check React assets
curl -I https://mosaic-saas.azurewebsites.net/assets/

# Test removed demo pages (should serve React portal)
curl -I https://mosaic-saas.azurewebsites.net/counter
```

## Post-Deployment Verification

After deployment completes, verify:

1. ✅ Root URL shows MOSAIC portal landing page
2. ✅ No Blazor demo pages visible at root
3. ✅ CMS routes require authentication
4. ✅ Login page accessible and branded correctly
5. ✅ Successful login grants CMS access
6. ✅ Return URLs work after login
7. ✅ Navigation shows MOSAIC branding
8. ✅ Static assets load correctly
9. ✅ No console errors in browser
10. ✅ Mobile responsive design works

## Issue Resolution

### Issue: Portal not loading at root
**Symptom**: Blazor or 404 at root URL
**Check**:
- Verify `index.html` in `wwwroot/`
- Check deployment artifacts
- Verify fallback routing configured

**Fix**: Re-run deployment with frontend build

### Issue: CMS accessible without auth
**Symptom**: Can access `/admin` or `/cms-*` without login
**Check**:
- Verify authentication middleware added
- Check middleware order
- Verify auth state provider

**Fix**: Redeploy with authentication fix

### Issue: Login redirect loop
**Symptom**: Keeps redirecting to login
**Check**:
- Verify authentication logic
- Check cookie settings
- Verify auth state persistence

**Fix**: Check authentication service implementation

## Success Criteria

✅ All test cases pass
✅ No console errors
✅ Authentication works correctly
✅ Portal loads at root
✅ CMS protected behind authentication
✅ Branding updated throughout
✅ No security vulnerabilities
✅ Performance acceptable

## Rollback Plan

If deployment fails:
1. Revert to previous commit
2. Redeploy known-good version
3. Investigate issues
4. Fix and redeploy

## Sign-off

- [ ] Development team verified
- [ ] QA team verified
- [ ] Security team verified
- [ ] Product owner approved
- [ ] Ready for production

---

**Last Updated:** December 2024  
**Test Engineer:** Orkinosai Team  
**Status:** Ready for Testing
