namespace PointToPoint.Payload
{
    public interface IPayloadSerializer
    {
        byte[] MessageToPayload(object message);
        object PayloadToMessage(byte[] payloadBytes, int offset, int length);
    }
}
