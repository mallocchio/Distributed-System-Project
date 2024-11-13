using Actors.Messages.External;
using Actors.DeliveryPriority;
using Actors.DeliveryLists;
using Akka.Actor;

namespace Actors.States
{
    internal class InitState : DroneState
    {

        private const string _timeoutKeyName = "connectResponse-timeout";
        private readonly ISet<IActorRef> _expectedConnectResponses;

        internal IReadOnlySet<IActorRef> GetDeliveryConnectResponses() => _expectedConnectResponses.ToHashSet();

        internal InitState(DroneContext context, 
            ConflictList conflictList, ControlTower controlTower) 
            : base(context, conflictList, controlTower)
        {
            _expectedConnectResponses = Context.Nodes.ToHashSet();
        }

        internal override DroneState RunState()
        {
            if (_expectedConnectResponses.Count > 0)
            {
                // invio richiesta di connessione a tutti i nodi noti
                var connectRequest = new ConnectRequest(DeliveryPath);

                foreach (var node in _expectedConnectResponses)
                {
                    node.Tell(connectRequest, ActorRef);
                }

                Context.StartMessageTimeout(_timeoutKeyName, _expectedConnectResponses.Count);
            }

            // se non ho vicini, annullo i timeout e posso passare
            // direttamente allo stato successivo
            return NextState();
        }

        internal override DroneState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            // verifico se c'è conflitto
            if (DeliveryPath.ClosestCollisionPoint(msg.Path) != null)
            {
                ConflictList.AddDelivery(sender, msg.Path);
            }

            _expectedConnectResponses.Remove(sender);

            return NextState();
        }

        internal override DroneState OnReceive(TravellingResponse msg, IActorRef sender)
        {
            base.OnReceive(msg, sender);

            _expectedConnectResponses.Remove(sender);

            return NextState();
        }

        internal override DroneState OnReceive(MetricMessage msg, IActorRef sender)
        {
            // sono ancora in stato di inizializzazione,
            // quindi non partecipo alle negoziazioni
            sender.Tell(new MetricMessage(Priority.MinPriority, LastArrangingRound));

            return this;
        }

        internal override DroneState OnReceive(StillHereMessage msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _expectedConnectResponses.Remove(sender);

            return NextState();
        }

        private DroneState NextState()
        {
            if (_expectedConnectResponses.Count == 0)
            {
                Context.CancelMessageTimeout(_timeoutKeyName);
                return CreateArrangingState(this).RunState();
            }

            return this;
        }
    }
}
