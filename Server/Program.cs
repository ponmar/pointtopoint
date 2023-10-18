using PointToPoint.MessageRouting;
using PointToPoint.Network;
using PointToPoint.Payload;
using Protocol.Messages;
using Server;

var messageRouter = new ReflectionBasedMessageRouter();
var clientHandler = new ClientHandler(new JsonPayload(), typeof(Hello).Namespace!, messageRouter);
var messageHandler = new MessageHandler(clientHandler);
messageRouter.MessageHandler = messageHandler;

var serverPort = 12345;
var tcpServer = new TcpServer(serverPort);
var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connection on port {serverPort}");

while (true)
{
}
