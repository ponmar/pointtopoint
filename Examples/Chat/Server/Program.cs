using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;

var port = Constants.DefaultPort;

var clientHandler = new ClientsHandler(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Namespace!),
    Constants.KeepAliveSendInterval,
    typeof(ChatClientHandler));

var tcpServer = new TcpServer(port);
var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {port}...");

while (true)
{
    Thread.Sleep(1000);
}
