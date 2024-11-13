using Actors.Messages.Internal;
using Actors.DeliveryPriority;
using Actors.Utils;
using Akka.Actor;
using Akka.Event;
using MathNet.Spatial.Euclidean;

namespace Actors
{
    internal class TravellingDrone : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        private readonly Delivery _delivery;
        private readonly IActorRef _supervisor;
        private Point2D _position;
        private DateTime _lastUpdateTime;

        private readonly DebugLog _logger = new(Context.GetLogger()); 

        public TravellingDrone(Delivery delivery, IActorRef supervisor)
        {
            _delivery = delivery;
            _supervisor = supervisor;
            _position = delivery.Path.StartPoint;
            _lastUpdateTime = DateTime.Now;

            Receive<PositionRequest>(msg => OnReceive(msg));
            Receive<UpdatePosition> (msg => OnReceive(msg));

            Timers!.StartPeriodicTimer("updatePosition", new UpdatePosition(), TimeSpan.FromSeconds(1));
        }

        private void OnReceive(PositionRequest msg)
        {
            Sender.Tell(new PositionResponse(_position));
        }

        private void OnReceive(UpdatePosition msg)
        {
            var elapsedTime = DateTime.Now - _lastUpdateTime;
            var distanceTraveled = (_delivery.Path.Speed * elapsedTime.TotalSeconds) * _delivery.Path.PathSegment().Direction;
            
            _position += distanceTraveled;
            _lastUpdateTime = DateTime.Now;

            //Vettore punto di arrivo - punto attuale
            var distanceToEnd = _delivery.Path.PathSegment().EndPoint - _position;
            if (distanceToEnd.Normalize().Equals(-_delivery.Path.PathSegment().Direction, 1e-3) || distanceToEnd.Length == 0)
            {
                _supervisor.Tell(new DeliveryCompleted(_position));
                _logger.Warning("Delivery completed.");
            }
        }

        public static Props Props(Delivery delivery, IActorRef supervisor)
        {
            return Akka.Actor.Props.Create(() => new TravellingDrone(delivery, supervisor));
        }
    }
}
