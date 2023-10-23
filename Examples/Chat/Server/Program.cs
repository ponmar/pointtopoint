using PointToPoint.Server;
using PointToPoint.Payload;
using Protocol;
using Server;

var clientHandler = new ClientsHandler(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Namespace!),
    Constants.KeepAliveSendInterval,
    typeof(ChatClientHandler));

var tcpServer = new TcpServer(Constants.DefaultPort);
var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
tcpServerThread.Start();

Console.WriteLine($"Listening for connections on port {Constants.DefaultPort}");

while (true)
{
    Thread.Sleep(1000);
}
