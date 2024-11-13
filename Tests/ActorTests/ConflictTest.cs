using Actors;
using Actors.Messages.External;
using Actors.DeliveryPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;

namespace Tests.ActorTests
{
    public class EasyConflictTests : TestKit
    {
        // un drone spawna, non conosce nessun nodo, va in volo.
        [Fact]
        public void ClearAirSpace1()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // quando riceve una richiesta di connessione, mi aspetto che sia partito
            subject.Tell(new ConnectRequest(delivery2), TestActor);
            ExpectMsgFrom<TravellingResponse>(subject);

            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo non in conflitto e va in volo.
        [Fact]
        public void ClearAirSpace2()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(0, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(25, 25));

            var drones = new HashSet<IActorRef> {TestActor};
            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la cui risposta è una tratta senza conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo in volo ma non problematico, va in volo a sua volta.
        [Fact]
        public void ClearAirSpace3()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(0, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(25, 25));

            var drones = new HashSet<IActorRef> {TestActor};
            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la cui risposta è una tratta in volo senza conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new TravellingResponse(delivery2), TestActor);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, lo vince, vola e termina.
        [Fact]
        public void EasyConflictWin1()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue - 5, TestActor), 0));

            // la negoziazione la vince lui e quindi va in volo
            ExpectMsgFrom<TravellingResponse>(subject);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }
 
        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde e aspetta per volare.
        [Fact]
        public void EasyConflictLoose1()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunico il volo
            subject.Tell(new TravellingResponse(delivery2));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde e aspetta per volare.
        [Fact]
        public void EasyConflictLoose2()
        {
            var delivery1 = new DeliveryPath(new Point2D(0, 25), new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(5, 0), new Point2D(5, 30));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunico il volo
            subject.Tell(new TravellingResponse(delivery2));

            // Mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }


        // un drone spawna, conosce un nodo e scopre che è in volo, parte al termine
        [Fact]
        public void EasyConflictLoose3()
        {
            var delivery1 = new DeliveryPath(new Point2D(0, 25), new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(5, 0), new Point2D(5, 30));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            ExpectMsgFrom<ConnectRequest>(subject);

            // rispondo al drone che sono in volo
            subject.Tell(new TravellingResponse(delivery2));

            // Mi aspetto un'uscita per missione completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde, riceve prima un StillHere e poi la notifica sul volo.
        [Fact]
        public void EasyConflictLoose4()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            subject.Tell(new StillHereMessage());

            Thread.Sleep(1500);
            subject.Tell(new TravellingResponse(delivery2));

            // Mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }
 
        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // in negoziazione emerge che hanno lo stesso ID
        [Fact]
        public void EasyConflictSameMetric()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto che mi richieda di negoziare
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            // gli rispondo con la stessa metrica ma ID diverso
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue, TestActor), 0));

            if (TestActor.CompareTo(subject) < 0)
            {
                // se ho l'ID minore, mi aspetto di vincere
                Thread.Sleep(500);
                subject.Tell(new TravellingResponse(delivery2));

                // Mi aspetto un'uscita per consegna completata
                ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            }
            else
            {
                // altrimenti mi aspetto che vinca lui e mi aspetto un'uscita per consegna completata
                ExpectMsgFrom<TravellingResponse>(subject);
                ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));
            }

            Sys.Terminate();
        }
    }
    public class HardConflictTest : TestKit
    {
        // un drone spawna, prima di riuscire a contattare un nodo riceve da lui una
        // metrica, perde una negoziazione, riceve la connessione e poi vince la 
        // seconda negoziazione.
        [Fact]
        public void SecondArrangingRound()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            ExpectMsgFrom<ConnectRequest>(subject);

            subject.Tell(new MetricMessage(new Priority(500, TestActor), 0));

            // mi aspetto una metrica come messaggio
            MetricMessage metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            Assert.Equal(double.MinValue, metricMsg.Priority.MetricValue);
            Assert.Equal(0, metricMsg.RelativeRound);

            // termina la negoziazione con un StillHere
            subject.Tell(new StillHereMessage());

            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto un'altra metrica (di un altro round da perdere)
            MetricMessage metricMsg2 = ExpectMsgFrom<MetricMessage>(subject);
            Assert.Equal(1, metricMsg2.RelativeRound);
            subject.Tell(new MetricMessage(new Priority(metricMsg2.Priority.MetricValue - 5, TestActor), 0));

            // In seguito alla perdita della negoziazione attendo un messaggio di TravellingResponse
            ExpectMsgFrom<TravellingResponse>(subject);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }


        // un drone spawna, conosce un nodo, osserva il conflitto 
        // (anche se le tratte non si incrociano), 
        // lo perde e aspetta per volare.
        [Fact]
        public void NotIntersectConflict()
        {
            var delivery1 = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var delivery2 = new DeliveryPath(new Point2D(15, 0), new Point2D(15, 14));

            var drones = new HashSet<IActorRef> {TestActor};

            var subject = Sys.ActorOf(Drone.Props(drones, delivery1), "TestActor");

            // mi aspetto una richiesta di connessione
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(delivery2), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            subject.Tell(new TravellingResponse(delivery2));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }
    }
}
