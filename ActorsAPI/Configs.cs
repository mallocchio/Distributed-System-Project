namespace ActorsAPI
{
    public class Configs
    {
        public string SystemName { get; }
        public string RegisterDroneName { get; }

        private Configs(string systemName, string registerActorName)
        {
            SystemName = systemName;
            RegisterDroneName = registerActorName;
        }

        public static Configs Default()
        {
            return new Configs("DroneDeliverySystem", "register");
        }
    }
}
