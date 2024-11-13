using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using ActorsAPI.APIClasses;
using ActorsAPI.APIClasses.Utils;

namespace Tests.APITest
{
    public class RegisterAPITest : TestKit
    {
        private readonly string _systemName; 
        private readonly string _registerActorName;

        public RegisterAPITest()
        {
            _systemName = Sys.Name;
            _registerActorName = "register";

            Sys.ActorOf(Props.Create(() => new Spawner()), "spawner");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Sys.Terminate();
        }

        // Avvia un register e verifica il suo corretto funzionamento
        [Fact]
        public void SpawnRegister()
        {
            var api = new DeliveryEnvironmentAPI(Sys, _systemName, _registerActorName);

            api.DeployRegister(Host.GetTestHost());

            IActorRef? register = api.RegisterAddress;

            Assert.NotNull(register);
            CheckRegister(register!);
        }


        // Connessione ad un registro già esistente
        [Fact]
        public void ConnectToExistentRegister()
        {
            // spawn del registro
            LocationAPI LocationAPI = new LocationAPI(
                Sys, DeployPointDetails.GetTestDetails());
            LocationAPI.SpawnDrone(DronesRegister.Props(), _registerActorName);

            // connessione (usando le API)
            var api = new DeliveryEnvironmentAPI(Sys, _systemName, _registerActorName);

            api.SetRegister(Host.GetTestHost());

            IActorRef? register = api.RegisterAddress;

            Assert.NotNull(register);
            CheckRegister(register!);

            Sys.Terminate();
        }

        // Controllo del funzionamento del registro
        private void CheckRegister(IActorRef register)
        {
            register.Tell(new RegisterRequest(TestActor), TestActor);
            ExpectMsgFrom<RegisterResponse>(register);
        }
    }
}
