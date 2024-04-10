using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;

var port = Constants.DefaultPort;

var clientsHandler = new ClientsHandler(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Namespace!),
    typeof(ChatClientHandler),
    Constants.KeepAliveSendInterval);

var tcpServer = new TcpServer(port);
var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {port}...");

while (true)
{
    Thread.Sleep(1000);
}
