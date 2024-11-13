using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.DeliveryPriority;
using MathNet.Spatial.Euclidean;
using ActorsAPI.APIClasses;

namespace Tests.APITest
{
    // Rappresentazione di situazioni nelle quali un nuovo nodo 
    // spawna e deve gestire dei semplici conflitti con un unico nodo
    public class DeliveryAPITest : TestKit
    {
        private readonly string _systemName;
        private readonly string _registerActorName;

        private DeliveryEnvironmentAPI _api;

        public DeliveryAPITest()
        {
            _systemName = Sys.Name;
            _registerActorName = "register";

            Sys.ActorOf(Props.Create(() => new Spawner()), "spawner");

            _api = new DeliveryEnvironmentAPI(Sys, _systemName, _registerActorName);
            _api.DeployRegister(Host.GetTestHost());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Sys.Terminate();
        }


        // Tentativo di avvio di una consegna tramite le API
        [Fact]
        public void DeliveryStart()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            IDeliveryAPI a = _api.SpawnDelivery(
                Host.GetTestHost(), 
                deliveryA, 
                "DroneA",
                DeliveryAPI.Factory());

            IDeliveryAPI b = _api.SpawnDelivery(
                Host.GetTestHost(),
                deliveryB,
                "DroneB",
                DeliveryAPI.Factory());
        }


        // spawn di una consegna senza aver creato/impostato un registro
        [Fact]
        public void SpawnWithoutRegister()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(100, 100));

            var api2 = new DeliveryEnvironmentAPI(Sys, _systemName, _registerActorName);

            Assert.ThrowsAny<Exception>(
                () => api2.SpawnDelivery(
                    Host.GetTestHost(), deliveryA, "DroneA", DeliveryAPI.Factory())
            );

            // riprovo impostando un registro e mi aspetto funzioni
            api2.SetRegister(Host.GetTestHost());
            Assert.IsAssignableFrom<IDeliveryAPI>(api2.SpawnDelivery(
                    Host.GetTestHost(), deliveryA, "DroneA", DeliveryAPI.Factory()));

        }
    }
}
