using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;
using PointToPoint.MessageRouting;

var messageRouter = new ReflectionMessageRouter();
var clientHandler = new ClientHandler(new NewtonsoftJsonPayloadSerializer(typeof(Text).Namespace!), messageRouter, Constants.KeepAliveSendInterval);

clientHandler.ClientConnected += (sender, guid) => Console.WriteLine($"Client connected: {guid}");
clientHandler.ClientDisconnected += (sender, guid) => Console.WriteLine($"Client disconnected: {guid}");

var messageHandler = new ChatMessageForwarder(clientHandler);
messageRouter.MessageHandler = messageHandler;

var tcpServer = new TcpServer(Constants.DefaultPort);
var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {Constants.DefaultPort}");

while (true)
{
    Thread.Sleep(1000);
}
