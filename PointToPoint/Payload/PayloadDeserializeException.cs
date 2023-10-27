using System;

namespace PointToPoint.Payload
{
    public class PayloadDeserializeException : Exception
    {
        public PayloadDeserializeException(string message) : base(message) { }
    }
}
