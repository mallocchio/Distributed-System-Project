using CommandLine;

namespace UI.Verbs
{
    [Verb("terminate-actor-system", HelpText = "Terminate the actor system managed by this terminal.")]
    internal class TerminateActorSystem : IVerb
    {
        [Option('p', HelpText = "Port of the ActorSystem to terminate.", Required = true)]
        public int Port { get; set; }

        public UIEnvironment Run(UIEnvironment env)
        {
            if (!env.ActorSystems.ContainsKey(Port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error! The actor system does not exist, or it is not managed by this terminal.");
                return env;
            }

            try
            {
                env.ActorSystems[Port].Terminate();
                env.ActorSystems.Remove(Port);

            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error in terminating the system. " + $"Exception detected:\n{e}.");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("System terminated successfully.");

            return env;
        }
    }
}
