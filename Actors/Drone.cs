using Actors.DeliveryPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.States;
using Actors.Messages.Register;

namespace Actors
{
    public abstract class Drone : ReceiveActor, IWithTimers
    {
        public ITimerScheduler? Timers { get; set; }

        private DroneState? _droneState; 

        // Eventuale riferimento a chi mi ha spawnato
        private readonly IActorRef? _spawner;

        public Drone(IActorRef? spawner = null)
        {
            _spawner = spawner;
        }

        // Algoritmo di schedulazione delle partenze
        protected void AlgorithmRunBehaviour(ISet<IActorRef> nodes, DeliveryPath deliveryPath)
        {
            var droneContext = new DroneContext(Context, nodes, new WaitingDelivery(Self, deliveryPath, Priority.MinPriority), Timers!);

            // avvio dello stato iniziale
            _droneState = DroneState.CreateInitState(droneContext, Timers!).RunState();

            ReceiveMainProtocolMessages();
            ReceiveInternalMessage();
        }

        // Riceve e gestisce tutti i messaggi esterni del protocollo 
        // di schedulazione dei voli.
        // la modalità di gestione dei messaggi dipende dallo stato del drone
        private void ReceiveMainProtocolMessages()
        {
            Receive<ConnectRequest> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ConnectResponse>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<TravellingResponse> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<MetricMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<StillHereMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ExitMessage>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        // Riceve e gestisce tutti i messaggi interni del protocollo 
        // di schedulazione dei voli.
        private void ReceiveInternalMessage()
        {
            Receive<ClearAirSpaceMessage>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<DeliveryCompleted>   (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ExpiredTimeout>      (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        public static Props Props(IActorRef register, DeliveryPath deliveryPath, IActorRef? spawner=null)
        {
            return Akka.Actor.Props.Create(() => new RegisterDrone(register, deliveryPath, spawner));
        }

        public static Props Props(ISet<IActorRef> nodes, DeliveryPath deliveryPath, IActorRef? spawner=null)
        {
            return Akka.Actor.Props.Create(() => new TestDrone(nodes, deliveryPath, spawner));
        }
    }

    // Quando un drone spawna, comunica il suo ingresso al registro e riceve
    // una lista di tutti i nodi attivi.
    internal class RegisterDrone : Drone
    {
        public RegisterDrone(IActorRef register, DeliveryPath deliveryPath, IActorRef? spawner = null) : base(spawner)
        {
            try
            {
                Task<RegisterResponse> t = register.Ask<RegisterResponse>(new RegisterRequest(Self), TimeSpan.FromSeconds(10));
                t.Wait();

                AlgorithmRunBehaviour(t.Result.Nodes, deliveryPath);
            }
            catch (AskTimeoutException)
            {
                Context.GetLogger().Info($"Timeout expired. Unable to communicate with the register at address {register}.");
                Context.Stop(Self);
            }
        }
    }

    internal class TestDrone : Drone
    {
        public TestDrone(ISet<IActorRef> nodes, DeliveryPath deliveryPath, IActorRef? spawner = null) : base(spawner)
        {
            AlgorithmRunBehaviour(nodes.ToHashSet(), deliveryPath);
        }
    }

}
