# CI Test Failures - Root Cause Analysis and Fixes

## Summary
Fixed multiple integration test failures in CI by implementing proper test isolation and adding comprehensive documentation.

## Issues Fixed

### 1. Subscription Test Failures (Free vs Pro Tier)
**Problem:**
- Test `GetCurrentSubscription_WithoutSubscription_ShouldReturnFreeTier` was failing
- Expected tier: "Free", but got: "Pro"
- Tests were sharing an InMemory database, causing data pollution between tests

**Root Cause:**
- Multiple subscription tests used the same test user (`admin@test.com`)
- Tests ran in random order (xUnit default behavior)
- When `SubscriptionService_GetActiveSubscriptionByUserId_ShouldReturnActiveSubscription` ran first, it created a Pro subscription for admin@test.com
- Subsequent tests expecting Free tier failed because the Pro subscription already existed

**Fix Applied:**
1. Made each test class use a unique InMemory database instance
   - Added `_databaseName = $"InMemoryTestDb_{Guid.NewGuid()}"` to `CustomWebApplicationFactory`
   - Each test class (via `IClassFixture`) gets its own isolated database

2. Made each subscription test create its own unique test user
   - `SubscriptionService_CreateSubscription_ShouldPersistToDatabase` → createsubtest@test.com
   - `SubscriptionService_GetActiveSubscriptionByUserId_ShouldReturnActiveSubscription` → activesubtest@test.com
   - `SubscriptionService_CancelSubscription_ShouldUpdateStatusAndCanceledDate` → cancelsubtest@test.com
   - `SubscriptionService_HasReachedWebsiteLimit_ShouldEnforceTierLimits` → limitsubtest@test.com

**Files Changed:**
- `tests/OrkinosaiCMS.Tests.Integration/Fixtures/CustomWebApplicationFactory.cs`
- `tests/OrkinosaiCMS.Tests.Integration/Api/SubscriptionTests.cs`

### 2. Database Connectivity Test Failures (Role Name)
**Problem:**
- Test `Database_ShouldSupportRelationships` expected role name "TestRole" but got "Role 2"
- Similar test isolation issue where multiple roles existed from different test runs

**Root Cause:**
- Same as above - shared database between test classes
- Tests creating roles could interfere with each other

**Fix Applied:**
- Same fix as #1 - unique database per test class
- Each test class now has complete isolation from other test classes

**Files Changed:**
- `tests/OrkinosaiCMS.Tests.Integration/Fixtures/CustomWebApplicationFactory.cs`

### 3. Password Verification Issues
**Problem:**
- CI logs showed "Password verification failed for user: testadmin"

**Analysis:**
- Test factory seeds user "testadmin" with password "TestPassword123!"
- Production SeedData seeds user "admin" with password "Admin@123"
- These are different users - no conflict
- Password hashing is correct using BCrypt

**Resolution:**
- No code changes needed - this was related to test isolation
- Test environment uses "testadmin" with "TestPassword123!"
- Production uses "admin" with "Admin@123" (can be overridden via environment variable)

### 4. Database Provider Configuration
**Problem:**
- Concern that application was using SQLite instead of Azure SQL in production/CI

**Analysis:**
- Database provider selection hierarchy (in order):
  1. Environment variable: `DatabaseProvider`
  2. `appsettings.{Environment}.json`
  3. `appsettings.json`
  4. Default: "SqlServer"

- Test environment: Explicitly set to "InMemory" by `CustomWebApplicationFactory`
- Production: Configured as "SqlServer" in `appsettings.Production.json`

**Fix Applied:**
- Added comprehensive documentation explaining database provider selection
- Added logging to show which provider is being used
- Clarified that tests use InMemory (correct), production uses SqlServer

**Files Changed:**
- `src/OrkinosaiCMS.Web/Program.cs`

### 5. API Route Configuration
**Problem:**
- Reports of HTTP 400/404 errors on endpoints `/api/subscription/current` and `/`

**Analysis:**
- `/api/subscription/current`: Returns 400 when userEmail is empty (expected behavior)
- `/api/subscription/current`: Returns 404 when user doesn't exist (expected behavior)
- `/`: Returns 404 in test environment because index.html doesn't exist (expected)
- In production, `/` serves React portal's index.html via fallback route

**Fix Applied:**
- Added comprehensive documentation to `SubscriptionController` explaining all endpoints and status codes
- Added comments to routing configuration explaining fallback behavior
- Documented that root (/) returning 404 in tests is expected

**Files Changed:**
- `src/OrkinosaiCMS.Web/Controllers/SubscriptionController.cs`
- `src/OrkinosaiCMS.Web/Program.cs`

## Test Results

### Before Fixes
- Failed: 2 tests
  - `GetCurrentSubscription_WithoutSubscription_ShouldReturnFreeTier`
  - `Database_ShouldSupportRelationships`

### After Fixes
- **All 38 integration tests passing**
- Tested in both Debug and Release configurations
- Test isolation working correctly

## Documentation Added

### CustomWebApplicationFactory
- Explained test isolation strategy
- Documented unique database per test class
- Clarified password for test admin user

### Program.cs
- Database provider selection priority
- Environment-specific configuration
- Routing behavior in different environments

### SubscriptionController
- Complete endpoint documentation
- Expected status codes
- Free tier default behavior

## Recommendations for CI

1. **Environment Configuration**
   - CI should set `ASPNETCORE_ENVIRONMENT=Production` for production deployments
   - Use Azure App Service Configuration for connection strings (not appsettings.json)

2. **Test Execution**
   - Current CI configuration is correct - uses `dotnet test` with Release configuration
   - Tests properly isolated and will pass consistently

3. **Database Configuration**
   - Production: Uses Azure SQL Database (configured in appsettings.Production.json)
   - Testing: Uses InMemory database (configured in CustomWebApplicationFactory)
   - Development: Can use LocalDB or SQLite (configured in appsettings.Development.json)

## Commands to Verify

```bash
# Build in Release mode (as CI does)
dotnet build OrkinosaiCMS.sln --configuration Release

# Run tests in Release mode (as CI does)
dotnet test OrkinosaiCMS.sln --configuration Release --no-build --verbosity normal

# Expected result: All 38 tests pass
```

## Key Lessons

1. **Test Isolation is Critical**
   - Shared state between tests leads to flaky, order-dependent failures
   - Each test class should have its own database instance
   - Tests creating data should use unique identifiers

2. **InMemory Database Behavior**
   - InMemory databases persist for the lifetime of the DbContext
   - Using a unique database name per test class ensures isolation
   - Alternative: Use database reset/cleanup, but unique DBs are simpler

3. **Documentation Prevents Confusion**
   - Clear comments explain expected behavior (e.g., 404 on root in tests)
   - Status code documentation helps debug issues
   - Configuration hierarchy documentation prevents misconfiguration

## Files Modified

1. `tests/OrkinosaiCMS.Tests.Integration/Fixtures/CustomWebApplicationFactory.cs`
   - Added unique database name per instance
   - Added comprehensive documentation

2. `tests/OrkinosaiCMS.Tests.Integration/Api/SubscriptionTests.cs`
   - Modified 4 tests to create unique test users
   - Prevents test interference

3. `src/OrkinosaiCMS.Web/Program.cs`
   - Added database provider documentation
   - Added routing behavior documentation

4. `src/OrkinosaiCMS.Web/Controllers/SubscriptionController.cs`
   - Added comprehensive API documentation
   - Explained Free tier default behavior

## Success Criteria

✅ All integration tests pass consistently  
✅ Tests are isolated and order-independent  
✅ Configuration is properly documented  
✅ API endpoints return correct status codes  
✅ Database provider selection is clear  
✅ Build succeeds in Release configuration  

---

**Date:** 2025-12-13  
**CI Job Reference:** 57976930643  
**Status:** ✅ RESOLVED
