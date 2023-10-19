using System;

namespace PointToPoint.Network.Messenger
{
    public static class Utils
    {
        public static byte[] SerializeInt(int value)
        {
            /*
            var bytes = new byte[4];
            var span = new Span<byte>(bytes);
            BinaryPrimitives.WriteInt32BigEndian(span, value);
            return bytes;
            */

            return BitConverter.GetBytes(value);
        }

        public static int DeserializeInt(byte[] bytes, int offset)
        {
            /*
            var span = new Span<byte>(bytes, offset, 4);
            return BinaryPrimitives.ReadInt32BigEndian(span);
            */
            return BitConverter.ToInt32(bytes, offset);
        }
    }
}
