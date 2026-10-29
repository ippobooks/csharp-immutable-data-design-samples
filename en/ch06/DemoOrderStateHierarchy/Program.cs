using System;

var submittedAt = new DateTimeOffset(
    2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
var paidAt = submittedAt.AddHours(1);

var draft = new DraftOrder("ORD-001");
var submitted = OrderWorkflow.Submit(draft, submittedAt);
var paid = OrderWorkflow.Pay(submitted, paidAt);

if (paid.Id != draft.Id || paid.SubmittedAt != submittedAt ||
    paid.PaidAt != paidAt || submitted.SubmittedAt != submittedAt)
{
    throw new InvalidOperationException(
        "The transition did not preserve the expected data.");
}

Console.WriteLine(OrderQueries.Describe(draft));
Console.WriteLine(OrderQueries.Describe(submitted));
Console.WriteLine(OrderQueries.Describe(paid));

// Uncommenting fails to compile: Pay only accepts SubmittedOrder.
// OrderWorkflow.Pay(draft, paidAt);

// Ordinary hierarchies allow new states; this switch still compiles.
Order returned = new ReturnedOrder("ORD-002");
try
{
    OrderQueries.Describe(returned);
    throw new Exception("The unexpected state was not rejected.");
}
catch (InvalidOperationException)
{
    Console.WriteLine(
        "The unsupported state was rejected by a runtime branch.");
}

public abstract record Order(string Id);

public sealed record DraftOrder(string Id) : Order(Id);

public sealed record SubmittedOrder(
    string Id,
    DateTimeOffset SubmittedAt) : Order(Id);

public sealed record PaidOrder(
    string Id,
    DateTimeOffset SubmittedAt,
    DateTimeOffset PaidAt) : Order(Id);

public sealed record ReturnedOrder(string Id) : Order(Id);

public static class OrderWorkflow
{
    public static SubmittedOrder Submit(
        DraftOrder draft,
        DateTimeOffset submittedAt) =>
        new(draft.Id, submittedAt);

    public static PaidOrder Pay(
        SubmittedOrder submitted,
        DateTimeOffset paidAt) =>
        new(submitted.Id, submitted.SubmittedAt, paidAt);
}

public static class OrderQueries
{
    public static string Describe(Order order) => order switch
    {
        DraftOrder => "Draft",
        SubmittedOrder => "Submitted",
        PaidOrder => "Paid",
        _ => throw new InvalidOperationException(
            "Unsupported order state")
    };
}
