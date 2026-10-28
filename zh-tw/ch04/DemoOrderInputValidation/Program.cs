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
    // 明確執行輸入檢查，再依結果決定是否繼續。
    var validator = new CreateOrderRequestValidator();
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        return Results.ValidationProblem(result.ToDictionary());
    }

    // 此示範在這裡結束；實際流程會再呼叫 application service。
    return Results.Ok(new { message = "示範的輸入檢查已通過" });
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
