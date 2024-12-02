using PointToPoint.Server;
using Protocol;
using Server;
using PointToPoint.Server.ClientHandler.Factories;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Payload.NewtonsoftJson;

var port = Constants.DefaultPort;

var clientsHandler = new ClientsHandler<ChatClientHandler>(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Assembly),
    new ActivatorClientHandlerFactory(),
    new QueuingReflectionMessageRouterFactory());

var tcpServer = new TcpServer(port);
var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {port}...");

while (true)
{
    clientsHandler.UpdateAll();
    Thread.Sleep(100);
}
