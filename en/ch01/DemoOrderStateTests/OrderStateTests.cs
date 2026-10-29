using Xunit;

namespace DemoOrderStateTests;

public class OrderStateTests
{
    [Fact]
    public void Order_Should_Track_Each_State_Transition()
    {
        // Arrange: create the initial test state.
        //   Later operations do not change the state of draft.
        //   fixedTime keeps time assertions stable and repeatable.
        var fixedTime = new DateTimeOffset(
            2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
        var draft = OrderWorkflow.Create("CUST-42", [], fixedTime);

        // Act + Assert: Submit returns a new order version.
        var submitted = OrderWorkflow.Submit(draft, fixedTime);
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Equal(fixedTime, submitted.SubmittedAt);

        // Pay takes submitted and returns another new order version.
        var paid = OrderWorkflow.Pay(submitted, fixedTime.AddHours(1));
        Assert.Equal(OrderStatus.Paid, paid.Status);
        Assert.Equal(fixedTime.AddHours(1), paid.PaidAt);

        // submitted keeps its snapshot; Pay does not change it.
        Assert.Equal(OrderStatus.Submitted, submitted.Status);
        Assert.Null(submitted.PaidAt);
    }
}
