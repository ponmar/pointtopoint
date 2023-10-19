namespace PointToPoint.Messenger
{
    public class ByteBuffer
    {
        public byte[] buffer;
        public int offset;
        public int numBytesToRead;

        public int NumBytesLeft => numBytesToRead - offset;
        public bool Finished => offset == numBytesToRead;

        public ByteBuffer(int target)
        {
            buffer = new byte[0];
            SetTarget(target);
        }

        public void SetTarget(int numBytesToRead)
        {
            offset = 0;
            this.numBytesToRead = numBytesToRead;
            if (buffer.Length < numBytesToRead)
            {
                // Make message fit in buffer if needed
                buffer = new byte[numBytesToRead];
            }
        }
    }
}
