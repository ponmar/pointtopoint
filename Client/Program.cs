using Client;
using PointToPoint.MessageRouting;
using PointToPoint.Network;
using PointToPoint.Payload;
using Protocol.Messages;

var serverHostname = "127.0.0.1";
var serverPort = 12345;

Console.WriteLine($"Conecting to {serverHostname}:{serverPort}...");

var messageHandler = new MessageHandler();

var client = new TcpMessenger(serverHostname,
    serverPort,
    new JsonPayload(),
    typeof(Hello).Namespace,
    new ReflectionBasedMessageRouter() { MessageHandler = messageHandler });

client.StartCommunication();
client.Send(new Hello(1));

while (client.IsHealthy())
{
}