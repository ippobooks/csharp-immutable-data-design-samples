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
    throw new InvalidOperationException("轉換沒有保留預期資料。");
}

Console.WriteLine(OrderQueries.Describe(draft));
Console.WriteLine(OrderQueries.Describe(submitted));
Console.WriteLine(OrderQueries.Describe(paid));

// 取消註解後會編譯失敗：付款只接受 SubmittedOrder。
// OrderWorkflow.Pay(draft, paidAt);

// 一般繼承階層仍允許新狀態；既有 switch 不會因此編譯失敗。
Order returned = new ReturnedOrder("ORD-002");
try
{
    OrderQueries.Describe(returned);
    throw new Exception("未預期的狀態沒有被拒絕。");
}
catch (InvalidOperationException)
{
    Console.WriteLine("未支援的狀態由執行期分支拒絕。");
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
        DraftOrder => "草稿",
        SubmittedOrder => "已提交",
        PaidOrder => "已付款",
        _ => throw new InvalidOperationException("未支援的訂單狀態")
    };
}
