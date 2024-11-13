using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DeliveryPriority;
using Actors.DeliveryLists;
using Akka.Actor;

namespace Actors.States
{
    internal struct MetricSenderPair
    {
        public IActorRef Sender { get; }
        public MetricMessage MetricMessage { get; }

        public MetricSenderPair(IActorRef sender, MetricMessage metricMessage)
        {
            Sender = sender;
            MetricMessage = metricMessage;
        }
    }

    internal class ArrangingState : DroneState
    {
        private readonly ISet<IActorRef> _expectedMetrics;
        private readonly ISet<IActorRef> _expectedIntentions;
        private readonly IDictionary<IActorRef, int> _arrangingRounds;
        private readonly IList<MetricSenderPair> _metricMessages;

        private const string _metricTimeoutKey = "metricMessage-timeout";
        private const string _intentionsTimeoutKey = "intentionMessage-timeout";

        // Priorità utilizzata in questo singolo round di negoziazione
        private readonly Priority _priority;

        internal Priority GetPriority() => _priority;

        public ArrangingState(DroneState previousState) 
            : base(previousState)
        {
            _expectedMetrics = ConflictList.GetNodes();
            _expectedIntentions = new HashSet<IActorRef>();
            _priority = PriorityCalculator.CalculatePriority(
                Context.ThisDelivery, 
                Context.Age, 
                ConflictList.GetDeliveries(), 
                ControlTower.GetTravellingDeliveries()
            );
            _arrangingRounds = new Dictionary<IActorRef, int>();
            _metricMessages = new List<MetricSenderPair>();
        }

        internal override DroneState RunState()
        {
            LastArrangingRound++;
            if (_expectedMetrics.Count > 0)
            {
                foreach (var node in _expectedMetrics)
                {
                    node.Tell(new MetricMessage(_priority, LastArrangingRound));
                }

                Context.StartMessageTimeout(_metricTimeoutKey, _expectedMetrics.Count);
            }

            return NextState();
        }

        internal override DroneState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneState OnReceive(TravellingResponse msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            _ = _expectedMetrics.Remove(sender);
            _ = _expectedIntentions.Remove(sender);
            var s = NextState();
            return s;
        }

        internal override DroneState OnReceive(MetricMessage msg, IActorRef sender)
        {
            if (!_arrangingRounds.ContainsKey(sender))
            {
                _arrangingRounds.Add(sender, msg.RelativeRound);
            }

            if (msg.RelativeRound < _arrangingRounds[sender])
            {
                return this;
            }

            if (msg.RelativeRound > _arrangingRounds[sender])
            {
                _metricMessages.Add(new MetricSenderPair(sender, msg));
                return this;
            }

            _ = _expectedMetrics.Remove(sender);
            var delivery = ConflictList.GetDelivery(sender);
            delivery!.Priority = msg.Priority;

            if (_priority.CompareTo(delivery!.Priority) < 0)
            {
                _expectedIntentions.Add(sender);
            }

            return NextState();
        }

        internal override DroneState OnReceive(StillHereMessage msg, IActorRef sender)
        {
            _ = _expectedMetrics.Remove(sender);
            _ = _expectedIntentions.Remove(sender);
            return NextState();
        }

        internal override DroneState OnReceive(ClearAirSpaceMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        internal override DroneState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        private DroneState NextState()
        {
            // attendo tutte le metriche
            if (_expectedMetrics.Count > 0)
                return this;
            else
            {
                Context.CancelMessageTimeout(_metricTimeoutKey);
                Context.StartMessageTimeout(_intentionsTimeoutKey, _expectedIntentions.Count);
            }

            // se ho ricevuto tutte le metriche, ho vinto tutte le negoziazioni
            // e non attendo droni in volo, posso partire
            if (ControlTower.GetTravellingDeliveries().Count == 0 && ConflictList.GetGreaterPriorityDeliveries(_priority).Count == 0)
            {
                Context.CancelMessageTimeout(_intentionsTimeoutKey);
                ResendMetricsToMailBox();
                return CreateTravellingState(this).RunState();
            }

            // se ho ricevuto tutte le metriche (non ho vinto tutte le negoziazioni)
            // e ho ricevuto le intenzioni da tutti coloro che hanno metrica maggiore, 
            // entro in stato di attesa 
            if (_expectedIntentions.Count == 0)
            {
                Context.CancelMessageTimeout(_intentionsTimeoutKey);
                ResendMetricsToMailBox();
                return CreateWaitingState(this, _priority).RunState();
            }
            else 
                return this;
        }

        private void ResendMetricsToMailBox()
        {
            foreach (var msg in _metricMessages)
            {
                ActorRef.Tell(msg.MetricMessage, msg.Sender);
            }
        }
    }
}
