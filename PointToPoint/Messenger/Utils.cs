using System;

namespace PointToPoint.Messenger
{
    public static class Utils
    {
        public static byte[] SerializeInt(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static int DeserializeInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
