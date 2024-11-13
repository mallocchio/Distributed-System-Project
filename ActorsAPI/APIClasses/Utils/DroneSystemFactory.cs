using Actors;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Configuration;

namespace ActorsAPI.APIClasses.Utils
{
    public static class DroneSystemFactory
    {
        // Crea un nuovo actor system, inizializzato con l'attore spawner
        public static ActorSystem Create(DeployPointDetails deployPointDetails, out int assignedPort)
        {
            ActorSystemImpl? system = ActorSystem.Create(
                    deployPointDetails.ActorSystemName,
                    ConfigurationFactory.ParseString(MakeHookon(deployPointDetails.Host))
                    ) as ActorSystemImpl;

            assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port!.Value;

            system.ActorOf(Props.Create(() => new Spawner()), "spawner");

            return system;
        }

        private static string MakeHookon(Host host)
        {
            return @$"
akka {{
    loglevel = WARNING
    actor {{
        provider = remote
    }}
    remote {{
        dot-netty.tcp {{
            port = {host.Port}
            hostname = {host.HostName}
        }}
    }}
}}";
        }
    }
}
