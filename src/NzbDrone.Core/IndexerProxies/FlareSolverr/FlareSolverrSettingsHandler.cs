using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.IndexerProxies.FlareSolverr
{
    public class FlareSolverrSettingsHandler : IHandle<ProviderUpdatedEvent<IIndexerProxy>>,
        IHandle<ProviderDeletedEvent<IIndexerProxy>>
    {
        private readonly FlareSolverrSessionService _sessionService;

        public FlareSolverrSettingsHandler(FlareSolverrSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public void Handle(ProviderUpdatedEvent<IIndexerProxy> message)
        {
            var providerDefinition = message.Definition;
            if (IsFlareSolverrSettings(providerDefinition) && ShouldDeleteSession(providerDefinition))
            {
                DeleteFlareSolverrSession(providerDefinition);
            }
        }

        public void Handle(ProviderDeletedEvent<IIndexerProxy> message)
        {
            var providerDefinition = message.Definition;
            if (IsFlareSolverrSettings(providerDefinition) && IsSessionEnabled(providerDefinition))
            {
                DeleteFlareSolverrSession(providerDefinition);
            }
        }

        private void DeleteFlareSolverrSession(ProviderDefinition providerDefinition)
        {
            if (providerDefinition.Settings is FlareSolverrSettings settings)
            {
                const string sessionType = FlareSolverrSessionService.DefaultSession;
                _sessionService.DeleteSession(settings, sessionType);
            }
        }

        private static bool IsFlareSolverrSettings(ProviderDefinition providerDefinition)
        {
            return providerDefinition.ConfigContract == "FlareSolverrSettings";
        }

        private static bool ShouldDeleteSession(ProviderDefinition definition)
        {
            return definition.Settings is FlareSolverrSettings settings && !settings.SessionEnabled;
        }

        private static bool IsSessionEnabled(ProviderDefinition definition)
        {
            return definition.Settings is FlareSolverrSettings settings && settings.SessionEnabled;
        }
    }
}
