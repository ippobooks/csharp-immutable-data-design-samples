using Xunit;

namespace DemoOrderStateTests;

public class OrderStateTests
{
    [Fact]
    public void Order_Should_Track_Each_State_Transition()
    {
        // Arrange: 建立測試起點。
        //   draft 不會被後來的任何操作改變狀態。
        //   fixedTime 讓時間相關斷言保持穩定、可重複。
        var fixedTime = new DateTimeOffset(
            2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
        var draft = OrderWorkflow.Create("CUST-42", [], fixedTime);

        // Act + Assert: Submit 回傳新的訂單版本。
        var submitted = OrderWorkflow.Submit(draft, fixedTime);
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Equal(fixedTime, submitted.SubmittedAt);

        // Pay 接收 submitted，並回傳另一個新的訂單版本。
        var paid = OrderWorkflow.Pay(submitted, fixedTime.AddHours(1));
        Assert.Equal(OrderStatus.Paid, paid.Status);
        Assert.Equal(fixedTime.AddHours(1), paid.PaidAt);

        // submitted 仍然保留送出後的快照，不會被 Pay 的結果連動修改。
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Null(submitted.PaidAt);
    }
}
