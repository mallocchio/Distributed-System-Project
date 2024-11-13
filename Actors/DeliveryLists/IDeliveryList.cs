using Actors.DeliveryPriority;
using Akka.Actor;

namespace Actors.DeliveryLists
{
    // Strumento generico per gestire le liste di consegne attraverso 
    // i riferimenti ai nodi.
    public abstract class IDeliveryList<D> where D : Delivery
    {
        // Tutte le consegne della collezione
        protected IDictionary<IActorRef, D> Deliveries { get; } = new Dictionary<IActorRef, D>();

        // Cerca una consegna nella lista attraverso il nome del suo nodo
        public D? GetDelivery(IActorRef nodeRef)
        {
            if (Deliveries.ContainsKey(nodeRef))
                return Deliveries[nodeRef];
            return null;
        }

        // Crea e aggiunge una nuova consegna alla lista
        public void AddDelivery(IActorRef nodeRef, DeliveryPath path)
        {
            if (!Deliveries.ContainsKey(nodeRef))
                Deliveries.Add(nodeRef, CreateDelivery(nodeRef, path));
        }

        // Rimuovi una consegna dalla lista
        public D? RemoveDelivery(IActorRef nodeRef)
        {
            if (!Deliveries.ContainsKey(nodeRef))
                return null;

            var delivery = Deliveries[nodeRef];
            Deliveries.Remove(nodeRef);
            return delivery;
        }

        // Crea una nuova consegna
        protected abstract D CreateDelivery(IActorRef nodeRef, DeliveryPath path);

        public virtual ISet<IActorRef> GetNodes()
        {
            return Deliveries.Keys.ToHashSet();
        }

        public virtual ISet<D> GetDeliveries()
        {
            return Deliveries.Values.ToHashSet();
        }
    }
}
