namespace ActorsAPI.APIClasses
{
    public class AkkaProtocol
    {
        public static readonly AkkaProtocol TCP = new ("akka.tcp");
        public static readonly AkkaProtocol LOCAL = new ("akka");

        public string ProtocolName { get; }

        private AkkaProtocol(string protocolName)
        {
            ProtocolName = protocolName;
        }

        public override string ToString()
        {
            return ProtocolName;
        } 
    }
    
    public class Host
    {
        public string HostName { get; } = "localhost";
        public int Port { get; }
        public AkkaProtocol Protocol { get; } = AkkaProtocol.TCP;

        public Host(int port)
        {
            Port = port;
        }

        public Host(int port, AkkaProtocol protocol)
        {
            Port = port;
            Protocol = protocol;
        }


        public virtual string GetSystemAddress(string systemName)
        {
            return $"{Protocol}://{systemName}@{HostName}:{Port}";
        }

        public string GetDroneAddress(string systemName, string actorName, string actorSpace = "user")
        {
            return $"{GetSystemAddress(systemName)}/{actorSpace}/{actorName}"; 
        }

        public override string? ToString()
        {
            return $"{Protocol}://{HostName}:{Port}";
        }


        // Ottiene un'istzanza di Host da utilizzare per i test
        public static Host GetTestHost() => new TestHost();
    }

    internal class TestHost : Host
    {
        public TestHost() : base(0, AkkaProtocol.LOCAL)
        {
        }

        public override string GetSystemAddress(string systemName)
        {
            return $"{Protocol}://{systemName}";
        }
    }
}
