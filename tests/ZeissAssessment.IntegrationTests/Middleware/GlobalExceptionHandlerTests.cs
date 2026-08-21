using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.Exceptions.Stock;
using ZeissAssessment.Middleware;

namespace ZeissAssessment.IntegrationTests.Middleware;

/// <summary>
/// Isolated, no-container tests: exercises GlobalExceptionHandler.TryHandleAsync directly
/// against a bare HttpContext. Lives in IntegrationTests (rather than UnitTests) purely
/// because GlobalExceptionHandler is defined in the Api project, which only IntegrationTests
/// references. This also covers the unhandled-exception/500 fallback branch, which no real
/// endpoint can trigger through the container-backed controller tests.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Test]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsWithNotFoundStatus_WhenNotFoundExceptionThrown()
    {
        // Arrange
        var exception = new NotFoundException(nameof(Product), 1);

        // Act
        var problem = await InvokeHandlerAsync(exception);

        // Assert
        problem.ShouldSatisfyAllConditions(
            () => problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status404NotFound),
            () => problem.GetProperty("errorCode").GetString().ShouldBe("NOT_FOUND"));
    }

    [Test]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsWithValidationErrorsExtension_WhenValidationExceptionThrown()
    {
        // Arrange
        var exception = new ValidationException(new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        });

        // Act
        var problem = await InvokeHandlerAsync(exception);

        // Assert
        problem.ShouldSatisfyAllConditions(
            () => problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status400BadRequest),
            () => problem.GetProperty("errorCode").GetString().ShouldBe("VALIDATION_FAILED"),
            () => problem.GetProperty("errors").GetProperty("Name")[0].GetString().ShouldBe("Name is required."));
    }

    [Test]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsWithBadRequestStatus_WhenDomainExceptionThrown()
    {
        // Arrange
        var exception = new InvalidStockQuantityException(-1);

        // Act
        var problem = await InvokeHandlerAsync(exception);

        // Assert
        problem.ShouldSatisfyAllConditions(
            () => problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status400BadRequest),
            () => problem.GetProperty("errorCode").GetString().ShouldBe("DOMAIN_RULE_VIOLATION"));
    }

    [Test]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsWithInternalServerError_WhenUnhandledExceptionThrown()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var problem = await InvokeHandlerAsync(exception);

        // Assert
        problem.ShouldSatisfyAllConditions(
            () => problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status500InternalServerError),
            () => problem.GetProperty("errorCode").GetString().ShouldBe("INTERNAL_ERROR"));
    }

    private async Task<JsonElement> InvokeHandlerAsync(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);
        return document.RootElement.Clone();
    }
}
