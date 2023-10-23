namespace Protocol;

public record AssignName(string Name);
public record ChangeName(string NewName);
public record PublishText(string Message);
public record Text(string Sender, string Message, DateTime Time);
