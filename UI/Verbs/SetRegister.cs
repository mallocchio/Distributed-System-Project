using CommandLine;
using ActorsAPI.APIClasses;

namespace UI.Verbs
{
    [Verb("set-register", HelpText = "Set the register for deliveries created from this terminal.")]
    internal class SetRegister : IVerb
    {
        [Option('p', HelpText = "Port of the ActorSystem to connect to.", Required = false)]
        public int Port { get; set; }

        [Option('f', HelpText = "Force the setting of the new register even if one already exists.", Default = false)]
        public bool Force { get; set; }

        [Option('w', HelpText = "Print the address of the currently set register.", Required = false, Default = false)]
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
                Console.Error.WriteLine("Error! Unable to set a register without setting a host.");
                return env;
            }

            var host = new Host(Port);

            if (env.DeliveryEnvironmentAPI.HasRegister())
            {
                if (!Force)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Error: A register already exists: " + $"{env.DeliveryEnvironmentAPI.RegisterAddress}. Use the -f option to force the operation.");
                    return env;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: The register {env.DeliveryEnvironmentAPI.RegisterAddress} will be overwritten.");
                    Console.WriteLine($"It is possible to reset it with the command set-register -pPort -f.");
                }
            }

            try
            {
                env.DeliveryEnvironmentAPI.SetRegister(host);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error while connecting to the register\n{e}.");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Register set successfully: {env.DeliveryEnvironmentAPI.RegisterAddress}.");


            return env;
        }
    }
}
