using Actors.Messages.Internal;
using Actors.DeliveryPriority;
using Actors.DeliveryLists;
using Akka.Actor;

namespace Actors
{
    // Strumento per il tracciamento delle consegne in volo. 
    // Permette di:
    // -   registrare la partenza di una consegna in attesa
    // -   inviare al nodo un messaggio ogni qual volta
    //     una delle consegne in volo raggiunge un punto per 
    //     il quale la partenza è sicura.
    public class ControlTower
    {
        private readonly Delivery _thisDelivery;

        private readonly TravellingList _travellingList;

        // Strumento che l'attore usa per schedulare l'invio di
        // un messaggio a se stesso.
        private readonly ITimerScheduler _timers;

        public ControlTower(Delivery thisDelivery, TravellingList travellingList, ITimerScheduler timers)
        {
            _thisDelivery = thisDelivery;
            _travellingList = travellingList;
            _timers = timers;
        }

        // Prende una consegna ferma e registra la sua partenza
        public void MakeDeliveryTravel(WaitingDelivery delivery)
        {
            _travellingList.AddDelivery(delivery.NodeRef, delivery.Path);
            TravellingDelivery travellingDelivery = _travellingList.GetDelivery(delivery.NodeRef)!;

            var safeWaitTime = travellingDelivery.GetRemainingTimeForClearAirspace(_thisDelivery);

            if (safeWaitTime > TimeSpan.Zero)
            {
                // pianifico l'invio a me stesso di un messaggio (che arriva quando si libera la tratta)
                _timers.StartSingleTimer(travellingDelivery,
                    new ClearAirSpaceMessage(travellingDelivery.NodeRef),
                    safeWaitTime
                    );
            }
            else
            {
                _timers.StartSingleTimer(travellingDelivery,
                    new ClearAirSpaceMessage(travellingDelivery.NodeRef),
                    TimeSpan.Zero
                    );
            }   
        }

        // Chiama questo metodo per liberare la travelling-list da
        // consegne che hanno raggiunto un punto safe.
        public void OnReceive(ClearAirSpaceMessage msg)
        {
            _travellingList.RemoveDelivery(msg.ClearDeliveryNodeRef);
        }

        public ISet<TravellingDelivery> GetTravellingDeliveries()
        {
            return _travellingList.GetDeliveries();
        }
    }
}
