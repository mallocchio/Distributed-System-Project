using Akka.Actor;
using MathNet.Spatial.Euclidean;
using System.Diagnostics;

namespace Actors.DeliveryPriority
{
    // Consegna. Caratterizzata dalla tratta e  
    // da un riferimento al nodo che la porta a termine.
    public abstract class Delivery
    {
        public IActorRef NodeRef { get; }

        public DeliveryPath Path { get; }

        protected Delivery(IActorRef nodeRef, DeliveryPath path)
        {
            NodeRef = nodeRef;
            Path = path;
        }
    }

    // Consegna in attesa di partire, caratterizzata da una priorità modificabile.
    public class WaitingDelivery : Delivery
    {
        public Priority Priority { get; set; }

        public WaitingDelivery(IActorRef nodeRef, DeliveryPath path, Priority priority) : base(nodeRef, path)
        {
            Priority = priority;
        }
    }

    // Consegna in volo. 
    // 
    // Utilizzata per calcolare:
    // - il tempo rimanente per il completamento
    // - il tempo rimanente per il raggiungimento di un certo punto
    // - il tempo che una certa consegna in conflitto deve attendere per effettuare una partenza sicura.
    public class TravellingDelivery : Delivery
    {
        private readonly DateTime _startTime;

        public TravellingDelivery(IActorRef nodeRef, DeliveryPath path, DateTime startTime) : base(nodeRef, path)
        {
            _startTime = startTime;
        }


        public TimeSpan GetRemainingTimeForClearAirspace(Delivery thisDelivery)
        {
            Point2D? collisionPoint = thisDelivery.Path.ClosestCollisionPoint(Path);

            if (collisionPoint == null) return TimeSpan.Zero;

            var timeDistTravelPoint = Path.TimeDistance(collisionPoint.Value);
            var timeDistWaitPoint = thisDelivery.Path.TimeDistance(collisionPoint.Value);
            var alreadyWaitedTime = DateTime.Now - _startTime;
            var marginTime = TimeSpan.FromSeconds((int)(DeliveryPath.SafetyDistance / thisDelivery.Path.Speed));
            
            return timeDistTravelPoint - timeDistWaitPoint - alreadyWaitedTime + marginTime;
        }
    }
}
