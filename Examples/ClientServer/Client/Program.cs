using Client;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Payload;
using Protocol;

var serverHostname = "127.0.0.1";
var serverPort = 12345;

Console.WriteLine($"Conecting to {serverHostname}:{serverPort}...");

var messageHandler = new MessageHandler();

var client = new TcpMessenger(serverHostname,
    serverPort,
    new NewtonsoftJsonPayload(),
    typeof(Hello).Namespace,
    new ReflectionMessageRouter() { MessageHandler = messageHandler },
    messageHandler);

client.Start();
client.Send(new Hello(1));

while (true)
{
}