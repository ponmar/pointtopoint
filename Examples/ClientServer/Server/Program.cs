using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;
using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.MessageRouting;

var messageRouter = new ReflectionMessageRouter();

var clientHandler = new ClientHandler(
    new NewtonsoftJsonPayload(typeof(Hello).Namespace!),
    messageRouter,
    new ConsoleLoggerErrorHandler());

clientHandler.ClientConnected += (sender, guid) => Console.WriteLine($"Client connected: {guid}");
clientHandler.ClientDisconnected += (sender, guid) => Console.WriteLine($"Client disconnected: {guid}");

var messageHandler = new MessageHandler(clientHandler);
messageRouter.MessageHandler = messageHandler;

var serverPort = 12345;
var tcpServer = new TcpServer(serverPort);
var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {serverPort}");

while (true)
{
    Thread.Sleep(1000);
}
