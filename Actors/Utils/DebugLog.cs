using Akka.Event;
using System.Diagnostics;

namespace Actors.Utils
{
    // Tool per effettuare il logging su console da parte
    // degli attori, ma solo in caso che la configurazione
    // di debug sia attiva
    public class DebugLog
    {
        private readonly ILoggingAdapter _logger;
        private bool _forceDeactivate = false;

        public DebugLog(ILoggingAdapter logger, bool forceDeactivate = false)
        {
            _logger = logger;
            _forceDeactivate = forceDeactivate;
        }

        [ConditionalAttribute("DEBUG")]
        public void Info(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Info(debugMessage);
        }

        [ConditionalAttribute("DEBUG")]
        public void Warning(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Warning(debugMessage);
        }

        [ConditionalAttribute("DEBUG")]
        public void Error(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Error(debugMessage);
        }

        // Forza la disattivazione del log (indipendentemente dall'ambiente)
        public void ForcedDeactivate()
        {
            _forceDeactivate = true;
        }

        // Rimuovi la disattivazione forzata del log 
        // (ora il log dipenderà solo dall'ambiente). 
        public void RemoveForcedDeactivation()
        {
            _forceDeactivate = false;
        }
    }
}
