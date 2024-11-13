using Actors.DeliveryPriority;

namespace Actors.Messages.External
{
    // Messaggio di presentazione del nodo alla rete
    public class ConnectRequest
    {
        public DeliveryPath Path { get; }

        public ConnectRequest(DeliveryPath path)
        {
            Path = path;
        }
    }

    // Messaggio di risposta dei nodi al nodo in rete
    public class ConnectResponse
    {
        public DeliveryPath Path { get; }

        public ConnectResponse(DeliveryPath path)
        {
            Path = path;
        }
    }

    // Messaggio di risposta di un nodo in volo
    public class TravellingResponse
    {
        public DeliveryPath Path { get; }

        public TravellingResponse(DeliveryPath path)
        {
            Path = path;
        }
    }

    // Messaggio per lo scambio delle metriche
    public class MetricMessage
    {
        public Priority Priority { get; }
        public int RelativeRound { get; }

        public MetricMessage(Priority priority, int relativeRound)
        {
            Priority = priority;
            RelativeRound = relativeRound;
        }
    }

    // Messaggio di attesa. Chi lo riceve attende il mittente per il round corrente
    public class StillHereMessage
    {

    }

    // Messaggio di uscita del nodo dalla rete
    public class ExitMessage
    {

    }

    public class DeliveryCompletedMessage : ExitMessage
    {

    }
}
