namespace PointToPoint.Payload
{
    public interface IPayloadSerializer
    {
        byte[] MessageToPayload(object message);
        object PayloadToMessage(byte[] payloadBytes, int length);
    }
}
