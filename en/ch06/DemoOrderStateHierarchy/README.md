# DemoOrderStateHierarchy

Uses record inheritance to represent different order states in C# 14 /
.NET 10, without a preview SDK. Demonstrates the data required by draft,
submitted, and paid orders, and transition methods that accept an old state
and return a new one.

```powershell
dotnet run --project DemoOrderStateHierarchy.csproj
```

Verifies that transitions preserve order identity and timestamps, and shows
that the existing pattern match must reject an unsupported `ReturnedOrder`
at runtime. This is not a closed union, and adding a state does not produce
an exhaustiveness warning from the compiler.

Uncommenting `OrderWorkflow.Pay(draft, paidAt)` causes a compile error because
the method accepts only `SubmittedOrder`. Public constructors are a teaching
simplification; a real system must also design its business rules and
controlled creation methods.
