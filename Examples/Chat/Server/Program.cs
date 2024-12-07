using PointToPoint.Server;
using Protocol;
using Server;
using PointToPoint.Server.ClientHandler.Factories;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Payload.NewtonsoftJson;
using PointToPoint.Server.TcpListener;

var port = Constants.DefaultPort;

var clientsHandler = new ClientsHandler<ChatClientHandler>(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Assembly),
    new ActivatorClientHandlerFactory(),
    new QueueingReflectionMessageRouterFactory());

var tcpServer = new TcpServer(NetworkInterface.AnyIPv4, port, new TcpListenerFactory());
var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {port}...");

while (true)
{
    clientsHandler.UpdateClients();
    Thread.Sleep(100);
}
