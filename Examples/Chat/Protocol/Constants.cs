namespace Protocol;

public static class Constants
{
    public const int Port = 12345;
    public static TimeSpan KeepAliveSendInterval => TimeSpan.FromSeconds(1);
}
