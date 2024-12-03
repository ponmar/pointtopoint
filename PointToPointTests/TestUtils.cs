namespace PointToPointTests;

internal class TestUtils
{
    public static TimeSpan WaitFor(Func<bool> condition, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(10);
        }

        var enteredAt = DateTime.Now;

        while (true)
        {
            var now = DateTime.Now;
            if (condition())
            {
                return now - enteredAt;
            }

            if (now - enteredAt > timeout)
            {
                Assert.Fail("Timeout");
            }

            Thread.Sleep(100);
        }
    }
}
