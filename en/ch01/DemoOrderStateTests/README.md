# Unit tests for order state transitions

The xUnit example from Section 1.4 verifies the new versions returned by
submission and payment, and confirms that payment does not modify the
original submitted order.

References `DemoImmutableOrder` and tests its `OrderWorkflow` and order types
directly.

Requires the .NET 10 SDK. Run from this directory:

```shell
dotnet test
```

Or run from the Chapter 1 directory:

```shell
dotnet test ch01.slnx
```
