# OrkinosaiCMS Automated Test Suite

This directory contains comprehensive automated tests for the Mosaic Conversational CMS project. The test suite is organized into Unit Tests and Integration Tests, following .NET best practices using xUnit.

## 📁 Test Project Structure

```
tests/
├── OrkinosaiCMS.Tests.Unit/              # Unit tests for isolated component testing
│   ├── Services/                         # Service layer tests
│   │   ├── UserServiceTests.cs           # User management logic tests
│   │   └── AuthenticationServiceTests.cs # Authentication logic tests
│   ├── Repositories/                     # Repository pattern tests
│   │   └── RepositoryTests.cs            # Generic repository operations
│   ├── Middleware/                       # Middleware tests
│   │   └── GlobalExceptionHandlerMiddlewareTests.cs
│   └── Configuration/                    # Configuration tests
│       └── AuthenticationConfigurationTests.cs
│
└── OrkinosaiCMS.Tests.Integration/       # Integration tests for end-to-end scenarios
    ├── Fixtures/                         # Test fixtures and setup
    │   └── CustomWebApplicationFactory.cs
    ├── Database/                         # Database connectivity tests
    │   └── DatabaseConnectivityTests.cs
    └── Api/                             # API endpoint tests
        ├── HealthCheckTests.cs          # Health check and basic endpoints
        ├── AuthenticationTests.cs       # Authentication/Login tests
        └── CrudOperationsTests.cs       # CRUD operations for main entities
```

## 🎯 Test Coverage

### Unit Tests (`OrkinosaiCMS.Tests.Unit`)

#### 1. **UserService Tests**
- Password hashing and verification
- User creation with automatic defaults
- User retrieval by username and email
- User update and modification tracking
- Password change functionality
- Last login timestamp updates
- Active user filtering

#### 2. **AuthenticationService Tests**
- Login with valid credentials
- Login failure scenarios (wrong password, inactive user, non-existent user)
- Default role assignment for users without roles
- Logout functionality
- Exception handling during authentication

#### 3. **Repository Pattern Tests**
- Generic CRUD operations (Create, Read, Update, Delete)
- Soft delete functionality
- Query filtering (FindAsync, FirstOrDefaultAsync)
- Count and existence checks (CountAsync, AnyAsync)
- Batch operations (AddRangeAsync, UpdateRange, RemoveRange)

#### 4. **GlobalExceptionHandlerMiddleware Tests**
- Exception logging
- Nested exception handling
- Aggregate exception logging
- User context logging for authenticated requests
- Request details logging

#### 5. **Authentication Configuration Tests**
- Authentication scheme registration
- Default authentication scheme configuration
- Cookie authentication handler setup
- Service registration verification
- Missing authentication scheme handling

### Integration Tests (`OrkinosaiCMS.Tests.Integration`)

#### 1. **Database Connectivity Tests**
- Database connection verification
- Basic CRUD operations on Users
- Entity relationships (User-Role associations)
- Site and Page entity operations
- Soft delete query filter enforcement
- Automatic timestamp (CreatedOn, ModifiedOn) management

#### 2. **Health Check Tests**
- Root endpoint accessibility
- Test controller endpoints
- Non-existent endpoint handling (404 responses)
- Concurrent request handling

#### 3. **Authentication Tests**
- User credential verification
- Password validation
- User role retrieval
- Login/Logout flow
- New user creation
- Password change functionality
- Anonymous access to admin routes

#### 4. **CRUD Operations Tests**
- Site management (Create, Read, Update, Delete)
- Page management with site relationships
- User management
- Role management
- User-Role assignment and removal
- Multi-entity operations (GetAll, GetBySite, etc.)

## 🚀 Running the Tests

### Prerequisites

- .NET 10.0 SDK
- Visual Studio 2022 or later (optional)
- Command line or IDE test runner

### Run All Tests

```bash
# From the repository root
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Run Unit Tests Only

```bash
dotnet test tests/OrkinosaiCMS.Tests.Unit/OrkinosaiCMS.Tests.Unit.csproj
```

### Run Integration Tests Only

```bash
dotnet test tests/OrkinosaiCMS.Tests.Integration/OrkinosaiCMS.Tests.Integration.csproj
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~UserServiceTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "Name=CreateAsync_ShouldHashPasswordAndSetDefaults"
```

## 🧪 Test Frameworks and Libraries

- **xUnit** - Primary testing framework
- **FluentAssertions** - Readable assertion library
- **Moq** - Mocking framework for unit tests
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing** - WebApplicationFactory for integration tests

## 📝 Writing New Tests

### Unit Test Example

```csharp
[Fact]
public async Task MyMethod_WithValidInput_ShouldReturnExpectedResult()
{
    // Arrange
    var mockService = new Mock<IMyService>();
    mockService.Setup(x => x.GetData()).ReturnsAsync("test data");
    
    var sut = new MyClass(mockService.Object);
    
    // Act
    var result = await sut.ProcessData();
    
    // Assert
    result.Should().NotBeNull();
    result.Should().Contain("test");
}
```

### Integration Test Example

```csharp
[Fact]
public async Task ApiEndpoint_ShouldReturnSuccess()
{
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<IMyService>();
    
    // Act
    var result = await service.DoSomething();
    
    // Assert
    result.Should().BeTrue();
}
```

## 🔧 Best Practices

1. **Test Isolation**: Each test should be independent and not rely on other tests
2. **Descriptive Names**: Use clear, descriptive test method names that explain what is being tested
3. **AAA Pattern**: Structure tests using Arrange-Act-Assert pattern
4. **One Assertion per Test**: Focus on testing one thing per test method
5. **Use FluentAssertions**: Write readable assertions with clear error messages
6. **Mock External Dependencies**: Use Moq to mock dependencies in unit tests
7. **In-Memory Database**: Use in-memory database for fast, isolated integration tests
8. **Clean Up**: Integration tests use unique in-memory databases per test run

## 🐛 Troubleshooting

### Common Issues

**Issue**: Tests fail with database connection errors
**Solution**: Integration tests use in-memory databases, ensure EF Core InMemory provider is installed

**Issue**: Authentication tests fail
**Solution**: Check that test data seeding in `CustomWebApplicationFactory` is working correctly

**Issue**: Tests pass locally but fail in CI
**Solution**: Ensure all test dependencies are properly referenced in project files

## 📊 Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
  
- name: Publish Test Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: '**/test-results.trx'
```

## 🤝 Contributing

When adding new features to the CMS:

1. **Write tests first** (TDD approach recommended)
2. **Add unit tests** for business logic in `OrkinosaiCMS.Tests.Unit`
3. **Add integration tests** for API endpoints and database operations in `OrkinosaiCMS.Tests.Integration`
4. **Ensure all tests pass** before submitting pull requests
5. **Update this README** if adding new test categories or patterns

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [EF Core Testing](https://docs.microsoft.com/en-us/ef/core/testing/)

## 📈 Test Metrics

Current test coverage:
- **Unit Tests**: 50+ tests covering core business logic
- **Integration Tests**: 30+ tests covering API and database operations
- **Total Tests**: 80+ comprehensive tests

---

**Note**: This test suite is actively maintained and expanded as new features are added to the Mosaic CMS platform.
