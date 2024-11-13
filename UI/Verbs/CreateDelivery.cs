using Actors.DeliveryPriority;
using CommandLine;
using ActorsAPI.APIClasses;

namespace UI.Verbs
{
    [Verb("create-delivery", HelpText = "Create a new delivery.")]
    internal class CreateDelivery : IVerb
    {
        [Value(0, HelpText = "X coordinate of the starting point.", Required = true)]
        public double Xstart { get; set; }

        [Value(1, HelpText = "Y coordinate of the starting point.", Required = true)]
        public double Ystart { get; set; }

        [Value(2, HelpText = "X coordinate of the destination point.", Required = true)]
        public double Xend { get; set; }

        [Value(3, HelpText = "Y coordinate of the destination point.", Required = true)]
        public double Yend { get; set; }

        [Option('p', HelpText = "Port of the ActorSystem on which to create the delivery.", Required = true)]
        public int Port { get; set; }

        [Option('n', HelpText = "Name of the delivery.", Required = true)]
        public string? DeliveryName { get; set; }

        public UIEnvironment Run(UIEnvironment env)
        {
            if (!env.DeliveryEnvironmentAPI.HasRegister())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error! First set a register with create-register or set-register!.");
                return env;
            }

            Host host = new Host(Port);
            if (!env.DeliveryEnvironmentAPI.VerifyLocation(host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error! There is no active actor system at the location {host}.");
                return env;
            }

            var start = new MathNet.Spatial.Euclidean.Point2D(Xstart, Ystart);
            var end = new MathNet.Spatial.Euclidean.Point2D(Xend, Yend);
            if (start == end)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error! The starting point and destination point are the same.");
                return env;
            }

            var deliveryPath = new DeliveryPath(start, end);

            // costruzione di un nome univoco per la consegna
            DeliveryName = (DeliveryName is not null)
                ? DeliveryName 
                : deliveryPath.GetHashCode().ToString();
            var ID = $"{DeliveryName}-{"localhost"}:{Port}";

            DeliveryAPI? deliveryAPI;

            try
            {
                deliveryAPI = env.DeliveryEnvironmentAPI
                    .SpawnDelivery(host, deliveryPath, ID, 
                    DeliveryAPI.Factory()) 
                    as DeliveryAPI;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error! The creation of the delivery {DeliveryName} on host {host} failed due to an exception:\n{e}.");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Delivery started!.");
            Console.WriteLine($"Name:\t{DeliveryName}.");

            return env;
        }
    }
}
