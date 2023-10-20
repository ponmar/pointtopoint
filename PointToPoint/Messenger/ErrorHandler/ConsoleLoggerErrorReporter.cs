using System;

namespace PointToPoint.Messenger.ErrorHandler
{
    public class ConsoleLoggerErrorReporter : IMessengerErrorReporter
    {
        public void Disconnected(Guid messengerId, Exception e)
        {
            Console.WriteLine($"{messengerId} disconnected: {e?.Message}");
        }
    }
}
