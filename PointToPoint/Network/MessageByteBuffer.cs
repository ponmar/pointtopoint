using System;

namespace PointToPoint.Network
{
    public class MessageByteBuffer
    {
        public byte[] buffer = new byte[4];
        public int offset;

        public int MessageLengthBytesLeft => Math.Max(4 - offset, 0);
        public int MessageLength { get; private set; }

        public int MessageBytesLeft => Math.Max(MessageLength - offset + 4, 0);

        public MessageByteBuffer()
        {
            Reset();
        }

        public void Reset()
        {
            offset = 0;
            MessageLength = 0;
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
            if (buffer.Length < 4 + messageLength)
            {
                // Make message fit in buffer if needed
                buffer = new byte[4 + messageLength];
            }
        }
    }
}
