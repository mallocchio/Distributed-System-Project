using Akka.Actor;

namespace ActorsAPI.APIClasses
{
    // API per interfacciarsi con una consegna
    public interface IDeliveryAPI
    {
        // Riferimento al drone che sta portando a termine la consegna
        public IActorRef GetDroneRef();
    }

    // Strumento per creare istanze di IDeliveryAPI
    // a partire da una consegna già avviata.
    public interface IDeliveryAPIFactory
    {
       public IDeliveryAPI GetDeliveryAPI(IActorRef nodeRef);
    }

    public class DeliveryIsUnreachableException : AkkaException
    {

    }

    public class DeliveryAPI : IDeliveryAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);
        private readonly TimeSpan _timeout = DEFAULT_TIMEOUT;
        private readonly IActorRef _nodeRef;

        public DeliveryAPI(IActorRef nodeRef)
        {
            _nodeRef = nodeRef;
        }

        public DeliveryAPI(IActorRef nodeRef, TimeSpan timeout) : this(nodeRef)
        {
            _timeout = timeout;
        }

        public IActorRef GetDroneRef() => _nodeRef;

        public static IDeliveryAPIFactory Factory() => new DeliveryAPIFactory();
    }

    internal class DeliveryAPIFactory : IDeliveryAPIFactory
    {
        public IDeliveryAPI GetDeliveryAPI(IActorRef nodeRef)
        {
            return new DeliveryAPI(nodeRef);
        }
    }
}
