using Actors;
using Actors.Messages.External;
using Actors.Messages.Register;
using Actors.DeliveryPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;

namespace Tests.ActorTests
{
    // Simulazione di timeout dei droni nelle varie fasi
    // Il TestActor fa da register e da "vicino" del nodo, ovvero
    // colui che crasha e provoca il timeout nel drone di prova
    // 
    // Ci si aspetta di ricevere sul TestActor un ExitMessage
    public class DroneErrorTests : TestKit
    {
        [Fact]
        public void TimeoutDroneOnConnect()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            _ = Sys.ActorOf(Drone.Props(TestActor, deliveryA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);
            ExpectMsg<ConnectRequest>();

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }

        [Fact]
        public void TimeoutDroneOnNegotiate()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            _ = Sys.ActorOf(Drone.Props(TestActor, deliveryA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);
            
            ExpectMsg<ConnectRequest>();
            LastSender.Tell(new ConnectResponse(deliveryB), TestActor);

            ExpectMsg<MetricMessage>();

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }

        [Fact]
        public void TimeoutDroneOnIntentions()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            _ = Sys.ActorOf(Drone.Props(TestActor, deliveryA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);

            ExpectMsg<ConnectRequest>();
            LastSender.Tell(new ConnectResponse(deliveryB), TestActor);

            var msg = ExpectMsg<MetricMessage>();
            LastSender.Tell(new MetricMessage(new Priority(msg.Priority.MetricValue + 1, TestActor), 0), TestActor);

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }
    }
}
