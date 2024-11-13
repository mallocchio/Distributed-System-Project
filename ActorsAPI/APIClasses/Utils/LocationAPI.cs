using Actors;
using Akka.Actor;

namespace ActorsAPI.APIClasses.Utils
{
    // Strumento per reperire gli indirizzi di attori collocati su sistemi noti e
    // spawnare attori
    public class LocationAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);

        private readonly TimeSpan _timeout;

        private ActorSystem _interfaceDroneSystem;

        public DeployPointDetails DeployPointDetails { get; }

        public LocationAPI(ActorSystem interfaceDroneSystem, DeployPointDetails deployPointDetails) 
        {
            _interfaceDroneSystem = interfaceDroneSystem;
            DeployPointDetails = deployPointDetails;
            _timeout = DEFAULT_TIMEOUT;
        }

        public IActorRef? GetActorRef(string droneName)
        {
            try
            {
                var address = DeployPointDetails.SpawnerAddress() + "/" + droneName;
                return _interfaceDroneSystem.ActorSelection(address).ResolveOne(_timeout).Result;
            } catch (ActorNotFoundException)
            {
                return null;
            } catch (AggregateException)
            {
                return null;
            }
        }

        public IActorRef? SpawnDrone(Props droneProps, string droneName)
        {
            try
            {
                IActorRef spawner = _interfaceDroneSystem
                        .ActorSelection(DeployPointDetails.SpawnerAddress())
                        .ResolveOne(_timeout).Result;

                var result = spawner.Ask(
                    new SpawnDroneRequest(droneProps, droneName)).Result;

                if (result is IActorRef @ref) 
                    return @ref;
                else 
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Verifica che la locazione sia raggiungibile e inizializzata.
        public bool Verify()
        {
            try
            {
                IActorRef spawner = _interfaceDroneSystem
                        .ActorSelection(DeployPointDetails.SpawnerAddress())
                        .ResolveOne(_timeout).Result;

                var res = spawner.Ask(new SpawnDroneTestMessage()).Result;
                return res is bool boolean && boolean;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
