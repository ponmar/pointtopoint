namespace Protocol;

// Client -> Server
public record ChangeName(string NewName);
public record PublishText(string Message);

// Server -> Client
public record Users(List<string> Names);
public record AssignName(string Name);
public record Text(string Sender, string Message, DateTime Time);
