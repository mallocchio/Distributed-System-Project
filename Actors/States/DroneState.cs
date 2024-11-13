using Actors.DeliveryPriority;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DeliveryLists;
using Akka.Actor;

namespace Actors.States
{
    public abstract class DroneState
    {
        internal DroneContext Context { get; }
        internal int LastArrangingRound { get; set; }

        
        // Per la gestione delle tratte in conflitto
        internal ConflictList ConflictList { get; }

        // Per la gestione delle tratte in volo
        internal ControlTower ControlTower { get; }

    
        // shortcut per la tratta della consegna corrente
        protected DeliveryPath DeliveryPath { get => Context.ThisDelivery.Path; }

        // shortcut per il riferimento al nodo corrente
        protected IActorRef ActorRef { get => Context.Context.Self; }

        internal DroneState(DroneContext context, ConflictList conflictList, ControlTower controlTower)
        {
            Context = context;
            ConflictList = conflictList;
            ControlTower = controlTower;
            LastArrangingRound = 0;
        }

        protected DroneState(DroneState state)
        {
            Context = state.Context;
            ConflictList = state.ConflictList;
            ControlTower = state.ControlTower;
            LastArrangingRound = state.LastArrangingRound;
        }

        internal static DroneState CreateInitState(DroneContext context, ITimerScheduler timer)
            => new InitState(
                context, 
                new ConflictList(), 
                new ControlTower(context.ThisDelivery, new TravellingList(), timer)
                );

        internal static DroneState CreateArrangingState(DroneState previousState)
            => new ArrangingState(previousState);

        internal static DroneState CreateWaitingState(DroneState previousState, Priority priority)
            => new WaitingState(previousState, priority);

        internal static DroneState CreateTravellingState(DroneState previousState)
            => new TravellingState(previousState);

        internal static DroneState CreateExitState(DroneState previousState, bool isDeliveryCompleted, string motivation, bool error=false) 
            => new ExitState(previousState, isDeliveryCompleted, motivation, error);

        internal virtual DroneState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            // rispondo con la mia tratta
            sender.Tell(new ConnectResponse(DeliveryPath));

            // se il nodo è sconosciuto lo aggiungo alla lista
            if (!Context.Nodes.Contains(sender))
                Context.Nodes.Add(sender);

            // verifico conflitto
            if (DeliveryPath.ClosestCollisionPoint(msg.Path) != null)
            {
                ConflictList.AddDelivery(sender, msg.Path);
            }

            return this;
        }

        internal virtual DroneState OnReceive(TravellingResponse msg, IActorRef sender)
        {
            // è in volo => non ci devo più negoziare
            WaitingDelivery? eventualDelivery = ConflictList.RemoveDelivery(sender);

            // se ho conflitto, aggiungo il nodo alla lista di nodi in volo
            if (DeliveryPath.ClosestCollisionPoint(msg.Path) is not null)
            {
                ControlTower.MakeDeliveryTravel(
                    eventualDelivery ??
                    new WaitingDelivery(sender, msg.Path, Priority.MaxPriority)
                    );
            }

            return this;
        }

        internal virtual DroneState OnReceive(ExitMessage msg, IActorRef sender)
        {
            ConflictList.RemoveDelivery(sender);
            Context.Nodes.Remove(sender);

            return this;
        }

        internal abstract DroneState OnReceive(ConnectResponse msg, IActorRef sender);

        internal abstract DroneState OnReceive(MetricMessage msg, IActorRef sender);

        internal abstract DroneState OnReceive(StillHereMessage msg, IActorRef sender);

        internal virtual DroneState OnReceive(ClearAirSpaceMessage msg, IActorRef sender)
        {
            ControlTower.OnReceive(msg);
            return this;
        }

        internal virtual DroneState OnReceive(DeliveryCompleted msg, IActorRef sender)
        {
            return this;
        }

        internal virtual DroneState OnReceive(ExpiredTimeout msg, IActorRef sender)
        {

            return CreateExitState(this, false, $"ERROR: Timeout {msg.TimerKey} expired!.", true).RunState();
        }

        internal abstract DroneState RunState();
    }
}
