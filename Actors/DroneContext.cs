using Actors.DeliveryPriority;
using Actors.Utils;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.Internal;

namespace Actors
{
    internal sealed class DroneContext
    {
        private readonly DateTime _timeSpawn = DateTime.Now;
        private readonly ITimerScheduler _timers;

        internal IActorContext Context { get; }
        internal DebugLog Log { get; }
        internal ISet<IActorRef> Nodes { get; }
        internal Delivery ThisDelivery { get; }
        internal TimeSpan Age
        {
            get { return DateTime.Now - _timeSpawn; }
        }

        public DroneContext(IActorContext context, ISet<IActorRef> nodes, Delivery thisDelivery, ITimerScheduler timers)
        {
            Context = context;
            Log = new(context.GetLogger());
            Nodes = nodes;
            ThisDelivery = thisDelivery;
            _timers = timers;
        }

        internal void StartMessageTimeout(string key, int count)
        {
            _timers.StartSingleTimer(
                key, 
                new ExpiredTimeout(key),
                count * TimeSpan.FromSeconds(10));
        }

        internal void CancelMessageTimeout(string key)
        {
            _timers.Cancel(key);
        }
    }
}
