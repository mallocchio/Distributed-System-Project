using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.Messages.Internal
{
    // Messaggio che un attore usa per comunicare a se stesso 
    // che una certa consegna ora ha raggiunto un punto tale da
    // permettere una partenza sicura
    public class ClearAirSpaceMessage
    {
        public IActorRef ClearDeliveryNodeRef { get; }

        public ClearAirSpaceMessage(IActorRef clearDeliveryNodeRef)
        {
            ClearDeliveryNodeRef = clearDeliveryNodeRef;
        }
    }

    public class UpdatePosition { }

    public class PositionRequest { }

    public class PositionResponse 
    {
        public Point2D Position { get; }

        public PositionResponse(Point2D position)
        {
            Position = position;
        }
    }

    public class DeliveryCompleted 
    {
        public Point2D Position { get; }

        public DeliveryCompleted(Point2D position)
        {
            Position = position;
        }
    }

    internal class ExpiredTimeout
    {
        public string TimerKey { get; }

        public ExpiredTimeout(string timerKey)
        {
            TimerKey = timerKey;
        }
    }
}
