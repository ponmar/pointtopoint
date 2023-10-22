namespace Protocol;

public record PublishText(string Message);
public record Text(string Sender, string Message, DateTime Time);
