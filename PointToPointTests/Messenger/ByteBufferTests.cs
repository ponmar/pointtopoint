using PointToPoint.Messenger;

namespace PointToPointTests.Messenger;

[TestClass]
public class ByteBufferTests
{
    [TestMethod]
    public void Constructor()
    {
        var buffer = new ByteBuffer(3);
        Assert.IsFalse(buffer.Finished);
        Assert.AreEqual(0, buffer.offset);
        Assert.AreEqual(3, buffer.NumBytesLeft);
    }

    [TestMethod]
    public void SetTarget()
    {
        var buffer = new ByteBuffer(1);
        Assert.IsFalse(buffer.Finished);
        Assert.AreEqual(0, buffer.offset);
        Assert.AreEqual(1, buffer.NumBytesLeft);

        buffer.SetTarget(3);
        Assert.IsFalse(buffer.Finished);
        Assert.AreEqual(3, buffer.NumBytesLeft);
    }

    [TestMethod]
    public void Finished()
    {
        var buffer = new ByteBuffer(2);
        Assert.IsFalse(buffer.Finished);
        Assert.AreEqual(0, buffer.offset);
        Assert.AreEqual(2, buffer.NumBytesLeft);

        buffer.offset++;
        Assert.IsFalse(buffer.Finished);
        Assert.AreEqual(1, buffer.offset);
        Assert.AreEqual(1, buffer.NumBytesLeft);

        buffer.offset++;
        Assert.IsTrue(buffer.Finished);
        Assert.AreEqual(2, buffer.offset);
        Assert.AreEqual(0, buffer.NumBytesLeft);
    }
}
