using Akka.Actor;
using Actors.Utils;
using Akka.Event;
using Actors.Messages.Register;

namespace Actors
{
    // Si occupa si spawnare e supervisionare una serie di droni.
    public class DronesRegister : ReceiveActor
    {
        private readonly ISet<IActorRef> _nodes;
        private readonly DebugLog _logger;

        public DronesRegister()
        {
            _logger = new(Context.GetLogger());
            _nodes = new HashSet<IActorRef>();

            Receive<RegisterRequest>(msg => OnReceive(msg));
            Receive<Terminated>(msg => OnReceive(msg));
        }

        private void OnReceive(Terminated msg)
        {
            if(_nodes.Remove(Sender))
            {
                _logger.Warning($"Delivery of drone {Sender} completed. Removing from the registry.");
            }
        }

        private void OnReceive(RegisterRequest msg)
        {
            _logger.Warning($"Registering node {msg.Actor} in the registry.");
            Sender.Tell(new RegisterResponse(_nodes.ToHashSet()), Self);

            Context.Watch(msg.Actor);
            _nodes.Add(msg.Actor);
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new DronesRegister());
        }
    }
}
