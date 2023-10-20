using System;

namespace PointToPoint.Messenger.ErrorHandler
{
    public class ConsoleLoggerErrorHandler : IMessengerErrorReporter
    {
        public void Disconnected(Guid messengerId, Exception e)
        {
            Console.WriteLine($"{messengerId} disconnected: {e?.Message}");
        }
    }
}
