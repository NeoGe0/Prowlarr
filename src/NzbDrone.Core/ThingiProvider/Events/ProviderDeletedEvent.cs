using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderDeletedEvent<TProvider> : IEvent
    {
        public int ProviderId { get; private set; }
        public ProviderDefinition Definition { get; private set; }

        public ProviderDeletedEvent(int id, ProviderDefinition providerDefinition)
        {
            ProviderId = id;
            Definition = providerDefinition;
        }
    }
}
