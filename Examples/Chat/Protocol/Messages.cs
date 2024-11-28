namespace Protocol;

// Client -> Server
public class ChangeName
{
    public string NewName { get; set; }
}

public class PublishText
{
    public string Message { get; set; }
}

// Server -> Client
public class Users()
{
    public List<string> Names { get; set; }
}

public class AssignName()
{
    public string Name { get; set; }
}

public class Text()
{
    public string Sender { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
}
