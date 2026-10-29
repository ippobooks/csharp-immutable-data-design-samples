using Xunit;

namespace DemoOrderStateTests;

public class OrderStateTests
{
    [Fact]
    public void Order_Should_Track_Each_State_Transition()
    {
        // Arrange: テストの初期状態を作る。
        //   後続の操作で draft の状態は変わらない。
        //   fixedTime で時刻のアサーションを安定させ、再現可能にする。
        var fixedTime = new DateTimeOffset(
            2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
        var draft = OrderWorkflow.Create("CUST-42", [], fixedTime);

        // Act + Assert: Submit は注文の新しいバージョンを返す。
        var submitted = OrderWorkflow.Submit(draft, fixedTime);
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Equal(fixedTime, submitted.SubmittedAt);

        // Pay は submitted を受け取り、別の新しいバージョンを返す。
        var paid = OrderWorkflow.Pay(submitted, fixedTime.AddHours(1));
        Assert.Equal(OrderStatus.Paid, paid.Status);
        Assert.Equal(fixedTime.AddHours(1), paid.PaidAt);

        // submitted の確定後のスナップショットは Pay で変更されない。
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Null(submitted.PaidAt);
    }
}
