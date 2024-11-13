using Akka.Actor;
using System.Diagnostics;

namespace Actors.DeliveryPriority
{
    // Priorità di una consegna; è rappresentata da una metrica. 
    // A parita di metrica, per il confronto si considera l'identificatore del nodo.
    // Non possono esistere due noti di priorità uguale.
    public class Priority : IComparable<Priority>
    {
        // Misura della priorità di un nodo: numero reale.
        public double MetricValue { get; }

        // L'identificatore del nodo. Necessario per confrontare l'ID
        public IActorRef? NodeRef { get; }

        // I nodi in volo e quelli che non hanno ancora condiviso la propria metrica hanno priorità infinita
        public static Priority MaxPriority { get { return new MaxPriority(); } }

        // I nodi in fase di inizializzazione hanno priorità nulla
        public static Priority MinPriority { get { return new MinPriority(); } }

        public Priority(double metricValue, IActorRef? nodeRef)
        {
            MetricValue = metricValue;
            NodeRef = nodeRef;
        }

        // Confronta due priorità. Restituisce un numero positivo 
        // se questa priorità è maggiore, un numero negativo se è minore.
        public virtual int CompareTo(Priority? other)
        {
            Debug.Assert(other != null);
            Debug.Assert(!this.Equals(other));
            Debug.Assert(NodeRef != null);
            Debug.Assert(!NodeRef!.Equals(other.NodeRef));

            if (MetricValue.CompareTo(other.MetricValue) != 0) return MetricValue.CompareTo(other.MetricValue);

            // in caso di parità di metrica vince il nodo
            // con identificatore più piccolo
            return - NodeRef.CompareTo(other.NodeRef);
        }

        public override string? ToString()
        {
            return "\n{"
                + $"\n\tMetricValue: {MetricValue}, "
                + $"\n\tNodeRef: {NodeRef}, "
                + "\n}";
        }
    }

    internal class MaxPriority : Priority
    {
        internal MaxPriority() : base(double.MaxValue, null)
        {
        }

        public override int CompareTo(Priority? other)
        {
            Debug.Assert(other != MaxPriority);
            return +1;
        }
    }

    internal class MinPriority : Priority
    {
        internal MinPriority() : base(double.MinValue, null)
        {
        }

        public override int CompareTo(Priority? other)
        {
            Debug.Assert(other != MinPriority);
            return -1;
        }
    }


}
