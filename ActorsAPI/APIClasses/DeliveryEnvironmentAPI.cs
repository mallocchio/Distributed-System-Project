using Actors;
using Actors.DeliveryPriority;
using Akka.Actor;
using ActorsAPI.APIClasses.Utils;

namespace ActorsAPI.APIClasses
{
    public class DeliveryEnvironmentAPI
    {
        private readonly ActorSystem _interfaceDroneSystem;

        // L'indirizzo del register
        public IActorRef? RegisterAddress { get; private set; }

        // Il nome di tutti gli actor system che compongono questo sistema
        public string SystemName { get; }

        public string RegisterActorName { get; }

        public DeliveryEnvironmentAPI(ActorSystem interfaceDroneSystem, string systemName, string registerActorName)
        {
            _interfaceDroneSystem = interfaceDroneSystem;
            SystemName = systemName;
            RegisterActorName = registerActorName;
        }

        // Verifica se la locazione è attiva
        public bool VerifyLocation(Host host)
        {
            try
            {
                LocationAPI location = new LocationAPI(
                _interfaceDroneSystem,
                new DeployPointDetails(host, SystemName));

                return location.Verify();
            } catch (Exception)
            {
                return false;
            }
        }

        public bool HasRegister()
        {
            return RegisterAddress is not null;
        }

        // Dispiega un register su una locazione
        public void DeployRegister(Host host)
        {
            RegisterAddress = null;

            LocationAPI location = new LocationAPI(
                _interfaceDroneSystem,
                new DeployPointDetails(host, SystemName));

            if (!location.Verify())
                throw new Exception($"Unable to deploy the registry. The location {host} is not active.");

            RegisterAddress = location
                .SpawnDrone(DronesRegister.Props(), RegisterActorName);

            if (RegisterAddress is null)
                throw new Exception(
                    $"The deployment of the registry on {host} has failed.");
        }

        public void SetRegister(Host host)
        {
            RegisterAddress = null;

            LocationAPI location = new LocationAPI(
                _interfaceDroneSystem,
                new DeployPointDetails(host, SystemName));

            if (!location.Verify())
                throw new Exception($"Unable to connect to the registry. The location {host} is not active.");

            RegisterAddress = location.GetActorRef(RegisterActorName);

            if (RegisterAddress is null)
                throw new Exception(
                    $"The deployment of the registry on {host} was not successful.");
        }

        public IDeliveryAPI SpawnDelivery(Host host, 
            DeliveryPath deliveryPath, 
            string deliveryName, 
            IDeliveryAPIFactory deliveryAPIFactory)
        {
            if (!HasRegister())
                throw new Exception($"Unable to deploy the delivery {deliveryName} without first setting up a register.");

            LocationAPI location = new LocationAPI(
                _interfaceDroneSystem,
                new DeployPointDetails(host, SystemName));

            if (!location.Verify())
                throw new Exception($"Unable to deploy the delivery {deliveryName}. The location {host} is not active.");

            // spawn
            IActorRef? deliveryAddress = location
                .SpawnDrone(Drone.Props(RegisterAddress!, deliveryPath), deliveryName);

            if (deliveryAddress is null)
                throw new Exception(
                    $"The deployment of the delivery {deliveryName} on {host} was not successful.");

            // costruzione dell'API
            return deliveryAPIFactory.GetDeliveryAPI(deliveryAddress!);
        }

    }
}
