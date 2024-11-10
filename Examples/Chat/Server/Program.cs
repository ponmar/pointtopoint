using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;
using PointToPoint.Server.ClientHandler;

var port = Constants.DefaultPort;

var clientsHandler = new ClientsHandler<ChatClientHandler>(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Namespace!),
    new ActivatorClientHandlerFactory());

var tcpServer = new TcpServer(port);
var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {port}...");

while (true)
{
    Thread.Sleep(1000);
}
