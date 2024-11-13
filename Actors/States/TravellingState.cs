using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DeliveryPriority;
using Actors.DeliveryLists;
using Akka.Actor;
using MathNet.Spatial.Euclidean;
using System.Diagnostics;

namespace Actors.States
{
    internal class TravellingState : DroneState
    {
        // Riferimento all'attore che sta gestendo il "volo fisico"
        private IActorRef? _travellingActor;

        private bool _isDeliveryCompleted = false;

        // Ultima posizione del drone registrata.
        // Nel caso in cui la consegna termini si ritorna
        // qualcosa quando viene chiamato il metodo GetCurrentPosition.
        private Point2D _lastPositionCache;

        private readonly DateTime _startTime = DateTime.Now;
        internal TimeSpan DroneTravelTime() => DateTime.Now - _startTime;
        internal TimeSpan RemainingTravelTime() => DeliveryPath.ExpectedDuration() - DroneTravelTime();
        
        public TravellingState(DroneState previousState): base(previousState) 
        {
            _lastPositionCache = DeliveryPath.StartPoint;
        }

        internal override DroneState RunState()
        {
            Context.Log.Warning("Delivery started.");
            foreach (var node in ConflictList.GetNodes())
            {
                node.Tell(new TravellingResponse(DeliveryPath));
            }

            // Avvio un attore figlio che gestisce il processo del volo
            _travellingActor = Context.Context.ActorOf(
                TravellingDrone.Props(Context.ThisDelivery, ActorRef), "travel-actor");

            // lo supervisiono (in modo da rilevare quando e come termina)
            Context.Context.Watch(_travellingActor);

            return this;
        }

        internal override DroneState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            sender.Tell(new TravellingResponse(GetCurrentPath()));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!Context.Nodes.Contains(sender))
                Context.Nodes.Add(sender);

            return this;
        }

        internal override DroneState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            
            return this;
        }

        internal override DroneState OnReceive(TravellingResponse msg, IActorRef sender)
        {

            Debug.Assert(GetCurrentPath().ClosestCollisionPoint(msg.Path) == null);

            return this;
        }

        internal override DroneState OnReceive(MetricMessage msg, IActorRef sender)
        {

            sender.Tell(new MetricMessage(Priority.MaxPriority, LastArrangingRound));
            sender.Tell(new TravellingResponse(GetCurrentPath()));

            return this;
        }

        internal override DroneState OnReceive(StillHereMessage msg, IActorRef sender)
        {
            sender.Tell(new TravellingResponse(GetCurrentPath()));

            return this;
        }

        internal override DroneState OnReceive(DeliveryCompleted msg, IActorRef sender)
        {
            if (_isDeliveryCompleted)
                return this;

            _isDeliveryCompleted = true;

            _lastPositionCache = msg.Position;

            // termino l'attore che gestisce il volo e cancello il riferimento
            _travellingActor.Tell(PoisonPill.Instance);
            _travellingActor = null;   
            
            return CreateExitState(this, true, "Delivery ENDED! Killing myself").RunState();
        }


        private DeliveryPath GetCurrentPath()
        {
            return new DeliveryPath(
                GetCurrentPosition(),
                DeliveryPath.EndPoint
                );
        }

        // Richiedi la posizione all'attore che gestisce il volo 
        internal Point2D GetCurrentPosition()
        {
            if (_travellingActor is null)
                return _lastPositionCache;

            Task<PositionResponse> t = _travellingActor.Ask<PositionResponse>(
                new PositionRequest(), new TimeSpan(0, 0, 10));

            t.Wait();

            if (t.IsCompleted)
            {
                _lastPositionCache = t.Result.Position;
            }

            return _lastPositionCache;
        }
    }
}
