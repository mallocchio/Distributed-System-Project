using System.Diagnostics;

namespace Actors.DeliveryPriority
{
    // Strumento per il calcolo della priorità di una certa consegna.
    internal static class PriorityCalculator
    {
        public static Priority CalculatePriority(Delivery thisDelivery, TimeSpan age, ISet<WaitingDelivery> conflictList, ISet<TravellingDelivery> travellingList)
        {
            
            // calcolo della somma dei tempi che faccio attendere alla mia conflct-list
            TimeSpan sumOfWaitsICause = conflictList.Aggregate(
                TimeSpan.Zero,
                (partialSum, delivery) =>
                {
                    var collisionPoint = delivery.Path.ClosestCollisionPoint(thisDelivery.Path);
                    Debug.Assert(collisionPoint != null);
                    return partialSum + thisDelivery.Path.TimeDistance(collisionPoint.Value) - delivery.Path.TimeDistance(collisionPoint.Value);
                });

            return new Priority(
                    ParseValue(age)
                    - conflictList.Count
                    - ParseValue(sumOfWaitsICause),
                thisDelivery.NodeRef
            );
        }

        private static double ParseValue(TimeSpan time)
        {
            return (double)time.TotalMinutes;
        }
    }
}
