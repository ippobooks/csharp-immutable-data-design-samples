using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/orders/validate", async Task<IResult> (
    CreateOrderRequest request,
    CancellationToken ct) =>
{
    // Validate input explicitly, then decide whether to continue.
    var validator = new CreateOrderRequestValidator();
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        return Results.ValidationProblem(result.ToDictionary());
    }

    // This demo ends here; a real workflow calls the application service.
    return Results.Ok(new
    {
        message = "The demonstrated input checks passed"
    });
});

app.Run();

public sealed record CreateOrderRequest(
    string CustomerId,
    IReadOnlyList<CreateOrderLineRequest> Lines);

public sealed record CreateOrderLineRequest(
    string ProductId,
    int Quantity);

public sealed class CreateOrderRequestValidator
    : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Lines).NotEmpty();
    }
}
