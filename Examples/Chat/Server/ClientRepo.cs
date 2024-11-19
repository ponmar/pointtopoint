
namespace Server;

public class ClientRepo
{
    public static readonly ClientRepo Instance = new();

    private readonly List<ChatClientHandler> clients = [];
    private readonly Lock clientsLock = new();

    public List<string> GetUsernames()
    {
        lock (clientsLock)
        {
            return new List<string>(clients.Select(x => x.Name));
        };
    }

    public void AddClient(ChatClientHandler client)
    {
        lock (clientsLock)
        {
            clients.Add(client);
        }
    }

    public void RemoveClient(ChatClientHandler client)
    {
        lock (clientsLock)
        {
            clients.Remove(client);
        }
    }
}
