using Akka.Actor;
using Akka.Event;

namespace Actors
{
    // Attore che gestisce lo spawn dei nodi
    public class Spawner : ReceiveActor
    {
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        public Spawner()
        {
            Receive<SpawnDroneRequest>((msg) => OnReceive(msg));
            Receive<SpawnDroneTestMessage>((msg) => OnReceive(msg));
        }

        private void OnReceive(SpawnDroneRequest msg)
        {
            try
            {
                IActorRef child = Context.ActorOf(msg.ActorProps, msg.ActorName);
                Sender.Tell(child);
            }
            catch (Exception ex)
            {
                Sender.Tell(ex);
            }
        }

        private void OnReceive(SpawnDroneTestMessage msg)
        {
            Sender.Tell(true);
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 0,
                withinTimeRange: Timeout.InfiniteTimeSpan, 
                localOnlyDecider: ex =>
                {
                    _logger.Error($"A drone delivery failed due to exception: {ex}");
                    return Directive.Stop;
                });
        }
    }

    public class SpawnDroneRequest
    {
        public Props ActorProps { get; }
        public string ActorName { get; }

        public SpawnDroneRequest(Props actorProps, string actorName)
        {
            ActorProps = actorProps;
            ActorName = actorName;
        }
    }

    public class SpawnDroneTestMessage
    {

    }
}
