using Actors;
using Actors.Messages.External;
using Actors.Messages.Register;
using Actors.DeliveryPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;

namespace Tests.ActorTests
{
    public class RegisterTest : TestKit
    {
        // un drone spawna, non conosce nessun nodo, va in volo.
        [Fact]
        public void FreeSky1()
        {
            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var register = Sys.ActorOf(DronesRegister.Props());
            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // quando riceve una richiesta di connessione, mi aspetto che sia partito
            subject.Tell(new ConnectRequest(deliveryB), TestActor);
            ExpectMsgFrom<TravellingResponse>(subject);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo non in conflitto e parte.
        [Fact]
        public void FreeSky2()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(0, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(25, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo in volo e in conflitto ma 
        // può effettuare una partenza sicura.
        [Fact]
        public void FreeSky3()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(0, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(25, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in volo che non prevede conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new TravellingResponse(deliveryB), TestActor);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


        // un drone spawna, conosce un nodo, osserva il conflitto, lo vince, vola e termina.
        [Fact]
        public void SimpleConflictWin1()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, risponde con una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue - 5, TestActor), 0));

            // perde la negoziazione
            ExpectMsgFrom<TravellingResponse>(subject);

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde e aspetta per volare.
        [Fact]
        public void SimpleConflictLoose1()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunica il volo
            subject.Tell(new TravellingResponse(deliveryB));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde e aspetta per volare.
        [Fact]
        public void SimpleConflictLoose2()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(new Point2D(0, 25), new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(5, 0), new Point2D(5, 30));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunica il volo
            subject.Tell(new TravellingResponse(deliveryB));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }


        // un drone spawna, conosce un nodo e scopre che è in volo, parte al termine
        [Fact]
        public void SimpleConflictLoose3()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(new Point2D(0, 25), new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(5, 0), new Point2D(5, 30));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            // risponde al drone che è in volo
            subject.Tell(new TravellingResponse(deliveryB));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // lo perde, riceve prima un StillHere e poi la notifica sul volo.
        [Fact]
        public void SimpleConflictLoose4()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // Comunica StillHere e successivamente parte per il volo
            subject.Tell(new StillHereMessage());
            Thread.Sleep(1500);
            subject.Tell(new TravellingResponse(deliveryB));

            // mi aspetto un'uscita per consegna completata
            ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        // un drone spawna, conosce un nodo, osserva il conflitto, 
        // in negoziazione emerge che hanno lo stesso ID
        [Fact]
        public void SimpleConflictSameMetric1()
        {
            var register = Sys.ActorOf(DronesRegister.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var deliveryA = new DeliveryPath(Point2D.Origin, new Point2D(25, 25));
            var deliveryB = new DeliveryPath(new Point2D(25, 0), new Point2D(0, 25));

            var subject = Sys.ActorOf(Drone.Props(register, deliveryA), "droneProva");

            // mi aspetto una richiesta di connessione, la risposta è una tratta in conflitto
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(deliveryB), TestActor);

            // mi aspetto una richiesta di negoziazione
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            // risponde con la stessa metrica ma ID diverso
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue, TestActor), 0));

            if (TestActor.CompareTo(subject) < 0)
            {
                // se ho l'ID minore, mi aspetto di vincere
                Thread.Sleep(500);
                subject.Tell(new TravellingResponse(deliveryB));

                ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));

            }
            else
            {
                // altrimenti mi aspetto che vinca lui
                ExpectMsgFrom<TravellingResponse>(subject);
                ExpectMsgFrom<DeliveryCompletedMessage>(subject, new TimeSpan(0, 0, 10));
            }

            Sys.Terminate();
        }
    }
}
