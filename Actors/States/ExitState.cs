using Actors.Messages.External;
using Actors.Messages.Internal;
using Akka.Actor;

namespace Actors.States
{
    internal class ExitState : DroneState
    {
        internal readonly DroneState PreviousState;
        internal bool IsDeliveryCompleted { get; }
        internal bool Error { get;  }
        internal string Motivation { get; }

        
        public ExitState(DroneState previousState, bool isDeliveryCompleted, string motivation, bool error=false)
            : base(previousState)
        {
            PreviousState = previousState;
            IsDeliveryCompleted = isDeliveryCompleted;
            Motivation = motivation;
            Error = error;
        }

        internal override DroneState RunState()
        {
            Context.Log.Error(Motivation);

            // comunico la mia uscita a tutti i nodi noti 
            foreach (var node in Context.Nodes)
            {
                node.Tell(new DeliveryCompletedMessage());
            }

            if (!Error)
                ActorRef.Tell(PoisonPill.Instance, ActorRefs.NoSender);
            else
                Context.Context.Stop(ActorRef);

            return this;
        }

        internal override DroneState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(MetricMessage msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(StillHereMessage msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(TravellingResponse msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(ExitMessage msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(ClearAirSpaceMessage msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(DeliveryCompleted msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(ExpiredTimeout msg, IActorRef sender)
        {
            return PreviousState.OnReceive(msg, sender);
        }
    }
}
