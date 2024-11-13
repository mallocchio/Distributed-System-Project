namespace ActorsAPI.APIClasses
{
    public class DeployPointDetails
    {
        public Host Host { get; }
        public string ActorSystemName { get; }

        public DeployPointDetails(Host host, string actorSystemName)
        {
            Host = host;
            ActorSystemName = actorSystemName;
        }

        public string SystemAddress()
        {
            return Host.GetSystemAddress(ActorSystemName);
        }

        public string SpawnerAddress()
        {
            return Host.GetSystemAddress(ActorSystemName) + "/user/spawner";
        }

        public static DeployPointDetails GetTestDetails()
        {
            return new DeployPointDetails(Host.GetTestHost(), "test");
        }
    }
}
