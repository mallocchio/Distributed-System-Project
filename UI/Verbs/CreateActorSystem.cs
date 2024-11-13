using CommandLine;
using ActorsAPI;
using ActorsAPI.APIClasses;
using ActorsAPI.APIClasses.Utils;

namespace UI.Verbs
{
    [Verb("create-actor-system", HelpText = "Create an ActorSystem with a specified port.")]
    internal class CreateActorSystem : IVerb
    {
        [Option('p', HelpText = "Port on which to generate the ActorSystem.", Default = 0)]
        public int Port { get; set; }

        public UIEnvironment Run(UIEnvironment env)
        {
            try
            {
                var droneSystem = DroneSystemFactory.Create(

                    // dettagli della locazione
                    new DeployPointDetails(
                        new Host(Port), 
                        Configs.Default().SystemName
                        ), out var port);

                env.ActorSystems.Add(port, droneSystem);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Created drone system on the port {port}.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {e.Message}.");
            }

            return env;
        }
    }
}
