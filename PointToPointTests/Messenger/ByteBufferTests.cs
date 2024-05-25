using PointToPoint.Messenger;

namespace PointToPointTests.Messenger;

public class ByteBufferTests
{
    [Fact]
    public void Constructor()
    {
        var buffer = new ByteBuffer(3);
        Assert.False(buffer.Finished);
        Assert.Equal(0, buffer.offset);
        Assert.Equal(3, buffer.NumBytesLeft);
    }

    [Fact]
    public void SetTarget()
    {
        var buffer = new ByteBuffer(1);
        Assert.False(buffer.Finished);
        Assert.Equal(0, buffer.offset);
        Assert.Equal(1, buffer.NumBytesLeft);

        buffer.SetTarget(3);
        Assert.False(buffer.Finished);
        Assert.Equal(3, buffer.NumBytesLeft);
    }

    [Fact]
    public void Finished()
    {
        var buffer = new ByteBuffer(2);
        Assert.False(buffer.Finished);
        Assert.Equal(0, buffer.offset);
        Assert.Equal(2, buffer.NumBytesLeft);

        buffer.offset++;
        Assert.False(buffer.Finished);
        Assert.Equal(1, buffer.offset);
        Assert.Equal(1, buffer.NumBytesLeft);

        buffer.offset++;
        Assert.True(buffer.Finished);
        Assert.Equal(2, buffer.offset);
        Assert.Equal(0, buffer.NumBytesLeft);
    }
}
