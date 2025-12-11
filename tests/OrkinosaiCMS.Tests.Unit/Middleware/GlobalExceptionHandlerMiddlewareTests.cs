using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OrkinosaiCMS.Web.Middleware;

namespace OrkinosaiCMS.Tests.Unit.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandlerMiddleware
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly GlobalExceptionHandlerMiddleware _middleware;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _environmentMock = new Mock<IHostEnvironment>();
        
        var nextMock = new Mock<RequestDelegate>();
        _middleware = new GlobalExceptionHandlerMiddleware(
            nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object
        );
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(
            next,
            _loggerMock.Object,
            _environmentMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test-path";
        context.Request.Method = "GET";
        context.TraceIdentifier = "test-trace-id";

        var expectedException = new InvalidOperationException("Test exception");
        RequestDelegate next = (HttpContext ctx) => throw expectedException;

        var middleware = new GlobalExceptionHandlerMiddleware(
            next,
            _loggerMock.Object,
            _environmentMock.Object
        );

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("/test-path")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithAggregateException_ShouldLogAllInnerExceptions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test-path";
        context.Request.Method = "POST";

        var innerException1 = new ArgumentException("First inner exception");
        var innerException2 = new InvalidOperationException("Second inner exception");
        var aggregateException = new AggregateException("Aggregate error", innerException1, innerException2);

        RequestDelegate next = (HttpContext ctx) => throw aggregateException;

        var middleware = new GlobalExceptionHandlerMiddleware(
            next,
            _loggerMock.Object,
            _environmentMock.Object
        );

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();

        // Verify aggregate exception details were logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AggregateException")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithNestedException_ShouldLogInnerExceptions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test-path";

        var innerException = new ArgumentException("Inner exception");
        var outerException = new InvalidOperationException("Outer exception", innerException);

        RequestDelegate next = (HttpContext ctx) => throw outerException;

        var middleware = new GlobalExceptionHandlerMiddleware(
            next,
            _loggerMock.Object,
            _environmentMock.Object
        );

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify inner exception was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inner Exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUser_ShouldLogUserContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test-path";
        
        // Set up authenticated user
        var identity = new System.Security.Claims.ClaimsIdentity(
            new[] { new System.Security.Claims.Claim("name", "testuser") },
            "TestAuthType"
        );
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);

        var expectedException = new Exception("Test exception");
        RequestDelegate next = (HttpContext ctx) => throw expectedException;

        var middleware = new GlobalExceptionHandlerMiddleware(
            next,
            _loggerMock.Object,
            _environmentMock.Object
        );

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        // Verify user context was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User Context")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
