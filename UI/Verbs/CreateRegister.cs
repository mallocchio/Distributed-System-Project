using CommandLine;
using ActorsAPI.APIClasses;

namespace UI.Verbs
{
    [Verb("create-register", HelpText = "Create a register.")]
    internal class CreateRegister : IVerb
    {
        [Option('p', HelpText = "Port of the ActorSystem.", Required = false, Default = 0)]
        public int Port { get; set; }

        [Option('f', HelpText = "Force the creation of the new register even if one already exists.", Required = false, Default = false)]
        public bool Force { get; set; }

        [Option('w', HelpText = "Print the address of the register set for this drone.", Required = false, Default = false)]
        public bool Rep { get; set; }

        public UIEnvironment Run(UIEnvironment env)
        {
            if(Rep)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Register set: " + $"{env.DeliveryEnvironmentAPI.RegisterAddress}.");
                return env;
            }
            
            if (Port == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error! Unable to create the register without setting a host.");
                return env;
            }

            var host = new Host(Port);

            if (!env.DeliveryEnvironmentAPI.VerifyLocation(host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error! There is no active actor system at the location {host}.");
                return env;
            }

            if (env.DeliveryEnvironmentAPI.HasRegister())
            {
                if (!Force)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Error: A register already exists at the current state: " + $"{env.DeliveryEnvironmentAPI.RegisterAddress}. Use the -f option to force the operation.");
                    return env;
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: The register {env.DeliveryEnvironmentAPI.RegisterAddress} will be overwritten.");
                    Console.WriteLine($"It is possible to reset it with the command set-register -pPort -f.");
                }
            }
            
            try
            {
                env.DeliveryEnvironmentAPI.DeployRegister(host);
            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error during the creation of the register. Exception detected:\n{e}.");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Register created successfully: {env.DeliveryEnvironmentAPI.RegisterAddress}.");

            return env;
        }
    }
}
