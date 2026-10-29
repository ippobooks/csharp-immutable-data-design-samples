# DemoOrderInputValidation

Uses C# 14, .NET 10, and FluentValidation 12.2.0 to demonstrate defining
input rules in a separate validator and explicitly calling `ValidateAsync`
in a Minimal API. DTOs carry data only, without data annotations or
validation methods.

```powershell
dotnet run --project DemoOrderInputValidation.csproj
```

Send `POST /orders/validate` to the listening address shown by the app,
using a body such as:

```json
{
  "customerId": "CUST-42",
  "lines": [{ "productId": "P001", "quantity": 2 }]
}
```

Returns 200 when the customer ID is nonblank and no longer than 50
characters, and the lines collection is nonempty. Otherwise, returns a
400 validation problem response containing field errors.

This example checks only the customer ID and the lines collection. It does
not validate each product ID or quantity, or create or save orders. A real
system needs complete input validation, an application service to coordinate
later steps, and a domain model to protect business rules.
