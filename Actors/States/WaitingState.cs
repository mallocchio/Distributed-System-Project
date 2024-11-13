using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DeliveryPriority;
using Actors.DeliveryLists;
using Akka.Actor;

namespace Actors.States
{
    public class WaitingState : DroneState
    {
        private readonly Priority _priority;
        internal Priority GetPriority() => _priority;


        public WaitingState(DroneState previousState, Priority priority): base(previousState)
        {
            _priority = priority;
        }

        internal override DroneState RunState()
        {
            foreach (var node in ConflictList.GetSmallerPriorityDeliveries(_priority).Keys)
            {
                node.Tell(new StillHereMessage());
            }

            return this;
        }

        internal override DroneState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneState OnReceive(MetricMessage msg, IActorRef sender)
        {
           return CreateArrangingState(this).RunState().OnReceive(msg, sender);
        }

        internal override DroneState OnReceive(StillHereMessage msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        internal override DroneState OnReceive(ClearAirSpaceMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        private DroneState NextState()
        {
            if (ControlTower.GetTravellingDeliveries().Count == 0 && ConflictList.GetGreaterPriorityDeliveries(_priority).Count == 0)
                return CreateTravellingState(this).RunState();
            else return this;
        }
    }
}
