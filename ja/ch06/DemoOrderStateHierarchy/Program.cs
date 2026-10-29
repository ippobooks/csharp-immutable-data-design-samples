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
        "状態遷移で期待したデータが保持されませんでした。");
}

Console.WriteLine(OrderQueries.Describe(draft));
Console.WriteLine(OrderQueries.Describe(submitted));
Console.WriteLine(OrderQueries.Describe(paid));

// コメント解除でコンパイルエラー: Pay は SubmittedOrder のみ受付。
// OrderWorkflow.Pay(draft, paidAt);

// 通常の継承階層は新状態を許し、既存の switch はコンパイルに成功する。
Order returned = new ReturnedOrder("ORD-002");
try
{
    OrderQueries.Describe(returned);
    throw new Exception("予期しない状態が拒否されませんでした。");
}
catch (InvalidOperationException)
{
    Console.WriteLine("未対応の状態は実行時の分岐で拒否されました。");
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
        DraftOrder => "下書き",
        SubmittedOrder => "確定済み",
        PaidOrder => "支払い済み",
        _ => throw new InvalidOperationException("未対応の注文状態")
    };
}
