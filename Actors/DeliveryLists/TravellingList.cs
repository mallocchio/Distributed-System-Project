using Actors.DeliveryPriority;
using Akka.Actor;

namespace Actors.DeliveryLists
{
    // Strumento per la gestione delle consegne in volo. 
    public class TravellingList : IDeliveryList<TravellingDelivery>
    {
        protected override TravellingDelivery CreateDelivery(IActorRef nodeRef, DeliveryPath path)
        {
            return new TravellingDelivery(nodeRef, path, DateTime.Now);
        }
    }
}
