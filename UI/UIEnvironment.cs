using Akka.Actor;
using Akka.Configuration;
using ActorsAPI;
using ActorsAPI.APIClasses;

namespace UI
{
    internal class UIEnvironment
    {
        public IDictionary<int, ActorSystem> ActorSystems { get; }

        public ActorSystem InterfaceActorSystem { get; }

        // API principale per coordinare il sistema
        public DeliveryEnvironmentAPI DeliveryEnvironmentAPI { get; }

        public UIEnvironment()
        {
            // inizializza actor system a scopo di interfaccia
            InterfaceActorSystem = ActorSystem.Create(
                "InterfaceActorSystem",
                ConfigurationFactory.ParseString(InterfaceActorSystemHocon));

            // inizializza le liste degli actor system
            ActorSystems = new Dictionary<int, ActorSystem>();

            // inizializza API
            DeliveryEnvironmentAPI = new DeliveryEnvironmentAPI(
                InterfaceActorSystem,
                Configs.Default().SystemName,
                Configs.Default().RegisterDroneName
                );
        }

        public void Terminate()
        {
            InterfaceActorSystem?.Terminate();
            foreach (var system in ActorSystems)
            {
                ActorSystems.Remove(system);
                system.Value.Terminate();
            }
        }

        private static string InterfaceActorSystemHocon => @"
akka {
    loglevel = WARNING
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 0
            hostname = localhost
        }
    }
}";

    }
}
