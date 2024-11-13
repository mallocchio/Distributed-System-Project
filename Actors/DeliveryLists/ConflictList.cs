using Actors.DeliveryPriority;
using Akka.Actor;

namespace Actors.DeliveryLists
{
    // Lista di consegne con cui sono in conflitto 
    // 
    // Utilizzato per estrarre insiemi
    // di consegne di priorità inferiore e superiore.
    public class ConflictList : IDeliveryList<WaitingDelivery>
    {
        // Estrae tutte le consegne di priorità superiore a quella
        // data in input.
        public IDictionary<IActorRef, WaitingDelivery> GetGreaterPriorityDeliveries(Priority p)
        {
            return Deliveries.Where(pair => pair.Value.Priority.CompareTo(p) > 0).ToDictionary(
                delivery => delivery.Key,
                delivery => delivery.Value
                );
        }

        // Estrae tutte le consegne di priorità inferiore a quella
        // data in input.
        public IDictionary<IActorRef, WaitingDelivery> GetSmallerPriorityDeliveries(Priority p)
        {
            return Deliveries.Where(pair => pair.Value.Priority.CompareTo(p) < 0).ToDictionary(
                delivery => delivery.Key,
                delivery => delivery.Value
                );
        }
        
        protected override WaitingDelivery CreateDelivery(IActorRef nodeRef, DeliveryPath path)
        {
            return new WaitingDelivery(nodeRef, path, Priority.MaxPriority);
        }
    }


}
